using Godot;
using System;

public partial class RetrievableObjectRB : RigidBody3D
{
	public bool retrievable = true;
	//split into weapons and pickups that unlock stuff
	public void Pickup()
	{
		this.QueueFree();
	}
}
