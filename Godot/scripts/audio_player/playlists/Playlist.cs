using Godot;
using System.IO;
using FFmpeg;
using FFmpeg.FFprobe;
using System.Text.Json.Serialization;

public partial class Playlist : Window
{
	//const string PLAYLISTS_DIR = "user://Playlists";

	//private DirAccess playlistsFolder;

	private string[] _currentPlaylist = [];
	//private Godot.Collections.Array<string> _playlists = [];

	[Export]
	public PackedScene TrackLabelTemplate;
	[Export]
	public FFmpegPlayer Player;
	[Export]
	public Container TrackLabelsContainer;
	//[Export]
	//public OptionButton PlaylistDropdown;


	public override void _Ready()
	{
		/*
			// creating playlists directory

			string playlistsAbsolutePath = ProjectSettings.GlobalizePath(PLAYLISTS_DIR);


			DirAccess userFolder = DirAccess.Open("user://");

			if (!userFolder.DirExists(PLAYLISTS_DIR)) // if playlists folder isn't exist
			{
				userFolder.MakeDir(PLAYLISTS_DIR); // cat :3 will create it
			}
			playlistsFolder = DirAccess.Open(PLAYLISTS_DIR);

			UpdatePlaylists(playlistsFolder.GetFiles(), 0); // zero for new playlist

		*/
		CloseRequested += Hide;
	}
	/*
	private void UpdatePlaylists(string[] playlists, uint selectedPlaylist)
	{
		// clearing music list container
		foreach (Node child in TrackLabelsContainer.GetChildren())
		{
			child.QueueFree();
		}
		// clearing dropdown list
		for (int i = 0; i < PlaylistDropdown.ItemCount; i++)
		{
			PlaylistDropdown.RemoveItem(i);
		}
		// Adding playlists
		foreach (string file in playlists)
		{
			// if file is a playlist (m3u8, m3u or pls)
			if (Path.GetExtension(file).Equals(".m3u8", StringComparison.OrdinalIgnoreCase) 
			 || Path.GetExtension(file).Equals(".m3u", StringComparison.OrdinalIgnoreCase))
			{
				AddPlaylist(Path.Combine(playlistsFolder.GetCurrentDir(), file));
			}
		}
	}

	private void AddPlaylist(string PlaylistPath)
	{
		if (playlistsFolder.FileExists(PlaylistPath))
		{
			PlaylistDropdown.AddItem(Path.GetFileNameWithoutExtension(PlaylistPath));
			_playlists.Add(PlaylistPath);
		}
	}
	*/

	//private void ParseAndSelectPlaylist(uint playlistId)
	public void ParsePlaylist(string PlaylistPath)
	{
		if (Godot.FileAccess.FileExists(PlaylistPath))
		{
			GD.Print($"Playlist {PlaylistPath}");
			DirAccess Folder = DirAccess.Open(Path.GetDirectoryName(PlaylistPath));
			Godot.FileAccess Playlist = Godot.FileAccess.Open(PlaylistPath, Godot.FileAccess.ModeFlags.Read);
			foreach (Node child in TrackLabelsContainer.GetChildren())
			{
				child.QueueFree();
			}
			string[] Lines = Playlist.GetAsText().Replace("\r", "").Split("\n");

			int i = 0;
			foreach (string line in Lines)
			{
				if (!line.StartsWith('#'))
				{
					if (Folder.FileExists(line))
					{
						GD.Print($"Track {line}");
						FFprobeResult Metadata = FFprobe.GetMetadata(RelToAbs(Folder, line));
						if (Metadata == null) break;

						Track trackLabel = TrackLabelTemplate.Instantiate<Track>();
						trackLabel.TrackIndex = ++i;
						trackLabel.TrackName = Path.GetFileNameWithoutExtension(line);

						GD.Print($"Has meta");

						trackLabel.TrackType = Metadata.Format.FormatName;
						trackLabel.TrackLength = float.Parse(Metadata.Format.Duration.Replace('.', ','));
						trackLabel.TrackSize = ulong.Parse(Metadata.Format.Size.ToString());

						if (Metadata.Format != null)
						{
							GD.Print($"Has tags");
							if (Metadata.Format.Tags != null)
								if (Metadata.Format.Tags.Title != null)
									if (Metadata.Format.Tags.Artist != null)
										trackLabel.TrackName = $"{Metadata.Format.Tags.Title}[color=dim_gray] — {Metadata.Format.Tags.Artist}[/color]";
									else
										trackLabel.TrackName = Metadata.Format.Tags.Title;
								else
									trackLabel.TrackName = Path.GetFileNameWithoutExtension(line);
						}
						//trackLabel.TrackLength = Godot.FileAccess.GetSize(Folder.GetCurrentDir());
						TrackLabelsContainer.AddChild(trackLabel);
					}
				}
			}
		}
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
}
