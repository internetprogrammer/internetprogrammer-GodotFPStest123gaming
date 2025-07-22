using Godot;

public partial class WeaponHandler : Node3D
{
    [Export] public PackedScene weaponScene;
    public Weapon weapon;
    public bool HasWeapon = false;
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
            HasWeapon = true;
            weapon.DisablePhysics();
        }
        else
        {
            GD.PrintErr("Weapon is null after cast.");
        }
    }
    public void DropWeapon()
    {
        Node3D weaponParentNode = (Node3D)weapon.GetParent();
        Vector3 tempPos = weaponParentNode.GlobalPosition;
        weapon.EnablePhysics();
        weaponParentNode.GetParent().RemoveChild(weaponParentNode);
        GetNode<Node3D>("/root/World").AddChild(weaponParentNode);
        weaponParentNode.GlobalPosition = tempPos;
        weaponParentNode.RotateZ(80f);
    }
}
