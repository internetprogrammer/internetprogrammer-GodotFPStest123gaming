using Godot;

public partial class Button : InteractableObject
{

    [Export] AnimationPlayer AnimationPlayer;

    [Export] Timer ButtonTimer;
    [Export] bool CanInteract = true;
    [Export] bool TemporaryButton = false; //ie will be active only when it is shifted once
    public override void _Ready()
    {
        ButtonTimer.Timeout += OnTimerTimeoutSignal;
    }
    public new void Activate()
    {
        AnimationPlayer.Play("Click");
    }
    public new void Deactivate()
    {
        AnimationPlayer.PlayBackwards("Click");
    }
    public new void Interact()
    {
        if (OneShot && ClickedOnce) { return; }
        if (TemporaryButton && CanInteract)
        {
            CanInteract = false;
            ButtonTimer.Start();
            Activate();
        }
        else if (!TemporaryButton)
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
            ClickedOnce = true;
        }


    }

    void OnTimerTimeoutSignal()
    {
        ButtonTimer.Stop();
        CanInteract = true;
        Deactivate();
    }

}
