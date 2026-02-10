using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public partial class Paw : Node
{
	[Export]
	public Node2D[] PawNodes = []; // only for building Line2D, insert nodes from end
	[Export]
	public Line2D PawLine;
	[Export]
	public RigidBody2D NodeToPush;
	[Export]
	public Control PawDragControl;
	[Export]
	public Joint2D PawDragJoint;
	//[Export]
	//public Joint2D PawJoint;
	[Export]
	public Node2D PawRoot;
	[Export]
	public float MaxPushMagnitude = 10f;
	[Export]
	public float PushPower = 2;

	private bool _isDragging = false;

	private Vector2 _oldWindowPos;

	private Window _mainWindow;
	//private NodePath _oldNodeA;

	public override void _Ready()
	{
		//_oldNodeA = PawJoint.NodeA;
		_mainWindow = GetWindow();
		_oldWindowPos = _mainWindow.Position;

		PawDragControl.GuiInput += @event =>
		{
			if(@event is InputEventMouseButton mouseEvent)
			{
				_isDragging = mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed;
			}
		};
	}

	public override void _Process(double delta)
	{
		if (_mainWindow.Position != _oldWindowPos)
		{
			Vector2 windowDelta = _mainWindow.Position - _oldWindowPos;
			Vector2 pushVector = windowDelta;
			//if (pushVector.Length() > MaxPushMagnitude)
			{
			//	pushVector = pushVector.Normalized() * MaxPushMagnitude;
			}
			NodeToPush.ApplyCentralImpulse(-pushVector * PushPower);
		}//
		if (_isDragging)
		{
			PawDragJoint.Position = PawDragControl.GetLocalMousePosition();
			if ((PawDragJoint.GlobalPosition - PawRoot.GlobalPosition).Length() > 34f)
			{
				PawDragJoint.GlobalPosition = PawRoot.GlobalPosition + (PawDragJoint.GlobalPosition - PawRoot.GlobalPosition).Normalized() * 34f;
			}
			PawDragJoint.NodeA = PawDragJoint.GetChildren()[0].GetPath();
			//PawJoint.NodeA = null;
		}
		else
		{
			PawDragJoint.NodeA = null;
			//PawJoint.NodeA = _oldNodeA;
		}

		Vector2[] points = PawLine.Points;
		for (int i = 0; i < PawNodes.Length; i++)
		{
			points[i] = PawNodes[i].Position;
		}
		PawLine.Points = points;

		_oldWindowPos = _mainWindow.Position;
	}
}
