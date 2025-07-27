using Godot;
using Planets.SystemGenerator;
using Planets.UI;
using System;
using System.Threading.Tasks;
using Planets.SystemGenerator.Terrain;

namespace Planets;

public partial class Main : Node
{
    [Export]
    public bool Generated { get; set; } = false;

    [Export]
    public int Scale { get; set; } = 1000;

    [Export]
    public CharacterBody3D Player { get; set; }

    [Export]
    public Mesh Mesh { get; set; }
    // [Export]
    // public PackedScene UI { get; set; }

    public MainUi Ui { get; private set; } = null;
    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        Ui = (MainUi)GetNode<InstancePlaceholder>("UI").CreateInstance();
        UiManager.Instance.Ui = Ui;
        Mesh m = Mesh;
        if (!Generated)
        {
            PlanetNode p = PlanetGenerator.GeneratePlanet(scale: Scale);
            GD.Print("Planet generation complete.");
            Node3D worldNode = GetNode<Node3D>("%World");
            worldNode.AddChild(p);
            GD.Print("Planet added to scene.");
            p.Save();
            GD.Print("Planet saved.");
            // p.Scale *= 500f;
            p.Position = new Vector3(0, 0, -15000);
        }
        else
        {
            Error sceneLoader =
                ResourceLoader.LoadThreadedRequest("res://scenes/planets/00000000-0000-0000-0000-000000000000.scn",
                    useSubThreads: true);
            PackedScene scene =
                ResourceLoader.LoadThreadedGet("res://scenes/planets/00000000-0000-0000-0000-000000000000.scn") as
                    PackedScene;
            if (scene?.Instantiate() is not Node3D sceneNode) return;
            sceneNode.Position = new Vector3(0, 0, -500);
            GetNode("%World").AddChild(sceneNode);
            GD.Print("Planet loaded");
        }

        // PackedScene ps = new PackedScene();
        // ps.Pack(GetNode<Noise2DTerrain>("World/Noise2DTerrain"));
        // Error e = ResourceSaver.Save(ps, "res://scenes/planets/noise_tests.tscn");
        // GD.Print(e != Error.Ok ? $"Error saving scene: {e.ToString()}" : "Scene saved.");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}