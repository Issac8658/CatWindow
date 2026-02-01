using Godot;
using System;

public partial class AudioVisualizator : Control
{
	[Export]
	public float ShakePower = 20f;
	[Export]
	public int scale = 8;
	[Export]
	public int bufferOffset = 0;
	[Export]
	public float InterpalationTime = 0.1f;
	[Export]
	public FFmpeg.FFmpegPlayer Player;

	private Vector2I _lastShake = new();

	private AudioEffectCapture Capture = AudioServer.GetBusEffect(0, 0) as AudioEffectCapture;
	private AudioEffectSpectrumAnalyzerInstance Spectrum = AudioServer.GetBusEffectInstance(0, 1) as AudioEffectSpectrumAnalyzerInstance;
	
	private int _availableFrames;

	private Godot.Collections.Array<float> _oldSamples = [];

	public override void _Ready()
	{
		_availableFrames = Capture.GetFramesAvailable();
	}
	public override void _Process(double delta)
	{
		if (Capture.GetFramesAvailable() >= _availableFrames)
		QueueRedraw();
	}

	public override void _Draw()
	{
		float Volume = Mathf.DbToLinear(AudioServer.GetBusPeakVolumeLeftDb(0, 0));
		float PoweredVolume = Mathf.Pow(Volume - 0.0025f, 0.25f); // for wave transparency
		Vector2[] Samples = Capture.GetBuffer((int)Size.X * scale + bufferOffset);
		if (_oldSamples.Count != Samples.Length)
			_oldSamples.Resize(Samples.Length);
		for (int i = bufferOffset; i < Samples.Length - scale; i += scale)
		{
			// left channel
			int i1 = i / scale;
			float Sample1 = Samples[i].X;
			_oldSamples[i1] = Mathf.Lerp(_oldSamples[i1], Sample1, InterpalationTime); // interpalation
			float X1 = (i - bufferOffset) / (float)scale;
			float Y1 = Size.Y / 2 + _oldSamples[i1] * Size.Y / 2;

			// right channel
			int i2 = (i + scale) / scale; // i know about (i + scale) and choosing a different channel wave can look like saw, but im too lazy to fix it :P
			float Sample2 = Samples[i + scale].Y;
			_oldSamples[i2] = Mathf.Lerp(_oldSamples[i2], Sample2, InterpalationTime); // interpalation
			float X2 = (i - bufferOffset)/(float)scale + 1f;
			float Y2 = Size.Y / 2 + _oldSamples[i2] * Size.Y / 2;

			// Making lines red if amplitude >1
			Color col = new Color(1, 1, 1, PoweredVolume);
			if (MathF.Abs(_oldSamples[i1]) > 1f || MathF.Abs(_oldSamples[i2]) > 1f)
				col = new Color(1, 0, 0, PoweredVolume);

			DrawLine(new Vector2(X1, Y1), new Vector2(X2, Y2), col);
		}
		// window shaking
		if (Player.Playing)
		{
			float spec = Spectrum.GetMagnitudeForFrequencyRange(60, 1000).X;
			Vector2I shake = (Vector2I)(new Vector2((float)GD.RandRange(-ShakePower, ShakePower), (float)GD.RandRange(-ShakePower, ShakePower)) * Mathf.Pow(spec, 2f));
			GetWindow().Position += shake - _lastShake;
			_lastShake = shake; // cat window, come back now
		}
	}
}
