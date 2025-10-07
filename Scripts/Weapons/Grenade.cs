using Godot;

public partial class Grenade : RigidBody3D
{
    [Export] public int Damage = 200;
    [Export] public int Penetration = 0;

    [Export] public float range = 25f;
    [Export] public float GrenadeTime = 5;
    [Export] public float Force = 20;
    [Export] public AudioStreamPlayer3D ExplosionSound;
    Timer timer;
    Timer DecayTimer;
    [Export] public Node3D ModelNode;// deletes the grenade visibly to the player while the decay timer removes the grenade so that the sound can play
    [Export] GpuParticles3D Effect;
    public override void _Ready()
    {
        timer = new Timer();
        DecayTimer = new Timer();
        AddChild(DecayTimer);
        AddChild(timer);
        timer.Timeout += GrenadeTimeout;
        DecayTimer.Timeout += DecayTimeout;
        timer.Start(GrenadeTime);
    }

    private void DecayTimeout()
    {
        QueueFree();
    }

    public void GrenadeTimeout()
    {
        Explode();
        timer.Stop();

    }
    public void Explode()
    {
        ExplosionSound.Play();
        Node3D player = Global.Player;  
        Effect.LookAt(player.GlobalPosition);
        Effect.Emitting = true;
        explosion Explosion = new explosion(range);
        AddChild(Explosion);
        Explosion.ParentNode = ModelNode;
        Explosion.Damage = Damage;
        Explosion.Penetration = Penetration;
        Explosion.Force = Force;
        DecayTimer.Start(3);
        //ExplosionInstance.Range = range;
    }
}
