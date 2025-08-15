
using Godot;
using Planets.Util;

namespace Planets;

public class PlayerData
{
    public Vector3 Position { get; set; }

    public Vector3 SpawnPosition { get; set; }

    public Vector3 Up { get; set; }

    public int Speed { get; set; } = 20;

    public int JumpSpeed { get; set; } = 8;

    public float MouseSensitivty { get; set; } = 0.005f;

    public string SpawnPlanet { get; set; } = string.Empty;

    public string CurrentPlanet { get; set; } = string.Empty;
    public PlayerData(PlayerDataProto data)
    {
        Position = ProtoUtils.ProtoToGodotVector3(data.Position);
        SpawnPosition = ProtoUtils.ProtoToGodotVector3(data.SpawnPosition);
        Up = ProtoUtils.ProtoToGodotVector3(data.Up);
        Speed = data.Speed;
        JumpSpeed = data.JumpSpeed;
        MouseSensitivty = data.MouseSensitivity;
        SpawnPlanet = data.SpawnPlanet;
        CurrentPlanet = data.CurrentPlanet;
    }

    public PlayerData()
    {

    }

    public PlayerDataProto ToProto()
    {
        return new PlayerDataProto()
        {
            Position = ProtoUtils.GodotToProtoVector3(Position),
            SpawnPosition = ProtoUtils.GodotToProtoVector3(SpawnPosition),
            Up = ProtoUtils.GodotToProtoVector3(Up),
            Speed = Speed,
            JumpSpeed = JumpSpeed,
            MouseSensitivity = MouseSensitivty,
            SpawnPlanet = SpawnPlanet,
            CurrentPlanet = CurrentPlanet,
        };
    }

}
