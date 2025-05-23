using Godot;
using System;
using static Godot.TextServer;

public partial class DamagableCharacter : CharacterBody3D
{
    [Export] public float Health = 100;
    [Export] public float Armour = 0;
	[Export] public int FallDamageLimit = 150;
	[Export] public int FallDamageAmount = 5;
    [Export] public RayCast3D rayCast3DGrounded;
    public Vector3 velocity;
	public Damagable DamageHandler = new Damagable();
	float FallingVariable =0;
	int init =0;
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
    }
	public void CheckFallDamage()
	{
		if (!CheckGrounded())
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
    public bool CheckGrounded()
    {
        if (rayCast3DGrounded.IsColliding())//raycast to check if grounded
        {
            return true;
        }
        return false;
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
