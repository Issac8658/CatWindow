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

	private Vector2 CatScale = new();
	private Vector2 CatScaleAcceleration = new();
	private Vector2 CatScalePosition = new();
	private Vector2 CatScaleLerped = new();
	private Vector2 CatScaleDoubleLerped = new();

	public Vector2I WindowSize = new(250, 200);
	public Vector2I WindowPosition;

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
						MouseOffset = GlobalCursorController.MousePosition - WindowPosition;
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
		CatScale = new Vector2
		(
			Mathf.Pow(Spectrum.GetMagnitudeForFrequencyRange(60, 300, AudioEffectSpectrumAnalyzerInstance.MagnitudeMode.Max).X * 1.5f, 2),
			Mathf.Pow(Spectrum.GetMagnitudeForFrequencyRange(300, 60000, AudioEffectSpectrumAnalyzerInstance.MagnitudeMode.Max).X * 1.5f, 2) * 1.25f
		) * 1000;
		CatScaleAcceleration = (CatScale - CatScalePosition) * 0.9f;
		CatScalePosition += CatScaleAcceleration;
		CatScaleLerped = CatScaleLerped.Lerp(CatScalePosition, 0.3f).Max(CatScalePosition);
		CatScaleDoubleLerped = CatScaleDoubleLerped.Lerp(CatScaleLerped, 0.3f);

		int CurrentScreen = DisplayServer.WindowGetCurrentScreen(_mainWindow.GetWindowId());
		Vector2 DPosition = DisplayServer.ScreenGetPosition(CurrentScreen);
		Vector2 DSize = DisplayServer.ScreenGetSize(CurrentScreen) - WindowSize;
		Vector2 Offset = ((Vector2)WindowPosition - DPosition) / DSize;
		
		_mainWindow.Position = WindowPosition - (Vector2I)(CatScaleDoubleLerped * Offset);
		_mainWindow.Size = WindowSize + (Vector2I)CatScaleDoubleLerped;
	}
}
