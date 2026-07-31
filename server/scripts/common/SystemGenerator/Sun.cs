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
    public string Name { get; set; } = "Sol";

    [Export]
    public int Radius { get; set; } = 100;

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
