using Godot;
using System;

public partial class Enemy : NPC	
{
	[Export] public int Skill = 25;//min 0 max 100
	public int ActualSkill;// skill after calculating skill and awareness
	[Export] public int GuardAwarenessOffset;
	[Export] public int PatrolAwarenessOffset;
	[Export] public int RetreatSkillPenalty;
	[Export] public int AssaultingAwarenessOffset;
	[Export] public Vector3 PatrolOffset = new Vector3(10,0,10);
	[Export] public Node3D PatrolPoint;
	[Export] public Sight Sight;
	Node3D target;
	Vector3 LastKnownLocation;




    public void Guard()
	{

	}
	public void Retreat(Node3D target) // run away from the target
	{

	}
	public void Patrol() // patrol a certain area away from where you spawned
	{

	}
	public void Assaulting(Node3D target)// move closer to the target and shoot
	{

	}
	public void SoundHeard(Vector3 Target)
	{
		if(PanicGenerator.RandiRange(0, 100) > (Skill * PanicMultiplier))
		{
			
			Panic();
		}
		CheckSound( Target);
	}
	public void CheckSound(Vector3 Target){
		
		Navigate(Target);
	}
	public void ShootAt(Node3D target){
		
	}
    public override void _Process(double delta)
    {
		if (target != null)
		{
            if (Sight.CheckStillSightLine(aimCast, target) == null)
			{
				CheckSound(LastKnownLocation);
				target = null;
               // LookAt(LastKnownLocation);
                return;
			}
			target = Sight.CheckStillSightLine(aimCast, target);
			LastKnownLocation = target.GlobalPosition;
            LookAt(target.GlobalPosition);
			Stop();
        }
		else
		{
            target = Sight.Check(aimCast);

            if (target != null)
            {
                LastKnownLocation = target.GlobalPosition;
                LookAt(target.GlobalPosition);
				Stop();
            }
        }

    }
 
}
