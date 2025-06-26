using Godot;
using System;

namespace Planets
{
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

        private Vector2 _rotation = new();

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

                    if (direction != Vector3.Zero)
                    {
                        // GD.Print("Direction before Adjustment: ", direction);
                        var newZ = -_camera.GlobalBasis.Z.Slide(_up).Normalized();
                        var newX = newZ.Cross(_up).Normalized();
                        direction = (newX * direction.X + -newZ * direction.Z).Normalized();
                        // GD.Print("Direction after Adjustment: ", direction);
                        _targetVelocity = direction * Speed;
                        Velocity = _targetVelocity;
                        RotatePlayer((float)delta);
                    }
                    else
                    {
                        Velocity = Vector3.Zero;
                    }
                    if (!IsOnWall())
                        Velocity += _up * GetGravity().Y * (float)delta;
                }
                else
                {
                    if (direction != Vector3.Zero)
                    {
                        direction = direction.Normalized();
                        // _pivot.Basis = Basis.LookingAt(direction);
                        _targetVelocity = _camera.GlobalBasis * direction * Speed;
                        Velocity = _targetVelocity;
                        RotatePlayer((float)delta);
                    }
                }

            }
            MoveAndSlide();
            for (int i = 0; i < GetSlideCollisionCount() - 1; i++)
            {
                var collision = GetSlideCollision(i);
                var collider = (Node)collision.GetCollider();
                var cParent = collider.GetOwner<PlanetNode>();
                // GD.Print($"Collided with: {cParent}");
                if (cParent is PlanetNode && MotionMode == MotionModeEnum.Floating)
                {
                    MotionMode = MotionModeEnum.Grounded;
                    Velocity = Vector3.Zero;
                    _up = -(cParent.GlobalPosition - GlobalPosition).Normalized();
                    _planet = cParent.GetNode<MeshInstance3D>($"{cParent.Planet.Name}");
                    _camera.Basis = Basis.Identity;
                    GD.Print("Motion mode set to grounded.");
                    GD.Print($"{_planet.Name}");
                    ApplyFloorSnap();
                }
            }
        }

        private void RotatePlayer(float delta)
        {
            Transform3D target = new();
            target.Origin = GlobalPosition;
            Vector3 left = _up.Cross(GlobalBasis.Z).Normalized();
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
}