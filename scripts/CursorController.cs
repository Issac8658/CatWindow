using Godot;

// for future if i want to add ability to make more cats at once
public static class GlobalCursorController
{
	public static Vector2I MousePosition;
}

public partial class CursorController : Node
{
	public override void _Process(double delta)
	{
		GlobalCursorController.MousePosition = DisplayServer.MouseGetPosition();
	}
}
