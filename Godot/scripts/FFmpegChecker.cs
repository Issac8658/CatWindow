using FFmpeg;
using Godot;

public partial class FFmpegChecker : Window
{
	[Export]
	public Button ContinueButton;
	[Export]
	public Button ExitButton;

	public override void _Ready()
	{
		if (!FFmpegPlayer.FFmpegIsExist)
		{
			Visible = true;
		}

		ContinueButton.Pressed += () => Visible = false;
		ExitButton.Pressed += () => { GetTree().Quit(); };
		CloseRequested += () => { GetTree().Quit(); };
	}
}
