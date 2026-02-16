using Godot;
using FFmpeg;
using System.IO;
using System.Globalization;

public partial class AudioControls : Control
{
	private Window _window;

	private bool _holding = false;

	[Export]
	public FFmpegPlayer Player;
	[Export]
	public Button StopButton;
	[Export]
	public Button PlayButton;
	[Export]
	public Button PauseButton;
	[Export]
	public Slider SeekSlider;
	[Export]
	public SpinBox PlaybackSpeedSpinBox;
	[Export]
	public Label TimeLabel;
	[Export]
	public RichTextLabel NameLabel;

	public override void _Ready()
	{
		_window = GetWindow();

		StopButton.Pressed += Player.Stop;
		PlayButton.Pressed += Player.UnPause;
		PauseButton.Pressed += Player.Pause;

		PlaybackSpeedSpinBox.ValueChanged += value =>
		{
			Player.PlaybackSpeed = value;
		};

		SeekSlider.DragStarted += () => _holding = true;
		SeekSlider.DragEnded += ValueChanged =>
		{
			Player.Seek(SeekSlider.Value);
			_holding = false;
		};
		Player.Played += () => // чем гуще лес if else if else...
		{
			if (Player.Metadata != null)
			{
				if (Player.Metadata.Format.StartTime != null)
					SeekSlider.MinValue = float.Parse(Player.Metadata.Format.StartTime, CultureInfo.InvariantCulture);
				else
					SeekSlider.MinValue = 0;
				SeekSlider.MaxValue = float.Parse(Player.Metadata.Format.Duration, CultureInfo.InvariantCulture);
	
				if (Player.Metadata.Format.Tags != null)
					if (Player.Metadata.Format.Tags.Title != null)
						if (Player.Metadata.Format.Tags.Artist != null)
							NameLabel.Text = $"{Player.Metadata.Format.Tags.Title}[color=dim_gray] — {Player.Metadata.Format.Tags.Artist}[/color]";
						else
							NameLabel.Text = Player.Metadata.Format.Tags.Title;
					else
						NameLabel.Text = Path.GetFileNameWithoutExtension(Player.CurrentFile);
			}
			else
				NameLabel.Text = Path.GetFileNameWithoutExtension(Player.CurrentFile);
			UpdateButtons();
		};
		Player.Stopped += () => 
		{
			NameLabel.Text = "";
			UpdateButtons();
		};
		Player.Paused += UpdateButtons;
	}
	public override void _Process(double Delta)
	{
		if (_window.Visible)
		{
			double playbackPos = Player.PlaybackPosition;
			if (!_holding)
				SeekSlider.Value = playbackPos;
			if (Player.Metadata != null)
				TimeLabel.Text = $"{FormatTime((int)playbackPos)} / {FormatTime((int)float.Parse(Player.Metadata.Format.Duration, CultureInfo.InvariantCulture))}";
			else
				TimeLabel.Text = "0:00 / 0:00";
		}
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

	private void UpdateButtons()
	{
		PlayButton.Visible = Player.IsPaused || !Player.Playing;
		PauseButton.Visible = !Player.IsPaused && Player.Playing;
	}
}
