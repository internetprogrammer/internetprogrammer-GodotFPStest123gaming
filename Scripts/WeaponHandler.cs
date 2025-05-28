using Godot;
using System;

public partial class WeaponHandler : Node3D
{
    [Export] public PackedScene weaponScene;
    public Weapon weapon;
    public override void _Ready()
    {
        Node3D weaponNode = weaponScene.Instantiate<Node3D>();
        AddChild(weaponNode);

        CallDeferred(nameof(InitWeapon), weaponNode);
    }

    private void InitWeapon(Node3D weaponNode)
    {
        Node weaponNode2 = weaponNode.GetChild(0);
        GD.Print("Type: " + weaponNode2.GetType().Name);

        weapon = weaponNode2 as Weapon;
        if (weapon != null)
        {
            weapon.DisablePhysics();
        }
        else
        {
            GD.PrintErr("Weapon is null after cast.");
        }
    }
}
