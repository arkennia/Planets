using Godot;


namespace Planets.SystemGenerator;

[Tool]
public partial class PlanetGenerator : EditorScript
{
    private string PlanetName { get; set; } = "Earth";

    private Mesh Mesh { get; set; } // new SphereMesh
    // {
    //     Radius = 1.0f,
    //     Height = 2.0f,
    //     RadialSegments = 128,
    //     Rings = 64
    // };

    private int Radius { get; set; } = 2;
    private int Resolution { get; set; } = 128;

    public override void _Run()
    {
        PlanetNode p = GeneratePlanet(PlanetName, Mesh, Radius, Resolution);
        p.Save();
    }

    public static PlanetNode GeneratePlanet(string name = "Earth", Mesh mesh = null, int scale = 1000,
        int resolution = 64)
    {
        Planet planet = new(name, mesh, scale, resolution);
        PlanetNode mI = planet.Generate();
        return mI;
    }

    public static PlanetNode GeneratePlanet(Planet p)
    {
        return p.Generate();
    }
}