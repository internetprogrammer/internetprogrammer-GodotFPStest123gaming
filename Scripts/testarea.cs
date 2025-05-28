using Godot;
using System;

public partial class testarea : Area3D
{
    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = true;
        var OverlapingBodies = GetOverlappingBodies();
        foreach (var body in OverlapingBodies)
        {

        }
    }
}
