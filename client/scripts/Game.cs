using System.Threading.Tasks;
using Godot;
using Planets;
using Planets.SystemGenerator;
using Planets.SystemGenerator.Terrain;

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
        _planetNode = Networking.Instance.planets[0];
        _SetSpawnPoint();
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
        Terrain3D.SpawnPoint spawn = _planetNode.GetSpawnPoint();
        GD.Print($"Spawn Local Position: {spawn.Node.Position}");
        GD.Print($"Spawn Global Position:{spawn.Node.GlobalPosition}");
        // spawn.mI.GlobalPosition = spawn.Node.GlobalPosition;
        // Player.GlobalPosition = spawn;
        Player.Spawn(spawn, _planetNode);
    }

    public void OnPlanetLoaded(PlanetNode planet)
    {
        _planetNode = planet;
        _Load();
        _SetSpawnPoint();
    }
}