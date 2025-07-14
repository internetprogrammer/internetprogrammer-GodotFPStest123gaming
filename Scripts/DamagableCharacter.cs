using Godot;
using System;
using static Godot.TextServer;

public partial class DamagableCharacter : CharacterBody3D
{
    [Export] public float Health = 100;
    [Export] public float Armour = 0;
	[Export] public int FallDamageLimit = 150;
	[Export] public int FallDamageAmount = 5;

    [Export] public float FloorMaxAngle = 45f;
    [Export] public float MaxStepHeight = 0.5f;
    [Export] public RayCast3D StairBelowRaycast;
    [Export] public RayCast3D StairAheadRaycast;

    public Vector3 velocity;
	public Damagable DamageHandler = new Damagable();
	float FallingVariable =0;
	int init =0;
    public bool SnappedToStairLastFrame = false;
    public override void _PhysicsProcess(double delta)
	{
		if(init == 0)
		{
            DamageHandler.Health = Health;
            DamageHandler.Armour = Armour;
			init++;
		}
        velocity = Velocity;

        velocity.X = Mathf.Lerp((float)velocity.X, 0, (float)delta * 14.0f);
        velocity.Z = Mathf.Lerp((float)velocity.Z, 0, (float)delta * 14.0f);
        velocity += GetGravity() * (float)delta;
        Velocity = velocity;
        MoveAndSlide();
        SnapDownToStair();
    }
	public void CheckFallDamage()
	{
		if (!IsOnFloor())
		{
			FallingVariable += -Velocity.Y;

		}
		else
		{
            if (FallingVariable > FallDamageLimit)
            {
                int FallingVariableInt = (int)FallingVariable;
                DamageHandler.DamageTarget(FallDamageAmount * (FallingVariableInt / FallDamageLimit), 0);

            }
            FallingVariable = 0;
		}
			
	}
    public bool SnapUpToStair(double delta)
    {
        if (!IsOnFloor() && ! SnappedToStairLastFrame) return false;
        if (Velocity.Y > 0 || (Velocity * new Vector3(1, 0, 1)).Length() ==0 ) return false;
        Vector3 ExpectedMoveMotion = Velocity * new Vector3(1, 0, 1) * (float)delta;
            Transform3D StepPositionWithClearence = GlobalTransform.Translated(ExpectedMoveMotion + new Vector3(0,MaxStepHeight *2, 0));
        PhysicsTestMotionResult3D DownCheckResult = new PhysicsTestMotionResult3D();

        if ((RunBodyTestMotion(StepPositionWithClearence, new Vector3(0, -MaxStepHeight * 2, 0), DownCheckResult)) 
            && (!DownCheckResult.GetCollider().IsClass("RigidBody3D") && !DownCheckResult.GetCollider().IsClass("CharacterBody3D")))   
        {
            float StepHeight = ((StepPositionWithClearence.Origin + DownCheckResult.GetTravel()) - GlobalPosition).Y;
            /*
            GD.Print(StepPositionWithClearence.Origin.Y, " origin");
            GD.Print(DownCheckResult.GetTravel().Y, " travel");
            GD.Print(-GlobalPosition.Y, "position");
            GD.Print(StepHeight); debug in case this shit breaks again
            */
            if (StepHeight < MaxStepHeight || StepHeight <= 0.01f || (DownCheckResult.GetCollisionPoint() - GlobalPosition).Y > MaxStepHeight)
            {
                StairAheadRaycast.GlobalPosition = DownCheckResult.GetCollisionPoint() + new Vector3 (0,MaxStepHeight,0) + ExpectedMoveMotion.Normalized() * 0.1f;
                StairAheadRaycast.ForceRaycastUpdate();
                if (StairAheadRaycast.IsColliding() && !IsSurfaceTooSteep(StairAheadRaycast.GetCollisionNormal())) {
                    GlobalPosition = StepPositionWithClearence.Origin + DownCheckResult.GetTravel();
                    ApplyFloorSnap();
                    SnappedToStairLastFrame = true;
                    return true;
                }
                return false;
            }
            return false;
        }
;
        return false;

    }
    public void SnapDownToStair()
    {
        bool DidSnap = false;
        StairBelowRaycast.ForceRaycastUpdate();
        bool FloorBelow = StairBelowRaycast.IsColliding() && IsSurfaceTooSteep(StairBelowRaycast.GetCollisionNormal());
        if (IsOnFloor() && velocity.Y == 0 & FloorBelow)
        {
            PhysicsTestMotionResult3D BodyTestResult = new PhysicsTestMotionResult3D();
            if (RunBodyTestMotion(GlobalTransform, new Vector3(0, -MaxStepHeight, 0), BodyTestResult))
            {
                float TranslateY = BodyTestResult.GetTravel().Y;
                //temp swap to evade .Y restriction on transform somewhat like how abc temp programs work
                Vector3 NewPosition = GlobalPosition;
                NewPosition.Y += TranslateY;
                GlobalPosition = NewPosition;
                ApplyFloorSnap();
                DidSnap = true;
            }
        }
        SnappedToStairLastFrame = DidSnap;
    }
    bool IsSurfaceTooSteep(Vector3 normal)
    {
        return normal.AngleTo(Vector3.Up) > FloorMaxAngle;
    }
    bool RunBodyTestMotion(Transform3D from, Vector3 motion, PhysicsTestMotionResult3D result = null)// bullshit code that does stuff required for body test motions
    {
        if (result == null) result = new PhysicsTestMotionResult3D();
        PhysicsTestMotionParameters3D param = new PhysicsTestMotionParameters3D();
        param.From = from;
        param.Motion = motion;
        return PhysicsServer3D.BodyTestMotion(GetRid(), param, result);
    }

    /*
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}*/
}
