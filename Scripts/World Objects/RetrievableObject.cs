using Godot;

public partial class RetrievableObject : Node3D
{
    [Export] public bool DisappearsForeverAfterRetrieved = false;
    public bool Retrieved = false;
    [Export] public int money = 0;// zero for no money
                                  //split into weapons and pickups that unlock stuff
    public void Pickup()
    {
        Retrieved = true;
        if (money != 0)
        {
            GD.Print(money);
        }
        if (DisappearsForeverAfterRetrieved)
        {
            GD.Print("gone forever");
        }
        GetParent().QueueFree();
    }
    public override void _PhysicsProcess(double delta)
    {
    }

}
