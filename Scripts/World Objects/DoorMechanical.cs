using Godot;
using Godot.Collections;

[Tool]
public partial class DoorMechanical : InteractableObject
{

    [Export] public bool CanBeInteracted = true;
    [Export] public int DoorID = 0;

    public Damagable DamageHandler;
    [Export] bool Damageable = false;
    [Export] public string EffectPath = "res://Prefabs/WoodParticle.tscn";

    [Export] public float Health = 100;
    [Export] public float Armour = 0;

    MeshInstance3D MeshInstance = null;
    CollisionShape3D Collision_Shape = null;

    [Export] public float RiseSpeed = 2.0f;
    bool Stop = true;
    float RiseCounter = 0;

    string SoundPath = "";
    float VolumeDB = 10;
    float MaxDB = 10;
    float Pitch = 1;

    public Timer DoorTimer;
    [Export] public float DoorTimerDelay = 5;

    void _func_godot_apply_properties(Dictionary Properties)
    {

        GD.Print(this.Name);
        Damageable = (bool)Properties["damagable"];
        Health = (int)Properties["health"];
        Armour = (int)Properties["armour"];
        RiseSpeed = (float)Properties["risespeed"];
        foreach (var key in Properties.Keys)
        {
            GD.Print($"{key} = {Properties[key]}");
        }
    }
    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return; //prevents from running in editor due to [tool]
        //INIT


        DoorTimer = new Timer();
        AddChild(DoorTimer);
        DoorTimer.Timeout += OnTimerTimeoutSignal;
        DoorTimer.OneShot = true;

        if (this.GetChild(0) is MeshInstance3D Mesh)
        {
            MeshInstance = Mesh;
        }
        if (this.GetChild(1) is CollisionShape3D Shape)
        {
            Collision_Shape = Shape;
        }
        if (MeshInstance == null || Collision_Shape == null) { return; }

        Transform3D Transform = MeshInstance.Transform;
        GlobalPosition -= new Vector3(MeshInstance.GetAabb().Position.X, 0, MeshInstance.GetAabb().Position.Z);
        Transform = Transform.Translated(new Vector3(MeshInstance.GetAabb().Position.X, 0, MeshInstance.GetAabb().Position.Z));

        MeshInstance.Transform = Transform;

        Collision_Shape.Transform = Transform;

        if (Damageable)
        {
            DamageHandler = new Damagable();
            AddChild(DamageHandler);
            DamageHandler.Health = Health;
            DamageHandler.Armour = Armour;
            DamageHandler.Effect = GD.Load<PackedScene>(EffectPath);
            DamageHandler.EffectSpawnOffset = new(MeshInstance.GetAabb().Size.X * 0.5f, 0f, MeshInstance.GetAabb().Size.Z * .5f);
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint()) return; //prevents from running in editor due to [tool] 
        if (!OnOff && !Stop)
        {
            if (MeshInstance.GetAabb().Size.X > MeshInstance.GetAabb().Size.Z)
            {
                Position += new Vector3(RiseSpeed * (float)delta, 0, 0);
            }
            else if (MeshInstance.GetAabb().Size.X < MeshInstance.GetAabb().Size.Z)
            {
                Position += new Vector3(0, RiseSpeed * (float)delta, 0);
            }
            else
            {
                Position += new Vector3(RiseSpeed * (float)delta, 0, RiseSpeed * (float)delta);
            }
            RiseCounter += RiseSpeed * (float)delta;
        }
        if (OnOff && !Stop)
        {
            if (MeshInstance.GetAabb().Size.X > MeshInstance.GetAabb().Size.Z)
            {
                Position -= new Vector3(RiseSpeed * (float)delta, 0, 0);
            }
            else if (MeshInstance.GetAabb().Size.X < MeshInstance.GetAabb().Size.Z)
            {
                Position -= new Vector3(0, RiseSpeed * (float)delta, 0);
            }
            else
            {
                Position -= new Vector3(RiseSpeed * (float)delta, 0, RiseSpeed * (float)delta);
            }
            RiseCounter += RiseSpeed * (float)delta;
        }
        if (RiseCounter > MeshInstance.GetAabb().Size.X * 1.5f && RiseCounter > MeshInstance.GetAabb().Size.Z * 1.5f)
        {
            RiseCounter = 0;
            Stop = true;
        }



    }
    public void Interact(int[] IDS)
    {
        if (DoorID != 0)
        {
            if (!CheckIDS(IDS))
            {
                return;
            }
        }

        Stop = !Stop;
        OnOff = !OnOff;
        DoorTimer.Start(DoorTimerDelay);

    }
    public void ObjectInteract()
    {

        if (OnOff == true)
        {

            OnOff = false;
            Activate();
        }
        else
        {
            OnOff = true;
            Deactivate();
        }

    }
    bool CheckIDS(int[] IDS)
    {
        foreach (int id in IDS)
        {
            if (id == DoorID)
            {
                return true;
            }
        }
        return false;

    }
    void OnTimerTimeoutSignal()
    {
        DoorTimer.Stop();
        Stop = !Stop;
        OnOff = !OnOff;
    }
}
