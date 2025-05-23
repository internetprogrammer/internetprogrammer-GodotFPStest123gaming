using Godot;
using System;

public partial class Sight : Area3D
{
	public float Range = 25f;
	//[Export] Timer timerDelay;
	[Export] public float SightAngle = Mathf.DegToRad(180);// the degrees of where the player can le see, +- too saar
		
	public override void _Ready()
	{
		CollisionShape3D CollisionShape = (GetChild(0) as CollisionShape3D);
		CollisionShape.Scale = Vector3.One * Range;
		Monitoring = true;
		Monitorable = true;
	}
		public Node3D Check(RayCast3D rayCast)
	{
		var OverlapingBodies = GetOverlappingBodies();
		foreach (var body in OverlapingBodies)
		{
			if (body is Player)
			{
				if (HasSightLine(body, this, rayCast)) { 
					return body;
				}
			}
		}
        return null;
    }
    public Node3D CheckStillSightLine(RayCast3D rayCast, Node3D target)
    {
        var OverlapingBodies = GetOverlappingBodies();
        foreach (var body in OverlapingBodies)
        {
            if (body is Player)
            {
                if (body == target)
                {
                    if (StillHasSightLine(body, this, rayCast))
                    {
                        return body;
                    }
                }
            }
        }
        return null;
    }
    public bool HasSightLine(Node3D target,Node3D attacker,RayCast3D rayCast){
		rayCast.LookAt(target.GlobalPosition);
			if (rayCast.GetCollider() is Player) {
				rayCast.Rotation = Vector3.Zero;
				return true;
			}
       rayCast.Rotation = Vector3.Zero;
        return false;
	}
    public bool StillHasSightLine(Node3D target, Node3D attacker, RayCast3D rayCast)
    {
        rayCast.LookAt(target.GlobalPosition);
        //GD.Print("1: ", attacker.GlobalRotation.Y + SightAngle, ",", rayCast.GlobalRotation.Y, "\n2:", attacker.GlobalRotation.Y - SightAngle, ",", rayCast.GlobalRotation.Y);

        if (attacker.GlobalRotation.Y + SightAngle > rayCast.GlobalRotation.Y && attacker.GlobalRotation.Y - SightAngle < rayCast.GlobalRotation.Y && rayCast.IsColliding())
        {
            if (rayCast.GetCollider() is Player)
            {
                return true;
            }
        }
        return false;
    }
}
