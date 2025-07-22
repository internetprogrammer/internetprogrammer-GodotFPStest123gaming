using Godot;

public partial class Damagable : Node3D
{

    [Export] public float Health = 100;
    [Export] public float Armour = 0;
    public Vector3 velocity;
    [Export] public PackedScene Effect;
    [Export] public Vector3 EffectSpawnOffset = Vector3.Zero;
    [Export] public string EffectScenePath = "";

    public string SoundPath = "";
    public float VolumeDB = 10;
    public float MaxDB = 10;
    public float Pitch = 1;


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
        if (Effect != null)
        {
            GpuParticles3D e = Effect.Instantiate<GpuParticles3D>();
            GetNode<Node3D>("/root/World").AddChild(e);
            e.GlobalPosition = ((Node3D)GetParent()).GlobalPosition;
            e.Emitting = true;
        }
        else if (EffectScenePath != "")
        {
            Effect = GD.Load<PackedScene>(EffectScenePath);
            GpuParticles3D e = Effect.Instantiate<GpuParticles3D>();
            GetNode<Node3D>("/root/World").AddChild(e);
            e.GlobalPosition = ((Node3D)GetParent()).GlobalPosition;
            e.Emitting = true;
        }
        QueueFree();
    }
    public void KillTarget(Node3D character)
    {
        if (Effect != null) // if another object is giving the damagable an effect
        {

            Node3D player = GetNode<Node3D>("/root/World/Player");

            GpuParticles3D e = Effect.Instantiate<GpuParticles3D>();
            GetNode<Node3D>("/root/World").AddChild(e);
            e.GlobalPosition = ((Node3D)GetParent()).GlobalPosition;
            e.LookAt(player.GlobalPosition);
            e.Emitting = true;
        }
        else if (EffectScenePath != "") // if the damagable has its own effect
        {
            Effect = GD.Load<PackedScene>(EffectScenePath);
            GpuParticles3D e = Effect.Instantiate<GpuParticles3D>();
            GetNode<Node3D>("/root/World").AddChild(e);
            e.GlobalPosition = ((Node3D)GetParent()).GlobalPosition;
            e.Emitting = true;
        }
        if (GetParent() is Enemy enemy)
        {
            if (enemy.weaponHandler.HasWeapon) enemy.weaponHandler.DropWeapon();
        }
        character.QueueFree();
    }
}
