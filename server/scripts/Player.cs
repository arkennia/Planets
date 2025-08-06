using System;
using Godot;
using Planets.SystemGenerator;
using Planets.SystemGenerator.Terrain;

namespace Planets;

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
    private Camera3D _camera;
    private Node3D _pivot;

    public Player()
    {

    }

    public Player(long id)
    {
        MultiplayerId = id;
    }

    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("./Pivot/MainCamera");
        _pivot = GetNode<Node3D>("Pivot");
        MotionMode = MotionModeEnum.Floating;
        FloorSnapLength = 0.5f;
        ProcessMode = ProcessModeEnum.Disabled;
        SetPhysicsProcess(false);
    }

    public void Spawn(Terrain3D.SpawnPoint sp, PlanetNode planet)
    {
        DisableMovement();
        GlobalPosition = sp.Node.GlobalPosition;
        Vector3 dir = sp.Normal; //-(planet.GlobalPosition - GlobalPosition).Normalized();
        _up = UpDirection = dir;
        _ChangeMotionMode(planet, sp.Normal);
        // float angle = GlobalPosition.AngleTo(_up);
        ProcessMode = ProcessModeEnum.Pausable;
        SetPhysicsProcess(true);
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
}