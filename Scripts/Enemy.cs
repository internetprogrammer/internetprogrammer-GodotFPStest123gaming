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
	[Export] public WeaponHandler weaponHandler;

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
	public void ShootAt(RayCast3D aimCast){
		weaponHandler.weapon.Shoot(aimCast, this);
	}
    public override void _Process(double delta)
    {
		if(weaponHandler.weapon.Ammo == 0)
		{
			weaponHandler.weapon.Reload(100);

        }
		if (target != null)
		{
            IsRunning = true;
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
			ShootAt(aimCast);
        }
		else
		{
			IsRunning = false;
            target = Sight.Check(aimCast);

            if (target != null)
            {
				IsRunning = true;
                LastKnownLocation = target.GlobalPosition;
                LookAt(target.GlobalPosition);
				Stop();
            }
        }

    }
 
}
