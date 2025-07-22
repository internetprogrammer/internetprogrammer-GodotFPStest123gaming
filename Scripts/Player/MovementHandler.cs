using Godot;

namespace CharacterControllerCSharp.Scripts.Player
{
    internal class MovementHandler
    {
        private const float VelocityLerpTimeValue = 14.0f;

        float JumpVelocity { get; set; }
        float Speed { get; set; }
        float RunMultiplier { get; set; }
        float CrouchMultiplier { get; set; }

        float RunMultiplierActual;
        float CrouchMultiplierActual;
        float ActualLeanOffset;
        float ActualLeanRotation;
        float LeanRotationAmount { get; set; }
        float LeanOffsetAmount { get; set; }
        float CameraRotationAmount { get; set; }

        Camera3D camera { get; set; }
        Node3D head { get; set; }
        public MovementHandler(float speed, float jumpvelocity, float runmultiplier, float crouchmultiplier, Camera3D Camera, Node3D Head, float leanRotationAmount, float leanOffsetAmount, float cameraRotationAmount)
        {
            Speed = speed;
            JumpVelocity = jumpvelocity;
            RunMultiplier = runmultiplier;
            CrouchMultiplier = crouchmultiplier;
            camera = Camera;
            head = Head;
            LeanRotationAmount = leanRotationAmount;
            LeanOffsetAmount = leanOffsetAmount;
            CameraRotationAmount = cameraRotationAmount;
        }

        public void HandleMovement(Vector3 velocity, double delta, bool IsOnFloor, Vector3 GetGravity, ref Vector2 inputDir, CharacterBody3D self)
        {
            // Handle Jump
            if (Input.IsActionJustPressed("jump") && IsOnFloor)
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
            // Lean(delta); // this used to be under the isonfloor check so if issues are caused think about this
            // Add the gravity.
            if (!IsOnFloor)
            {
                velocity += GetGravity * (float)delta;
            }

            // Get the input direction and handle the movement/deceleration.
            // As good practice, you should replace UI actions with custom gameplay actions.
            inputDir = Input.GetVector("left", "right", "up", "down");
            Vector3 direction = (self.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
            //HeadRotation(direction, delta);
            if (direction != Vector3.Zero)
            {
                //the regular movement function with small additions for spice in the game
                velocity.X = direction.X * Speed * RunMultiplierActual * CrouchMultiplierActual * (float)delta;
                velocity.Z = direction.Z * Speed * RunMultiplierActual * CrouchMultiplierActual * (float)delta;
            }
            else
            {
                //can be used for decreasing air time too
                velocity.X = Mathf.Lerp(velocity.X, direction.X * Speed, (float)delta * VelocityLerpTimeValue);
                velocity.Z = Mathf.Lerp(velocity.Z, direction.Z * Speed, (float)delta * VelocityLerpTimeValue);
            }

            self.Velocity = velocity; // formality for translating calculated velocity into actual velocity in the game

        }
        private void HeadRotation(Vector3 direction, double delta, ref Vector2 inputDir)
        {
            camera.Rotation = new Vector3(camera.Rotation.X,
                camera.Rotation.Y,
                (float)Mathf.Lerp(camera.Rotation.Z, -inputDir.X * CameraRotationAmount, 10 * delta));

            head.Rotation = new Vector3(head.Rotation.X, head.Rotation.Y, ActualLeanRotation);

            //rotates head in the Z rotation with the value of velocity X lerped need to get relative X instead of X
        }
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
                ActualLeanOffset = Mathf.Lerp(ActualLeanOffset, 0, 10 * (float)delta);
                ActualLeanRotation = Mathf.Lerp(ActualLeanRotation, 0, 10 * (float)delta);
            }

        }
    }
}
