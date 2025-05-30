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
	Vector3 LastKnownLocation = Vector3.Zero;
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
	public void HandleSight()
	{
            target = Sight.Check(aimCast, this);

            if (target != null)
            {
                LastKnownLocation = target.GlobalPosition;
                LookAt(target.GlobalPosition);
                Stop();
                ShootAt(aimCast);
            }
			else if(LastKnownLocation != Vector3.Zero)
		{
			Navigate(LastKnownLocation);
		}
        
    }
    public override void _Process(double delta)
    {
		//GD.Print(IsRunning);
		if(weaponHandler.weapon.Ammo == 0)
		{
			weaponHandler.weapon.Reload(100);
        }
		HandleSight();

    }
 
}
