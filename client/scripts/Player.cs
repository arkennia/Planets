using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Google.Protobuf;
using Planets.SystemGenerator;
using Planets.SystemGenerator.Terrain;
using Planets.UI;
using Planets.Util;

namespace Planets;

[GlobalClass]
/// <summary>
/// Player inherits CharacterBody3D and is the controller for the player character.
/// </summary>
public partial class Player : CharacterBody3D
{
    /// <summary>
    /// Speed of the player.
    /// </summary>
    [Export]
    public int Speed { get; set; } = 20;

    /// <summary>
    /// Upward momentary speed when jumping.
    /// </summary>
    [Export]
    public int JumpSpeed { get; set; } = 8;

    [Export]
    public float MouseSensitivty { get; set; } = 0.005f;

    public Guid Uuid { get; set; }
    public float Gravity { get; set; }
    public PlanetNode Planet { get; set; }

    public PlayerData PlayerData { get; set; }

    private Vector3 _targetVelocity = Vector3.Zero;

    private Vector2 _rotation = new();

    private bool _movementDisabled = false;

    private Camera3D _camera;

    private Node3D _pivot;

    private Vector3 _up = Vector3.Up;

    private bool _isInAir = false;

    private bool _jumped = false;

    public Player()
    {
        // if (!FileAccess.FileExists("user://uuid")) {
        //     FileAccess file = FileAccess.Open("user://uuid", FileAccess.ModeFlags.Write);
        //     file.StoreLine(Guid.NewGuid().ToString());
        //     file.Close();
        // } else {
        //     FileAccess file = FileAccess.Open("user://uuid", FileAccess.ModeFlags.Read);
        //     Uuid = Guid.Parse(file.GetLine());
        //     file.Close();
        // }
    }

    public override void _Ready()
    {
        
        Input.MouseMode = Input.MouseModeEnum.Captured;
        _camera = GetNode<Camera3D>("./Pivot/MainCamera");
        _pivot = GetNode<Node3D>("Pivot");
        MotionMode = MotionModeEnum.Floating;
        _ = InitUiSignals();
        FloorSnapLength = 0.5f;
        ProcessMode = ProcessModeEnum.Disabled;
        SetPhysicsProcess(false);
    }

    /// <summary>
    /// Spawn the character at <paramref name="coords"/>.
    /// </summary>
    /// <remarks><paramref name="coords"/> is in Global coordinates.
    /// <param name="coords">The spawn location.</param>
    public void Spawn(/* Terrain3D.SpawnPoint sp, PlanetNode planet */)
    {
        DisableMovement();
        GlobalPosition = PlayerData.SpawnPosition;
        _up = UpDirection = PlayerData.Up;
        PlanetNode p = GameManager.Planets[PlayerData.SpawnPlanet];
        _ChangeMotionMode(p, PlayerData.Up);
        // float angle = GlobalPosition.AngleTo(_up);
        ProcessMode = ProcessModeEnum.Pausable;
        SetPhysicsProcess(true);
    }

    private async Task InitUiSignals()
    {
        await ToSignal(GetTree().Root.GetNode<Node>("/root/Main"), Node.SignalName.Ready);
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
                Input.MouseMode = Input.MouseModeEnum.Visible;
            else
                Input.MouseMode = Input.MouseModeEnum.Captured;
        }

        if (!_movementDisabled)
            if (@event is InputEventMouseMotion motionEvent)
            {
                Vector2 mouseMovement = motionEvent.ScreenRelative;
                _rotation.X = -mouseMovement.X * MouseSensitivty;
                _rotation.Y = -mouseMovement.Y * MouseSensitivty;
                float currentRotation = _camera.Rotation.X;
                _pivot.Rotate(Vector3.Up, _rotation.X);
                // Prevents the camera for doing a 360 spin and going upside down.
                if (Mathf.Abs(currentRotation + _rotation.Y) > MathF.PI / 2f)
                {
                    _rotation.Y = 0f;
                }
                else
                {
                    _camera.Rotate(Vector3.Right, _rotation.Y);
                }
            }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 direction = GetDirection();
        bool jumpedLocal = _jumped;
        _up = -(Planet.GlobalPosition - GlobalPosition).Normalized();
        _UpdateCoordsUI(Planet.CalculatePosition(GlobalPosition));
        if (MotionMode == MotionModeEnum.Grounded)
        {
            if (direction != Vector3.Zero)
            {
                // Translate the input direction(s) to actual world direction while on a planet.
                Vector3 newZ = -_camera.GlobalBasis.Z.Slide(_up).Normalized();
                Vector3 newX = newZ.Cross(_up).Normalized();
                direction = (newX * direction.X + _up * direction.Y + -newZ * direction.Z).Normalized();
                _targetVelocity = _targetVelocity.Lerp(direction * Speed, 0.8f * (float)delta);
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
            if (Planet is not null && !IsOnFloor())
                Velocity += -_up * Gravity * 50f * (float)delta;
            // Apply gravity when not on the ground.
        }
        else
        {
            // Control the floating movement when not tied to a planet.
            if (direction != Vector3.Zero)
            {
                direction = direction.Normalized();
                // _pivot.Basis = Basis.LookingAt(direction);
                _targetVelocity = _camera.GlobalBasis * direction * Speed * 2f;
                Velocity = _targetVelocity;
                RotatePlayer((float)delta);
                if (Planet is not null)
                    Velocity += -_up * Gravity * 50f * (float)delta;

            }
        }
        PlayerMovementProto movement = new()
        {
            CurrentGlobalPosition = ProtoUtils.GodotToProtoVector3(GlobalPosition),
            Rotation = ProtoUtils.GodotToProtoVector3(Rotation),
            Velocity = ProtoUtils.GodotToProtoVector3(Velocity),
            Up = ProtoUtils.GodotToProtoVector3(_up),
            MovementDirection = ProtoUtils.GodotToProtoVector3(direction),
            IsJumping = jumpedLocal,
            IsInAir = _isInAir
        };
        Networking.Instance.RpcId(1, Networking.MethodName.RequestMovement, movement.ToByteArray());
        MoveAndSlide();
        GlobalPosition += new Vector3(0, 1f, 0) * direction;

        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision = GetSlideCollision(i);
            Node collider = (Node)collision.GetCollider();
            PlanetNode cParent = collider.GetOwnerOrNull<PlanetNode>();
            _ChangeMotionMode(cParent, _up);
        }
        // Raycast for detecting where the ground is, and for calculating the ground normal. It then snaps the player to the floor.

        if (!_isInAir && MotionMode == MotionModeEnum.Grounded)
        {
            Vector3 dest = -_up * 100f;
            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(Position, dest);
            query.Exclude = [GetRid()];
            query.CollisionMask = CollisionMask;
            query.HitFromInside = true;
            Dictionary result = spaceState.IntersectRay(query);
            Control debugUI = GetNodeOrNull<Control>("%DebugUI");
            if (debugUI != null)
            {
                GetNode<Label>("%DebugUI/VBoxContainer/HBoxContainer/PlayerPosition").Text =
                    Position.ToString("F");
                GetNode<Label>("%DebugUI/VBoxContainer/HBoxContainer2/RayDest").Text = dest.ToString("F");
                GetNode<Label>("%DebugUI/VBoxContainer/HBoxContainer3/Result").Text = result.ToString();
                GetNode<Label>("%DebugUI/VBoxContainer/HBoxContainer4/Up").Text = _up.ToString("F");
            }

            if (result.Count > 0)
            {
                _up = UpDirection = (Vector3)result["normal"];
                ApplyFloorSnap();
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

    private void _UpdateCoordsUI(Vector2 p)
    {
        UiManager.Instance.Ui.UpdateCoords(p);
    }

    private void _ChangeMotionMode(PlanetNode node, Vector3 up)
    {
        if (node is not null && MotionMode == MotionModeEnum.Floating)
        {
            MotionMode = MotionModeEnum.Grounded;
            Velocity = Vector3.Zero;
            _up = up;
            Planet = node;
            _camera.Basis = Basis.Identity;
            GD.Print("Motion mode set to grounded.");
            GD.Print($"Current Planet GUID: {Planet.Name}");
            Gravity = node.PlanetArea.Gravity;
            GD.Print($"Current gravity: {node.PlanetArea.Gravity} {node.PlanetArea.GravityDirection}");
            // ApplyFloorSnap();
        }
        else if (MotionMode == MotionModeEnum.Grounded && node is not null && _isInAir)
        {
            _isInAir = false;
            // GD.Print($"Is in air: {_isInAir}");
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
        if (Input.IsActionPressed("MoveLeft")) direction.X -= 1.0f;
        if (Input.IsActionPressed("MoveRight")) direction.X += 1.0f;
        if (Input.IsActionPressed("MoveForward")) direction.Z -= 1.0f;
        if (Input.IsActionPressed("MoveBackward")) direction.Z += 1.0f;
        if (Input.IsActionPressed("MoveUp"))
        {
            if (MotionMode == MotionModeEnum.Grounded && !_isInAir)
            {
                _isInAir = true;
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

        if (Input.IsActionPressed("MoveDown") &&
            MotionMode != MotionModeEnum.Grounded) direction.Y -= 1.0f;
        return direction.Normalized();
    }
}