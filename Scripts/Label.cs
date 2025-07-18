using Godot;
using System;

public partial class Label : Godot.Label
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        this.Text = Engine.GetFramesPerSecond().ToString();
        //fast exit from window
        if (Input.IsActionPressed("quit"))
        {
            GetTree().Quit();
        }
    }
}
