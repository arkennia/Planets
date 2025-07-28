using Godot;
using Planets.SystemGenerator.Terrain;


namespace Planets.SystemGenerator;

[Tool]
[GlobalClass]
public partial class PlanetGenerator : EditorScript
{
    private string PlanetName { get; set; } = "Earth";

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
}