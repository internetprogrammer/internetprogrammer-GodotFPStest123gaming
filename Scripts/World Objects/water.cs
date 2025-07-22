using Godot;

public partial class water : Area3D
{
    MeshInstance3D MeshInstance;
    bool ILikeFog = true;
    public override void _Ready()
    {
        if (GetChild(0) is MeshInstance3D meshinstance)
        {
            MeshInstance = meshinstance;
            if (ILikeFog)
            {
                PackedScene fog = (PackedScene)GD.Load("res://Prefabs/WaterFog.tscn");
                FogVolume fogVolume = fog.Instantiate<FogVolume>();
                AddChild(fogVolume);
                fogVolume.Size = meshinstance.GetAabb().Grow(-0.25f).Size;
            }
        }
        BodyEntered += EnteredWater;
        BodyExited += LeftWater;
        Monitorable = true;
        Monitoring = true;
    }
    public void EnteredWater(Node3D node)
    {
        if (node is Player player)
        {
            player.IsUnderwater = true;

        }


    }
    public void LeftWater(Node3D node)
    {
        if (node is Player player)
        {
            player.LeaveWater();
        }
    }/*
    public bool ShouldDrawCameraUnderwater() {

        Camera3D camera = GetViewport().GetCamera3D();

        if (camera == null) return false;

        var aabb = GlobalTransform * MeshInstance.GetAabb().Grow(0.025f);

        if (!aabb.HasPoint(camera.GlobalPosition)) return false;
        // Don't draw multiple overlays at once, incase 2 water bodies overlap
        Player player = GetNode<Player>("/root/World/Player");
        foreach(var node in player.HeadShapeCast.GetCollider())

        if % CameraPosShapeCast3D.get_collider(i) == % SwimmableArea3D:
			return true;

    return false;
    }*/
}
