using Godot;
using Planets.UI;
using System;
using System.Threading.Tasks;

namespace Planets
{
    public partial class Player : CharacterBody3D
    {
        [Export]
        public int Speed { get; set; } = 20;
        [Export]
        public int JumpSpeed { get; set; } = 3;
        [Export]
        public float MouseSensitivty { get; set; } = 0.01f;

        private Vector3 _targetVelocity = Vector3.Zero;

        private Vector2 _rotation = new();

        private bool _movementDisabled = false;

        private Camera3D _camera;

        private Node3D _pivot;

        private Vector3 _up = Vector3.Up;

        private Node3D _planet;

        private float _gravity;

        private bool _isInAir = false;

        private bool _jumped = false;

        public override void _Ready()
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            _camera = GetNode<Camera3D>("./Pivot/MainCamera");
            _pivot = GetNode<Node3D>("Pivot");
            MotionMode = MotionModeEnum.Floating;
            _ = InitUiSignals();
        }

        private async Task InitUiSignals()
        {
            await ToSignal(GetNode<Node>("/root/Main"), Node.SignalName.Ready);
            UiManager.Instance.Ui.GameMenuOpened += DisableMovement;
            UiManager.Instance.Ui.GameMenuClosed += EnableMovement;
            GD.Print("Signals connected to UI.");
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

            Vector3 direction = GetDirection();

            if (MotionMode == MotionModeEnum.Grounded)
            {
                _up = -(_planet.GlobalPosition - GlobalPosition).Normalized();
                //UpDirection = _up;
                if (direction != Vector3.Zero)
                {
                    var newZ = -_camera.GlobalBasis.Z.Slide(_up).Normalized();
                    var newX = newZ.Cross(_up).Normalized();
                    direction = (newX * direction.X + _up * direction.Y + -newZ * direction.Z).Normalized();
                    _targetVelocity = direction * Speed;
                    if (_jumped)
                    {
                        _targetVelocity += _up * JumpSpeed;
                        _jumped = false;
                    }
                    Velocity = _targetVelocity;

                }
                else
                {
                    Velocity = Vector3.Zero;
                }
                RotatePlayer((float)delta);
                if (_isInAir)
                {
                    Velocity += (_planet.GlobalTransform.Origin - GlobalTransform.Origin).Normalized() * _gravity * 3f * (float)delta;
                }
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
            MoveAndSlide();
            for (int i = 0; i < GetSlideCollisionCount(); i++)
            {
                var collision = GetSlideCollision(i);
                var collider = (Node)collision.GetCollider();
                var cParent = collider.GetOwnerOrNull<PlanetNode>();
                if (cParent is not null && MotionMode == MotionModeEnum.Floating)
                {
                    MotionMode = MotionModeEnum.Grounded;
                    Velocity = Vector3.Zero;
                    _up = -(cParent.GlobalPosition - GlobalPosition).Normalized();
                    _planet = cParent;
                    _camera.Basis = Basis.Identity;
                    GD.Print("Motion mode set to grounded.");
                    GD.Print($"{_planet.Name}");
                    _gravity = cParent.PlanetArea.Gravity;
                    GD.Print($"Current gravity: {cParent.PlanetArea.Gravity} {cParent.PlanetArea.GravityDirection}");
                    //ApplyFloorSnap();
                }
                else if (MotionMode == MotionModeEnum.Grounded && cParent is PlanetNode && _isInAir)
                {
                    _isInAir = false;
                    GD.Print($"Is in air: {_isInAir}");
                }
            }
        }

        public void DisableMovement()
        {
            _movementDisabled = true;
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        public void EnableMovement()
        {
            _movementDisabled = false;
            Input.MouseMode = Input.MouseModeEnum.Captured;
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
            if (Input.IsActionPressed("MoveUp"))
            {
                if (MotionMode == MotionModeEnum.Grounded && !_isInAir)
                {
                    _isInAir = true;
                    GD.Print($"Is in air: {_isInAir}");
                    _jumped = true;
                    direction.Y += 1.0f;
                }
                else if (MotionMode == MotionModeEnum.Floating)
                {
                    direction.Y += 1.0f;
                }
                else
                {
                    direction.Y += 0.0f;
                }
            }
            if (Input.IsActionPressed("MoveDown") && !IsOnWall())
            {
                direction.Y -= 1.0f;
            }
            return direction.Normalized();
        }
    }
}