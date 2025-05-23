using Godot;
using System;
using static Godot.TextServer;

public partial class Damagable : Node3D
{

    [Export]public float Health = 100;
    [Export]public float Armour = 0;
    public Vector3 velocity;


    public void DamageTarget(int damage, int penetration, Node3D character = null)
    {
        if (penetration >= Armour)
        {
            Health -= damage;
            GD.Print(Health);
            if (Health <= 0)
            {
                if (character == null)
                {
                    KillTarget();
                }
                else
                {
                    KillTarget(character);
                }
            }
        }
    }

    public void HealTarget(int heal)
    {
        Health += heal;
    }
    public void KillTarget()
    {
        QueueFree();
    }
    public void KillTarget(Node3D character)
    {
        character.QueueFree();
    }
}
