using Godot;
using System;

public partial class Sight : Area3D
{
	public float Range = 25f;
	//[Export] Timer timerDelay;
	[Export] public float SightAngle = 150;// the degrees of where the player can le see, +- too saar
		
	public override void _Ready()
	{
		CollisionShape3D CollisionShape = (GetChild(0) as CollisionShape3D);
		CollisionShape.Scale = Vector3.One * Range;
		Monitoring = true;
		Monitorable = true;
	}
		public Node3D Check(RayCast3D rayCast, Node3D attacker)
	    {
		var OverlapingBodies = GetOverlappingBodies();
		foreach (var body in OverlapingBodies)
		{
			if (body is Player)
			{
				if (HasSightLine(body,attacker, rayCast)) { 
					return body;
				}
			}
		}
        return null;
    }
    public bool HasSightLine(Node3D target, Node3D attacker, RayCast3D rayCast)
    {
        Vector3 to = target.GlobalTransform.Origin;
        Vector3 from = attacker.GlobalTransform.Origin;

        // Flatten Y for horizontal check only
        to.Y = from.Y;

        Vector3 direction = (to - from).Normalized();

        // Get the angle from attacker to the target
        float angleToTarget = Mathf.Atan2(direction.X, direction.Z); // Godot 3D: Z is forward

        float attackerYaw = attacker.GlobalRotation.Y;
        float angleDiff = Mathf.Wrap(Mathf.RadToDeg(Mathf.AngleDifference(angleToTarget, attackerYaw)), -180, 180);

        float distanceLeast = MathF.Abs(180 - angleDiff);
        float distanceMost = MathF.Abs(angleDiff + 180);
 
        GD.Print("distance least: ", distanceLeast);
        GD.Print("distance most: ", distanceMost);


        // Debug info
        GD.Print("Attacker Yaw: ", Mathf.RadToDeg(attackerYaw));
        GD.Print("Angle to Target: ", Mathf.RadToDeg(angleToTarget));
        GD.Print("Angle Diff: ", angleDiff);
        GD.Print("Sight Range:", SightAngle);

        if (distanceLeast <= SightAngle/2 || distanceMost <= SightAngle/2)
        {
            // Raycast setup
            Vector3 localTargetPos = rayCast.ToLocal(target.GlobalTransform.Origin);
            rayCast.TargetPosition = localTargetPos;

            if (rayCast.GetCollider() is Player)
            {
                //rayCast.Rotation = Vector3.Zero;
                return true;
            }
        }

        //rayCast.Rotation = Vector3.Zero;
        return false;
    }
}

