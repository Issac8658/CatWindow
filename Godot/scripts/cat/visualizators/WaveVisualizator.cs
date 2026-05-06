using Godot;

public partial class WaveVisualizator : Node2D
{
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
	[Export(PropertyHint.Range, "1, 1024,or_greater")]
	public int DrawFramesSkip = 1;
	[Export]
	public bool DisableDrawing = false;

	private AudioEffectCapture Capture = AudioServer.GetBusEffect(2, 0) as AudioEffectCapture;
	private SubViewport _viewport;

	private Godot.Collections.Array<Vector2> Buffer = [];

	public override void _Ready()
	{
		_viewport = GetViewport() as SubViewport;
	}
	public override void _Process(double delta)
	{
		if (IsVisibleInTree())
			QueueRedraw();
	}

	public override void _Draw()
	{
		if (!DisableDrawing)
		{
			Vector2I viewportSize = _viewport.Size;

			if (Capture.GetFramesAvailable() / MinFramesCount >= 1)
			{
				Buffer += [.. Capture.GetBuffer(Capture.GetFramesAvailable() / MinFramesCount * MinFramesCount)];
				Buffer.Reverse();
				Buffer.Resize(Mathf.FloorToInt((viewportSize.X + 1) * WaveScale));
				Buffer.Reverse();
			}
			for (int i = 0; i < Buffer.Count; i += DrawFramesSkip)
			{
				Vector2 Value1 = Buffer[Mathf.Clamp(i, 0, Buffer.Count - 1)];
				Vector2 Value2 = Buffer[Mathf.Clamp(i + DrawFramesSkip, 0, Buffer.Count - 1)];
				if (Mathf.Abs(Value1.X) > 0.0005 || Mathf.Abs(Value2.Y) > 0.0005)
				{
					// left channel
					float Y1 = viewportSize.Y / 2 + -Value1.X * viewportSize.Y / 2;
					// right channel
					float Y2 = viewportSize.Y / 2 + -Value2.Y * viewportSize.Y / 2;
					DrawLine(new(i / WaveScale, Y1), new((i + DrawFramesSkip - 1) / WaveScale, Y2), new(1, 1, 1));
				}
			}
		}
	}
}
