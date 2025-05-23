using Godot;
using System;

public partial class Door : InteractableObject
{

	[Export] AnimationPlayer AnimationPlayer;
	[Export] public bool CanBeInteracted = true;
	[Export] public int DoorID = 0;
	[Export] Timer DoorTimer;
	public Damagable DamageHandler ;
	[Export] bool Damageable = false;
    [Export] public float Health = 100;
    [Export] public float Armour = 0;
    public override void _Ready()
	{
		if (Damageable)
		{
			DamageHandler = new Damagable();
            DamageHandler.Health = Health;
            DamageHandler.Armour = Armour;
        }
		DoorTimer.Timeout += OnTimerTimeoutSignal;
	}
	public new void Activate()
	{
		AnimationPlayer.Play("Open");
	}
	public new void Deactivate()
	{
		AnimationPlayer.PlayBackwards("Open");
	}
	public new void Interact(int[] IDS)
	{
		if (DoorID != 0)
		{
			if (!CheckIDS(IDS))
			{
				return;
			}
		}
            if (OnOff == true)
            {

                OnOff = false;
                DoorTimer.Start();
                Activate();
            }
            else
            {
                OnOff = true;
                DoorTimer.Stop();
                Deactivate();
            }
        
	}
    public new void ObjectInteract()
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
			if(id == DoorID)
			{
				return true;
			}
		}
		return false;
		
	}
	void OnTimerTimeoutSignal()
	{
		DoorTimer.Stop();
		OnOff = true;
		Deactivate();
	}

}
