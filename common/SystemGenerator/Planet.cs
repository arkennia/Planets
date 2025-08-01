using System;
using Godot;

namespace Planets.SystemGenerator;

/// <summary>
/// The <c>Planet</c> class inherits Resource, and implements ICelestialBody. It contains all of the data describing a Planet. <br/>
/// To be used in conjunction with the PlanetNode class.
/// </summary>
[GlobalClass]
public partial class Planet : Resource, ICelestialBody
{
    [Export]
    public string Name { get; set; } = "Earth";

    [Export]
    public string Area3DName { get; private set; }
    /// <summary>
    /// The buffer space for the area3D which acts as a way to trigger gravity acting on a body that enters it.
    /// </summary>
    [Export]
    public int Area3DExtraSpace { get; set; } = 500;

    /// <summary>
    /// The acceleration due to gravity.
    /// </summary>
    [Export]
    public float Gravity { get; set; } = 9.8f;

    /// <summary>
    /// The amount to scale the Planet object.
    /// </summary>
    [Export]
    public int Scale { get; set; }


    [Export]
    public Vector2 Sector { get; private set; } = Vector2.Zero;

    [Export]
    public Vector3 SectorLocation { get; private set; } = Vector3.Zero;

    [Export]
    public ShaderMaterial ShaderMaterial { get; set; } =
        ResourceLoader.Load<ShaderMaterial>("res://materials/shader_materials/planet_material.tres")
            .Duplicate() as ShaderMaterial;

    [Export]
    public Terrain.TerrainColor Colors { get; set; } = new();

    public Guid Guid { get; private set; } = Guid.Empty;


    public Planet()
    {
        // if (Guid == Guid.Empty) Guid = Guid.NewGuid();
    }

    public Planet(string name = "Earth", int scale = 500)
    {
        Name = name;
        Scale = scale;
        Guid = Guid.Empty;
    }

    public void Save(string path = "res://resources")
    {
        ResourceSaver.Save(this, $"{path}/{Guid}.res", ResourceSaver.SaverFlags.Compress);
    }

}