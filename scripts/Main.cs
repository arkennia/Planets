using Godot;
using Planets.SystemGenerator;
using Planets.UI;
using System;
using System.Threading.Tasks;

namespace Planets;

public partial class Main : Node
{
    [Export]
    public bool Generated { get; set; } = false;

    [Export]
    public int Scale { get; set; } = 1000;

    [Export]
    public int Resolution { get; set; } = 512;

    [Export]
    public CharacterBody3D Player { get; set; }
    // [Export]
    // public PackedScene UI { get; set; }

    public MainUi Ui { get; private set; } = null;
    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        Ui = (MainUi)GetNode<InstancePlaceholder>("UI").CreateInstance();
        UiManager.Instance.Ui = Ui;
        // Mesh m = new CubeSphereV2();
        // AddChild(Ui);
        // if (!Generated)
        // {
        //     PlanetNode p = PlanetGenerator.GeneratePlanet(scale: Scale, resolution: Resolution);
        //     GD.Print("Planet generation complete.");
        //     Node3D worldNode = GetNode<Node3D>("%World");
        //     worldNode.AddChild(p);
        //     GD.Print("Planet added to scene.");
        //     p.Save();
        //     GD.Print("Planet saved.");
        //     p.Scale *= 500f;
        //     p.Position = new Vector3(0, 0, -550);
        // }
        // else
        // {
        //     Error sceneLoader =
        //         ResourceLoader.LoadThreadedRequest("res://scenes/planets/00000000-0000-0000-0000-000000000000.scn",
        //             useSubThreads: true);
        //     PackedScene scene =
        //         ResourceLoader.LoadThreadedGet("res://scenes/planets/00000000-0000-0000-0000-000000000000.scn") as
        //             PackedScene;
        //     if (scene?.Instantiate() is not Node3D sceneNode) return;
        //     sceneNode.Position = new Vector3(0, 0, -500);
        //     GetNode("%World").AddChild(sceneNode);
        //     GD.Print("Planet loaded");
        // }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}