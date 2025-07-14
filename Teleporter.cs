using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class Teleporter : Area3D
{
    [Export] public Vector3 TeleportPosition = Vector3.Zero;
    void _func_godot_apply_properties(Dictionary Properties)
    {
        GD.Print(this.Name);
        TeleportPosition = (Vector3)Properties["teleportlocation"];
        foreach (var key in Properties.Keys)
        {
            GD.Print($"{key} = {Properties[key]}");
        }
    }
    public override void _Ready()
    {

        if (Engine.IsEditorHint()) return; //prevents from running in editor due to [tool]
        CollisionShape3D CollisionShape = (GetChild(0) as CollisionShape3D);
        Monitoring = true;
        Monitorable = true;
        BodyEntered += PlayerEntered;
    }
    private void PlayerEntered(Node3D body)
    {

        if (Engine.IsEditorHint()) return; //prevents from running in editor due to [tool]
        if (body is Player player)
            {
                player.GlobalPosition = TeleportPosition;
            }
    }
}
