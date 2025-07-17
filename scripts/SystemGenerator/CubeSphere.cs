using System;
using System.Collections.Generic;
using Godot;

namespace Planets.SystemGenerator;

public partial class CubeSphere : Mesh
{
    public int Radius { get; set; } = 500;

    public int Resolution { get; set; } = 512;

    public int Sides { get; set; } = 6;
    public const string FOLDER_PATH = "res://meshes/planets";

    // public string MeshName { get; set; } = meshName;

    private static List<GeneratedCubeSphere> _generatedCubeSpheres;

    public CubeSphere()
    {
    }

    public CubeSphere(int radius = 500, int resolution = 512, int sides = 6)
    {
        Resolution = resolution;
        Radius = radius;
        Sides = sides;
    }


    private struct Side
    {
        public int Id;
        public Vector3 UvOrigin, UVector, VVector;
    }

    private struct GeneratedCubeSphere : IEquatable<GeneratedCubeSphere>
    {
        public int Scale;
        public int Resolution;
        public string Path;

        public static bool operator ==(GeneratedCubeSphere left, GeneratedCubeSphere right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GeneratedCubeSphere left, GeneratedCubeSphere right)
        {
            return !left.Equals(right);
        }

        public bool Equals(GeneratedCubeSphere comp)
        {
            if (GetType() != comp.GetType())
                return false;

            return Scale == comp.Scale && Resolution == comp.Resolution;
        }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;

            GeneratedCubeSphere comp = (GeneratedCubeSphere)o;
            return Scale == comp.Scale && Resolution == comp.Resolution;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Scale, Resolution);
        }
    }

    public int VertexCount => Sides * 4 * Resolution * Resolution;
    public int IndexCount => Sides * 6 * Resolution * Resolution;
    public int JobLength => Sides * Resolution;

    // Create lists(arrays) for each type you need for the mesh.
    // Non exhaustive list.
    // private List<Vector3> verts = [];
    // List<Vector2> uvs = [];
    // List<Vector3> normals = [];
    // // List<float> tangents = [];
    // private List<int> indices = [];


    private ArrayMesh _m;

    public ArrayMesh Generate()
    {
        if (_generatedCubeSpheres == null)
        {
            _generatedCubeSpheres = [];
            LoadGeneratedCubeSpheres();
        }

        GeneratedCubeSphere csg = Exists(Radius, Resolution);
        if (csg != default)
        {
            _m = (ArrayMesh)ResourceLoader.Load(csg.Path);
            GD.Print($"Cubesphere mesh found with {Radius} scale and {Resolution} resolution.");
        }
        else
        {
            SurfaceTool surfaceTool = new();

            surfaceTool.Begin(PrimitiveType.Triangles);
            surfaceTool = GenerateMesh(surfaceTool);
            surfaceTool.GenerateNormals();
            surfaceTool.GenerateTangents();
            surfaceTool.Index();

            _m = surfaceTool.Commit();
            // _m = GenerateNoise(_m);
            GD.Print($"Cubesphere mesh generated with {Radius} scale and {Resolution} resolution.");
            Save();
        }

        // CallDeferred(MethodName.Save);

        return _m;
    }

    private void Save()
    {
        ResourceSaver.Save(_m, $"{FOLDER_PATH}/{Resolution}_{Radius}_CubeSphere.res");
    }

    private SurfaceTool GenerateMesh(SurfaceTool surfaceTool)
    {
        for (int i = 0; i < JobLength; i++)
        {
            int u = i / Sides;
            Side side = GetSide(i - Sides * u);

            Vector3 uA = side.UvOrigin + side.UVector * u / Resolution;
            Vector3 uB = side.UvOrigin + side.UVector * (u + 1) / Resolution;
            Vector3 pA = uA.Normalized();
            Vector3 pB = uB.Normalized();

            int vi = 4 * Resolution * (Resolution * side.Id + u);
            int ti = 2 * Resolution * (Resolution * side.Id + u);


            for (int v = 1; v <= Resolution; v++, vi += 4, ti += 2)
            {
                Vector3 pC = (uA + side.VVector * v / Resolution).Normalized();
                Vector3 pD = (uB + side.VVector * v / Resolution).Normalized();

                surfaceTool.SetUV(Vector2.Zero);
                surfaceTool.SetSmoothGroup(1);
                surfaceTool.AddVertex(pB * Radius);
                surfaceTool.SetUV(new Vector2(1f, 0f));
                surfaceTool.SetSmoothGroup(1);
                surfaceTool.AddVertex(pA * Radius);
                surfaceTool.SetUV(Vector2.One);
                surfaceTool.SetSmoothGroup(1);
                surfaceTool.AddVertex(pC * Radius);
                surfaceTool.SetUV(new Vector2(0f, 1f));
                surfaceTool.SetSmoothGroup(1);
                surfaceTool.AddVertex(pD * Radius);
                surfaceTool.AddIndex(vi + 3);
                surfaceTool.AddIndex(vi + 2);
                surfaceTool.AddIndex(vi);
                surfaceTool.AddIndex(vi + 2);
                surfaceTool.AddIndex(vi + 1);
                surfaceTool.AddIndex(vi);

                pA = pC;
                pB = pD;
            }
        }

        return surfaceTool;
    }

    private static GeneratedCubeSphere Exists(int scale, int resolution)
    {
        GeneratedCubeSphere tmp = new() { Scale = scale, Resolution = resolution };
        int csg = _generatedCubeSpheres.FindIndex(x => x.Equals(tmp));
        if (csg != -1)
            return _generatedCubeSpheres[csg];
        else return default;
    }

    private static void LoadGeneratedCubeSpheres()
    {
        DirAccess dir = DirAccess.Open(FOLDER_PATH);
        foreach (string cs in dir.GetFiles())
            if (cs.EndsWith("res"))
            {
                string[] split = cs.Split("_");
                int res = split[0].ToInt();
                int scale = split[1].ToInt();
                _generatedCubeSpheres.Add(new GeneratedCubeSphere
                {
                    Scale = scale,
                    Resolution = res,
                    Path = $"{dir.GetCurrentDir()}/{cs}"
                });
            }
        // GD.Print($"Generated cubespheres: \n {_generatedCubeSpheres}");
    }

    private static Side GetSide(int id)
    {
        return id switch
        {
            0 => new Side
            {
                Id = id,
                UvOrigin = new Vector3(-1f, -1f, -1f),
                UVector = Vector3.Right * 2f,
                VVector = Vector3.Up * 2f
            },
            1 => new Side
            {
                Id = id,
                UvOrigin = new Vector3(1f, -1f, -1f),
                UVector = Vector3.Back * 2f,
                VVector = Vector3.Up * 2f
            },
            2 => new Side
            {
                Id = id,
                UvOrigin = new Vector3(-1f, -1f, -1f),
                UVector = Vector3.Back * 2f,
                VVector = Vector3.Right * 2f
            },
            3 => new Side
            {
                Id = id,
                UvOrigin = new Vector3(-1f, -1f, 1f),
                UVector = Vector3.Up * 2f,
                VVector = Vector3.Right * 2f
            },
            4 => new Side
            {
                Id = id,
                UvOrigin = new Vector3(-1f, -1f, -1f),
                UVector = Vector3.Up * 2f,
                VVector = Vector3.Back * 2f
            },
            var _ => new Side
            {
                Id = id,
                UvOrigin = new Vector3(-1f, 1f, -1f),
                UVector = Vector3.Right * 2f,
                VVector = Vector3.Back * 2f
            }
        };
    }
}