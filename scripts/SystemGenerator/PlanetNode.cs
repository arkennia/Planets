using Godot;
using Planets.SystemGenerator.Terrain;

namespace Planets.SystemGenerator;

public partial class PlanetNode : Node3D, ICelestialBodyNode<Planet>
{
    [Export]
    public Planet Planet { get; set; }

    [Export]
    public Area3D PlanetArea { get; set; }

    [Export]
    public Terrain3D PlanetTerrain { get; set; }

    [Export]
    public bool Generated { get; set; }

    [Export]
    public bool GenerateLods { get; set; }

    public ICelestialBody CelestialBody => Planet;

    private Mesh _mesh = ResourceLoader.Load<Mesh>("res://meshes/planets/Icosphere.res");

    // private Mesh _mesh = new SphereMesh
    // {
    //     Radius = 60,
    //     Height = 120,
    // };

    public override void _Ready()
    {
        // PlanetArea = GetNode<Area3D>($"./{Planet.Area3DName}");
    }

    public void Save(string path = "res://scenes/planets")
    {
        Name = new StringName($"{Planet.Guid}");
        PackedScene ps = new();
        ps.Pack(this);
        ResourceSaver.Save(ps, $"{path}/{CelestialBody.Guid}.scn", ResourceSaver.SaverFlags.Compress);
    }

    public void Generate()
    {
        // MeshInstance = new MeshInstance3D
        // {
        //     Name = Name,
        //     Mesh = m
        //     // Scale = new Vector3(Scale, Scale, Scale),
        // };
        SimplexTerrain3D terrain = new()
        {
            Colors = Planet.Colors,
            Mesh = ResourceLoader.Load<Mesh>("res://meshes/planets/Icosphere.res"),
            HeightmapSize = new NoiseImageSize(128),
            Noise1ImageSize = new NoiseImageSize(128),
            Noise2ImageSize = new NoiseImageSize(128),
            Noise3ImageSize = new NoiseImageSize(128),
            MoistureImageSize = new NoiseImageSize(128),
            WaterLevel = 0.3f,
            UseSeamless = false
        };
        terrain.Generate(false, Planet.ShaderMaterial);

        // PlanetNode rootNode = new();

        // if (ShaderMaterial.Duplicate() is ShaderMaterial material)
        // {
        //     MeshInstance.SetSurfaceOverrideMaterial(0, material);
        //     material.SetShaderParameter("noise1", NoiseTexture1);
        //     material.SetShaderParameter("noise2", NoiseTexture2);
        //     material.SetShaderParameter("noise3", NoiseTexture3);
        //     material.SetShaderParameter("moisture", MoistureTexture);
        // }

        StaticBody3D sB = new()
        {
            CollisionLayer = 0b10
        };

        Area3D area = new()
        {
            GravitySpaceOverride = Area3D.SpaceOverride.Replace,
            GravityPoint = true,
            GravityPointUnitDistance = (float)_mesh.GetMeta("Radius"),
            Gravity = Planet.Gravity,
            GravityDirection = new Vector3(0, -1, 0)
        };

        SphereShape3D areaColliderShape = new()
        {
            Radius = (float)_mesh.GetMeta("Radius") + Planet.Area3DExtraSpace
        };
        CollisionShape3D areaCollider = new()
        {
            Shape = areaColliderShape
        };


        // SphereShape3D colliderShape = new()
        // {
        //     Radius = Scale + 8
        // };
        ConcavePolygonShape3D colliderShape = new();

        colliderShape.SetFaces(terrain.Mesh.GetFaces());

        CollisionShape3D collider = new()
        {
            Shape = colliderShape,
        };
        collider.Scale *= 1.001f;

        // rootNode.Planet = this;
        AddChild(terrain);
        AddChild(area);
        area.Owner = this;
        area.AddChild(areaCollider);
        areaCollider.Owner = this;

        terrain.Owner = this;
        terrain.AddChild(sB);

        sB.Owner = this;
        sB.AddChild(collider);

        collider.Owner = this;
        Generated = true;

        PlanetArea = area;

        PlanetTerrain = terrain;

        Scale *= (float)Planet.Scale;

        // _mesh = null;
        // return rootNode;
    }
}