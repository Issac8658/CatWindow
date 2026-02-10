using Godot;
using System;

public partial class AudioVisualizator : Node2D
{
	[Export]
	public float ShakePower = 20f;
	[Export]
	public float WaveScale = 8;
	[Export]
	public int MinFramesCount = 1024;
	[Export]
	public FFmpeg.FFmpegPlayer Player;

	private Vector2I _lastShake = new();

	private AudioEffectCapture Capture = AudioServer.GetBusEffect(0, 0) as AudioEffectCapture;
	private AudioEffectSpectrumAnalyzerInstance Spectrum = AudioServer.GetBusEffectInstance(0, 1) as AudioEffectSpectrumAnalyzerInstance;
	private SubViewport _viewport;

	private Godot.Collections.Array<Vector2> Buffer = [];

	private int f;

	public override void _Ready()
	{
		_viewport = GetViewport() as SubViewport;
	}
	public override void _Process(double delta)
	{
			QueueRedraw();
		// window shaking
		if (Player.Playing)
		{
			float spec = Spectrum.GetMagnitudeForFrequencyRange(60, 1000).X;
			Vector2I shake = (Vector2I)(new Vector2((float)GD.RandRange(-ShakePower, ShakePower), (float)GD.RandRange(-ShakePower, ShakePower)) * Mathf.Pow(spec, 2f));
			GetWindow().Position += shake - _lastShake;
			_lastShake = shake; // cat window, come back now
		}
	}

	public override void _Draw()
	{
		float Volume = Mathf.DbToLinear(AudioServer.GetBusPeakVolumeLeftDb(0, 0));
		float PoweredVolume = Mathf.Pow(Volume - 0.0025f, 0.25f); // for wave transparency
		
		Godot.Collections.Array<Vector2> Samples = [.. Capture.GetBuffer(Capture.GetFramesAvailable())];
		Buffer += Samples;
		Buffer.Reverse();
		Buffer.Resize((int)((_viewport.Size.X + 1) * WaveScale));
		Buffer.Reverse();
		for (int i = 0; i < _viewport.Size.X; i += 1)
		{
			// left channel
			float Sample1 = Buffer[(int)(i * WaveScale)].X;
			float X1 = i;
			float Y1 = _viewport.Size.Y / 2 + Sample1 * _viewport.Size.Y / 2;

			// right channel
			float Sample2 = Buffer[(int)((i+1) * WaveScale)].Y;
			float X2 = i + 1;
			float Y2 = _viewport.Size.Y / 2 + Sample2 * _viewport.Size.Y / 2;

			// Making lines red if amplitude >1
			Color col = new Color(1, 1, 1, PoweredVolume);
			if (MathF.Abs(Sample1) > 1f || MathF.Abs(Sample2) > 1f)
				col = new Color(1, 0, 0, PoweredVolume);

			DrawLine(new Vector2(X1, Y1), new Vector2(X2, Y2), col);
		}
	}
}
