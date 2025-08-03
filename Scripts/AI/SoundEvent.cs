using Godot;
using System;
using System.Diagnostics;

public partial class SoundEvent : Area3D
{


    //[Export] Timer timerDelay;

    public SoundEvent(float Range= 25f)
    {
        //CollisionShape3D CollisionShape = (GetChild(0) as CollisionShape3D);
        CollisionShape3D CollisionShape = new CollisionShape3D();
        SphereShape3D Shape = new SphereShape3D();
        Shape.Radius = Range;
        CollisionShape.Shape = Shape;
        AddChild(CollisionShape);
        Monitoring = true;
        Monitorable = true;
    }

    public Node3D Check()
    {
        var OverlapingBodies = GetOverlappingBodies();
        foreach (var body in OverlapingBodies)
        {
            if (body is Enemy enemy)
            {
                enemy.HandleSound(GlobalPosition);
            }
            if(body is Civilian civilian)
            {
                civilian.Panic();
            }
        }
        return null;
    }
}
