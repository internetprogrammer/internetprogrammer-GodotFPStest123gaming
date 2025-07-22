using Godot;

public partial class Bullet : RigidBody3D
{
    public int Damage = 0;
    public int Penetration = 0;
    [Export] bool DoesBulletStay;
    [Export] PackedScene BulletCrater;


    public override void _Ready()
    {
        this.BodyEntered += OnBodyEntered;
    }
    public override void _PhysicsProcess(double delta)
    {
    }
    private void OnBodyEntered(Node body)
    {
        if (body is Enemy enemy)
        {
            enemy.DamageHandler.DamageTarget(Damage, Penetration, (Node3D)body);
            //Target.Velocity.X = Mathf.Lerp((float)velocity.X, (float)direction.X * Speed, (float)delta * 14.0f);
            //Target.Velocity.Z = Mathf.Lerp((float)velocity.Z, (float)direction.Z * Speed, (float)delta * 14.0f);
            //Target.Velocity = -GlobalTransform.Basis.X * (Damage * (Penetration + 1) * 5);
            QueueFree();
        }
        else if (body is RigidBody3D)
        {
            QueueFree();
        }
        else if (body is Damagable damagable)
        {
            damagable.DamageTarget(Damage, Penetration);
            QueueFree();
        }
        else if (body is RigidBody3D && body is Damagable damagablerigid)
        {
            damagablerigid.DamageTarget(Damage, Penetration);
            QueueFree();

        }
        else if (body is Door door)
        {
            if (door.DamageHandler == null) { QueueFree(); return; }
            door.DamageHandler.DamageTarget(Damage, Penetration, (Node3D)body);
            QueueFree();
        }
        else if (body is DoorMechanical doormech)
        {
            if (doormech.DamageHandler == null) { QueueFree(); return; }
            doormech.DamageHandler.DamageTarget(Damage, Penetration, (Node3D)body);
            QueueFree();
        }
        if (body is Civilian civilian)
        {
            civilian.DamageHandler.DamageTarget(Damage, Penetration, (Node3D)body);
            //Target.Velocity.X = Mathf.Lerp((float)velocity.X, (float)direction.X * Speed, (float)delta * 14.0f);
            //Target.Velocity.Z = Mathf.Lerp((float)velocity.Z, (float)direction.Z * Speed, (float)delta * 14.0f);
            //Target.Velocity = -GlobalTransform.Basis.X * (Damage * (Penetration + 1) * 5);
            QueueFree();
        }
        if (DoesBulletStay)
        {
            Freeze = true;
            ((CollisionShape3D)GetChild(0)).SetDeferred("Disabled", true);
        }
        else
        {

            QueueFree();
        }


    }

}
