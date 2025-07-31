using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace Planets.SystemGenerator;

[GlobalClass]
public partial class CubeSphereV2 : ArrayMesh
{
    [Export]
    public int Radius { get; set; } = 1;

    private const string _FOLDER_PATH = "res://meshes/planets";

    private const string _MESH_PATH = "res://meshes/planets/CubsphereV2.0.res";

    private ArrayMesh _m;

    // public string MeshName { get; set; } = meshName;

    public CubeSphereV2()
    {
        Generate();
    }

    public CubeSphereV2(int radius = 500, int resolution = 512)
    {
        _ = resolution;
        Radius = radius;
        Generate();
    }

    // Create lists(arrays) for each type you need for the mesh.
    // Non-exhaustive list.
    // private List<Vector3> verts = [];
    // List<Vector2> uvs = [];
    // List<Vector3> normals = [];
    // // List<float> tangents = [];
    // private List<int> indices = [];


    // private ArrayMesh _m;


    private void Generate()
    {
        Mesh m = ResourceLoader.Load<Mesh>(_MESH_PATH);
        AddSurfaceFromArrays(PrimitiveType.Triangles, m.SurfaceGetArrays(0));
    }


    private void Save()
    {
        ResourceSaver.Save(this, $"{_FOLDER_PATH}/CubeSphereV2.res");
    }
}