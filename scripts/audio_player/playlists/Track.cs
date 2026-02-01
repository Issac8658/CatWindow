using Godot;
using System;

public partial class Track : Node
{
	private int _trackIndex = 0;
	private string _trackName = "Unnamed track";
	private float _trackLength = 0; // in seconds
	private ulong _trackSize = 0; // in bytes
	private string _trackType = "CAT"; // mp3 ogg wav etc...

	[Export]
	public int TrackIndex
	{
		get => _trackIndex;
		set
		{
			TrackIndexLabel.Text = ToLenghtBBCode(value.ToString(), "0", 3);
			_trackIndex = value;
		}
	}
	[Export]
	public string TrackName
	{
		get => _trackName;
		set
		{
			_trackName = TrackNameLabel.Text = value;
		}
	}
	[Export]
	public float TrackLength
	{
		get => _trackLength;
		set
		{
			int remainder = (int)value;
			string formatedText = "";
			int hours = remainder / 3600;
			remainder -= hours * 3600;
			int minutes = remainder / 60;
			remainder -= minutes * 60;
			if (hours > 0)
			{
				formatedText += ToLenght(hours.ToString(), "0", 2) + ":";
				formatedText += ToLenght(minutes.ToString(), "0", 2) + ":";
			}
			else
				formatedText += minutes.ToString() + ":";
			formatedText += ToLenght(remainder.ToString(), "0", 2);
			TrackLengthLabel.Text = formatedText;
			_trackLength = value;
		}
	}
	[Export]
	public ulong TrackSize
	{
		get => _trackSize;
		set
		{
			TrackSizeLabel.Text = FormatBytes(value);
			_trackSize = value;
		}
	}
	[Export]
	public string TrackType
	{
		get => _trackType;
		set
		{
			_trackType = TrackTypeLabel.Text = value.ToUpper();
		}
	}
	[Export]
	public string TrackPath;

	[ExportCategory("Nodes")]
	[Export]
	public RichTextLabel TrackIndexLabel;
	[Export]
	public RichTextLabel TrackNameLabel;
	[Export]
	public Label TrackLengthLabel;
	[Export]
	public RichTextLabel TrackSizeLabel;
	[Export]
	public Label TrackTypeLabel;

	static string ToLenghtBBCode(string originalText, string filler, uint Length)
	{
		string formatedText = "[color=dim_gray]";
		for (int i = 0; i < Length - originalText.Length; i++)
			formatedText += filler;
		return formatedText + "[/color]" + originalText;
	}
	static string ToLenght(string originalText, string filler, uint Length)
	{
		string formatedText = "";
		for (int i = 0; i < Length - originalText.Length; i++)
			formatedText += filler;
		return formatedText + originalText;
	}
	public static string FormatBytes(ulong bytes)
	{
		string[] sizes = ["B", "KB", "MB", "GB", "TB"];
		double len = bytes;
		short order = 0;

		while (len >= 1024 && order < sizes.Length - 1)
		{
			order++;
			len /= 1024;
		}

		return $"{len:0.##} [color=dim_gray]{sizes[order]}[/color]";
	}
}
