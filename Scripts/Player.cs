using Godot;
using Microsoft.VisualBasic;
using System;
using System.ComponentModel.Design;
// todo camera effects on movement,
public partial class Player : DamagableCharacter
{
	[Export] public float Speed = 300.0f;
	[Export] public float RunMultiplier = 2.0f;
	[Export] public float CrouchMultiplier = 0.4f;
	[Export] public float JumpVelocity = 4.5f;
	[Export] public int[] LockIDS = new int[] {0 };
	[Export] public float Sensitivity = 0.03f;
	private float RunMultiplierActual;
	private float CrouchMultiplierActual;

	[Export] public Camera3D camera;
	[Export] public Node3D head;
	[Export] public Node3D body;

	[Export] public float ThrowPower = 5.0f;
	[Export] public RayCast3D rayCast3DCheckForObject;
	[Export] public Node3D GrabPoint;

	public float weaponNum = 1;
	public int WeaponAmmo1 = 120;
	public int WeaponAmmo2 = 120;
	[Export] Weapon Weapon1;
	[Export] Weapon Weapon2;
	[Export] public Node3D WeaponNode;
	[Export] public Node3D WorldNode;
	[Export] public RayCast3D rayCast3DHeadAim;
	[Export] public float WeaponRotationAmount = 8;

	private Node3D GrabbedObject;
	public bool Grabbing = false;

	private const float HeadMovementAmount = 0.06f;
	private const float HeadMovementFrequency = 2.5f;
	private float HeadMovementTime;
	private float CameraRotationAmount = 0.02f;
	Vector2 inputDir;

	InputEventMouseMotion MouseMotion; // will need for the procedural sway for the weapons 

	private const float WeaponSwayAmount = 0.01f;
	private float WeaponSwayTime;
	private Vector3 WeaponOffset = Vector3.Zero;
	Vector2 MouseRotationValue = Vector2.Zero;
	[Export] public float LeanRotationAmount = 0.20f;
	[Export] public float LeanOffsetAmount = 0.3f;
	[Export] public float RegularFov = 90;
	[Export] public float ZoomFov = 45;
	float ActualLeanRotation;
	float ActualLeanOffset;

	//reload mechanics
	[Export] public float MinReloadHeight = -0.4f;
	[Export] public float MaxReloadHeight = 0.4f;
	[Export] public float ReloadHeight = -0.35f;
	[Export] public float CurrentReloadHeight =0;
	[Export] public float ReloadSentivity =0.006f;
	bool IsReloading =false;
	[Export] Godot.Label Crosshair;
	[Export] string NormalCrosshair = ".";
	[Export] string InteractCrosshair = "+";
	[Export] float JumpSwayAmount = 0.01f;
	public override void _Ready() {
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
	public override void _UnhandledInput(InputEvent @event) {
		if (@event is InputEventMouseMotion) {
			
			MouseMotion = (InputEventMouseMotion)@event;
			MouseRotationValue = new Vector2(MouseMotion.Relative.X, MouseMotion.Relative.Y);
			//translates mouse motion to camera movement

			if (!IsReloading)
			{
				body.RotateY(-MouseMotion.Relative.X * Sensitivity);
				head.RotateX(-MouseMotion.Relative.Y * Sensitivity);

			}
			head.Rotation = new Vector3(Math.Clamp(head.Rotation.X, Mathf.DegToRad(-80), Mathf.DegToRad(80)),0,head.Rotation.Z);
		}
	}
	// handle anything that has to do with weapons
	private void HandleWeapon(double delta)
	{
		// checks if the equiped weapon is null because
		// its optimal to be put here and looks better
		if ((weaponNum == 1 && Weapon1 == null) || (weaponNum == 2 && Weapon2 == null)) 
		{
			return;
		}

		Weapon[] Weapons = {Weapon1,Weapon2};
		if (Weapons[(int)weaponNum - 1].WeaponFireMode == Weapon.FireMode.SemiAuto) {
			if (Input.IsActionJustPressed("shoot") && !Grabbing && !IsReloading)
			{
				switch (weaponNum)
				{
					case 1:
						if (Weapon1 != null)
						{
							Weapon1.Shoot(rayCast3DHeadAim, this);
						}
						break;
					case 2:
						if (Weapon2 != null)
						{
							Weapon2.Shoot(rayCast3DHeadAim, this);
						}
						break;
				}
			}
		}
		if(Weapons[(int)weaponNum -1].WeaponFireMode == Weapon.FireMode.FullAuto){
				if (Input.IsActionPressed("shoot") && !Grabbing && !IsReloading)
				{
					switch (weaponNum)
					{
						case 1:
							if (Weapon1 != null)
							{
								Weapon1.Shoot(rayCast3DHeadAim, this);
							}
							break;
						case 2:
							if (Weapon2 != null)
							{
								Weapon2.Shoot(rayCast3DHeadAim, this);
							}
							break;
				}
				
			}

		}
		if (Input.IsActionPressed("throwAway") && !IsReloading)
		{

			switch (weaponNum) // throws the weapon and removes its reference
			{
				case 1:
						ThrowWeapon(Weapon1);
						Weapon1 = null;
					break;
				case 2:
						ThrowWeapon(Weapon2);
						Weapon2 = null;
					break;
			}
		}
		if (Input.IsActionPressed("reload"))
		{
			IsReloading = true;
			CurrentReloadHeight = Mathf.Lerp(CurrentReloadHeight,-MouseMotion.Relative.Y * ReloadSentivity,10*(float)delta);
			CurrentReloadHeight = Mathf.Clamp(CurrentReloadHeight, MinReloadHeight, MaxReloadHeight);
			if (CurrentReloadHeight <= ReloadHeight)
			{
				switch (weaponNum) //reloads ammo ⚠️ need to put weaponammo variable into the weapon itself
				{
					case 1:
						WeaponAmmo1 = Weapon1.Reload(WeaponAmmo1);
						break;
					case 2:
						WeaponAmmo2 = Weapon2.Reload(WeaponAmmo1);
						break;
				}
			}
		}
		else
		{
			IsReloading = false;
			CurrentReloadHeight = Mathf.Lerp(CurrentReloadHeight,0,10*(float)delta);
		}
	}
	// handle anything that has to do with interactions
	private void HandleInteraction(double delta)
	{
		UpdateCrosshair();
		if (GrabbedObject != null)
		{
			UpdateGrabbedObject();
			 
		}
		if (Input.IsActionJustPressed("retrieve") && !Grabbing && !IsReloading)
		{

			
			//uses metadata which is said by the respective scripts of each object 
			Node3D tempNode = (Node3D)GetColliderAsGD();
			if (tempNode != null)
			{
				if (tempNode.GetChild(0) is Weapon) //there sure shit is a better way to do this and im pretty sure this is the fgc id which will fuck up the game when i add new guns so this is a temporary fix
				{
					Weapon tempWeapon = tempNode.GetChild(0) as Weapon;
					WeaponTake(tempWeapon);
				}
				else if (tempNode is Door)
				{
					if (((Door)tempNode).CanBeInteracted)
					{
						((Door)tempNode).Interact(LockIDS);//the one is because that is the number of the static bbc or something
					}
				}
				else if (tempNode is Lever)
				{
					((Lever)tempNode).Interact();
				}
				else if (tempNode is Button)
				{
					((Button)tempNode).Interact();
				}
				else if (tempNode.GetChild(0) is InteractableObject)
				{
					((InteractableObject)tempNode.GetChild(0)).Interact();
				}
				else if (tempNode.GetChild(0) is RetrievableObject)
				{
					((RetrievableObject)tempNode.GetChild(0)).Pickup();
				}
				else if(GetColliderFromRB() != null)
				{
					GrabbedObject = GetColliderFromRB();
				}
			}
		}
		else if (Input.IsActionJustPressed("retrieve") && Grabbing)
		{
			GrabbedObject = null;
			Grabbing = false;

		}
		if(Input.IsActionPressed("zoom")){
			camera.Fov = Mathf.Lerp(camera.Fov,ZoomFov,10 *(float)delta) ;
		}
		else{
			camera.Fov = Mathf.Lerp(camera.Fov,RegularFov,10 *(float)delta) ;
		}


	}
	// handle anything that has to do with movement
	private void HandleMovement(Vector3 velocity, double delta)
	{
				// Handle Jump
		if (Input.IsActionJustPressed("jump") && CheckGrounded())
		{
			velocity.Y += JumpVelocity;
		}
		if (Input.IsActionPressed("run")) // || IsRunning == true for toggleable with the addition of a untoggle function
		{
			RunMultiplierActual = RunMultiplier;
		}
		else
		{
			RunMultiplierActual = 1.0f;
		}
		if (Input.IsActionPressed("crouch")) // || IsCrouching == true for toggleable with the addition of a untoggle function
		{
			CrouchMultiplierActual = CrouchMultiplier;
		}
		else
		{
			CrouchMultiplierActual = 1.0f;
		}
		// Add the gravity.
		if (!CheckGrounded())
		{
			velocity += GetGravity() * (float)delta;
		}


		Lean(delta);


		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		inputDir = Input.GetVector("left", "right", "up", "down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
	   HeadRotation(direction, delta);
		if (direction != Vector3.Zero)
		{
			//the regular movement function with small additions for spice in the game
			velocity.X = direction.X * Speed * RunMultiplierActual * CrouchMultiplierActual * (float)delta;
			velocity.Z = direction.Z * Speed * RunMultiplierActual * CrouchMultiplierActual * (float)delta;
		}
		else
		{
			//can be used for decreasing air time too
			velocity.X = Mathf.Lerp((float)velocity.X, (float)direction.X * Speed, (float)delta * 14.0f);
			velocity.Z = Mathf.Lerp((float)velocity.Z, (float)direction.Z * Speed, (float)delta * 14.0f);
		}
		Velocity = velocity; // formality for translating calculated velocity into actual velocity in the game

	}
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		HandleWeapon(delta);
		HandleInteraction(delta);
		HandleMovement(velocity,delta);
		HeadMovement(delta);
		WeaponSway(delta,velocity.Y);
		CheckFallDamage();		
		MoveAndSlide();
	}
	//Gets the object that is touched by the rayCast3DGrabbing to get for grabbing 
	private void Lean(double delta)
	{
		if (Input.IsActionPressed("leanleft"))
		{
			ActualLeanOffset = Mathf.Lerp(ActualLeanOffset, -LeanOffsetAmount, 10 * (float)delta);
			ActualLeanRotation = Mathf.Lerp(ActualLeanRotation, LeanRotationAmount, 10 * (float)delta);
		}
		else if (Input.IsActionPressed("leanright"))
		{
			ActualLeanOffset = Mathf.Lerp(ActualLeanOffset, LeanOffsetAmount, 10 * (float)delta);
			ActualLeanRotation = Mathf.Lerp(ActualLeanRotation, -LeanRotationAmount, 10 * (float)delta);
		}
		else
		{
			ActualLeanOffset = Mathf.Lerp(ActualLeanOffset,0,10*(float)delta);
			ActualLeanRotation = Mathf.Lerp(ActualLeanRotation, 0, 10 * (float)delta);
		}
			
	}
	private void HeadRotation(Vector3 direction, double delta)
	{
		camera.Rotation = new Vector3(camera.Rotation.X,
			camera.Rotation.Y,
			(float)Mathf.Lerp(camera.Rotation.Z, -inputDir.X * CameraRotationAmount, 10 * delta));
			head.Rotation = new Vector3(head.Rotation.X, head.Rotation.Y, ActualLeanRotation);
		//rotates head in the Z rotation with the value of velocity X lerped need to get relative X instead of X
	}

	//head movement so the player isnt a stiff hot dog moving forward and left
	private void HeadMovement(double delta)
	{
		if (CheckGrounded())
		{
			HeadMovementTime += (float)delta * Velocity.Length();
			head.Position = new Vector3(Mathf.Cos(HeadMovementTime * HeadMovementFrequency / 2) * HeadMovementAmount + ActualLeanOffset,
						Mathf.Sin(HeadMovementTime * HeadMovementFrequency) * HeadMovementAmount + 0.75f,
						0);
		}
	} // find a way to edit the transform for this
	private void WeaponSway(double delta, float YVelocity)
	{
		if (WeaponOffset == Vector3.Zero && (Weapon1 != null || Weapon2 != null))
		{
			switch (weaponNum) // throws the weapon and removes its reference
			{
				case 1:
					WeaponOffset = Weapon1.WeaponOffset;
					break;
				case 2:
					WeaponOffset = Weapon2.WeaponOffset;
					break;
			}
		}
		WeaponSwayTime += (float)delta * Velocity.Length();
		if (CheckGrounded())// so that no movement is done
		{
			WeaponNode.Position = new Vector3(-Mathf.Cos(WeaponSwayTime * HeadMovementFrequency / 2) * WeaponSwayAmount,
					-Mathf.Sin(WeaponSwayTime * HeadMovementFrequency) * WeaponSwayAmount + CurrentReloadHeight,
					0) + WeaponOffset;
		}
		else
		{
			WeaponNode.Position.Lerp(new Vector3(0, CurrentReloadHeight, 0) + WeaponOffset,10*(float)delta);
		}
		WeaponNode.Position.Lerp(WeaponOffset, 10 * (float)delta);
		if (IsReloading) return;
		MouseRotationValue.Lerp(Vector2.Zero, 10 * (float)delta);
		WeaponNode.Rotation = new Vector3((float)Mathf.Lerp(WeaponNode.Rotation.X, MouseRotationValue.Y * WeaponRotationAmount * 1.5f - (YVelocity * JumpSwayAmount), 10 * delta),
			(float)Mathf.Lerp(WeaponNode.Rotation.Y, MouseRotationValue.X * WeaponRotationAmount * 1.5f, 10 * delta),
			(float)Mathf.Lerp(WeaponNode.Rotation.Z, -inputDir.X * WeaponRotationAmount * 50, 10 * delta));

	}

	// WIP
	public void WeaponTake(Weapon tempWeapon) {
		if (weaponNum == 1)
		{
			if (Weapon1 != null) {

				SwapWeapon(Weapon1 ,tempWeapon);
			}
			Weapon1 = tempWeapon;
			ReadyGun(Weapon1);
		}
		else {
			if (Weapon1 != null)
			{
				SwapWeapon(Weapon2, tempWeapon);
			}
			Weapon2 = tempWeapon;
			ReadyGun(Weapon2);
		}
	}
	public void ReadyGun(Weapon weapon)// probably will become an on tick thing since ill need to do weapon sway and such
	{
		weapon.DisablePhysics();
		weapon.GetParent<Node3D>().GetParent().RemoveChild(weapon.GetParent<Node3D>());
		WeaponNode.AddChild(weapon.GetParent<Node3D>());
		weapon.GetParent<Node3D>().GlobalPosition = WeaponNode.GlobalPosition;
		weapon.GetParent<Node3D>().GlobalRotation = WeaponNode.GlobalRotation;
	}
	public void SwapWeapon(Weapon weaponCurrent, Weapon weaponGrabbed)
	{
		if (weaponCurrent != null)
		{
			WeaponNode.RemoveChild(weaponCurrent.GetParent<Node3D>());
			WorldNode.AddChild(weaponCurrent.GetParent<Node3D>());
		}
		weaponCurrent.GetParent<Node3D>().GlobalPosition = weaponGrabbed.GetParent<Node3D>().GlobalPosition;
		weaponCurrent.GetParent<Node3D>().GlobalRotation = weaponGrabbed.GetParent<Node3D>().GlobalRotation;

		weaponCurrent.EnablePhysics();
	}
	public void ThrowWeapon(Weapon weapon)
	{
		weapon.EnablePhysics();
		WeaponNode.RemoveChild(weapon.GetParent<Node3D>());                                           
		WorldNode.AddChild(weapon.GetParent<Node3D>());// gets the rigidbody parent and does the physics stuff there instead of the fgc to first avoid errors and secondly to actually work since it the rigidbody doesnt like to coexist with the fgc9 script

		weapon.GetParent<Node3D>().GlobalPosition = GrabPoint.GlobalPosition;


		weapon.RigidBody.LinearVelocity = -head.GlobalTransform.Basis.Z * ThrowPower * 10f;
		weapon.RigidBody.AngularVelocity = -head.GlobalTransform.Basis.Y * ThrowPower * 20;
		weapon.GetParent<Node3D>().RotateZ(head.GlobalRotation.Z + 90f);
	}// !!!!!!!!!!!!!!!!!!! issues with gun phasing through floor

	private Vector3 GetColliderHead()
	{
		if (rayCast3DHeadAim.IsColliding())//raycast to check if grounded
		{
			return rayCast3DHeadAim.GetCollisionPoint();
		}
		return Vector3.Zero;
	}
	private Node3D GetColliderFromRB()
	{
		if (rayCast3DCheckForObject.IsColliding())
		{
			if (rayCast3DCheckForObject.GetCollider().IsClass("RigidBody3D")) //only rigid bodies should be grabbable, this eliminates everything else
			{
				Grabbing = true;// sets the grabbing boolean to true so that grabbing is solely toggleable
				return (Node3D)rayCast3DCheckForObject.GetCollider();// gets the collision object and then makes it a node3d
			}
		}
		return null;
	}
	private Node3D CheckColliderFromRB()
	{
		if (rayCast3DCheckForObject.IsColliding())
		{
			if (rayCast3DCheckForObject.GetCollider().IsClass("RigidBody3D")) //only rigid bodies should be grabbable, this eliminates everything else
			{
				return (Node3D)rayCast3DCheckForObject.GetCollider();// gets the collision object and then makes it a node3d
			}
		}
		return null;
	}
	private GodotObject GetColliderAsGD()
	{
		if (rayCast3DCheckForObject.IsColliding())
		{
			return rayCast3DCheckForObject.GetCollider();
		}
		return null;
	}
	private void UpdateGrabbedObject()
	{
		Vector3 LastPosition = GrabbedObject.GlobalPosition; // gets the previous frame from the object to use for calculating the velocity when letting go
		if(GrabbedObject is Damagable) { GD.Print("Fix me"); return; }//⚠️⚠️⚠️⚠️⚠️⚠️⚠️I MADE THIS TO STOP THE OBJECT FROM TRYING TO CAST ITSELF FROM DAMAGABLE AND DESTROYING THE SCRIPT,FIX IMMEDIATELY⚠️
		RigidBody3D rb = (RigidBody3D)GrabbedObject;// gets the rigidbody from said object
		GrabbedObject.GlobalRotation = head.GlobalRotation;                                         //GrabbedObject.Position = GrabPoint.GlobalPosition;// puts the grabbed object in the location of the grab node
		rb.LinearVelocity = (GrabPoint.GlobalPosition - GrabbedObject.GlobalPosition) * 13;
		if (Input.IsActionJustPressed("throw"))// this is what allows throwing
		{
			rb.LinearVelocity = (GrabbedObject.GlobalPosition - LastPosition) * 13 - head.GlobalTransform.Basis.Z * (ThrowPower* 2); // gets the head forward direction so that both x and y are calculated since the y is used only in the head object in order not to have the player do unwanted gymnastics
			GrabbedObject = null;// removes the object from the player so it doesnt return
			Grabbing = false; // this is used to untoggle grabbing
		}
		else if (Input.IsActionJustPressed("grab")) //this is what allows for untoggling
		{
			rb.LinearVelocity = (GrabbedObject.GlobalPosition - LastPosition) * 13; // gets the current position minus the old position to calculate the direction and multiplies to add more force to it
			GrabbedObject = null;// removes the object from the player so it doesnt return
			Grabbing = false;// this is used to untoggle grabbing
		}
	}
	public void UpdateCrosshair()
	{
		Node3D tempNode = (Node3D)GetColliderAsGD();	
		if (tempNode != null)
		{
			if (tempNode.GetChild(0) is Weapon ||  tempNode.GetChild(0) is InteractableObject || tempNode.GetChild(0) is RetrievableObject || tempNode is Lever || tempNode is Button || CheckColliderFromRB() != null)
			{

				Crosshair.Text = InteractCrosshair;
			}
			else if (tempNode is Door)
			{
				if((tempNode as Door).CanBeInteracted)
				{
					Crosshair.Text = InteractCrosshair;
				}
			}
			else
			{
				Crosshair.Text = NormalCrosshair;
			}
		}
		else
		{
			Crosshair.Text = NormalCrosshair;
		}

	}
}
