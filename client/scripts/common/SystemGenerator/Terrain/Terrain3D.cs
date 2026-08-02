using System;
using Godot;
using Godot.Collections;

namespace Planets.SystemGenerator.Terrain;

/// <summary>
/// Abstract class for procedural terrain.
/// Inherits <c>MeshInstance3D</c>.
/// </summary>
[GlobalClass]
public abstract partial class Terrain3D : MeshInstance3D
{

    public class SpawnPoint
    {
        public Node3D Node { get; set; }
        public Vector3 Normal { get; set; }
    }
    /// <summary>
    /// The <c>TerrainColor</c> object to use for coloring the Terrain.
    /// </summary>
    [Export]
    public TerrainColor Colors { get; set; }

    [Export]
    public bool Generated { get; protected set; } = false;

    // [Export(PropertyHint.NodeType, $"{nameof(ArrayMesh)}")]
    // public new Mesh Mesh { get; set; }

    /// <summary>
    /// The random seed to initialize with.
    /// </summary>
    [Export]
    public ulong Seed { get; set; } = 69000;

    /// <summary>
    /// The heights by vertex index.
    /// </summary>
    public double[] Heights { get; protected set; }

    /// <summary>
    /// The origin to use to calculate surface coordinates.
    /// </summary>
    public Vector3 CoordinateOrigin { get; protected set; }

    /// <summary>
    /// Randomly generated spawn points.
    /// </summary>
    public SpawnPoint[] SpawnPoints => _spawnPoints;

    public const int NUM_SPAWN_POINTS = 10;
    protected SpawnPoint[] _spawnPoints = new SpawnPoint[NUM_SPAWN_POINTS];

    /// <summary>
    /// Generates the terrain.
    /// </summary>
    /// <remarks>
    /// If <paramref name="shaderMaterial"/> is not null, you must set the shader params manually in this method.
    /// </remarks>
    /// <param name="generateLods">Optional, generate LODs.</param>
    /// <param name="shaderMaterial">The shader material to use.</param>
    public abstract void Generate(bool generateLods, ShaderMaterial shaderMaterial = null);

    public abstract void FromHeights(bool generateLods, int seed, Array<double> heights, ShaderMaterial shaderMaterial = null);

    protected void _GenerateSpawnPoints(MeshDataTool mdt, int vCount)
    {
        RandomNumberGenerator rng = new();
        for (int i = 0; i < NUM_SPAWN_POINTS; i++)
        {
            int vIdx = rng.RandiRange(0, vCount - 1);
            Vector3 vertex = mdt.GetVertex(vIdx);
            Vector3 normal = mdt.GetVertexNormal(vIdx);
            vertex += normal * 0.09f;
            Node3D node = new()
            {
                Position = vertex,
            };
            _spawnPoints[i] = new()
            {
                Node = node,
                Normal = normal
            };
            AddChild(_spawnPoints[i].Node);
        }
    }

}