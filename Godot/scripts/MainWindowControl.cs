using System.Threading.Tasks;
using Godot;

public partial class MainWindowControl : Node
{
	[Signal] public delegate void DraggingChangedEventHandler();

	[Export]
	public Control TopBar;
	[Export]
	public Button CloseButton;
	private AudioEffectSpectrumAnalyzerInstance Spectrum = AudioServer.GetBusEffectInstance(2, 1) as AudioEffectSpectrumAnalyzerInstance;

	public bool IsDragging
	{
		get => _isDragging;
		set {}
	}

	private Window _mainWindow;

	private bool _isDragging = false;
	private Vector2I MouseOffset = new();
	private Vector2I WindowPosition;
	private Vector2 CatScale = new();

	public Vector2I WindowSize = new(250, 250);

	public override void _Ready()
	{
		_mainWindow = GetWindow();
		WindowPosition = _mainWindow.Position;

		TopBar.GuiInput += Event =>
		{
			if (Event is InputEventMouseButton)
				if ((Event as InputEventMouseButton).ButtonIndex == MouseButton.Left)
				{
					if (Event.IsPressed())
					{
						MouseOffset = GlobalCursorController.MousePosition - _mainWindow.Position;
						_isDragging = true;
					}
					else
						_isDragging = false;
					
					EmitSignal(SignalName.DraggingChanged);
				}
		};

		CloseButton.Pressed += () => GetTree().Quit();
	}

	public override void _Process(double delta)
	{
		if (_isDragging)
		{
			WindowPosition = DisplayServer.MouseGetPosition() - MouseOffset;
		}
		CatScale = CatScale.Lerp(new Vector2(Spectrum.GetMagnitudeForFrequencyRange(60, 500).X, Spectrum.GetMagnitudeForFrequencyRange(500, 60000).X) * 300, 1f);
		_mainWindow.Position = WindowPosition - (Vector2I)CatScale;
		_mainWindow.Size = WindowSize + (Vector2I)CatScale * 2;
	}
}
