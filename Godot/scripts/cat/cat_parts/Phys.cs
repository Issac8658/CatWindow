using Godot;

public partial class Phys : Node2D
{
	[Export]
	public float Mass = 1f;
	[Export]
	public float Length = 100;
	[Export(PropertyHint.Range, "0,1,or_greater,or_less")]
	public float GravityScale = 1;
	[Export(PropertyHint.Range, "0,1,or_greater,or_less")]
	public float WindowForceMultiplier = 1;
	[Export(PropertyHint.Range, "0,1,or_greater,or_less")]
	public float Damping = 0.5f;

	private Vector2 OldPos;
	private float AngularVelocity = 0;

	public override void _Ready()
	{
		OldPos = GlobalPosition + GetWindow().Position;
	}


	public override void _Process(double delta)
	{
		float dt = (float)delta;

		Vector2 PhysPos = new(Length * Mathf.Sin(Rotation), Length * Mathf.Cos(Rotation));

		Vector2 NewPos = GlobalPosition + GetWindow().Position;
		Vector2 Force = -(NewPos - OldPos) * WindowForceMultiplier;
		OldPos = NewPos;

		float Torque = PhysPos.X * Force.Y - PhysPos.Y * Force.X;
		
		float AngularAcceleration = -(ProjectSettings.GetSetting("physics/2d/default_gravity", 0f).AsSingle() * GravityScale / Length)* Mathf.Sin(Rotation) + Torque / (Mass * Length * Length);

		AngularVelocity += AngularAcceleration * dt;
		AngularVelocity -= AngularVelocity * Damping * dt;
		Rotation += AngularVelocity * dt;
	}
}
