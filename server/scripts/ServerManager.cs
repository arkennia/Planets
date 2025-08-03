using System;
using Godot;
using Godot.Collections;
using Planets.SystemGenerator;

namespace Planets.Server;

public partial class ServerManager : Node
{
    public static Array<PlanetNode> Planets { get => _planets; }
    private static Array<PlanetNode> _planets = [];

    // public static MultiplayerSpawner PlanetSpawner { get; private set; }

    public static ServerManager Instance { get => _instance; }

    private static readonly ServerManager _instance = new();

    // public const string PLANET_SPAWNER_PATH = "/root/Main/Main/Game/PlanetSpawner";
    // public const string PLANET_SPAWNER_SPAWN_PATH = "/root/Main/Main/Game/World";
    // Called when the node enters the scene tree for the first time.
    private ServerManager()
    {

    }

    public static void AddPlanet(PlanetNode planet)
    {
        _planets.Add(planet);
        //PlanetSpawner.AddSpawnableScene(planet.SaveLocation);
        // GD.Print(PlanetSpawner.GetSpawnableScene(0));
    }

    public override void _Ready()
    {
        // PlanetSpawner = GetTree().Root.GetNode<MultiplayerSpawner>(PLANET_SPAWNER_PATH);
        // PlanetSpawner.SpawnPath = GetNode(PLANET_SPAWNER_SPAWN_PATH).GetPath();
        //Instance ??= this;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
