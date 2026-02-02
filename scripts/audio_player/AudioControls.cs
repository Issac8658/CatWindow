using Godot;
using FFmpeg;

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

	public override void _Ready()
	{
		StopButton.Pressed += Player.Stop;
		SeekSlider.DragStarted += () =>
		{
			_holding = true;
		};
		SeekSlider.DragEnded += ValueChanged =>
		{
			Player.Seek(SeekSlider.Value);
			_holding = false;
		};
		Player.Played += () =>
		{
			SeekSlider.MinValue = float.Parse(Player.Metadata.Format.StartTime.Replace('.', ','));
			SeekSlider.MaxValue = float.Parse(Player.Metadata.Format.Duration.Replace('.', ','));
		};
	}
	public override void _Process(double Delta)
	{
		if (!_holding)
			SeekSlider.Value = Player.PlaybackPosition;
	}
}
