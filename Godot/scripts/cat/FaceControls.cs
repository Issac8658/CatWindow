using Godot;
using System;

public partial class FaceControls : Control
{
	public enum FaceStateEnum
	{
		Loading,
		Default,
		ChangingVolume
	}
	public enum EyesStateEnum
	{
		Open,
		Closed,
		Closed2
	}

	private Vector2 _defaultFaceAnchorPos;
	private Vector2 _defaultFaceAnchorGlobalPos;
	private Control _faceAnchorParent;

	[Export]
	public Control FaceAnchor;
	[Export]
	public Control OpenedEyes;
	[Export]
	public Control ClosedEyes;
	[Export]
	public Control ClosedEyes2;
	[Export]
	public CanvasItem NormalMouth;
	[Export]
	public CanvasItem SadMouth;
	[Export]
	public MainWindowControl ControlNode;
	[Export]
	public Control FaceContainer;
	[Export]
	public AudioStreamPlayer PurrSound;
	[Export]
	public AnimationPlayer MouthAnimatior;
	[Export]
	public Button CloseButton;
 
	private EyesStateEnum _eyesState = EyesStateEnum.Open;
	private EyesStateEnum EyesState
	{
		get => _eyesState;
		set
		{
			OpenedEyes.Visible = false;
			ClosedEyes.Visible = false;
			ClosedEyes2.Visible = false;
			switch (value)
			{
				case EyesStateEnum.Open:
					OpenedEyes.Visible = true;
					break;
				case EyesStateEnum.Closed:
					ClosedEyes.Visible = true;
					break;
				case EyesStateEnum.Closed2:
					ClosedEyes2.Visible = true;
					break;
			}
			_eyesState = value;
		}
	}
	private FaceStateEnum _faceState = FaceStateEnum.Loading;
	private FaceStateEnum FaceState
	{
		get => _faceState;
		set
		{
			OpenedEyes.Visible = false;
			ClosedEyes.Visible = false;
			ClosedEyes2.Visible = false;
			switch (value)
			{
				case FaceStateEnum.Loading:
					OpenedEyes.Visible = true;
					break;
				case FaceStateEnum.Default:
					ClosedEyes.Visible = true;
					break;
				case FaceStateEnum.ChangingVolume:
					ClosedEyes2.Visible = true;
					break;
			}
			_faceState = value;
		}
	}

	private bool _purring = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_faceAnchorParent = (Control)FaceAnchor.GetParent();

		ControlNode.DraggingChanged += () =>
		{
			if (ControlNode.IsDragging)
				EyesState = EyesStateEnum.Closed2;
			else
				EyesState = EyesStateEnum.Open;
		};

		CloseButton.MouseEntered += () =>
		{
			SadMouth.Visible = true;
			NormalMouth.Visible = false;
		};
		CloseButton.MouseExited += () =>
		{
			SadMouth.Visible = false;
			NormalMouth.Visible = true;
		};

		Blincking();
	}

	public override void _Process(double delta)
	{
		_defaultFaceAnchorPos = _faceAnchorParent.Size / 2;
		_defaultFaceAnchorGlobalPos = _faceAnchorParent.GlobalPosition + _faceAnchorParent.Size / 2;

		Vector2 pos;
		if (!ControlNode.IsDragging && !_purring)
		{
			pos = GlobalCursorController.MousePosition - GetWindow().Position - _defaultFaceAnchorGlobalPos;
			pos = pos.Normalized() * Mathf.Min(pos.Length() / 2f, Mathf.Min(Size.X / 2.0f - 24f, Size.Y / 2.0f - 24f));
		}
		else
		{
			pos = Vector2.Zero;
		}

		FaceAnchor.Position = FaceAnchor.Position.Lerp(pos + _defaultFaceAnchorPos, (float)Mathf.Exp(-10 * delta));
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
			if ((@event as InputEventMouseButton).ButtonIndex == MouseButton.Right)
			{
				if (@event.IsPressed())
				{
					PurrSound.Play();
					EyesState = EyesStateEnum.Closed;
				}
				else
				{
					PurrSound.Stop();
					EyesState = EyesStateEnum.Open;
				}
				_purring = @event.IsPressed();
			}
	}

	private void Blincking()
	{
		if (!ControlNode.IsDragging && !_purring)
		{
			EyesState = EyesStateEnum.Closed;
		}
		GetTree().CreateTimer(0.1f).Timeout += () =>
		{
			if (!ControlNode.IsDragging && !_purring)
			{
				EyesState = EyesStateEnum.Open;
			}
			GetTree().CreateTimer(GD.RandRange(0.1f, 5.0f)).Timeout += () =>
			{
				Blincking();
			};
		};
	}
}
