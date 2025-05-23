using Godot;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static System.Net.Mime.MediaTypeNames;

public partial class Bullet : RigidBody3D
{
    public int Damage = 0;
    public int Penetration = 0;
    [Export] bool DoesBulletStay;


    public override void _Ready()
    {
        this.BodyEntered += OnBodyEntered;
    }
    public override void _PhysicsProcess(double delta)
    {
    }
    private void OnBodyEntered(Node body)
    {
        if (body is Enemy)
        {
            Enemy Target = (Enemy)body;
            Target.DamageHandler.DamageTarget(Damage, Penetration, (Node3D)body);
            //Target.Velocity.X = Mathf.Lerp((float)velocity.X, (float)direction.X * Speed, (float)delta * 14.0f);
            //Target.Velocity.Z = Mathf.Lerp((float)velocity.Z, (float)direction.Z * Speed, (float)delta * 14.0f);
            //Target.Velocity = -GlobalTransform.Basis.X * (Damage * (Penetration + 1) * 5);
            QueueFree();
        }
        else if (body is RigidBody3D)
        {
            RigidBody3D Target2 = (RigidBody3D)body;
            QueueFree();
        }
        else if (body is Damagable)
        {
            Damagable Target3 = (Damagable)body;
            Target3.DamageTarget(Damage, Penetration);
            QueueFree();
        }
        else if (body is RigidBody3D && body is Damagable)
        {
            Damagable Target4 = (Damagable)body;
            RigidBody3D Target5 = (RigidBody3D)body;
            Target4.DamageTarget(Damage, Penetration);
            QueueFree();

        }
        else if (body is Door)
        {
            Door Target6 = (Door)body;
            if (Target6.DamageHandler == null) { QueueFree(); return; }
            Target6.DamageHandler.DamageTarget(Damage, Penetration, (Node3D)body);
            QueueFree();
        }
        if (body is Civilian)
        {
            Enemy Target = (Enemy)body;
            Target.DamageHandler.DamageTarget(Damage, Penetration, (Node3D)body);
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
