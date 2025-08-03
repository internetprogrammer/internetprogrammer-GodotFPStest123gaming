using Godot;

public partial class Enemy : NPC
{
    [ExportGroup("Search")]
    [Export] public float TargetSearchRotate = 180f; // this will be itself - 45 since the targesearchsideoffset will subtract at start
    [Export] public float TargetSearchSideOffset = 90F;
    bool NeedsToChangeRotation = false;
    const float SearchRate = 0.3f;
    Vector3 OldRotation;

    [ExportGroup("Skill")]
    [Export] public int Skill = 25;//min 0 max 100
    public int ActualSkill;// skill after calculating skill and awareness
    [Export] public int GuardAwarenessOffset;
    [Export] public int PatrolAwarenessOffset;
    [Export] public int RetreatSkillPenalty;
    [Export] public int AssaultingAwarenessOffset;
    [ExportGroup("Patrol")]
    [Export] public Vector3 PatrolOffset = new(10, 0, 10);
    Vector3 PatrolCount;

    [ExportGroup("General")]
    [Export] public Sight Sight;
    [Export] public float ThinkingDelay = 1; // delay before the enemy decides to shoot
    Timer ThinkingTimer = new();
    [Export] public float ReloadDelay = 3;
    Timer ReloadTimer = new();
    [Export] public float ForgetDelay = 5; // delay before the enemy decides to stop searching
    Timer ForgetTimer = new();

    bool IsAiming = false;

    Node3D target;
    Vector3 LastKnownLocation = Vector3.Zero;
    [Export] public WeaponHandler weaponHandler;
    public override void _Ready()
    {
        AddChild(ThinkingTimer);
        AddChild(ReloadTimer);
        AddChild(ForgetTimer);
        ThinkingTimer.OneShot = true;
        ReloadTimer.OneShot = true;
        ForgetTimer.OneShot = true;
        ThinkingTimer.Timeout += OnThinkingTimerTimeout;
        ReloadTimer.Timeout += OnReloadTimerTimeout;
        ForgetTimer.Timeout += OnForgetTimerTimeout;

        InitializeDamagableCharacter();
        AddChild(DamageHandler);
    }
    void OnThinkingTimerTimeout()
    {
        IsAiming = false;
    }
    void OnReloadTimerTimeout()
    {
        weaponHandler.weapon.Reload(100);
    }
    void OnForgetTimerTimeout()
    {
        IsRunning = false;
        CurrentState = State.Aware;
    }
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

    public void ShootAt(RayCast3D aimCast)
    {
        weaponHandler.weapon.Shoot(aimCast, this);
    }
    public void HandleSight(double delta)
    {
        target = Sight.Check(aimCast, this); if (target != null)
        {
            IsRunning = true;
            if (CurrentState != State.Engaging && CurrentState != State.Searching && ThinkingTimer.IsStopped())
            {
                IsAiming = true;
                ThinkingTimer.Start(ThinkingDelay);
            }

            CurrentState = State.Engaging;
            LastKnownLocation = target.GlobalPosition;

            Vector3 currentRot = Rotation;

            TargetRotation = GlobalTransform.LookingAt(
                target.GlobalTransform.Origin,
                Vector3.Up
            ).Basis.GetEuler();
            Rotation = Rotation.Lerp(TargetRotation, NPCTurnRate * (float)delta);

            Stop();

            if (!IsAiming) ShootAt(aimCast);
        }
        else if (LastKnownLocation != Vector3.Zero)
        {


            Navigate(LastKnownLocation);
            if (GetWhetherReachedDestination() && CurrentState != State.Searching)
            {
                GD.Print("bibixi");
                StartSearch();
                LastKnownLocation = Vector3.Zero;
            }
        }

    }
    void SearchInit()
    {

    }
    public void StartSearch()
    {
        OldRotation = Rotation;
        CurrentState = State.Searching;
        NeedsToChangeRotation = true;
    }
    public void Search(double delta)
    {

        if (NeedsToChangeRotation) { Rotation = new(Rotation.X, Rotation.Y - Mathf.DegToRad(TargetSearchSideOffset), Rotation.Z); NeedsToChangeRotation = !NeedsToChangeRotation; GD.Print("fuuuuck"); }
        Rotation = new(Rotation.X, Mathf.Lerp(Rotation.Y, Rotation.Y + Mathf.DegToRad(TargetSearchRotate), SearchRate * (float)delta), Rotation.Z);
        if (Mathf.Abs(OldRotation.Y - Rotation.Y) > Mathf.DegToRad(TargetSearchRotate))
        {
            IsRunning = false;
            CurrentState = State.Aware;
            Rotation = OldRotation;
        }
    }
    public void HandleSound(Vector3 Target)
    {
        if (CurrentState != State.Aware && CurrentState != State.Moving) return;
        if (PanicGenerator.RandiRange(0, 100) > (Skill * PanicMultiplier))
        {

            Panic();
        }
        CheckSound(Target);

    }
    public void CheckSound(Vector3 Target)
    {
        Navigate(Target);
        if(GetWhetherReachedDestination() && CurrentState != State.Searching) StartSearch();
    }
    public override void _Process(double delta)
    {
        if (weaponHandler.weapon.Ammo == 0 && ReloadTimer.IsStopped())
        {
            ReloadTimer.Start(ReloadDelay);
        }
        HandleSight(delta);
        if (CurrentState == State.Searching) Search(delta);
    }

}
