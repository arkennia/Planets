using System;
using Godot;
using Godot.Collections;
using Planets.SystemGenerator.Terrain;


namespace Planets.SystemGenerator;

/// <summary>
/// PlanetGenerator generates a planet node to be placed into the scenetree. It can be run in the editor.
/// </summary>
[Tool]
[GlobalClass]
public partial class PlanetGenerator : EditorScript
{
    private string PlanetName { get; set; } = "Earth";

    private int Scale { get; set; } = 2;

    // public override void _Run()
    // {
    //     PlanetNode p = GeneratePlanet(PlanetName, Scale);
    //     p.Save();
    // }

    public static PlanetNode GeneratePlanet(string name = "Earth", int scale = 1000, int seed = 0, Array<double> heights = null, Guid? guid = null)
    {
        Planet planet = new(name, scale, guid);
        PlanetNode mI = new()
        {
            Planet = planet
        };
        if (heights is not null)
            mI.Generate(true, seed, heights);
        else
            mI.Generate();
        return mI;
    }
}