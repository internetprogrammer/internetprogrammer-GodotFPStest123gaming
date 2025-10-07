using Godot;

public partial class Alert : Area3D
{
    public float Range = 5f;
    [Export] Timer timerDelay;
    [Export] public bool OneShot = false;

    public override void _Ready()
    {
        CollisionShape3D CollisionShape = (GetChild(0) as CollisionShape3D);
        CollisionShape.Scale = Vector3.One * Range;
        Monitoring = true;
        Monitorable = true;
        if (OneShot)
        {
            timerDelay.Timeout += Trigger;
            timerDelay.Start(0.1f);
        }
    }
    public void Trigger()
    {
        var OverlapingBodies = GetOverlappingBodies();
        foreach (var body in OverlapingBodies)
        {
            GD.Print(body);
            if (body is Enemy)
            {
                Enemy target = (Enemy)body;
                target.CheckSound(GlobalPosition);

            }
            else if (body is Civilian)
            {
                Civilian target = (Civilian)body;
                target.Panic();// panicked state is used so that the civvy can panick for some time and then uncope himself after some time
            }
            if (OneShot)
            {
                QueueFree();
            }
        }
    }
}
