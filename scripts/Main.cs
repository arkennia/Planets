using Godot;
using Planets.SystemGenerator;
using Planets.UI;
using System;

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
        public CharacterBody3D player { get; set; }
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
                PlanetNode p = SystemGenerator.PlanetGenerator.GeneratePlanet(scale: Scale, resolution: Resolution);
                p.Position = new Vector3(0, 0, -1300);
                GetNode<Node3D>("%World").AddChild(p);
                var spawn = Planet.GetRandomSurfacePosition(Scale);
                spawn += p.Planet.MeshInstance.GlobalPosition;
                player.GlobalPosition = spawn;
                p.Save();
            }
            else
            {
                var scene = ResourceLoader.Load<PackedScene>("res://scenes/planets/00000000-0000-0000-0000-000000000000.tscn").Instantiate();
                GetNode("%World").AddChild(scene);
                GD.Print("Planet loaded");
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }
    }
}
