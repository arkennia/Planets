using Godot;
using Planets.SystemGenerator;
using System;

namespace Planets;

[GlobalClass]
public partial class Ship : CharacterBody3D
{
	[Export]
	public double Speed { get; set; } = 5.0;
	[Export]
	public double JumpVelocity { get; set; } = 4.5;
	[Export]
	public PlanetNode Planet { get; set; } = null;
	[Export]
	public double PlanetGravity { get; set; } = 0.0;
	[Export]
	public double ShipGravity { get; set; } = 9.8;
	[Export]
	public Node3D SpawnPoint { get; set; } = null;
	[Export]
	public Area3D ShipArea { get; set; } = null;

	public Vector3 Up => _up;

	private bool _hasPilot = false;

	private Vector3 _up = Vector3.Up;

	private CollisionShape3D _floor;



	public override void _Ready()
	{
		_floor = GetNode<CollisionShape3D>("%FloorCollider");
		ShipArea = GetNode<Area3D>("%ShipArea");
		SpawnPoint = GetNode<Node3D>("%SpawnPoint");
		_SetupShipArea();
	}


	public override void _PhysicsProcess(double delta)
	{
		if (Planet is null)
			_up = Vector3.Up;
		else
		{
			_up = -(Planet.GlobalPosition - _floor.GlobalPosition).Normalized();
			_RotateShip();
		}
		if (_hasPilot)
		{
			_HandleMovement(delta);
		}
		// if (Planet is not null && !IsOnFloor())
		// 	Velocity += -_up * Gravity * 50f * delta;
		// MoveAndSlide();
	}

	private void _RotateShip()
	{
		Transform3D target = new()
		{
			Origin = GlobalPosition
		};
		Vector3 left = _up.Cross(GlobalBasis.Z).Normalized();
		Vector3 z = GlobalTransform.Basis.Z;
		target.Basis = new Basis(left, _up, z).Orthonormalized();
		Quaternion currentRotation = GlobalBasis.GetRotationQuaternion().Normalized();
		Quaternion targetRotation = target.Basis.GetRotationQuaternion().Normalized();

		Quaternion r = currentRotation.Slerp(targetRotation, 1f).Normalized();
		GlobalBasis = new Basis(r);
	}

	private void _HandleMovement(double delta)
	{
		Vector3 velocity = Velocity;

		Vector3 direction = GetDirection();
		velocity = velocity.Lerp(direction * Speed, 0.8f * delta);

		Velocity = velocity;
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
			if (MotionMode == MotionModeEnum.Grounded)
			{
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

		if (Input.IsActionPressed("MoveDown")) direction.Y -= 1.0f;
		return direction.Normalized();
	}

	private void _SetupShipArea()
	{
		ShipArea.BodyEntered += body =>
		{
			if (body is Player p)
			{
				p.Ship = this;
				// p.Gravity = ShipGravity;
				// p.MotionMode = MotionModeEnum.Grounded;
				// p.MovementLocation = Player.MovementLocationEnum.Ship;
				GD.Print("Player entered ship area.");
			}
		};

		ShipArea.BodyExited += body =>
		{
			if (body is Player p)
			{
				p.Ship = null;
				// p.Planet = null;
				// p.Gravity = 0.0;
				// p.MotionMode = MotionModeEnum.Floating;
				// p.MovementLocation = Player.MovementLocationEnum.Space;
				GD.Print("Player exited ship area.");
			}
		};
	}
}
