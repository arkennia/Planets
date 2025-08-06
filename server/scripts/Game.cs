using Godot;
using Planets.SystemGenerator;

namespace Planets.Server;

public partial class Game : Node
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        PlanetNode p = PlanetGenerator.GeneratePlanet(scale: 250);
        // ServerManager.Instance.planets.Add(p);
        // p.Save();
        ServerManager.AddPlanet(p);
        GetTree().Root.GetNode("/root/Main/Game/World").AddChild(p);
        // PackedScene packedScene = ResourceLoader.Load<PackedScene>(p.SaveLocation);
        // Node3D planetScene = packedScene.Instantiate() as Node3D;
        // GetTree().Root.GetNode("/root/Main/Main/Game/World").AddChild(planetScene);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
