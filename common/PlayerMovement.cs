using Godot;
using Planets.Util;

namespace Planets;

public class PlayerMovement(PlayerMovementProto movement)
{
    public Vector3 CurrentGlobalPosition = ProtoUtils.ProtoToGodotVector3(movement.CurrentGlobalPosition);

    public Vector3 Velocity = ProtoUtils.ProtoToGodotVector3(movement.Velocity);

    public Vector3 Rotation = ProtoUtils.ProtoToGodotVector3(movement.Rotation);

    public Vector3 Up = ProtoUtils.ProtoToGodotVector3(movement.Up);

    public Vector3 MovementDirection = ProtoUtils.ProtoToGodotVector3(movement.MovementDirection);

    public bool IsJumping = movement.IsJumping;

    public bool IsInAir = movement.IsInAir;
}