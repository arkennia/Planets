using Godot;
using Planets.SystemGenerator;
using Planets.UI;
using System;
using System.Threading.Tasks;

namespace Planets
{
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
            // AddChild(Ui);
            if (!Generated)
            {
                Task.Run(() =>
                {
                    PlanetNode p = PlanetGenerator.GeneratePlanet(scale: Scale, resolution: Resolution);
                    p.Position = new Vector3(0, 0, -1000);
                    Node3D worldNode = GetNode<Node3D>("%World");
                    worldNode.CallDeferred(Node.MethodName.AddChild, p);
                    // var spawn = Planet.GetRandomSurfacePosition(Scale + 5);
                    // spawn += p.Planet.MeshInstance.GlobalPosition;
                    // Player.GlobalPosition = spawn;
                    p.Save();
                });
            }
            else
            {
                Error sceneLoader = ResourceLoader.LoadThreadedRequest("res://scenes/planets/00000000-0000-0000-0000-000000000000.scn", useSubThreads: true);
                PackedScene scene = ResourceLoader.LoadThreadedGet("res://scenes/planets/00000000-0000-0000-0000-000000000000.scn") as PackedScene;
                GetNode("%World").AddChild(scene.Instantiate());
                GD.Print("Planet loaded");
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }
    }
}
