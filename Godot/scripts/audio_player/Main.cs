using System.IO;
using Godot;

// draw your ascii cat here :3c

public partial class Main : Control
{
	[Export]
	public Control ClickableFace;
	[Export]
	public FFmpeg.FFmpegPlayer Player;
	[Export]
	public Playlist PlaylistWindow;

	public override void _Ready()
	{
		if (OS.GetCmdlineArgs().Length > 0)
			if (Godot.FileAccess.FileExists(OS.GetCmdlineArgs()[0]) || FFmpeg.FFmpeg.IsUrl(OS.GetCmdlineArgs()[0]))
			{
				Player.Play(OS.GetCmdlineArgs()[0]);
			}

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

		GetNode("/root/MutexListener").Connect("FileCaught", Callable.From((string path) =>
		{
			if (Godot.FileAccess.FileExists(path) || FFmpeg.FFmpeg.IsUrl(path))
				Player.Play(path);
		}));
	}
}
