using Godot;
using System;

public partial class Ear : Control
{
	[Export]
	public Vector2I EarEndPoint = new();
	[Export]
	public float VelocityDamping = 0.01f;
	[Export]
	public float AccelerationDamping = 0.01f;

	private Vector2I _earEnd = new();
	private Vector2 Velocity = new();
	private Vector2I _windowPos;
	private Window window;

	private Vector2 _earPos => Position + _windowPos + new Vector2(Size.X / 2, Size.Y);

	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public override void _Ready()
	{
		window = GetWindow();
		if (window.Visible)
		{
			_windowPos = DisplayServer.WindowGetPosition(window.GetWindowId());
		}
		_earEnd = EarEndPoint + (Vector2I)_earPos;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (window.Visible)
		{
			Velocity += (EarEndPoint + _earPos - _earEnd) * AccelerationDamping;

			Velocity *= VelocityDamping;

			_earEnd += (Vector2I)Velocity;
		}
	}


	public override void _Process(double delta)
	{
		if (window.Visible)
		{
			_windowPos = DisplayServer.WindowGetPosition(window.GetWindowId());

			Rotation = Mathf.Atan2((_earEnd - _earPos).X, (_earPos - _earEnd).Y);
		}
	}
}
