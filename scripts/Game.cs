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

    private PlanetNode _planetNode;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _Load();
        _SetSpawnPoint();
    }


    private void _Load()
    {
        if (!Generated)
        {
            PlanetNode p = PlanetGenerator.GeneratePlanet(scale: Scale);
            GD.Print("Planet generation complete.");
            Node3D worldNode = GetNode<Node3D>("%World");
            worldNode.AddChild(p);
            GD.Print("Planet added to scene.");
            p.Save();
            GD.Print("Planet saved.");
            _planetNode = p;
            _planetNode.Position = new Vector3(0, 0, -13800);
            // p.Scale *= 500f;
        }
        else
        {
            Error sceneLoader =
                ResourceLoader.LoadThreadedRequest("res://scenes/planets/00000000-0000-0000-0000-000000000000.scn",
                    useSubThreads: true);
            if (sceneLoader != Error.Ok)
                GD.PrintErr(sceneLoader);
            PackedScene scene =
                ResourceLoader.LoadThreadedGet("res://scenes/planets/00000000-0000-0000-0000-000000000000.scn") as
                    PackedScene;
            if (scene?.Instantiate() is not Node3D sceneNode) return;
            sceneNode.Position = new Vector3(0, 0, -500);
            GetNode("%World").AddChild(sceneNode);
            GD.Print("Planet loaded");
        }
    }

    private void _SetSpawnPoint()
    {
        Terrain3D.SpawnPoint spawn = _planetNode.GetSpawnPoint();
        GD.Print($"Global Position:{Player.GlobalPosition}");
        GD.Print($"Spawn: {spawn}");
        // Player.GlobalPosition = spawn;
        Player.Spawn(spawn, _planetNode);
        GD.Print($"New Global Position:{Player.GlobalPosition}");
    }
}
