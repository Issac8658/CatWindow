using System.IO;
using Godot;

// draw your ascii cat here

public partial class Main : Node
{
	[Export]
	public Control ClickableFace;
	[Export]
	public FFmpeg.FFmpegPlayer Player;
	[Export]
	public Playlist PlaylistWindow;

	private Window _mainWindow;

	public override void _Ready()
	{
		_mainWindow = GetWindow();

		_mainWindow.MaximizeDisabled = true;
		_mainWindow.MinimizeDisabled = true;

		GetWindow().FilesDropped += (files) =>
		{
			if (files.Length > 0)
				if (Path.GetExtension(files[0]).Equals(".m3u", System.StringComparison.InvariantCultureIgnoreCase)
				 || Path.GetExtension(files[0]).Equals(".m3u8", System.StringComparison.InvariantCultureIgnoreCase))
					PlaylistWindow.ParsePlaylist(files[0]);
				else
					Player.Play(files[0]);
		};

		ClickableFace.GuiInput += Event =>
		{
			if (Event is InputEventMouseButton EventMouse)
				if (Event.IsPressed() && EventMouse.ButtonIndex == MouseButton.Left)
					PlaylistWindow.Visible = !PlaylistWindow.Visible;
		};
	}
}
