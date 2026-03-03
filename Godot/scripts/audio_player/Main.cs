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
		string[] CMDArgs = OS.GetCmdlineArgs();
		if (OS.GetCmdlineArgs().Length > 0)
			if (Godot.FileAccess.FileExists(CMDArgs[0]) || FFmpeg.FFmpeg.IsUrl(CMDArgs[0]))
			{
				PlayFile(CMDArgs[0]);
				GD.Print($"Found file in cmd arguments: {CMDArgs[0]}");
			}

		GetWindow().FilesDropped += (files) =>
		{
			GD.Print($"Files dropped:");
			GD.Print(new Godot.Collections.Array<string>(files));

			if (files.Length > 0) PlayFile(files[0]);
		};

		ClickableFace.GuiInput += Event =>
		{
			if (Event is InputEventMouseButton EventMouse)
				if (Event.IsPressed() && EventMouse.ButtonIndex == MouseButton.Left)
					PlaylistWindow.Visible = !PlaylistWindow.Visible;
		};

		GetNode("/root/PipeListener").Connect("FileCaught", Callable.From((string path) =>
		{
			GD.Print($"Received file from pipe {path}");

			if (Godot.FileAccess.FileExists(path) || FFmpeg.FFmpeg.IsUrl(path))
				PlayFile(path);
		}));
	}

	public void PlayFile(string file)
	{
		if (Path.GetExtension(file).Equals(".m3u", System.StringComparison.InvariantCultureIgnoreCase)
		 || Path.GetExtension(file).Equals(".m3u8", System.StringComparison.InvariantCultureIgnoreCase))
			PlaylistWindow.ParsePlaylist(file);
		else
		{
			PlaylistWindow.Unload();
			Player.Play(file);
		}
	}
}
