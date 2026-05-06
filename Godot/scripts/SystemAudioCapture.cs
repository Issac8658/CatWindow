using Godot;
using FFmpeg;
using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;

public partial class SystemAudioCapture : Node
{
	private static MMDeviceEnumerator enumerator = new();
	private WasapiLoopbackCapture _capture;
	private AudioStreamGeneratorPlayback _playback;
	private AudioStreamPlayer _player;
	private string _oldNAudioDevice;

	[Export]
	public int TargetBus = 0;

	public override void _Ready()
	{

		_player = new() { Bus = AudioServer.GetBusName(TargetBus) };
		AddChild(_player);

		AudioStreamGenerator gen = new()
		{
			MixRate = FFmpeg.FFmpeg.SAMPLE_RATE,
			BufferLength = 0.05f
		};
		_player.Stream = gen;

		_player.Play();
		_playback = _player.GetStreamPlayback() as AudioStreamGeneratorPlayback;

		StartCapture();
		CheckLoop();
	}

	private void CheckLoop()
	{
		string id = GetCurrentDevice().ID;
		if (_oldNAudioDevice != id)
		{
			GD.Print("Device changed, recapturing...");
			StartCapture();
			_oldNAudioDevice = id;
		}

		GetTree().CreateTimer(1).Timeout += CheckLoop;
	}

	public void StartCapture(string DeviceOverride = null)
	{
		StopCapture();

		MMDevice device;
		
		//GD.Print(DeviceOverride);
		if (DeviceOverride == null || DeviceOverride == "Default")
			device = GetCurrentDevice();
		else
			device = GetDeviceFromFriendlyName(DeviceOverride);

		_capture = new(device)
		{
			ShareMode = AudioClientShareMode.Shared,
			WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2)
		};

		_capture.DataAvailable += (s, e) => ProcessAudio(e.Buffer, e.BytesRecorded);

		_capture.StartRecording();
		GD.Print("Capture started");
	}
	
	public void StopCapture()
	{
		if (_capture != null)
		{
			_capture.StopRecording();
			_capture = null;
			GD.Print("Capture stopped");
		}
	}

	private void ProcessAudio(byte[] buffer, int bytes)
	{
		int floatCount = bytes / 4;
		float[] samples = new float[floatCount];
		Buffer.BlockCopy(buffer, 0, samples, 0, bytes);

		FFmpegPlayer.PushSamples(_playback, samples);
	}

	private static MMDevice GetCurrentDevice() => enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

	private static MMDevice GetDeviceFromFriendlyName(string FriendlyName)
	{
		var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
		foreach (var device in devices)
			if (device.FriendlyName == FriendlyName) return device;
		return GetCurrentDevice();
	}
}
