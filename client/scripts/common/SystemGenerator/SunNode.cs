using Godot;
using System;

namespace Planets.SystemGenerator;

/// <summary>
/// The <c>SunNode</c> class inherits Node3D, and implements ICelestialBodyNode. <br/>
/// Used in conjunction with the <c>Sun</c> class to display a Sun in the scene tree.
/// </summary>
public partial class SunNode : Node3D, ICelestialBodyNode<Sun>
{
    [Export]
    public Sun Sun { get; set; } = null;

    [Export]
    public Area3D SunArea { get; private set; }

    public ICelestialBody CelestialBody => Sun;
    [Export]
    public string SaveLocation { get; set; }

    [Export]
    public MeshInstance3D SunMesh { get; set; }

    private Mesh _mesh;

    public void Generate()
    {
        Sun ??= new Sun();
        _mesh = new SphereMesh
        {
            Radius = Sun.Radius,
            Height = Sun.Radius * 2,
            RadialSegments = 32,
            Rings = 16,
        };
        SunMesh = new MeshInstance3D
        {
            Mesh = _mesh,
        };
        AddChild(SunMesh);
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

}
