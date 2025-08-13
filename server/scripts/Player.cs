using System;
using Godot;
using Google.Protobuf;
using Planets.Server;
using Planets.SystemGenerator;
using Planets.SystemGenerator.Terrain;
using Planets.Util;

namespace Planets;

[GlobalClass]
public partial class Player : CharacterBody3D
{
    public readonly long MultiplayerId = 0;
    [Export]
    public float Gravity { get; set; }

    [Export]
    public PlanetNode Planet { get; set; }

    public PlayerData PlayerData { get; set; } = new();

    private Vector3 _up = Vector3.Up;
    private bool _isInAir = false;
    private bool _jumped = false;
    private Vector3 _targetVelocity = Vector3.Zero;
    private Vector2 _rotation = new();
    private bool _movementDisabled = false;
    private Node3D _camera;
    private Node3D _pivot;

    private bool _spawned = false;

    [Signal]
    private delegate void SpawnedEventHandler();

    public Player()
    {

    }

    public Player(long id)
    {
        MultiplayerId = id;
        bool onSpawn() => _spawned = true;
        Connect(SignalName.Spawned, Callable.From(onSpawn), (uint)ConnectFlags.OneShot);
    }

    public override void _Ready()
    {
        SetCollisionLayerValue(1, true);
        SetCollisionMaskValue(1, false);
        SetCollisionMaskValue(2, true);
        AddChild(new Node3D()
        {
            Name = "Pivot"
        });
        _pivot = GetNode<Node3D>("Pivot");
        _pivot.AddChild(new Node3D()
        {
            Name = "MainCamera"
        });
        _camera = GetNode<Node3D>("./Pivot/MainCamera");
        MotionMode = MotionModeEnum.Floating;
        FloorSnapLength = 0.5f;
        ProcessMode = ProcessModeEnum.Disabled;
        SetPhysicsProcess(false);
        Scale *= 0.1f;
    }

    public override void _Process(double delta)
    {
        if (_spawned)
        {
            ProcessMode = ProcessModeEnum.Pausable;
            SetPhysicsProcess(true);
            _spawned = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // _up = -(Planet.GlobalPosition - GlobalPosition).Normalized();

        RotatePlayer((float)delta);
        if (!_isInAir && MotionMode == MotionModeEnum.Grounded)
        {
            Vector3 dest = -_up * 100f;
            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(Position, dest);
            query.Exclude = [GetRid()];
            query.CollisionMask = CollisionMask;
            // query.HitFromInside = true;
            Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
            if (result.Count > 0)
            {
                _up = UpDirection = (Vector3)result["normal"];
                ApplyFloorSnap();
            }
        }
        MoveAndSlide();
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision = GetSlideCollision(i);
            Node collider = (Node)collision.GetCollider();
            PlanetNode cParent = collider.GetOwnerOrNull<PlanetNode>();
            _ChangeMotionMode(cParent, _up);
            GD.Print("Collided with: " + collider.Name);
        }

        PlayerMovementProto protomovement = new()
        {
            CurrentGlobalPosition = ProtoUtils.GodotToProtoVector3(GlobalPosition),
            Velocity = new(),
            Rotation = new(),
            Up = new()

        };

        byte[] bytes = protomovement.ToByteArray();
        Networking.Instance.Rpc(Networking.MethodName.SendMovement, MultiplayerId, bytes);
    }

    public void Spawn()
    {
        DisableMovement();
        GlobalPosition = PlayerData.SpawnPosition;
        //-(planet.GlobalPosition - GlobalPosition).Normalized();
        _up = UpDirection = PlayerData.Up;
        PlanetNode p = ServerManager.Planets[PlayerData.SpawnPlanet];
        _ChangeMotionMode(p, PlayerData.Up);
        // float angle = GlobalPosition.AngleTo(_up);
        // ProcessMode = ProcessModeEnum.Pausable;
        // SetPhysicsProcess(true);
        EmitSignal(SignalName.Spawned);
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

    public void RotatePlayer(float delta)
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
}