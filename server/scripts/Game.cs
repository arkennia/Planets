using Godot;
using Planets.SystemGenerator;

namespace Planets.Server;

public partial class Game : Node
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //PlanetNode p = PlanetGenerator.GeneratePlanet(scale: 250);
        // ServerManager.Instance.planets.Add(p);
        //p.Save();
        PlanetData planetData = new();
        planetData.LoadPlanet("b884ede5-dfb7-467a-9034-15a27db83a13");
        PlanetNode p = PlanetGenerator.GeneratePlanet(planetData.Name, planetData.Scale, (int)planetData.Seed, planetData.Heights, planetData.Guid);
        ServerManager.AddPlanet(p);
        GetTree().Root.GetNode("/root/Main/Game/World").AddChild(p);
        // PlanetData planetData = new(p);
        // planetData.SavePlanet(p);
        // PackedScene packedScene = ResourceLoader.Load<PackedScene>("res://scenes/planets/799f1e7c-0b3d-4a5e-8f2c-1a2b3c4d5e6f.tscn");
        // Node3D planetScene = packedScene.Instantiate() as Node3D;
        // GetTree().Root.GetNode("/root/Main/Main/Game/World").AddChild(planetScene);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
