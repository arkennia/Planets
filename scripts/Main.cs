using System;
using System.Threading.Tasks;
using Godot;
using Planets.SystemGenerator;
using Planets.UI;

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

    public MainUi Ui { get; private set; } = null;

    public override void _Ready()
    {
        Ui = (MainUi)GetNode<InstancePlaceholder>("UI").CreateInstance();
        UiManager.Instance.Ui = Ui;
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
            p.Position = new Vector3(0, 0, -13800);
            Vector3 spawn = p.GetSpawnPoint();
            GD.Print($"Global Position:{Player.GlobalPosition}");
            GD.Print($"Spawn: {spawn}");
            Player.GlobalPosition = spawn;
            GD.Print($"New Global Position:{Player.GlobalPosition}");
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
    }
}