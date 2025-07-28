using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// ReSharper disable once RedundantUsingDirective
using Godot.Collections;

namespace Planets.SystemGenerator;

[GlobalClass]
public partial class Planet : Resource, ICelestialBody
{
    [Export]
    public string Name { get; set; } = "Earth";

    [Export]
    public string Area3DName { get; private set; }

    [Export]
    public int Area3DExtraSpace { get; set; } = 3000;

    [Export]
    public float Gravity { get; set; } = 9.8f;

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