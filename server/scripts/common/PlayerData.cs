using System;
using Godot;

namespace Planets;

public partial class PlayerData : Resource
{
    public Vector3 Position { get; set; }

    public Vector3 SpawnPosition { get; set; }

    public Vector3 Up { get; set; }

    public int Speed { get; set; } = 20;

    public int JumpSpeed { get; set; } = 8;

    public float MouseSensitivty { get; set; } = 0.005f;

    public string SpawnPlanet { get; set; } = string.Empty;

    public string CurrentPlanet { get; set; }

}
