using Godot;


public partial class Weaponrb : RigidBody3D
{
    //logic
    [ExportGroup("Stats")]
    [Export] public int Damage = 15;
    [Export] public int Penetration = 0;
    [Export] float Knockback = 5f;
    [Export] public int MaxAmmo = 15;
    [Export] public int Ammo = 15;
    [Export] public double RoundsPerMinute = 450;
    public enum FireMode { BoltAction, SemiAuto, FullAuto }
    [Export] public FireMode WeaponFireMode = FireMode.SemiAuto;
    [Export] WeaponType Type = WeaponType.HitScan;
    //init stuffs

    //recoil
    [ExportGroup("Recoil")]
    [Export] public Vector3 RecoilAmmount = new(.2f, .2f, .2f);
    [Export] public float LerpSpeed = 10.0f;
    [Export] public float LerpSpeedRecovery = 20.0f;
    //handle fire rate and fire mode

    [ExportGroup("Visual")]
    [Export] public Vector3 WeaponOffset = new(0.428f, -0.1f, -0.50f);
    [Export] public bool Lerp = false;
    [Export] PackedScene BulletCrater;

    //projectile
    [ExportGroup("Bullet")]
    [Export] PackedScene bullet;
    [Export] public float BulletVelocity;
    [ExportGroup("Casing")]
    [Export] float EjectionVelocity = 20f;
    [Export] PackedScene Casing;
    public enum WeaponType
    {
        HitScan, Projectile
    }
    Node3D World;
    public GpuParticles3D Effect;
    public CollisionShape3D Collider;
    public RayCast3D RayCast3DAim;
    public Node3D CassingSpawner;
    public Node3D BulletHole;
    public bool CanShoot = true;
    public Timer DelayTimer;

    Vector3 RecoilTargetRotation;
    Vector3 RecoilTargetPosition;

    [ExportGroup("Shoot Sound")]
    AudioHandler ShootSound;
    [Export] float AudioPitchStep = 0.02f;
    [Export] public string SoundPath = "res://Sounds/AK6.mp3";
    [Export] public float MaxDB = 10;
    [Export] public float Pitch = 1;
    [ExportGroup("Reload Sound")]
    public AudioHandler ReloadStartSound;
    [Export] public string ReloadStartSoundPath = "res://Sounds/ReloadStart.mp3";
    public AudioHandler ReloadEndSound;
    [Export] public string ReloadEndSoundPath = "res://Sounds/ReloadEnd.mp3";


    public override void _Ready()
    {


        //DelayTimer = new Timer();
        //AddChild(DelayTimer);
        //GD.Print(DelayTimer);
        CallDeferred("Initialize");


    }
    void Initialize()
    { 
        ShootSound = new AudioHandler(Path:SoundPath, MaxDB:MaxDB, Pitch: Pitch);
        AddChild(ShootSound);
        ReloadStartSound = new AudioHandler(Path: ReloadStartSoundPath);
        AddChild(ReloadStartSound);
        ReloadEndSound = new AudioHandler(Path: ReloadEndSoundPath);
        AddChild(ReloadEndSound);
        if (GetChild(0) is Node3D bulletHole && bulletHole != null)
        {
            BulletHole = bulletHole;

        }
        else GD.PrintErr("BulletHole is null!");
        if (GetChild(1) is RayCast3D rayCast3DAim) RayCast3DAim = rayCast3DAim; else GD.PrintErr("RayCast Aim is null!");
        if (GetChild(3) is Node3D cassingSpawner) CassingSpawner = cassingSpawner; else GD.PrintErr("Cassing Spawner is null!");
        if (GetChild(4) is GpuParticles3D effect) Effect = effect; else GD.PrintErr("Effect is null!");
        if (GetChild(6) is CollisionShape3D collider) Collider = collider; else GD.PrintErr("Collider is null!");

        DelayTimer = new Timer();
        AddChild(DelayTimer);
        DelayTimer.OneShot = true;
        DelayTimer.Timeout += OnDelayTimerTimeoutSignal;
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        if (Lerp)
        {
            RecoilTargetPosition = new Vector3(0, 0, Mathf.Lerp(RecoilTargetPosition.Z, 0, LerpSpeed * (float)delta));
            RecoilTargetRotation = new Vector3(Mathf.Lerp(RecoilTargetRotation.X, 0, 10 * (float)delta), 0, Mathf.Lerp(RecoilTargetRotation.Z, 0, LerpSpeed * (float)delta));
            Node3D Weapon = this;
            Weapon.Position = new Vector3(Weapon.Position.X, Weapon.Position.Y, Mathf.Lerp(Weapon.Position.Z, RecoilTargetPosition.Z, 20 * (float)delta));
            Weapon.Rotation = new Vector3(Mathf.Lerp(Weapon.Rotation.X, RecoilTargetRotation.X, LerpSpeedRecovery * (float)delta), Weapon.Rotation.Y, Mathf.Lerp(Weapon.Rotation.Z, -RecoilTargetRotation.Z, LerpSpeedRecovery * (float)delta));
        }
        else
        {

            RecoilTargetPosition = new Vector3(0, 0, Mathf.Lerp(RecoilTargetPosition.Z, 0, LerpSpeed * (float)delta));
            RecoilTargetRotation = new Vector3(Mathf.Lerp(RecoilTargetRotation.X, 0, 10 * (float)delta), 0, Mathf.Lerp(RecoilTargetRotation.Z, 0, LerpSpeed * (float)delta));
            Node3D Weapon = this;
            Weapon.Position = new Vector3(Weapon.Position.X, Weapon.Position.Y, Mathf.Lerp(Weapon.Position.Z, RecoilTargetPosition.Z, 20 * (float)delta));
            Weapon.Rotation = new Vector3(Mathf.Lerp(Weapon.Rotation.X, RecoilTargetRotation.X, LerpSpeedRecovery * (float)delta), Weapon.Rotation.Y, -Mathf.Lerp(Weapon.Rotation.Z, -RecoilTargetRotation.Z, LerpSpeedRecovery * (float)delta));
        }
    }
    public void Shoot(RayCast3D rayCast3DHeadAim, CharacterBody3D player)
    {
        ShootSound.PitchScale = Pitch - ((MaxAmmo - Ammo) * AudioPitchStep);
        if (Ammo > 0 && CanShoot)
        {
            ShootSound.Play();
            if (Type == WeaponType.HitScan)
            {
                ShootHitScan(rayCast3DHeadAim);

            }
            else if (Type == WeaponType.Projectile)
            {
                ShootProjectile(rayCast3DHeadAim);
            }
            if (Casing != null) { DropCasing(player); }
            ;
        }
        else
        {
            // funny click sound
        }

    }
    private void DropCasing(CharacterBody3D player)
    {
        RigidBody3D c = (RigidBody3D)Casing.Instantiate<Node3D>();
        (GetTree().Root.GetNode("World") as Node3D).AddChild(c);
        c.GlobalTransform = CassingSpawner.GlobalTransform;
        c.LinearVelocity = CassingSpawner.GlobalTransform.Basis.Z * 20 * EjectionVelocity - (CassingSpawner.GlobalTransform.Basis.X * 3 * EjectionVelocity) + (player.Velocity / 2);
    }
    void ShootProjectile(RayCast3D rayCast3DHeadAim)
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
        (GetTree().Root.GetNode("World") as Node3D).AddChild(b);
        b.GlobalTransform = BulletHole.GlobalTransform;

        b.LinearVelocity = BulletHole.GlobalTransform.Basis.X * BulletVelocity * (5);

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
        if (tempNode == null) return;
        switch (tempNode)
        {
            case Enemy enemy:
                enemy.DamageHandler.DamageTarget(Damage, Penetration, tempNode);
                //Target.Velocity.X = Mathf.Lerp((float)velocity.X, (float)direction.X * Speed, (float)delta * 14.0f);
                //Target.Velocity.Z = Mathf.Lerp((float)velocity.Z, (float)direction.Z * Speed, (float)delta * 14.0f);
                enemy.Velocity = -GlobalTransform.Basis.Z * (Damage * (Penetration + 1) * Knockback);
                break;
            case RigidBody3D rigidbody:
                //Target.Velocity.X = Mathf.Lerp((float)velocity.X, (float)direction.X * Speed, (float)delta * 14.0f);
                //Target.Velocity.Z = Mathf.Lerp((float)velocity.Z, (float)direction.Z * Speed, (float)delta * 14.0f);
                rigidbody.LinearVelocity += -GlobalTransform.Basis.Z * (Damage * (Penetration + 1) * Knockback / rigidbody.Mass);
                break;
            case Damagable damagable:
                damagable.DamageTarget(Damage, Penetration);
                if (tempNode is RigidBody3D rigid)
                {
                    rigid.LinearVelocity += -GlobalTransform.Basis.Z * (Damage * (Penetration + 1) * Knockback);
                }
                break;
            case Door door when door.CanBeInteracted:
                door.DamageHandler.DamageTarget(Damage, Penetration, tempNode);
                break;
            case DoorMechanical doormech when doormech.CanBeInteracted:
                doormech.DamageHandler.DamageTarget(Damage, Penetration, tempNode);
                break;
            case Civilian civilian:
                civilian.DamageHandler.DamageTarget(Damage, Penetration, tempNode);
                civilian.SetPanic();
                civilian.Velocity = -GlobalTransform.Basis.Z * (Damage * (Penetration + 1) * Knockback);
                break;
            case Node3D:
                Node3D crater = (Node3D)BulletCrater.Instantiate();
                (GetTree().Root.GetNode("World") as Node3D).AddChild(crater);


                // Set position to the hit point
                crater.GlobalPosition = rayCast3DHeadAim.GetCollisionPoint();

                // Align crater to the surface normal
                Vector3 normal = rayCast3DHeadAim.GetCollisionNormal();
                crater.LookAt(crater.GlobalPosition + normal, Vector3.Up);
                break;
        
        }

    }
    public void Reload(ref int PlayerAmmo)
    {
        ReloadEndSound.Play();
        if (PlayerAmmo <= MaxAmmo - Ammo) // if player ammo is less than needed ammo to fill
        {
            Ammo += PlayerAmmo;
            PlayerAmmo = 0;// accidentally did this and it should work
        }
        else
        {
            PlayerAmmo -= MaxAmmo - Ammo;// gets the required amount of ammo to remove
            Ammo = MaxAmmo;

        }
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
    public void Appear()
    {
        this.Visible = true;
    }
    public void Disappear()
    {
        this.Visible = false;
    }
    private void OnDelayTimerTimeoutSignal()
    {
        CanShoot = true;
    }
    public void DisablePhysics()
    {
        // need to get rigidbody without this throwing a fit
        Freeze = true;
        Collider.Disabled = true;
    }
    public void EnablePhysics()
    {
        Freeze = false;
        Collider.Disabled = false;
    }
}
// please find a way to remove that shitty error about not being able to cast x to x its getting really annoying

