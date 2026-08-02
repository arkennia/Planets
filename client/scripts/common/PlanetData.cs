using Godot;
using Godot.Collections;
using Google.Protobuf;
using Planets;
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
    public Array<double> Heights { get; private set; } = [];
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
        PlanetDataProto proto = new()
        {
            Guid = Guid.ToString(),
            Name = Name,
            Scale = Scale,
            Heights = { Heights },
            Seed = (int)Seed
        };
        using FileAccess file = FileAccess.Open($"{SaveFolder}/{Guid}.dat", FileAccess.ModeFlags.Write);
        file.StoreBuffer(proto.ToByteArray());
    }

    public void LoadPlanet(string guid)
    {
        using FileAccess file = FileAccess.Open($"{SaveFolder}/{guid}.dat", FileAccess.ModeFlags.Read);
        byte[] bytes = file.GetBuffer((int)file.GetLength());
        PlanetDataProto proto = PlanetDataProto.Parser.ParseFrom(bytes);
        GD.Print($"Loaded planet data from {SaveFolder}/{guid}.dat");
        //GD.Print($"{proto.ToString()}");
        Guid = Guid.Parse(proto.Guid);
        Name = proto.Name;
        Scale = proto.Scale;
        Heights = new Array<double>(proto.Heights);
        Seed = (ulong)proto.Seed;
    }
}
