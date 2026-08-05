using Godot;
using System;

namespace Planets.SystemGenerator;

/// <summary>
/// The <c>Sun</c> class inherits Resource, and implements ICelestialBody. It contains all of the data describing a Sun. <br/>
/// </summary>
public partial class Sun : Resource, ICelestialBody
{
    public Guid Guid { get; private set; } = Guid.Empty;

    [Export]
    public string GuidString { get => Guid.ToString(); set => Guid = new Guid(value); }

    [Export]
    public string Name { get; set; } = "Sol";

    [Export]
    public int Radius { get; set; } = 50;

    [Export]
    public int AreaSize { get; set; } = 100;

    [Export]
    public double Attenuation { get; set; } = 0.4;

    [Export]
    public int Range { get; set; } = 100000;

    [Export]
    public double Energy { get; set; } = 150;

    [Export]
    public StandardMaterial3D StandardMaterial { get; set; } = ResourceLoader.Load<StandardMaterial3D>("res://scripts/common/materials/sun_material.tres")
        .Duplicate() as StandardMaterial3D;

    [Export]
    public Vector3 Sector { get; private set; } = Vector3.Zero;

    [Export]
    public Vector3 SectorLocation { get; private set; } = Vector3.Zero;

    public Sun()
    {

    }

    public Sun(string name = "Sol", int radius = 100, Guid? guid = null)
    {
        Name = name;
        Radius = radius;
        Guid = (Guid)(guid is null ? Guid.NewGuid() : guid);
    }

    public void Save(string path = "res://resources")
    {
        ResourceSaver.Save(this, $"{path}/{Guid}.res", ResourceSaver.SaverFlags.Compress);
    }
}
