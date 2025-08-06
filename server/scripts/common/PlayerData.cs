using System;
using Godot;

namespace Planets;

public partial class PlayerData : Resource
{
    public Vector3 Position { get; set; }

    public Vector3 SpawnPosition { get; set; }

    public int Speed { get; set; } = 20;

    public int JumpSpeed { get; set; } = 8;

    public float MouseSensitivty { get; set; } = 0.005f;

    // public Guid? SpawnPlanet { get; set; } = null;

    // public Guid? CurrentPlanet { get; set; } = null;

}
