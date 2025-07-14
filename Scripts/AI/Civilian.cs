using Godot;
using System;

public partial class Civilian : NPC
{
	RandomNumberGenerator WanderGenerator;


	Timer WanderCheckTime;
	float TimeBetweenWanderCheck = 7;
	float WanderAmount = 6.0f;
	float DePanicTime = 30f;// use this with another timer

	public override void _Ready()
	{
		AddChild(DamageHandler);
		CurrentState = State.Wandering;
		WanderCheckTime = new Timer();
		AddChild(WanderCheckTime);
		PanicGenerator = new RandomNumberGenerator();
		PanicGenerator.Randomize();
        WanderGenerator = new RandomNumberGenerator();
		WanderGenerator.Randomize();
		WanderCheckTime.Timeout += WanderTimerTimeout;
		WanderCheckTime.Start(TimeBetweenWanderCheck);
	}
	void WanderAround()
	{
		if(!(CurrentState == State.Wandering)) { return; }
		if (WanderGenerator.RandiRange(0,1) == 1) {
			float x = WanderGenerator.RandfRange(-WanderAmount, WanderAmount);
			float y = WanderGenerator.RandfRange(-WanderAmount, WanderAmount);
			Vector3 target = new Vector3(x, 0, y);
			Navigate(GlobalPosition + target);
		}
	}
	public void Talk(Node3D target)// so that the civ looks at the player
	{
		LookAt(target.GlobalPosition);
	}

	public void Stare()
	{

	}
	public void TargetedWonder(Node3D target) {
		Navigate(target.GlobalPosition);
	}
	private void WanderTimerTimeout()
	{
		WanderAround();
	}
}
