using Godot;
using System.IO;
using FFmpeg;
using FFmpeg.FFprobe;
using System.Text.Json.Serialization;
using System.Linq;
using System.Threading.Tasks;

public partial class Playlist : Window
{
	//const string PLAYLISTS_DIR = "user://Playlists";

	//private DirAccess playlistsFolder;

	private Godot.Collections.Dictionary<string, bool> _currentPlaylist = [];
	private string _currentPlaylistFile;
	public int _current = 0;
	//private Godot.Collections.Array<string> _playlists = [];

	[Export]
	public PackedScene TrackLabelTemplate;
	[Export]
	public FFmpegPlayer Player;
	[Export]
	public Container TrackLabelsContainer;
	[Export]
	public Button NextButton;
	[Export]
	public Button PreviousButton;
	[Export]
	public Button StopButton;

	public override void _Ready()
	{
		CloseRequested += Hide;
		Player.Stopped += Next;

		NextButton.Pressed += Next;
		PreviousButton.Pressed += Previous;
	}

	public void ParsePlaylist(string PlaylistPath)
	{
		if (Godot.FileAccess.FileExists(PlaylistPath))
		{
			GD.Print($"Playlist {PlaylistPath}");
			DirAccess Folder = DirAccess.Open(Path.GetDirectoryName(PlaylistPath));
			Godot.FileAccess Playlist = Godot.FileAccess.Open(PlaylistPath, Godot.FileAccess.ModeFlags.Read);

			Unload();

			string[] Lines = Playlist.GetAsText().Replace("\r", "").Split("\n");

			int c = 0;
			for (int i = 0; i < Lines.Length; i++)
			{
				string line = Lines[i];
				if (!line.StartsWith('#'))
				{
					if (Folder.FileExists(line))
					{
						GD.Print($"Track {line}");
						string path = RelToAbs(Folder, line);

						Track trackLabel = TrackLabelTemplate.Instantiate<Track>();
						trackLabel.TrackIndex = c + 1;
						trackLabel.TrackName = Path.GetFileNameWithoutExtension(path);

						trackLabel.GuiInput += Event =>
						{
							if (Event is InputEventMouseButton mouseButtonEvent)
							{
								if (mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left)
								{
									Select(c);
								}
							}
						};
						TrackLabelsContainer.AddChild(trackLabel);

						Task.Run(async () => _ = SetMeta(trackLabel, path));
						
						_currentPlaylist.Add(path, true);
						c++;
					}
					//else
					//_currentPlaylist.Add(line, false);
				}
			}
			_currentPlaylistFile = PlaylistPath;

			Player.Loop = false;
			Select(0);
			UpdateTracks();

			NextButton.Visible = PreviousButton.Visible = true;
		}
	}

	public void Unload()
	{
		foreach (Node child in TrackLabelsContainer.GetChildren())
		{
			child.QueueFree();
		}
		_currentPlaylist = [];
		_currentPlaylistFile = null;
		_current = 0;
		Player.Loop = true;

		NextButton.Visible = PreviousButton.Visible = false;

		Player.Stop();
	}

	public void UpdateTracks()
	{
		for (int i = 0; i < TrackLabelsContainer.GetChildCount(); i++)
		{
			(TrackLabelsContainer.GetChildren()[i] as Track).Selected = i == _current;
		}
	}

	public void Next()
	{
		if (_currentPlaylist.Count == 0)
			return;

		_current++;
		if (_current >= _currentPlaylist.Count)
			_current = 0;
		Player.Play(_currentPlaylist.Keys.ElementAt(_current));
		UpdateTracks();
	}

	public void Previous()
	{
		if (_currentPlaylist.Count == 0)
			return;

		_current--;
		if (_current < 0)
			_current = _currentPlaylist.Count - 1;
		Player.Play(_currentPlaylist.Keys.ElementAt(_current));
		UpdateTracks();
	}

	public void Select(int Index)
	{
		if (_currentPlaylist.Count == 0)
			return;

		_current = Index;
		if (_current >= _currentPlaylist.Count)
			_current = 0;
		if (_current < 0)
			_current = _currentPlaylist.Count - 1;
		Player.Play(_currentPlaylist.Keys.ElementAt(_current));
		UpdateTracks();
	}

	private static string RelToAbs(DirAccess folder, string file)
	{
		foreach (string f in folder.GetFiles())
		{
			string fullPath = Path.Combine(folder.GetCurrentDir(), f);
			if (folder.IsEquivalent(fullPath, file))
			{
				GD.Print(fullPath);
				return fullPath;
			}
		}
		return file;
	}

	private static async Task SetMeta(Track Label, string path)
	{
		FFprobeResult Metadata = FFprobe.GetMetadata(path);
		if (Metadata == null) return;

		Label.TrackType = Metadata.Format.FormatName;
		Label.TrackLength = float.Parse(Metadata.Format.Duration.Replace('.', ','));
		Label.TrackSize = ulong.Parse(Metadata.Format.Size.ToString());

		if (Metadata.Format != null)
		{
			if (Metadata.Format.Tags != null)
				if (Metadata.Format.Tags.Title != null)
					if (Metadata.Format.Tags.Artist != null)
						Label.TrackName = $"{Metadata.Format.Tags.Title}[color=dim_gray] — {Metadata.Format.Tags.Artist}[/color]";
					else
						Label.TrackName = Metadata.Format.Tags.Title;
				else
					Label.TrackName = Path.GetFileNameWithoutExtension(path);
		}
	}
}
