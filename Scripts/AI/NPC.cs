using Godot;

public abstract partial class NPC : DamagableCharacter
{
    public const float PanickedRunDistance = 3f;
    public const float NPCTurnRate = 10f;
    [ExportGroup("Standard Movement")]
    [Export] public float Speed = 4.0f;
    [Export] public float RunMultiplier = 2.0f;
    [Export] public float JumpVelocity = 4.5f;
    [ExportGroup("Standard Behaviour")]
    [Export] public int sight = 25;// 0 - 100
    [Export] public bool TooChuddedToCare = false;// stops panic

    [ExportGroup("BS")]
    [Export] NavigationAgent3D NavigationAgent;
    [Export] public RayCast3D aimCast;

    Vector3 targetRotation;
    Vector3 lerpTargetRotation;

    public const float PanicMultiplier = 0.1f;

    public RandomNumberGenerator PanicGenerator = new RandomNumberGenerator();
    public RandomNumberGenerator WanderGenerator = new RandomNumberGenerator();

    public int UnpanicValue = 10;
    public int MinimumPanicRunDistance = 3;

    public int PanicValue = 0;

    bool IsPanicked = false;
    public bool IsRunning = false;

    float ActualSpeed = 0;
    public Vector3 TargetRotation { get;  set; }// rotation that the player ought to be 



    public enum State
    {
        Aware,
        Panicked,
        Searching,
        Moving,
        Engaging,
        Surrender
    }
    public enum Command
    {
        Standing, Wandering, LimitedWandering,Assaulting, Retreat, Guard, Patrol
    }
    public State CurrentState = State.Aware;
    State HashedState;
    public override void _Ready()
    {
        PanicGenerator.Randomize();
    }
    public void SetPanic()
    {
        if (TooChuddedToCare) return;
        if (CurrentState != State.Panicked)
        {
            Panic();
        }
    }
    public void Panic()
    {
        IsRunning = true;
        IsPanicked = true;
        if (CurrentState != State.Panicked)
        {
            HashedState = CurrentState;
            CurrentState = State.Panicked;
        }
        float x = PanicGenerator.RandfRange(-PanickedRunDistance, PanickedRunDistance);
        float y = PanicGenerator.RandfRange(-PanickedRunDistance, PanickedRunDistance);
        if (x < MinimumPanicRunDistance && x > 0) { x = MinimumPanicRunDistance; }
        else if (x > MinimumPanicRunDistance && x < 0) { x = MinimumPanicRunDistance; }
        if (y < MinimumPanicRunDistance && y > 0) { y = MinimumPanicRunDistance; }
        else if (y > MinimumPanicRunDistance && y < 0) { y = MinimumPanicRunDistance; }
        Vector3 target = new(x, 0, y);
        Navigate(GlobalPosition + target);
        IsPanicked = false;
        if (PanicGenerator.RandiRange(0, 100) < UnpanicValue) { IsRunning = false; CurrentState = HashedState; }
    }
    public void Navigate(Vector3 target)
    {
        Vector3 direction = (target - GlobalPosition); 
        direction.Y = 0; // Prevent vertical tilting

        if (direction.LengthSquared() > 0.01f)
        {
            LookAt(GlobalPosition + direction.Normalized(), Vector3.Up);
        }
        NavigationAgent.TargetPosition = target;
    }
    public void Stop()
    {
        NavigationAgent.TargetPosition = GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        //GD.Print(CurrentState, GetWhetherReachedDestination());

        if (CurrentState == State.Panicked && GetWhetherReachedDestination() && !IsPanicked)// twitching that doesnt look good btw so plis fix
        {
            Panic();
        }
        ActualSpeed = Speed;
        if (IsRunning) ActualSpeed *= RunMultiplier;

        Vector3 velocity = Velocity;
        Vector3 direction = (NavigationAgent.GetNextPathPosition() - GlobalPosition).Normalized();

        //GD.Print("pathlocation:", NavigationAgent.GetNextPathPosition() ,"\n position:", GlobalPosition , "\n not normalized:", NavigationAgent.GetNextPathPosition() - GlobalPosition,"\n",(NavigationAgent.GetNextPathPosition() - GlobalPosition).Normalized() );
        velocity = velocity.Lerp(direction * ActualSpeed, (float)delta * 14.0f) + (GetGravity() * (float)delta);
        Velocity = velocity;
        CheckFallDamage();
        if (!SnapUpToStair(delta))
        {
            MoveAndSlide();
            SnapDownToStair();
        }

    }
    public bool GetWhetherReachedDestination()
    {
        float distance = (NavigationAgent.GetNextPathPosition() - GlobalPosition).Length();
        if (distance < 0.7f) { return true; }
        return false;
    }
}
