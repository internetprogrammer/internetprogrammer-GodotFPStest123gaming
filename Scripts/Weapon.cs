using Godot;
using System;
using static Godot.TextServer;


public partial class Weapon : Node3D
{
	//logic
	[Export]public int Damage = 15;
    [Export] public int Penetration = 0;
    [Export] float Knockback = 5f;
    public enum WeaponType{
		HitScan,Projectile
	}
	[Export]WeaponType Type = WeaponType.HitScan;

    [Export] public int MaxAmmo = 15;
    [Export] public int Ammo = 15;
	//init stuffs
	[Export]public Node3D BulletHole;
	[Export] public CollisionShape3D Collider;
	[Export] public RayCast3D RayCast3DAim;
	public RigidBody3D RigidBody;
	//recoil
	[Export] public Vector3 RecoilAmmount = new(.2f, .2f, .2f);
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
	[Export]public Vector3 WeaponOffset = new(0.428f, -0.1f, -0.50f);

	[Export] AudioStreamPlayer3D AudioStreamPlayer;
    [Export] float AudioPitch = 1f;
    [Export] float AudioPitchStep = 0.02f;
	//projectile
	[Export] PackedScene bullet;
    [Export] PackedScene Casing;
    [Export] PackedScene BulletCrater;
    [Export] public float BulletVelocity;
	[Export] Node3D CassingSpawner;
	[Export] float EjectionVelocity = 20f;
    [Export] GpuParticles3D Effect;
    [Export] public bool Lerp = false;
    int init = 0;//funny _ready doesnt work so we do this

    public object RayCast3DHeadAim { get; private set; }

    public void Shoot(RayCast3D rayCast3DHeadAim, CharacterBody3D player)
	{
		Node world = GetNode<Node3D>("/root/World");

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

    private void DropCasing(Node world, CharacterBody3D player)
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
        Node3D player = GetNode<Node3D>("/root/World/Player");
        Effect.LookAt(player.GlobalPosition);
        Effect.Emitting = true;
        Recoil();
        Node3D tempNode = (Node3D)rayCast3DHeadAim.GetCollider();
        if (tempNode is Enemy enemy)
        {

            enemy.DamageHandler.DamageTarget(Damage, Penetration, tempNode);
            //Target.Velocity.X = Mathf.Lerp((float)velocity.X, (float)direction.X * Speed, (float)delta * 14.0f);
            //Target.Velocity.Z = Mathf.Lerp((float)velocity.Z, (float)direction.Z * Speed, (float)delta * 14.0f);
            enemy.Velocity = -GlobalTransform.Basis.X * (Damage * (Penetration + 1) * Knockback);
        }
        else if (tempNode is RigidBody3D rigidbody)
        {
            //Target.Velocity.X = Mathf.Lerp((float)velocity.X, (float)direction.X * Speed, (float)delta * 14.0f);
            //Target.Velocity.Z = Mathf.Lerp((float)velocity.Z, (float)direction.Z * Speed, (float)delta * 14.0f);
            rigidbody.LinearVelocity += -GlobalTransform.Basis.X * (Damage * (Penetration + 1) * Knockback / rigidbody.Mass);
        }
        else if (tempNode is Damagable damagable)
        {
            damagable.DamageTarget(Damage, Penetration);
        }
        else if (tempNode is Damagable damag && tempNode is RigidBody3D rigid)
        {

            damag.DamageTarget(Damage, Penetration);
            rigid.LinearVelocity += -GlobalTransform.Basis.X * (Damage * (Penetration + 1) * Knockback);
        }
        else if (tempNode is Door door)
        {
            if (door.DamageHandler == null) { return; }
            door.DamageHandler.DamageTarget(Damage, Penetration, tempNode);
        }
        else if (tempNode is DoorMechanical doormech)
        {
            if (doormech.DamageHandler == null) { return; }
            doormech.DamageHandler.DamageTarget(Damage, Penetration, tempNode);
        }
        else if (tempNode is Civilian civilian)
        {
            civilian.DamageHandler.DamageTarget(Damage, Penetration, tempNode);
            civilian.SetPanic();
            civilian.Velocity = -GlobalTransform.Basis.X * (Damage * (Penetration + 1) * Knockback);
        }
        else if (tempNode is Node3D)
        {
            Node3D crater = (Node3D)BulletCrater.Instantiate();
            GetNode<Node3D>("/root/World").AddChild(crater);

            // Set position to the hit point
            crater.GlobalPosition = rayCast3DHeadAim.GetCollisionPoint();

            // Align crater to the surface normal
            Vector3 normal = rayCast3DHeadAim.GetCollisionNormal();
            crater.LookAt(crater.GlobalPosition + normal, Vector3.Up);

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
        if (Lerp)
        {
            RecoilTargetPosition.Z = RecoilAmmount.Y;
            //RecoilTargetRotation.X = RecoilAmmount.X;
            RecoilTargetRotation.Z = RecoilAmmount.Z;

        }
        else
        {
            
            RecoilTargetPosition.Z += RecoilAmmount.Y;
            //RecoilTargetRotation.X = RecoilAmmount.X;
            RecoilTargetRotation.Z += RecoilAmmount.Z;

            Mathf.Clamp(RecoilTargetPosition.Z, 0, RecoilAmmount.Y * 2);
            Mathf.Clamp(RecoilTargetRotation.Z, 0, RecoilAmmount.Z * 2);
        }
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
	{
        
		if(init == 0)
		{
            DelayTimer.Timeout += OnDelayTimerTimeoutSignal;
            init++;
        }
        if (RigidBody == null )
		{
			RigidBody = GetParent() as RigidBody3D;
		}
        if (Lerp)
        {
            RecoilTargetPosition = new Vector3(0, 0, Mathf.Lerp(RecoilTargetPosition.Z, 0, LerpSpeed * (float)delta));
            RecoilTargetRotation = new Vector3(Mathf.Lerp(RecoilTargetRotation.X, 0, 10 * (float)delta), 0, Mathf.Lerp(RecoilTargetRotation.Z, 0, LerpSpeed * (float)delta));
            Node3D Weapon = (Node3D)this;
            Weapon.Position = new Vector3(Weapon.Position.X, Weapon.Position.Y, Mathf.Lerp(Weapon.Position.Z, RecoilTargetPosition.Z, 20 * (float)delta));
            Weapon.Rotation = new Vector3(Mathf.Lerp(Weapon.Rotation.X, RecoilTargetRotation.X, LerpSpeedRecovery * (float)delta), Weapon.Rotation.Y, Mathf.Lerp(Weapon.Rotation.Z, -RecoilTargetRotation.Z, LerpSpeedRecovery * (float)delta));
        }
        else
        {
  
            RecoilTargetPosition = new Vector3(0, 0, Mathf.Lerp(RecoilTargetPosition.Z, 0, LerpSpeed * (float)delta));
            RecoilTargetRotation = new Vector3(Mathf.Lerp(RecoilTargetRotation.X, 0, 10 * (float)delta), 0, Mathf.Lerp(RecoilTargetRotation.Z, 0, LerpSpeed * (float)delta));
            Node3D Weapon = (Node3D)this;
            Weapon.Position = new Vector3(Weapon.Position.X, Weapon.Position.Y, Mathf.Lerp(Weapon.Position.Z, RecoilTargetPosition.Z, 20 * (float)delta));
            Weapon.Rotation = new Vector3(Mathf.Lerp(Weapon.Rotation.X, RecoilTargetRotation.X, LerpSpeedRecovery * (float)delta), Weapon.Rotation.Y, -Mathf.Lerp(Weapon.Rotation.Z, -RecoilTargetRotation.Z, LerpSpeedRecovery * (float)delta));
            GD.Print(Weapon.Rotation);
        }
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
