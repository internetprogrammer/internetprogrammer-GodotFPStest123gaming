using Godot;
using System;
// todo camera effects on movement,
public partial class Player : DamagableCharacter
{
    //Constants
    private const float WeaponLerpTimeValue = 10f;
    private const float VelocityLerpTimeValue = 14.0f;
    private const float GrabbedPositionVelocityValue = 13.0f;// controls how fast the velocity will be that moves the grabbed object to the grab position 
    private const float StandardThrownWeaponRBAngularVelocity = 20f;
    private const float StandardThrownWeaponRBLinearVelocity = 10f;
    private const float HeadBobValue = 0.75f;
    private const float CameraYOffset = 0.0f;

    //Movement
    [ExportGroup("Standard Movement")]
    [Export] float Speed = 300.0f;
    [Export] float RunMultiplier = 2.0f;
    [Export] float CrouchMultiplier = 0.4f;
    [Export] float JumpVelocity = 4.5f;
    [Export] int[] LockIDS = new int[] { 0 };
    [Export] float Sensitivity = 0.03f;

    private float RunMultiplierActual = 1;
    private float CrouchMultiplierActual = 1;
    [ExportGroup("Sway")]
    [Export] float JumpSwayAmount = 0.01f;
    [Export] float WeaponSwayAmount = 0.01f;
    [Export] float HeadMovementAmount = 0.06f;
    [Export] float HeadMovementFrequency = 2.5f;

    // Nodes
    public UI UI;
    Node3D head;
    Camera3D camera;
    Node3D body;
    Node3D GrabPoint;
    Node3D WorldNode;
    public Node3D WeaponNode;
    RayCast3D RayCastCheckForObject;
    public RayCast3D RayCastHeadAim;
    RayCast3D RayCastKick;

    [ExportGroup("Throwing")]
    [Export] float ThrowPower = 5.0f;

    //Weapon Logic
    [ExportGroup("Weapon Mechanics")]
    public int weaponNum = 1;
    //ammo the player has
    public int[] WeaponAmmo = [120, 120];

    [Export] Weaponrb[] Weapons = new Weaponrb[2];


    [Export] public float WeaponRotationAmount = 8;
    [Export] public float WeaponSwayMultiplier = 1;
    private float WeaponSwayTime;
    private Vector3 WeaponOffset = Vector3.Zero;
    //Grab Logic
    private Node3D GrabbedObject;
    public bool Grabbing = false;
    private float HeadMovementTime;
    // Camera Logic
    private float CameraRotationAmount = 0.02f;
    Vector2 inputDir;
    InputEventMouseMotion MouseMotion; // will need for the procedural sway for the weapons 
    Vector2 MouseRotationValue = Vector2.Zero; // keeps the mouse value for sway rotation purposes i think
                                               //Leaning Logic
    [ExportGroup("Lean")]
    [Export] float LeanRotationAmount = 0.20f;
    [Export] float LeanOffsetAmount = 0.3f;
    [Export] float RegularFov = 90;
    [Export] float ZoomFov = 45;
    [Export] float RunFovIncrease = 10;
    float ZoomActualFovIncrease = 0;
    float RunActualFovIncrease = 0;
    float ActualFov = 0;
    float ActualLeanRotation;
    float ActualLeanOffset;

    //Reload Mechanics
    [ExportGroup("Reload Mechanics")]
    [Export] float MinReloadHeight = -0.4f;
    [Export] float MaxReloadHeight = 0.4f;
    [Export] float ReloadHeight = -0.35f;
    [Export] float CurrentReloadHeight = 0;
    [Export] float ReloadSentivity = 0.006f;
    bool IsReloading = false;
    //Crosshair
    [ExportGroup("Crosshair")]
    [Export] Godot.Label Crosshair;
    [Export] string NormalCrosshair = ".";
    [Export] string InteractCrosshair = "+";

    //Kicking Mechanics
    [ExportGroup("Kick")]
    [Export] int KickDamage = 100;
    [Export] int KickPenetration = 0;
    [Export] float KickBoost = 5.5f;
    [Export] float KickSelfPushForce = 5.5f;
    [Export] float KickKnockback = 0.5f;
    [Export] float NextKickDelay = 1.5f;
    Timer KickTimer;
    bool CanKick = true;


    public bool IsUnderwater = false;
    //Water Mechanics
    [ExportGroup("Water Mechanics")]
    [Export] float WaterGravityDecrease = 0.2f;
    [Export] float SwimSpeed = 75f;
    [Export] CanvasLayer watercanvas;
    [Export] public ShapeCast3D WaterCast;
    [ExportGroup("Walk Sound")]
    public AudioHandler WalkSound;
    [Export] public string WalkSoundPath = "res://Sounds/StepDefault.mp3";
    [Export] public float WalkSoundPitch = 0.9f;
    [Export] public float RunSpeedIncrease = 0.65f;
    [Export] public float RunActualIncrease = 0f;
    [ExportGroup("Jump Sound")]
    public AudioHandler JumpSound;
    [Export] public string JumpSoundPath = "res://Sounds/JumpSound.mp3";
    [ExportGroup("Kick Sound")]
    public AudioHandler KickSound;
    [Export] public string KickSoundPath = "res://Sounds/KickSound.mp3";
    public override void _Ready()
    {
        Global.Player = this;
        InitializeDamagableCharacter();
        WalkSound = new AudioHandler(WalkSoundPath);
        AddChild(WalkSound);
        JumpSound = new AudioHandler(JumpSoundPath);
        AddChild(JumpSound);
        KickSound = new AudioHandler(KickSoundPath);
        AddChild(KickSound);
        KickTimer = new Timer();
        AddChild(KickTimer);
        KickTimer.Timeout += KickDelayFinished;
        //INST
        if (GetChild(0) is Node3D headLocal && headLocal != null)
        {
            head = headLocal;
            if (head.GetChild(0) is Camera3D cameraLocal) camera = cameraLocal; else GD.PrintErr("camera is null!");
            if (head.GetChild(1) is Node3D grablocal) GrabPoint = grablocal; else GD.PrintErr("grabpoint is null!");
            if (head.GetChild(2) is Node3D weaponLocal) WeaponNode = weaponLocal; else GD.PrintErr("weaponnode is null!");
            if (head.GetChild(3) is RayCast3D grabcast) RayCastCheckForObject = grabcast; else GD.PrintErr("grab ray cast is null!");
            if (head.GetChild(4) is RayCast3D headaimcast) RayCastHeadAim = headaimcast; else GD.PrintErr("head aim ray cast is null!");
            if (head.GetChild(5) is RayCast3D kickaimcast) RayCastKick = kickaimcast; else GD.PrintErr("kick ray cast is null!");
        }
        else GD.PrintErr("head is null!");
        if (GetChild(1) is UI ui) UI = ui; else GD.PrintErr("UI is null!");
        if (this is CharacterBody3D characterbody && characterbody != null) body = characterbody; else GD.PrintErr("body is null!");
        if (GetNode<Node3D>("/root/World") is Node3D world && world != null)
        {
            WorldNode = world;
        }
        else GD.PrintErr("world is null!");

        Input.MouseMode = Input.MouseModeEnum.Captured;
    }
    // !! done up to here
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
        {

            MouseMotion = (InputEventMouseMotion)@event;
            MouseRotationValue = new Vector2(MouseMotion.Relative.X, MouseMotion.Relative.Y);
            //translates mouse motion to camera movement

            if (!IsReloading)
            {
                body.RotateY(-MouseMotion.Relative.X * Sensitivity);
                head.RotateX(-MouseMotion.Relative.Y * Sensitivity);

            }
            head.Rotation = new Vector3(Math.Clamp(head.Rotation.X, Mathf.DegToRad(-80), Mathf.DegToRad(80)), 0, head.Rotation.Z);
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        HandleWeapon(delta);
        HandleInteraction(delta);
        if (IsUnderwater) HandleMovementWater(velocity, delta);
        else HandleMovement(velocity, delta);//HandleMovement(velocity, delta);
        HeadMovement(delta);
        WeaponSway(delta, velocity.Y);
        CheckFallDamage();
        if (!SnapUpToStair(delta))
        {
            MoveAndSlide();
            SnapDownToStair();
        }
    }
    private void HeadMovement(double delta)
    {
        if (IsOnFloor())
        {
            HeadMovementTime += (float)delta * Velocity.Length();
            head.Position = new Vector3(Mathf.Cos(HeadMovementTime * HeadMovementFrequency / 2) * HeadMovementAmount + ActualLeanOffset,
                        Mathf.Sin(HeadMovementTime * HeadMovementFrequency) * HeadMovementAmount + HeadBobValue + CameraYOffset,
                        0);
        }
    } // find a way to edit the transform for this
    private void WeaponSway(double delta, float YVelocity)
    {
        if (WeaponOffset == Vector3.Zero && (Weapons[0] != null || Weapons[1] != null))
        {
            WeaponOffset = Weapons[weaponNum - 1].WeaponOffset;
        }
        WeaponSwayTime += (float)delta * Velocity.Length();
        if (IsOnFloor())// so that no movement is done
        {
            WeaponNode.Position = new Vector3(-Mathf.Cos(WeaponSwayTime * HeadMovementFrequency / 2) * (WeaponSwayAmount * WeaponSwayMultiplier),
                    -Mathf.Sin(WeaponSwayTime * HeadMovementFrequency) * (WeaponSwayAmount * WeaponSwayMultiplier) + CurrentReloadHeight,
                    0) + WeaponOffset;
        }
        else
        {
            //WeaponNode.Position.Lerp(new Vector3(0, CurrentReloadHeight, 0) + WeaponOffset, WeaponLerpTimeValue * (float)delta);
            WeaponNode.Position = new Vector3(0, CurrentReloadHeight, 0) + WeaponOffset;
        }

        WeaponNode.Position.Lerp(WeaponOffset, WeaponLerpTimeValue * (float)delta);

        if (IsReloading) return;

        MouseRotationValue.Lerp(Vector2.Zero, WeaponLerpTimeValue * (float)delta);

        WeaponNode.Rotation = new Vector3((float)Mathf.Lerp(WeaponNode.Rotation.X, MouseRotationValue.Y * WeaponRotationAmount * 1.5f - (YVelocity * JumpSwayAmount), WeaponLerpTimeValue * delta),
            (float)Mathf.Lerp(WeaponNode.Rotation.Y, MouseRotationValue.X * WeaponRotationAmount * 1.5f, WeaponLerpTimeValue * delta),
            (float)Mathf.Lerp(WeaponNode.Rotation.Z, -inputDir.X * WeaponRotationAmount * 50, WeaponLerpTimeValue * delta));
        // the magic number 1.5f is used to make the rotation of X and Y 50% more than Z for stylistic reasons, the * 50f is due to the inputdir being a very weak value and needing to be increased by it for standard rotation

    }
    // handle anything that has to do with movement
    private void HandleMovement(Vector3 velocity, double delta) // update the water handle movement with this 
    {
        // Handle Jump
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            JumpSound.play();
            velocity.Y += JumpVelocity;
        }
        if (Input.IsActionPressed("run")) // || IsRunning == true for toggleable with the addition of a untoggle function
        {
            RunActualFovIncrease = Mathf.Lerp(RunActualFovIncrease, RunFovIncrease, 10 * (float)delta);
            RunMultiplierActual = RunMultiplier;
            RunActualIncrease = RunSpeedIncrease;
        }
        else
        {
            RunActualFovIncrease = Mathf.Lerp(RunActualFovIncrease, 0, 10 * (float)delta);
            RunMultiplierActual = 1.0f;
            RunActualIncrease = 0.0f;
        }
        if (Input.IsActionPressed("crouch")) // || IsCrouching == true for toggleable with the addition of a untoggle function
        {
            CrouchMultiplierActual = CrouchMultiplier;
        }
        else
        {
            CrouchMultiplierActual = 1.0f;
        }
        Lean(delta); // this used to be under the isonfloor check so if issues are caused think about this
                     // Add the gravity.
        if (!IsOnFloor())
        {
            WalkSound.Playing = false;
            velocity += GetGravity() * (float)delta;
        }

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        inputDir = Input.GetVector("left", "right", "up", "down");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        HeadRotation(direction, delta);
        if (direction != Vector3.Zero)
        {
            if (!WalkSound.Playing)
            {
                WalkSound.Playing = true;
                GD.Randomize();
                WalkSound.PitchScale = (WalkSoundPitch + (GD.Randf() / 5f) - 0.2f + RunActualIncrease);
            }
            //the regular movement function with small additions for spice in the game
            velocity.X = direction.X * Speed * RunMultiplierActual * CrouchMultiplierActual * (float)delta;
            velocity.Z = direction.Z * Speed * RunMultiplierActual * CrouchMultiplierActual * (float)delta;
        }
        else
        {
            //can be used for decreasing air time too
            velocity.X = Mathf.Lerp(velocity.X, direction.X * Speed, (float)delta * VelocityLerpTimeValue);
            velocity.Z = Mathf.Lerp(velocity.Z, direction.Z * Speed, (float)delta * VelocityLerpTimeValue);
        }

        Velocity = velocity; // formality for translating calculated velocity into actual velocity in the game

    }
    private void HandleMovementWater(Vector3 velocity, double delta)
    {
        UpdateWaterCamCheck();
        if (Input.IsActionPressed("run")) // || IsRunning == true for toggleable with the addition of a untoggle function
        {
            RunActualFovIncrease = Mathf.Lerp(RunActualFovIncrease, RunFovIncrease, 10 * (float)delta);
            RunMultiplierActual = RunMultiplier;
        }
        else
        {
            RunActualFovIncrease = Mathf.Lerp(RunActualFovIncrease, 0, 10 * (float)delta);
            RunMultiplierActual = 1.0f;
        }
        if (Input.IsActionPressed("crouch")) // || IsCrouching == true for toggleable with the addition of a untoggle function
        {
            CrouchMultiplierActual = CrouchMultiplier;
        }
        else
        {
            CrouchMultiplierActual = 1.0f;
        }
        Lean(delta); // this used to be under the isonfloor check so if issues are caused think about this
                     // Add the gravity.
        if (!IsOnFloor())
        {
            velocity += GetGravity() * WaterGravityDecrease * (float)delta;
        }
        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        inputDir = Input.GetVector("left", "right", "up", "down");
        Vector3 direction = (new Basis(Transform.Basis.X, Transform.Basis.Y, head.GlobalTransform.Basis.Z) * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        //HeadRotation(direction, delta);
        if (direction != Vector3.Zero)
        {
            //the regular movement function with small additions for spice in the game
            velocity.X = direction.X * SwimSpeed * RunMultiplierActual * (float)delta;
            velocity.Y = direction.Y * SwimSpeed * RunMultiplierActual * (float)delta;
            velocity.Z = direction.Z * SwimSpeed * RunMultiplierActual * (float)delta;
        }
        else
        {
            //can be used for decreasing air time too
            velocity.X = Mathf.Lerp(velocity.X, direction.X * SwimSpeed, (float)delta * 14.0f);
            velocity.Z = Mathf.Lerp(velocity.Z, direction.Z * SwimSpeed, (float)delta * 14.0f);
        }

        Velocity = velocity; // formality for translating calculated velocity into actual velocity in the game

    }
    public void LeaveWater()
    {
        IsUnderwater = false;
    }
    private void HeadRotation(Vector3 direction, double delta)
    {
        camera.Rotation = new Vector3(camera.Rotation.X,
            camera.Rotation.Y,
            (float)Mathf.Lerp(camera.Rotation.Z, -inputDir.X * CameraRotationAmount, 10 * delta));

        head.Rotation = new Vector3(head.Rotation.X, head.Rotation.Y, ActualLeanRotation);

        //rotates head in the Z rotation with the value of velocity X lerped need to get relative X instead of X
    }
    private void Lean(double delta)
    {
        if (Input.IsActionPressed("leanleft"))
        {
            ActualLeanOffset = Mathf.Lerp(ActualLeanOffset, -LeanOffsetAmount, 10 * (float)delta);
            ActualLeanRotation = Mathf.Lerp(ActualLeanRotation, LeanRotationAmount, 10 * (float)delta);
        }
        else if (Input.IsActionPressed("leanright"))
        {
            ActualLeanOffset = Mathf.Lerp(ActualLeanOffset, LeanOffsetAmount, 10 * (float)delta);
            ActualLeanRotation = Mathf.Lerp(ActualLeanRotation, -LeanRotationAmount, 10 * (float)delta);
        }
        else
        {
            ActualLeanOffset = Mathf.Lerp(ActualLeanOffset, 0, 10 * (float)delta);
            ActualLeanRotation = Mathf.Lerp(ActualLeanRotation, 0, 10 * (float)delta);
        }

    }
    // handle anything that has to do with weapons
    private void HandleWeapon(double delta)
    {
        // I suffix means Input to avoid same names
        KickI();
        // checks if the equiped weapon is null because
        // its optimal to be put here and looks better
        //if ((weaponNum == 1 && Weapon1 == null) || (weaponNum == 2 && Weapon2 == null))
        //{
        //    return;
        //}
        SwapI();
        Weaponrb CurrentWeapon;
        if (Weapons[weaponNum - 1] != null) CurrentWeapon = Weapons[weaponNum - 1];
        else { return; }
        ShootI(ref Weapons, ref CurrentWeapon);
        ThrowI(ref CurrentWeapon);
        ReloadI(ref CurrentWeapon, delta);
    }
    void SwapI()
    {
        if (Input.IsActionJustPressed("weapon1"))
        {
            weaponNum = 1;
            if (Weapons[1] != null) Weapons[1].Disappear();
            if (Weapons[0] != null) Weapons[0].Appear();
        }
        else if (Input.IsActionJustPressed("weapon2"))
        {
            weaponNum = 2;
            if (Weapons[0] != null) Weapons[0].Disappear();
            if (Weapons[1] != null) Weapons[1].Appear();
        }
    }
    void KickI()
    {
        if (!(Input.IsActionJustPressed("kick") && !Grabbing && CanKick)) return;

        CanKick = false;
        KickTimer.Start(NextKickDelay);
        Node3D tempNode = (Node3D)GetColliderDamagable();
        //switchbros seething at the fact that i can else if for days and its still performant since it runs only once, scriptkiddies buckbroken
        if (tempNode == null) return;

        KickSound.play();
        switch (tempNode)
        { // dupe this for bullets and weapons
            case Enemy enemy:
                enemy.DamageHandler.DamageTarget(KickDamage, KickPenetration, tempNode);
                enemy.Velocity = -GlobalTransform.Basis.Z * (KickDamage * (KickPenetration + 1) * KickKnockback);
                break;
            case RigidBody3D rigidbody:
                rigidbody.LinearVelocity += -GlobalTransform.Basis.Z * (KickDamage * (KickPenetration + 1) * KickKnockback / rigidbody.Mass);
                break;
            case Damagable damagable:
                damagable.DamageTarget(KickDamage, KickPenetration);
                if (tempNode is RigidBody3D rigid)
                {
                    rigid.LinearVelocity += -GlobalTransform.Basis.Z * (KickDamage * (KickPenetration + 1) * KickKnockback / rigid.Mass);
                }
                break;
            case Door door when door.DamageHandler != null:
                door.DamageHandler.DamageTarget(KickDamage, KickPenetration, tempNode);
                break;
            case DoorMechanical doormech when doormech.DamageHandler != null:
                doormech.DamageHandler.DamageTarget(KickDamage, KickPenetration, tempNode);
                break;
            case Civilian civilian:
                civilian.DamageHandler.DamageTarget(KickDamage, KickPenetration, tempNode);
                civilian.SetPanic();
                civilian.Velocity = -GlobalTransform.Basis.Z * (KickDamage * (KickPenetration + 1) * KickKnockback);
                break;
            case Node3D:
                Velocity += GlobalTransform.Basis.Z * (KickDamage * (KickPenetration + 1) * KickBoost);
                break;
        }

    }
    void ShootI(ref Weaponrb[] Weapons, ref Weaponrb CurrentWeapon)
    {
        if (Weapons[weaponNum - 1].WeaponFireMode == Weaponrb.FireMode.SemiAuto)
        {
            if (Input.IsActionJustPressed("shoot") && !Grabbing && !IsReloading)
            {
                CurrentWeapon.Shoot(RayCastHeadAim, this);
            }
        }
        else if (Weapons[weaponNum - 1].WeaponFireMode == Weaponrb.FireMode.FullAuto)
        {
            if (Input.IsActionPressed("shoot") && !Grabbing && !IsReloading)
            {
                CurrentWeapon.Shoot(RayCastHeadAim, this);
            }
        }
    }
    void ThrowI(ref Weaponrb Weapon)
    {
        if (!(Input.IsActionPressed("throwAway") && !IsReloading)) return;
        ThrowWeapon(ref Weapons[weaponNum - 1]); // throws the weapon and removes its reference 

    }
    void ReloadI(ref Weaponrb Weapon, double delta)
    {
        if (Input.IsActionJustPressed("reload"))
        {

            Weapon.ReloadStartSound.play();
        }
        if (Input.IsActionPressed("reload"))
        {
            IsReloading = true;
            CurrentReloadHeight = Mathf.Lerp(CurrentReloadHeight, -MouseMotion.Relative.Y * ReloadSentivity, 10 * (float)delta);
            CurrentReloadHeight = Mathf.Clamp(CurrentReloadHeight, MinReloadHeight, MaxReloadHeight);
            if (CurrentReloadHeight <= ReloadHeight)
            {
                Weapons[weaponNum - 1].Reload(ref WeaponAmmo[weaponNum - 1]);
            }
        }
        else
        {
            IsReloading = false;
            CurrentReloadHeight = Mathf.Lerp(CurrentReloadHeight, 0, 10 * (float)delta);
        }
    }
    public void KickDelayFinished()
    {
        CanKick = true;
    }
    public void ReadyGun(Weaponrb weapon)// probably will become an on tick thing since ill need to do weapon sway and such
    {
        weapon.DisablePhysics();
        weapon.GetParent().RemoveChild(weapon);
        WeaponNode.AddChild(weapon);
        weapon.GlobalPosition = WeaponNode.GlobalPosition;
        weapon.GlobalRotation = WeaponNode.GlobalRotation;
    }
    public void SwapWeapon(Weaponrb weaponCurrent, Weaponrb weaponGrabbed)
    {
        if (weaponCurrent != null)
        {
            WeaponNode.RemoveChild(weaponCurrent);
            GetNode<Node3D>("/root/World").AddChild(weaponCurrent);
        }
        weaponCurrent.GlobalPosition = weaponGrabbed.GlobalPosition;
        weaponCurrent.GlobalRotation = weaponGrabbed.GlobalRotation;

        weaponCurrent.EnablePhysics();
    }
    public void ThrowWeapon(ref Weaponrb weapon)
    {
        weapon.EnablePhysics();
        WeaponNode.RemoveChild(weapon);
        GetNode<Node3D>("/root/World").AddChild(weapon);// gets the rigidbody parent and does the physics stuff there instead of the fgc to first avoid errors and secondly to actually work since it the rigidbody doesnt like to coexist with the fgc9 script

        weapon.GlobalPosition = GrabPoint.GlobalPosition;

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! weapon
        weapon.LinearVelocity = -head.GlobalTransform.Basis.Z * ThrowPower * StandardThrownWeaponRBLinearVelocity;
        weapon.AngularVelocity = -head.GlobalTransform.Basis.Y * ThrowPower * StandardThrownWeaponRBAngularVelocity;
        weapon.RotateZ(head.GlobalRotation.Z + 90f);
        weapon = null;
    }// !!!!!!!!!!!!!!!!!!! issues with gun phasing through floor
     // handle anything that has to do with interactions
    private void HandleInteraction(double delta)
    {

        UpdateCrosshair();
        if (GrabbedObject != null)
        {
            UpdateGrabbedObject();

        }
        if (Input.IsActionJustPressed("retrieve") && !Grabbing && !IsReloading)
        {


            //uses metadata which is said by the respective scripts of each object 
            Node3D tempNode = (Node3D)GetColliderAsGD();
            if (tempNode == null) return;

            GD.Print(tempNode as Weaponrb);
            switch (tempNode)
            {
                case Weaponrb weapon:
                    WeaponTake(weapon);
                    break;

                case Door door when door.CanBeInteracted:
                    door.Interact(LockIDS);
                    break;

                case DoorMechanical doormech when doormech.CanBeInteracted:
                    doormech.Interact(LockIDS);
                    break;
                case Civilian civilian:
                    civilian.Talk(this);
                    break;
                case Lever lever:
                    lever.Interact();
                    break;

                case Button button:
                    button.Interact();
                    break;

                case Node3D node when node.GetChild(0) is InteractableObject interactable:
                    interactable.Interact();
                    break;

                case Node3D node when node.GetChild(0) is RetrievableObject retrievable:
                    retrievable.Pickup();
                    break;

                default:
                    GrabbedObject = GetColliderFromRB();
                    break;
            }
        }
        else if (Input.IsActionJustPressed("retrieve") && Grabbing)
        {
            GrabbedObject = null;
            Grabbing = false;

        }
        if (Input.IsActionPressed("zoom"))
        {
            ZoomActualFovIncrease = Mathf.Lerp(ZoomActualFovIncrease, ZoomFov, 10 * (float)delta);
        }
        else
        {
            ZoomActualFovIncrease = Mathf.Lerp(ZoomActualFovIncrease, 0, 10 * (float)delta);
        }
        //fov init
        camera.Fov = RegularFov - ZoomActualFovIncrease + RunActualFovIncrease;

    }
    public void UpdateCrosshair()
    {
        Node3D tempNode = (Node3D)GetColliderAsGD();
        if (tempNode != null)
        {
            if (tempNode is Weaponrb || tempNode.GetChild(0) is InteractableObject || tempNode is Civilian|| tempNode.GetChild(0) is RetrievableObject || tempNode is Lever || tempNode is Button || CheckColliderFromRB() != null)
            {

                Crosshair.Text = InteractCrosshair;
            }
            else if (tempNode is Door door)
            {
                if (door.CanBeInteracted)
                {
                    Crosshair.Text = InteractCrosshair;
                }
            }
            else if (tempNode is DoorMechanical mechdoor)
            {
                if (mechdoor.CanBeInteracted)
                {
                    Crosshair.Text = InteractCrosshair;
                }
            }
            else
            {
                Crosshair.Text = NormalCrosshair;
            }
        }
        else
        {
            Crosshair.Text = NormalCrosshair;
        }

    }
    public void WeaponTake(Weaponrb tempWeapon)
    {
        if (Weapons[weaponNum - 1] != null) SwapWeapon(Weapons[weaponNum - 1], tempWeapon);
        Weapons[weaponNum - 1] = tempWeapon;
        //GD.Print(Weapons[weaponNum - 1].Name);
        ReadyGun(Weapons[weaponNum - 1]);
    }
    private void UpdateGrabbedObject()
    {
        Vector3 LastPosition = GrabbedObject.GlobalPosition; // gets the previous frame from the object to use for calculating the velocity when letting go
        if (GrabbedObject is Damagable) { GD.Print("Fix me"); return; }//⚠️⚠️⚠️⚠️⚠️⚠️⚠️I MADE THIS TO STOP THE OBJECT FROM TRYING TO CAST ITSELF FROM DAMAGABLE AND DESTROYING THE SCRIPT,FIX IMMEDIATELY⚠️
        RigidBody3D rb = (RigidBody3D)GrabbedObject;// gets the rigidbody from said object
        GrabbedObject.GlobalRotation = head.GlobalRotation;                                         //GrabbedObject.Position = GrabPoint.GlobalPosition;// puts the grabbed object in the location of the grab node
        rb.LinearVelocity = (GrabPoint.GlobalPosition - GrabbedObject.GlobalPosition) * GrabbedPositionVelocityValue;
        if (Input.IsActionJustPressed("throw"))// this is what allows throwing
        {
            rb.LinearVelocity = (GrabbedObject.GlobalPosition - LastPosition) * GrabbedPositionVelocityValue - head.GlobalTransform.Basis.Z * (ThrowPower * 2) / rb.Mass; // gets the head forward direction so that both x and y are calculated since the y is used only in the head object in order not to have the player do unwanted gymnastics
            GrabbedObject = null;// removes the object from the player so it doesnt return
            Grabbing = false; // this is used to untoggle grabbing
        }
        else if (Input.IsActionJustPressed("grab")) //this is what allows for untoggling
        {
            rb.LinearVelocity = (GrabbedObject.GlobalPosition - LastPosition) * GrabbedPositionVelocityValue; // gets the current position minus the old position to calculate the direction and multiplies to add more force to it
            GrabbedObject = null;// removes the object from the player so it doesnt return
            Grabbing = false;// this is used to untoggle grabbing
        }
    }
    private Node3D GetColliderFromRB()
    {
        if (RayCastCheckForObject.IsColliding())
        {
            if (RayCastCheckForObject.GetCollider().IsClass("RigidBody3D")) //only rigid bodies should be grabbable, this eliminates everything else
            {
                Grabbing = true;// sets the grabbing boolean to true so that grabbing is solely toggleable
                return (Node3D)RayCastCheckForObject.GetCollider();// gets the collision object and then makes it a node3d
            }
        }
        return null;
    }
    private Vector3 GetColliderHead()
    {
        if (RayCastHeadAim.IsColliding())//raycast to check if grounded
        {
            return RayCastHeadAim.GetCollisionPoint();
        }
        return Vector3.Zero;
    }

    private Node3D CheckColliderFromRB()
    {
        if (RayCastCheckForObject.IsColliding())
        {
            if (RayCastCheckForObject.GetCollider().IsClass("RigidBody3D")) //only rigid bodies should be grabbable, this eliminates everything else
            {
                return (Node3D)RayCastCheckForObject.GetCollider();// gets the collision object and then makes it a node3d
            }
        }
        return null;
    }
    private GodotObject GetColliderAsGD()
    {
        if (RayCastCheckForObject.IsColliding())
        {
            return RayCastCheckForObject.GetCollider();
        }
        return null;
    }


    private GodotObject GetColliderDamagable()
    {
        if (RayCastKick.IsColliding())
        {
            if (RayCastKick.IsColliding())
            {
                return RayCastKick.GetCollider();
            }
        }
        return null;
    }
    public void UpdateWaterCamCheck()
    {
        if (WaterCast.IsColliding())
        {
            bool atleastonewater = false;
            for (int i = 0; i < WaterCast.GetCollisionCount(); i++)
            {
                if (WaterCast.GetCollider(i) is water)
                {
                    Underwater();
                    atleastonewater = true;
                }
            }
            if (!atleastonewater)
            {
                NotUnderwater();
            }
        }
        else
        {
            NotUnderwater();
        }
    }
    //is head underwater code
    public void Underwater()
    {
        watercanvas.Visible = true;
    }
    public void NotUnderwater()
    {
        watercanvas.Visible = false;
    }
}
