using Godot;
using System;

public partial class WaveVisualizator : Node2D
{
	[Export]
	public float ShakePower = 20f;
	[Export]
	public float WaveScale = 8;
	[Export]
	// Do  - 733 (Semitone 0)
	// Do# - 692 (Semitone 1)
	// Re  - 653 (Semitone 2)
	// Re# - 617 (Semitone 3)
	// Mi  - 583 (Semitone 4)
	// Fa  - 550 (Semitone 5)
	// Fa# - 519 (Semitone 6)
	// So  - 490 (Semitone 7)
	// So# - 462 (Semitone 8)
	// La  - 436 (Semitone 9)
	// La# - 412 (Semitone 10)
	// Si  - 389 (Semitone 11)
	public int MinFramesCount = 1024;
	[Export]
	public FFmpeg.FFmpegPlayer Player;

	private Vector2I _lastShake = new();

	private AudioEffectCapture Capture = AudioServer.GetBusEffect(2, 0) as AudioEffectCapture;
	private AudioEffectSpectrumAnalyzerInstance Spectrum = AudioServer.GetBusEffectInstance(2, 1) as AudioEffectSpectrumAnalyzerInstance;
	private SubViewport _viewport;

	private Godot.Collections.Array<Vector2> Buffer = [];
	private Godot.Collections.Array<Vector2> CompressedBuffer = [];

	public override void _Ready()
	{
		_viewport = GetViewport() as SubViewport;
	}
	public override void _Process(double delta)
	{
		if (IsVisibleInTree())
			QueueRedraw();
		// window shaking
		//if (false)
		//{
		//	float spec = Spectrum.GetMagnitudeForFrequencyRange(60, 1000).X;
		//	Vector2I shake = (Vector2I)(new Vector2((float)GD.RandRange(-ShakePower, ShakePower), (float)GD.RandRange(-ShakePower, ShakePower)) * Mathf.Pow(spec, 2f));
		//	GetWindow().Position += shake - _lastShake;
		//	_lastShake = shake; // cat window, come back now
		//}
	}

	public override void _Draw()
	{

		float Volume = 1;
		float PoweredVolume = Mathf.Pow(Volume - 0.0025f, 0.125f); // for wave transparency

		Vector2I viewportSize = _viewport.Size;

		if (Capture.GetFramesAvailable() / MinFramesCount >= 1)
		{
			Godot.Collections.Array<Vector2> Samples = [.. Capture.GetBuffer(Capture.GetFramesAvailable() / MinFramesCount * MinFramesCount)];
			Buffer += Samples;
			Buffer.Reverse();
			Buffer.Resize(Mathf.FloorToInt((viewportSize.X + 1) * WaveScale));
			Buffer.Reverse();
		}

		for (int i = 0; i < Buffer.Count; i += 1)
		{
			// left channel
			float Sample1 = Buffer[Mathf.Clamp(i, 0, Buffer.Count - 1)].X;
			float Y1 = viewportSize.Y / 2 + -Sample1 * viewportSize.Y / 2;

			// right channel
			float Sample2 = Buffer[Mathf.Clamp(i + 1, 0, Buffer.Count - 1)].Y;
			float Y2 = viewportSize.Y / 2 + -Sample2 * viewportSize.Y / 2;

			// Making lines red if amplitude >1
			Color col = new(1, 1, 1, PoweredVolume);
			if (MathF.Abs(Sample1) > 1f || MathF.Abs(Sample2) > 1f)
				col = new Color(1, 0, 0, PoweredVolume);

			DrawLine(new Vector2(i / WaveScale, Y1), new Vector2(i / WaveScale, Y2), col);
		}
	}
}
