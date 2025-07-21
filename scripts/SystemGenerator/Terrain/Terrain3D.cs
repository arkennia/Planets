using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace Planets.SystemGenerator.Terrain;

[GlobalClass]
public abstract partial class Terrain3D : MeshInstance3D
{
    [Export]
    public TerrainColor Colors { get; set; }

    [Export]
    public bool Generated { get; protected set; } = false;

    // [Export(PropertyHint.NodeType, $"{nameof(ArrayMesh)}")]
    // public new Mesh Mesh { get; set; }

    [Export]
    public ulong Seed { get; set; } = 69000;

    public Terrain3D() : base()
    {

    }


    public abstract void Generate(bool generateLods, ShaderMaterial shaderMaterial);

    // public override void _ValidateProperty(Dictionary property)
    // {
    //     if (property["name"].AsStringName() != PropertyName.Mesh)
    //     {
    //         base._ValidateProperty(property);
    //         return;
    //     }
    //
    //     PropertyUsageFlags propertyUsageFlags = property["usage"].As<PropertyUsageFlags>();
    //
    //     if (_useGeneratedMesh)
    //         propertyUsageFlags |= PropertyUsageFlags.ReadOnly;
    //     else
    //         propertyUsageFlags &= ~PropertyUsageFlags.ReadOnly;
    //
    //     property["usage"] = (int)propertyUsageFlags;
    //     base._ValidateProperty(property);
    // }
}