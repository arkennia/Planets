using Godot;

namespace Planets.SystemGenerator.Terrain;

/// <summary>
/// Abstract class for procedural terrain.
/// Inherits <c>MeshInstance3D</c>.
/// </summary>
[GlobalClass]
public abstract partial class Terrain3D : MeshInstance3D
{
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
    /// Generates the terrain.
    /// </summary>
    /// <remarks>
    /// If <paramref name="shaderMaterial"/> is not null, you must set the shader params manually in this method.
    /// </remarks>
    /// <param name="generateLods">Optional, generate LODs.</param>
    /// <param name="shaderMaterial">The shader material to use.</param>
    public abstract void Generate(bool generateLods, ShaderMaterial shaderMaterial = null);

}