using Godot;
using System;

public partial class Lever : InteractableObject
{

    [Export] AnimationPlayer AnimationPlayer;


    public new void Activate()
    {
        Action();
        AnimationPlayer.Play("TurnLeverOn");
    }
    public new void Deactivate()
    {
        Action();
        AnimationPlayer.PlayBackwards("TurnLeverOn");
    }
    public new void Interact()
    {
        if(OneShot && ClickedOnce) { return; }
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
        ClickedOnce = true;
    }


}
