using System.Threading.Tasks;
using Godot;

public partial class MainWindowControl : Node
{
	[Signal] public delegate void DraggingChangedEventHandler();

	[Export]
	public Control TopBar;
	[Export]
	public Button CloseButton;

	public bool IsDragging
	{
		get => _isDragging;
		set {}
	}

	private Window _mainWindow;

	private bool _isDragging = false;
	private Vector2I MouseOffset = new();

	public override void _Ready()
	{
		_mainWindow = GetWindow();

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
			_mainWindow.Position = DisplayServer.MouseGetPosition() - MouseOffset;
		}
	}
}
