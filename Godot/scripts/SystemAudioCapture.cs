using Godot;
using FFmpeg;
using NAudio.Wave;
using System;
using NAudio.CoreAudioApi;

public partial class SystemAudioCapture : Node
{
	private WasapiLoopbackCapture capture;
	private AudioStreamGeneratorPlayback _playback;

	private AudioStreamPlayer _player;
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
	}

	public void StartCapture()
	{
		var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

		capture = new WasapiLoopbackCapture(device);
		capture.ShareMode = AudioClientShareMode.Shared;

		capture.DataAvailable += (s, e) =>
		{
			byte[] buffer = e.Buffer;
			int bytes = e.BytesRecorded;

			ProcessAudio(buffer, bytes);
		};

		capture.StartRecording();
	}

	private void ProcessAudio(byte[] buffer, int bytes)
	{
		int floatCount = bytes / 4;
		float[] samples = new float[floatCount];
		Buffer.BlockCopy(buffer, 0, samples, 0, bytes);

		FFmpegPlayer.PushSamples(_playback, samples);
	}
}
