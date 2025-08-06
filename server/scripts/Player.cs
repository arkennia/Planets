using System;
using Godot;
using Planets.SystemGenerator;

namespace Planets;

public partial class Player : CharacterBody3D
{
    public readonly long MultiplayerId = 0;
    [Export]
    public float Gravity { get; set; }

    [Export]
    public PlanetNode Planet { get; set; }

    public PlayerData PlayerData { get; set; } = new();

    private Camera3D _camera;
    private Node3D _pivot;

    public Player()
    {

    }

    public Player(long id)
    {
        MultiplayerId = id;
    }
}