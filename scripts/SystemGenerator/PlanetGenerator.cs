using Godot;
using Planets.SystemGenerator.Terrain;


namespace Planets.SystemGenerator;

[Tool]
[GlobalClass]
public partial class PlanetGenerator : EditorScript
{
    private string PlanetName { get; set; } = "Earth";

    // {
    //     Radius = 1.0f,
    //     Height = 2.0f,
    //     RadialSegments = 128,
    //     Rings = 64
    // };

    private int Scale { get; set; } = 2;
    private int Resolution { get; set; } = 128;

    public override void _Run()
    {
        PlanetNode p = GeneratePlanet(PlanetName, Scale);
        p.Save();
    }

    public static PlanetNode GeneratePlanet(string name = "Earth", int scale = 1000)
    {
        Planet planet = new(name, scale);
        PlanetNode mI = new()
        {
            Planet = planet
        };
        mI.Generate();
        return mI;
    }

    // public static PlanetNode GeneratePlanet(Planet p)
    // {
    //     return p.Generate();
    // }
}