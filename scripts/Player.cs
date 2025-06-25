using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export]
    public int Speed { get; set; } = 50;
    // The downward acceleration when in the air, in meters per second squared.
    [Export]
    public int FallAcceleration { get; set; } = 75;

    [Export]
    public float MouseSensitivty { get; set; } = 0.01f;
    private Vector3 _targetVelocity = Vector3.Zero;

    private Vector2 _rotation = new Vector2();

    private bool _movementDisabled = false;

    private Camera3D _camera;

    private Node3D _pivot;

    private Vector3 _up = Vector3.Up;

    private Node3D _planet;


    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        _camera = GetNode<Camera3D>("./Pivot/MainCamera");
        _pivot = GetNode<Node3D>("Pivot");
        MotionMode = MotionModeEnum.Floating;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("UnlockMouse"))
        {
            _movementDisabled = !_movementDisabled;
            if (_movementDisabled)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }

        }

        if (!_movementDisabled)
        {
            if (@event is InputEventMouseMotion motionEvent)
            {
                Vector2 mouseMovement = motionEvent.ScreenRelative;
                _rotation.X = -mouseMovement.X * MouseSensitivty;
                _rotation.Y = -mouseMovement.Y * MouseSensitivty;
                // Transform3D t = _pivot.Transform;
                // t.Basis = Basis.Identity;
                // _pivot.Transform = t;
                _pivot.Rotate(Vector3.Up, _rotation.X);
                _camera.RotateX((float)Mathf.Clamp(_rotation.Y, -Math.PI / 2, Math.PI / 2));
            }
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        if (!_movementDisabled)
        {
            Vector3 direction = GetDirection();

            if (MotionMode == MotionModeEnum.Grounded)
            {
                _up = -(_planet.GlobalPosition - GlobalPosition).Normalized();
                // UpDirection = _up;
                // GD.Print("Up: ", _up);

                if (direction != Vector3.Zero)
                {
                    GD.Print("Direction before Adjustment: ", direction);
                    var newZ = -_camera.GlobalBasis.Z.Slide(_up).Normalized();
                    var newX = newZ.Cross(_up).Normalized();
                    direction = (newX * direction.X + -newZ * direction.Z).Normalized();
                    //direction = (GlobalBasis.X * direction.X + GlobalBasis.Z * direction.Z).Normalized();
                    GD.Print("Direction after Adjustment: ", direction);
                    // direction = direction.Rotated(_up, _pivot.Rotation.Y);
                    // direction = new Vector3(direction.X, 0, direction.Y);

                    // GD.Print("Velocity before Gravity: ", Velocity);
                    // GD.Print("Gravity vector: ", GetGravity());
                    _targetVelocity = direction * Speed;
                    Velocity = _targetVelocity;
                    // var newY = GlobalPosition.Normalized();
                    // var newX = newY.Cross(GlobalBasis.Z);
                    // var newZ = newX.Cross(newY);
                    // Basis b = new Basis(newX, newY, newZ).Orthonormalized();
                    // GlobalBasis = b;
                    //UpDirection = GlobalBasis.Y;
                    RotatePlayer((float)delta);
                }
                else
                {
                    Velocity = Vector3.Zero;
                }

                // GD.Print("Character Up Dir:", UpDirection);
                if (!IsOnWall())
                    Velocity += _up * GetGravity().Y * (float)delta;
                // var up = -GetGravity().Normalized();
                // var globalForward = Vector3.Forward;
                // var right = Vector3.Forward.Cross(up).Normalized();
                // var forward = up.Cross(right).Normalized();
                // var a = new Quaternion(Basis);
                // var b = new Quaternion(new Basis(right, forward, up));

                // var c = a.Slerp(b, 1f);

                // Basis = Basis.LookingAt(globalForward, _up);
                // GD.Print("Velocity After: ", Velocity);
                //Velocity = _targetVelocity;


            }
            else
            {
                if (direction != Vector3.Zero)
                {
                    direction = direction.Normalized();
                    // _pivot.Basis = Basis.LookingAt(direction);
                    _targetVelocity = (_camera.GlobalBasis * direction) * Speed;
                    Velocity = _targetVelocity;
                    RotatePlayer((float)delta);
                }
            }

        }

        // var collision = MoveAndCollide(_targetVelocity * (float)delta);
        // if (collision != null)
        // {
        //     var collider = (Node)collision.GetCollider();
        //     var cParent = (Node3D)collider.GetParent();
        //     // GD.Print("Collided with(parent): ", cParent.Name);
        //     Velocity = Velocity.Slide(collision.GetNormal());
        //     if (cParent.Name == "Earth" && MotionMode == MotionModeEnum.Floating)
        //     {
        //         MotionMode = MotionModeEnum.Grounded;
        //         Velocity = Vector3.Zero;
        //         _up = -(cParent.GlobalPosition - GlobalPosition).Normalized();
        //         GD.Print("Motion mode set to grounded.");
        //         ApplyFloorSnap();

        //     }
        // }
        MoveAndSlide();
        for (int i = 0; i < GetSlideCollisionCount() - 1; i++)
        {
            var collision = GetSlideCollision(i);
            var collider = (Node)collision.GetCollider();
            var cParent = (Node3D)collider.GetParent();
            GD.Print("Collided with(parent): ", cParent.Name);
            // Velocity = Velocity.Slide(collision.GetNormal());
            if (cParent.Name == "Earth" && MotionMode == MotionModeEnum.Floating)
            {
                MotionMode = MotionModeEnum.Grounded;
                Velocity = Vector3.Zero;
                _up = -(cParent.GlobalPosition - GlobalPosition).Normalized();
                _planet = cParent;
                _camera.Basis = Basis.Identity;
                GD.Print("Motion mode set to grounded.");
                ApplyFloorSnap();
            }
        }
    }

    private void RotatePlayer(float delta)
    {
        Transform3D target = new Transform3D();
        target.Origin = GlobalPosition;
        Vector3 left = _up.Cross(GlobalBasis.Z).Normalized();
        // Vector3 left = GlobalBasis.Z.Cross(_up).Normalized();
        Vector3 z = GlobalTransform.Basis.Z;
        target.Basis = new Basis(left, _up, z).Orthonormalized();
        Quaternion currentRotation = GlobalBasis.GetRotationQuaternion().Normalized();
        Quaternion targetRotation = target.Basis.GetRotationQuaternion().Normalized();

        Quaternion r = currentRotation.Slerp(targetRotation, 1f).Normalized();
        GlobalBasis = new Basis(r);
    }

    private Vector3 GetDirection()
    {

        Vector3 direction = Vector3.Zero;
        if (Input.IsActionPressed("MoveLeft"))
        {
            direction.X -= 1.0f;
        }
        if (Input.IsActionPressed("MoveRight"))
        {
            direction.X += 1.0f;
        }
        if (Input.IsActionPressed("MoveForward"))
        {
            direction.Z -= 1.0f;
        }
        if (Input.IsActionPressed("MoveBackward"))
        {
            direction.Z += 1.0f;
        }
        if (Input.IsActionPressed("MoveUp") && MotionMode != MotionModeEnum.Grounded)
        {
            direction.Y += 1.0f;
        }
        if (Input.IsActionPressed("MoveDown") && !IsOnWall())
        {
            direction.Y -= 1.0f;
        }
        return direction.Normalized();
    }

}
