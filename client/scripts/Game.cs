using System;
using System.Linq;
using Godot;
using Planets.Ships;
using Planets.SystemGenerator;
using Planets.SystemGenerator.Terrain;

namespace Planets;

public partial class Game : Node
{
    [Export]
    public bool Generated { get; set; } = false;

    [Export]
    public int Scale { get; set; } = 1000;

    [Export]
    public Player Player { get; set; }

    [Export]
    public Mesh Mesh { get; set; }

    // [Export]
    // public MultiplayerSpawner PlanetSpawner { get; set; }

    private PlanetNode _planetNode;

    private int numSpawned = 0;

    // Called when the node enters the scene tree for the first time.

    public Game()
    {

    }

    public override void _Ready()
    {
        // Networking.Instance.PlanetLoaded += OnPlanetLoaded;
        // GD.Print(PlanetSpawner.GetPath());
        // PlanetSpawner.SpawnPath = "/root/Main/Main/Game/World";
        // PlanetSpawner.Spawned += (node) =>
        // {
        //     GD.Print("Spawned node: " + node.ToString());
        //     _planetNode = node as PlanetNode;
        //     numSpawned++;
        // };
    }

    public void SetupPlayer()
    {
        GD.Print("Setting up player");
        _planetNode = GameManager.Systems.Values.First().Planets.Values.First();
        Player = GetNode<InstancePlaceholder>("%Player").CreateInstance(true) as Player;
        if (Player.PlayerData == null)
        {
            Player.PlayerData = new PlayerData();
        }
        _SetSpawnPoint();
        //Networking.Player = Player;
        GameManager.Instance.PlayerSetupComplete();
    }

    private void _Load()
    {
        // GD.Print("Num spawnable scenes: " + PlanetSpawner.GetSpawnableSceneCount());
        // while (numSpawned < PlanetSpawner.GetSpawnableSceneCount())
        // {

        // }
        // GD.Print("Network Spawning finished");
        // if (!Generated)
        // {
        //     PlanetNode p = PlanetGenerator.GeneratePlanet(scale: Scale);
        //     GD.Print("Planet generation complete.");
        //     Node3D worldNode = GetNode<Node3D>("%World");
        //     worldNode.AddChild(p);
        //     GD.Print("Planet added to scene.");
        //     p.Save();
        //     GD.Print("Planet saved.");
        //     _planetNode = p;
        //     _planetNode.Position = new Vector3(0, 0, -13800);
        //     // p.Scale *= 500f;
        // }
        // else
        // {
        //     Error sceneLoader =
        //         ResourceLoader.LoadThreadedRequest("res://scenes/planets/00000000-0000-0000-0000-000000000000.scn",
        //             useSubThreads: true);
        //     if (sceneLoader != Error.Ok)
        //         GD.PrintErr(sceneLoader);
        //     PackedScene scene =
        //         ResourceLoader.LoadThreadedGet("res://scenes/planets/00000000-0000-0000-0000-000000000000.scn") as
        //             PackedScene;
        //     if (scene?.Instantiate() is not Node3D sceneNode) return;
        //     sceneNode.Position = new Vector3(0, 0, -500);
        //     GetNode("%World").AddChild(sceneNode);
        //     GD.Print("Planet loaded");
        // }
    }

    private void _SetSpawnPoint()
    {
        // Terrain3D.SpawnPoint spawn = _planetNode.GetSpawnPoint();
        // GD.Print($"Spawn Local Position: {spawn.Node.Position}");
        // GD.Print($"Spawn Global Position:{spawn.Node.GlobalPosition}");
        // Player.PlayerData.SpawnPlanet = _planetNode.Planet.Guid.ToString();
        // Player.PlayerData.SpawnPosition = spawn.Node.GlobalPosition;
        // Player.PlayerData.Up = spawn.Normal;
        // Player.Spawn();
        Ship ship = ResourceLoader.Load<PackedScene>("res://scenes/ships/spaceship_prototype.tscn")
                                  .Instantiate() as Ship;
        ship.UpDirection = Vector3.Up;
        GetNode("/root/Main/Game/World").AddChild(ship);
        ship.GlobalPosition = new Vector3(25000, 0, 0);
        ship.Planet = null;
        Player.PlayerData.SpawnPosition = ship.SpawnPoint.GlobalPosition;
        Player.PlayerData.Up = Vector3.Up;
        Player.Spawn(ship);
        // ship.Reparent(_planetNode);
        //ship.Scale = Vector3.One * 0.1f;
    }

    public void OnPlanetLoaded(PlanetNode planet)
    {
        _planetNode = planet;
        _Load();
        _SetSpawnPoint();
    }
}