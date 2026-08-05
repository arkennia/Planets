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

    [Export]
    public OmniLight3D SunLight { get; set; }

    private Mesh _mesh;

    public void Generate(int radius)
    {
        Sun ??= new Sun(radius: radius);
        Generate();
    }

    public void Generate()
    {
        Sun ??= new Sun();
        _mesh = new SphereMesh
        {
            Radius = Sun.Radius,
            Height = Sun.Radius * 2,
            RadialSegments = 32,
            Rings = 16,
            Material = Sun.StandardMaterial,
        };
        SunMesh = new MeshInstance3D
        {
            Mesh = _mesh,
        };
        AddChild(SunMesh);

        SunArea = new Area3D();
        CollisionShape3D collisionShape = new CollisionShape3D
        {
            Shape = new SphereShape3D
            {
                Radius = Sun.AreaSize,
            },
        };
        SunArea.AddChild(collisionShape);
        AddChild(SunArea);

        SunLight = new OmniLight3D
        {
            OmniAttenuation = Sun.Attenuation,
            OmniRange = Sun.Range,
            OmniShadowMode = OmniLight3D.ShadowMode.Cube,
            LightEnergy = Sun.Energy,
            ShadowEnabled = true,
        };
        AddChild(SunLight);
    }

    public void Save()
    {
        throw new NotImplementedException();
    }
}
