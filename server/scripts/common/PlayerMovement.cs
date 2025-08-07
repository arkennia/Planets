using System;
using Godot;

namespace Planets;

public partial class PlayerMovement : RefCounted
{
    public Vector3 CurrentGlobalPosition;
    public Vector3 Velocity;
    public Vector3 Rotation;
    public Vector3 Up;
}
