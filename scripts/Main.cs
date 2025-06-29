using Godot;
using Planets.UI;
using System;

namespace Planets
{
    public partial class Main : Node
    {
        [Export]
        public bool Generated { get; set; } = false;
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
                var p = SystemGenerator.PlanetGenerator.GeneratePlanet();
                GetNode<Node3D>("%World").AddChild(p);
                p.GetChild<MeshInstance3D>(0).Position = new Vector3(0, 0, -1300f);
                PackedScene scene = new();
                scene.Pack(p);
                ResourceSaver.Save(scene, "res://TestPlanet.tscn", ResourceSaver.SaverFlags.Compress);
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }
    }
}
