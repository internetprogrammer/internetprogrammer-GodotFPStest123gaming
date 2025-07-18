using Godot;
using System;

public partial class explosion : Area3D
{
	public float Range = 5f;
	public int Damage = 50;
	public float Force = 3;
	public int Penetration = 0;
	[Export] Timer timerDelay;
	public Node3D ParentNode;
	[Export] public float DamageFalloff = 0.5f;
	


	public override void _Ready()
	{
		CollisionShape3D CollisionShape = (GetChild(0) as CollisionShape3D);
		CollisionShape.Scale = Vector3.One * Range;
		Monitoring = true;
		Monitorable = true;
		timerDelay.Timeout += setExplode;
		timerDelay.Start(0.1f);
	}
	private void setExplode()
	{
		Explode();
	}

	private void Explode()
	{
		var OverlapingBodies = GetOverlappingBodies();
		foreach (var body in OverlapingBodies)
		{
			float distanceMult = GlobalPosition.DistanceTo(body.GlobalPosition);
			distanceMult /= (Range * DamageFalloff);
			if ((int)distanceMult == 0) distanceMult = 1;
			if (body is Enemy)
			{
				Enemy target = (Enemy)body;
				target.DamageHandler.DamageTarget(Damage / (int)distanceMult, Penetration, target);
			}
			else if (body is Civilian)
			{
				Civilian target = (Civilian)body;
				target.DamageHandler.DamageTarget(Damage / (int)distanceMult, Penetration, target);
			}
			else if (body is Player)
			{
				Player target = (Player)body;
				target.DamageHandler.DamageTarget(Damage / (int)distanceMult, Penetration, target);
			}
			else if (body is Damagable)
			{
				Damagable target = (Damagable)body;
				target.DamageTarget(Damage / (int)distanceMult, Penetration);
			}
			else if (body is RigidBody3D)
			{
				Vector3 direction = (this.GlobalPosition - body.GlobalPosition ).Normalized();
				((RigidBody3D)body).LinearVelocity = -direction * Force / distanceMult;
			}

		}
		if (ParentNode != null)
		{
			ParentNode.QueueFree();
		}
		QueueFree();

	}
}
