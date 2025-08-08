using Godot;

namespace Planets.Util;

public static class ProtoUtils
{
    public static Vector3 ProtoToGodotVector3(ProtoVector3 vector) => new(vector.X, vector.Y, vector.Z);

    public static ProtoVector3 GodotToProtoVector3(Vector3 vector) => new() { X = vector.X, Y = vector.Y, Z = vector.Z };
}
