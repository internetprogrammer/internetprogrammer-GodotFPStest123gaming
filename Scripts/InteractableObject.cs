using Godot;
using System;

public partial class InteractableObject : Node3D
{
	public bool OnOff = true;
	[Export]public bool OneShot = false;
	public bool ClickedOnce = false;
	[Export] Door doorToOpen;//can be left empty
	public void Activate()
	{

	}
	public void Deactivate()
	{

	}
	public void Interact()
	{
		GD.Print("Interact");
	}
	public void Action()
	{
		if (doorToOpen != null)
		{
            GD.Print("action");
            doorToOpen.ObjectInteract();
		}

		//here any external event will be handled for example when this function starts if there is a door connected it will either open or close
	}
	}
