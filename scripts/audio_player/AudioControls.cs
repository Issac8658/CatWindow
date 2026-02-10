using Godot;
using FFmpeg;
using System.IO;

public partial class AudioControls : Control
{
	private bool _holding = false;

	[Export]
	public FFmpegPlayer Player;
	[Export]
	public Button StopButton;
	[Export]
	public Slider SeekSlider;
	[Export]
	public Label TimeLabel;
	[Export]
	public RichTextLabel NameLabel;

	public override void _Ready()
	{
		StopButton.Pressed += Player.Stop;
		SeekSlider.DragStarted += () => _holding = true;
		SeekSlider.DragEnded += ValueChanged =>
		{
			Player.Seek(SeekSlider.Value);
			_holding = false;
		};
		Player.Played += () =>
		{
			if (Player.Metadata != null)
			{
				if (Player.Metadata.Format.StartTime != null)
					SeekSlider.MinValue = float.Parse(Player.Metadata.Format.StartTime.Replace('.', ','));
				else
					SeekSlider.MinValue = 0;
				SeekSlider.MaxValue = float.Parse(Player.Metadata.Format.Duration.Replace('.', ','));
	
				if (Player.Metadata.Format.Tags != null) // чем гуще лес if else if else...
					if (Player.Metadata.Format.Tags.Title != null)
						if (Player.Metadata.Format.Tags.Artist != null)
							NameLabel.Text = $"{Player.Metadata.Format.Tags.Title}[color=dim_gray] — {Player.Metadata.Format.Tags.Artist}[/color]";
						else
							NameLabel.Text = Player.Metadata.Format.Tags.Title;
					else
						NameLabel.Text = Path.GetFileNameWithoutExtension(Player.CurrentFile);
			}
		};
		Player.Stopped += () => NameLabel.Text = "";
	}
	public override void _Process(double Delta)
	{
		double playbackPos = Player.PlaybackPosition;
		if (!_holding)
			SeekSlider.Value = playbackPos;
		if (Player.Metadata != null)
			TimeLabel.Text = $"{FormatTime((int)playbackPos)} / {FormatTime((int)float.Parse(Player.Metadata.Format.Duration.Replace('.', ',')))}";
		else
			TimeLabel.Text = "0:00 / 0:00";
	}
	static string ToLenght(string originalText, string filler, uint Length)
	{
		string formatedText = "";
		for (int i = 0; i < Length - originalText.Length; i++)
			formatedText += filler;
		return formatedText + originalText;
	}

	static string FormatTime(int Time)
	{
		int remainder = Time;
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

		return formatedText;
	}
}
