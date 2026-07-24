using Godot;
using Godot.Collections;
using Planets.SystemGenerator;
using System;

/// <summary>
/// The data needed to generate a planet. This is used to save and load planets.
/// </summary>
[GlobalClass]
public partial class PlanetData : Resource
{
    public static readonly string SaveFolder = "res://planetdata";
    public Guid Guid { get; private set; } = Guid.Empty;
    [Export]
    public string GuidString
    {
        get => Guid.ToString();
        set => Guid = Guid.Parse(value);
    }
    [Export]
    public string Name { get; private set; } = "Earth";
    [Export]
    public int Scale { get; private set; } = 250;
    [Export]
    public Array<float> Heights { get; private set; } = [];
    [Export]
    public ulong Seed { get; private set; } = 0;

    public PlanetData()
    {

    }

    public PlanetData(PlanetNode planet)
    {
        Guid = planet.Planet.Guid;
        Name = planet.Name;
        Scale = planet.Planet.Scale;
        Heights = [.. planet.PlanetTerrain.Heights];
        Seed = planet.PlanetTerrain.Seed;
    }

    public void SavePlanet(PlanetNode planet)
    {
        Guid = planet.Planet.Guid;
        Name = planet.Name;
        Scale = planet.Planet.Scale;
        Heights = [.. planet.PlanetTerrain.Heights];
        Seed = planet.PlanetTerrain.Seed;
        ResourceSaver.Save(this, $"{SaveFolder}/{Guid}.tres", ResourceSaver.SaverFlags.BundleResources);
    }
}
