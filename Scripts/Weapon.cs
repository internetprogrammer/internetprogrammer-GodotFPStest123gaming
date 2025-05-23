using Godot;
using System;
using static Godot.TextServer;


public partial class Weapon : Node3D
{
	//logic
	[Export]public int Damage = 15;
    [Export] public int Penetration = 0;
	public enum WeaponType{
		HitScan,Projectile
	}
	[Export]WeaponType Type = WeaponType.HitScan;

    [Export] public int MaxAmmo = 15;
    [Export] public  int Ammo = 15;
	//init stuffs
	[Export]public Node3D BulletHole;
	[Export] public CollisionShape3D Collider;
	[Export] public RayCast3D rayCast3DAim;
	public RigidBody3D RigidBody;
	//recoil
	[Export] public Vector3 RecoilAmmount = new Vector3(.2f, .2f, .2f);
	[Export] public float LerpSpeed = 10.0f;
	[Export] public float LerpSpeedRecovery = 20.0f;
	//handle fire rate and fire mode
	[Export] public double RoundsPerMinute = 450;
	public enum FireMode { BoltAction, SemiAuto, FullAuto }
	[Export]public FireMode WeaponFireMode = FireMode.SemiAuto;
	public bool CanShoot = true;
    [Export] Timer DelayTimer;

    Vector3 RecoilTargetRotation;
	Vector3 RecoilTargetPosition;
	public Vector3 WeaponOffset = new Vector3(0.428f, -0.1f, -0.50f);

	[Export] AudioStreamPlayer3D AudioStreamPlayer;
    [Export] float AudioPitch = 1f;
    [Export] float AudioPitchStep = 0.02f;
	//projectile
	[Export] PackedScene bullet;
    [Export] PackedScene Casing;
    [Export] public float BulletVelocity;
	[Export] Node3D CassingSpawner;
	[Export] float EjectionVelocity = 20f;
	int init = 0;//funny _ready doesnt work so we do this
	


    public void Shoot(RayCast3D rayCast3DHeadAim,Node world, Player player)
	{
		AudioStreamPlayer.PitchScale = AudioPitch - ((MaxAmmo - Ammo) * AudioPitchStep);
        if (Ammo > 0 && CanShoot)
		{
            AudioStreamPlayer.Play();
            if (Type == WeaponType.HitScan) {
                ShootHitScan(rayCast3DHeadAim);

			}
			else if (Type ==WeaponType.Projectile)
			{
				ShootProjectile(rayCast3DHeadAim,world);
			}
			if(Casing != null) { DropCasing(world, player); };
		}
        else
        {
            // funny click sound
        }

    }

    private void DropCasing(Node world, Player player)
    {
        RigidBody3D c = (RigidBody3D)Casing.Instantiate<Node3D>();
        world.AddChild(c);
        c.GlobalTransform = CassingSpawner.GlobalTransform;
		c.LinearVelocity = CassingSpawner.GlobalTransform.Basis.Z *20* EjectionVelocity - (CassingSpawner.GlobalTransform.Basis.X * 3 * EjectionVelocity) + (player.Velocity / 2);



    }

    void ShootProjectile(RayCast3D rayCast3DHeadAim, Node world)
	{
        DelayTimer.Start(60.0 / RoundsPerMinute); //(60.0f)/RoundsPerMinute
        CanShoot = false;
        Ammo--;
        Recoil();
        RigidBody3D b = (RigidBody3D)bullet.Instantiate<Node3D>();
		Bullet BulletInit = (Bullet)b;
		BulletInit.Damage = Damage;
        BulletInit.Penetration = Penetration;
        BulletInit.ContactMonitor = true;
        BulletInit.ContinuousCd = true;
		BulletInit.MaxContactsReported = 100;
        world.AddChild(b);
        b.GlobalTransform = BulletHole.GlobalTransform;

        b.LinearVelocity = BulletHole.GlobalTransform.Basis.X * BulletVelocity*(5);

    }
	void ShootHitScan(RayCast3D rayCast3DHeadAim)
	{
        DelayTimer.Start(60.0 / RoundsPerMinute); //(60.0f)/RoundsPerMinute
        CanShoot = false;
        Ammo--;
        Recoil();
        Node3D tempNode = (Node3D)rayCast3DHeadAim.GetCollider();
        switch (GetColliderDamagable(rayCast3DHeadAim))
        {
            case 0:
                break;
            case 1:
                Enemy Target = (Enemy)tempNode;
                Target.DamageHandler.DamageTarget(Damage, Penetration, tempNode);
                //Target.Velocity.X = Mathf.Lerp((float)velocity.X, (float)direction.X * Speed, (float)delta * 14.0f);
                //Target.Velocity.Z = Mathf.Lerp((float)velocity.Z, (float)direction.Z * Speed, (float)delta * 14.0f);
                Target.Velocity = -GlobalTransform.Basis.X * (Damage * (Penetration + 1) * 5);
                break;
            case 2:
                RigidBody3D Target2 = (RigidBody3D)tempNode;
                //Target.Velocity.X = Mathf.Lerp((float)velocity.X, (float)direction.X * Speed, (float)delta * 14.0f);
                //Target.Velocity.Z = Mathf.Lerp((float)velocity.Z, (float)direction.Z * Speed, (float)delta * 14.0f);
                Target2.LinearVelocity += -GlobalTransform.Basis.X * (Damage * (Penetration + 1) * 5);
                break;
            case 3:
                Damagable Target3 = (Damagable)tempNode;
                Target3.DamageTarget(Damage, Penetration);
                break;
            case 4:
                Damagable Target4 = (Damagable)tempNode;
                RigidBody3D Target5 = (RigidBody3D)tempNode;
                Target4.DamageTarget(Damage, Penetration);
                Target5.LinearVelocity += -GlobalTransform.Basis.X * (Damage * (Penetration + 1) * 5);
                break;
            case 5:
                Door Target6 = (Door)tempNode;
                if (Target6.DamageHandler == null) { break; }
                Target6.DamageHandler.DamageTarget(Damage, Penetration, tempNode);
                break;
            case 6:
                Civilian Target7 = (Civilian)tempNode;
                Target7.DamageHandler.DamageTarget(Damage, Penetration, tempNode);
				Target7.Panic();
                Target7.Velocity = -GlobalTransform.Basis.X * (Damage * (Penetration + 1) * 5);
                break;
        }
    }
    private void OnDelayTimerTimeoutSignal()
    {
        CanShoot = true;
    }
    public int Reload(int PlayerAmmo)
	{
		if(PlayerAmmo <= MaxAmmo - Ammo)
		{
			Ammo += PlayerAmmo;
			PlayerAmmo = 0;// accidentally did this and it should work
		}
		else
		{
			Ammo = MaxAmmo;
			PlayerAmmo -= MaxAmmo - Ammo;
		}
		return PlayerAmmo;
	}
	public void DisablePhysics()
	{
		// need to get rigidbody without this throwing a fit
		RigidBody.Freeze = true;
		Collider.Disabled = true;
	}
	public void EnablePhysics()
	{
		RigidBody.Freeze = false;
		Collider.Disabled = false;
	}
	public void Jam()
	{

	}
	public void Recoil()
	{
		RecoilTargetPosition.Z = RecoilAmmount.Y;
		//RecoilTargetRotation.X = RecoilAmmount.X;
		RecoilTargetRotation.Z = RecoilAmmount.Z;
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
	{
		if(init == 0)
		{
            DelayTimer.Timeout += OnDelayTimerTimeoutSignal;
            init++;
		}
		if (RigidBody == null)
		{
			RigidBody = GetParent() as RigidBody3D;
		}
			RecoilTargetPosition = new Vector3(0, 0, Mathf.Lerp(RecoilTargetPosition.Z, 0, LerpSpeed * (float)delta));
			RecoilTargetRotation = new Vector3(Mathf.Lerp(RecoilTargetRotation.X, 0, 10 * (float)delta), 0, Mathf.Lerp(RecoilTargetRotation.Z, 0, LerpSpeed * (float)delta));
			Node3D Weapon = (Node3D)this;
			Weapon.Position = new Vector3(Weapon.Position.X, Weapon.Position.Y, Mathf.Lerp(Weapon.Position.Z ,RecoilTargetPosition.Z, 20 * (float)delta));
			Weapon.Rotation = new Vector3(Mathf.Lerp(Weapon.Rotation.X,RecoilTargetRotation.X, LerpSpeedRecovery * (float)delta), Weapon.Rotation.Y, Mathf.Lerp(Weapon.Rotation.Z,-RecoilTargetRotation.Z, LerpSpeedRecovery * (float)delta));


	}
	private static int GetColliderDamagable(RayCast3D rayCast3DHeadAim)
	{
		if (rayCast3DHeadAim.IsColliding())
		{
			if(rayCast3DHeadAim.GetCollider() is Enemy){
				return 1;
			}
            else if (rayCast3DHeadAim.GetCollider() is Damagable && rayCast3DHeadAim.GetCollider() is RigidBody3D)
            {
                return 4;
            }
            else if (rayCast3DHeadAim.GetCollider() is Damagable)
            {
                return 3;
            }
            else if(rayCast3DHeadAim.GetCollider() is RigidBody3D)
			{
                return 2;
            } //rayCast3DHeadAim.GetCollider()
            else if (rayCast3DHeadAim.GetCollider() is Door)
            {
                return 5;
            }
            else if (rayCast3DHeadAim.GetCollider() is Civilian)
            {
                return 6;
            }
        }
		return 0;
	}

    /*
	private GodotObject GetColliderDamagable(RayCast3D rayCast3DHeadAim)
	{
		Vector3 target;
		
		if (rayCast3DHeadAim.IsColliding())//raycast to check if grounded
		{
			target = rayCast3DHeadAim.GetCollisionPoint();
		}
		else{
			target = Vector3.Zero;
		}
		GD.Print(target);
		//this whole thing returns null for some reason 
		rayCast3DAim.TargetPosition = target;
		if (rayCast3DAim.IsColliding())//raycast to check if grounded
		{
			return rayCast3DAim.GetCollider();
		}
		GD.Print("you fucked up");
		return null;
	}*/
}
 // please find a way to remove that shitty error about not being able to cast x to x its getting really annoying
