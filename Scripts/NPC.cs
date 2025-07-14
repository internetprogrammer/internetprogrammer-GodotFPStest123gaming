using Godot;
using System;

public partial class NPC : DamagableCharacter
{
	[Export] public float Speed = 4.0f;
	[Export] public float RunMultiplier = 2.0f;
	public const float PanickedRunDistance = 3f;
	[Export] public float JumpVelocity = 4.5f;
	[Export] public int sight = 25;// 0 - 100
	[Export] NavigationAgent3D NavigationAgent;
	Vector3 targetRotation;
	Vector3 lerpTargetRotation;
	[Export] bool TooChuddedToCare = false;//used for civilians that shouldnt care about shooting, this logic can be used to set a response to player shooting
	[Export] public RayCast3D aimCast;
	public const float PanicMultiplier = 0.1f;
    public RandomNumberGenerator PanicGenerator;
	public int UnpanicValue = 10;
    public int MinimumPanicRunDistance = 3;
    public int PanicValue = 0;
	bool IsPanicked = false;
	public bool IsRunning = false;
	float ActualSpeed = 0;

    public enum State
	{
		Standing,Wandering,LimitedWandering,Panicked,Assaulting,Retreat,Guard,Patrol
	}
	public State CurrentState;
	State HashedState;
	public void SetPanic()
	{
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
		GD.Print("niggers");
        float x = PanicGenerator.RandfRange(-PanickedRunDistance, PanickedRunDistance);
        float y = PanicGenerator.RandfRange(-PanickedRunDistance, PanickedRunDistance);
		if (x < MinimumPanicRunDistance && x > 0) { x = MinimumPanicRunDistance; }
		else if(x > MinimumPanicRunDistance && x < 0) {x = MinimumPanicRunDistance;}
        if (y < MinimumPanicRunDistance && y > 0) { y = MinimumPanicRunDistance; }
        else if (y > MinimumPanicRunDistance && y < 0) { y = MinimumPanicRunDistance; }
        Vector3 target = new (x, 0, y);
        Navigate(GlobalPosition + target);
		IsPanicked = false;
		if(PanicGenerator.RandiRange(0, 100) < UnpanicValue) {IsRunning = false; CurrentState = HashedState; GD.Print("niggers2"); }
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
    public override void _Ready()
	{
	}
	public override void _PhysicsProcess(double delta)
	{
		//GD.Print(CurrentState, GetWhetherReachedDestination());
		if(CurrentState == State.Panicked && GetWhetherReachedDestination() && !IsPanicked)// twitching that doesnt look good btw so plis fix
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
		if(NavigationAgent.GetNextPathPosition() - GlobalPosition < (Vector3.One / 2)){ return true; }
		return false;
	}
}
