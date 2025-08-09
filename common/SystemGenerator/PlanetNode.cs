using System.Runtime.Serialization;
using Godot;
using Godot.Collections;
using Planets.SystemGenerator.Terrain;

namespace Planets.SystemGenerator;

/// <summary>
/// PlanetNode is a holder for a Planet object, and implements both Node3D and ICelestialBodyNode. The
/// purpose of this class is to add a Planet to the scene tree and display it.
/// </summary>
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

    public string SaveLocation { get; set; }


    public const string SAVE_PATH = "res://scenes/planets";

    private Mesh _mesh = ResourceLoader.Load<Mesh>("res://scripts/common/meshes/planets/Icosphere.res");

    public override void _Ready()
    {
        // PlanetArea = GetNode<Area3D>($"./{Planet.Area3DName}");
    }
    public Terrain3D.SpawnPoint GetSpawnPoint()
    {
        RandomNumberGenerator rng = new();
        int idx = rng.RandiRange(0, Terrain3D.NUM_SPAWN_POINTS - 1);
        return PlanetTerrain.SpawnPoints[idx];
    }

    public void Save()
    {
        Name = new StringName($"{Planet.Guid}");
        PackedScene ps = new();
        ps.Pack(this);
        string fullPath = $"{SAVE_PATH}/{CelestialBody.Guid}.scn";
        SaveLocation = fullPath;
        ResourceSaver.Save(ps, fullPath, ResourceSaver.SaverFlags.Compress);
    }

    public Vector2 CalculatePosition(Vector3 coord)
    {
        float x = coord.X - PlanetTerrain.CoordinateOrigin.X;
        float y = coord.Y - PlanetTerrain.CoordinateOrigin.Y;
        return new Vector2(x, y);
    }



    public void Generate(bool withHeights = false, int seed = 0, Array<float> heights = null)
    {
        Terrain3D terrain = null;
        if (!withHeights)
        {
            terrain = new SimplexTerrain3D()
            {
                Colors = Planet.Colors,
                Mesh = _mesh.Duplicate() as Mesh, //ResourceLoader.Load<Mesh>("res://meshes/planets/Icosphere.res"),
                HeightmapSize = new NoiseImageSize(128),
                // Noise1ImageSize = new NoiseImageSize(128),
                // Noise2ImageSize = new NoiseImageSize(128),
                // Noise3ImageSize = new NoiseImageSize(128),
                MoistureImageSize = new NoiseImageSize(128),
                WaterLevel = 0.3f,
                UseSeamless = false
            };
            AddChild(terrain);
            terrain.Generate(false, Planet.ShaderMaterial);
        }
        else
        {
            terrain = new SimplexTerrain3D()
            {
                Colors = Planet.Colors,
                Mesh = _mesh.Duplicate() as Mesh, //ResourceLoader.Load<Mesh>("res://meshes/planets/Icosphere.res"),
                HeightmapSize = new NoiseImageSize(128),
                // Noise1ImageSize = new NoiseImageSize(128),
                // Noise2ImageSize = new NoiseImageSize(128),
                // Noise3ImageSize = new NoiseImageSize(128),
                MoistureImageSize = new NoiseImageSize(128),
                WaterLevel = 0.3f,
                UseSeamless = false,
            };
            AddChild(terrain);
            terrain.FromHeights(false, seed, heights, Planet.ShaderMaterial);
        }

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
            Radius = (float)_mesh.GetMeta("Radius") + Planet.Area3DExtraSpace,
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
        collider.Scale *= 1.00f;


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

        Scale *= Planet.Scale;

        PlanetArea.BodyEntered += body =>
        {
            if (body is Player p)
            {
                p.Gravity = Planet.Gravity;
                p.MotionMode = CharacterBody3D.MotionModeEnum.Grounded;
                p.Planet = this;
            }
        };

        PlanetArea.BodyExited += body =>
        {
            if (body is Player p)
            {
                p.MotionMode = CharacterBody3D.MotionModeEnum.Floating;
            }
        };
    }

    void ICelestialBodyNode<Planet>.Save()
    {
        throw new System.NotImplementedException();
    }

    void ICelestialBodyNode<Planet>.Generate()
    {
        throw new System.NotImplementedException();
    }

}