using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Planets.SystemGenerator
{
    public partial class CubeSphere : Mesh
    {

        public int Scale { get; set; }

        public int Resolution { get; set; }

        public int Sides { get; set; }

        public string MeshName { get; set; } = "CubeSphere";

        struct Side
        {
            public int id;
            public Vector3 uvOrigin, uVector, vVector;
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

        public CubeSphere(int scale = 1000, int resolution = 32, string meshName = "CubeSphere", int sides = 6)
        {
            Scale = scale;
            Resolution = resolution;
            MeshName = meshName;
            Sides = sides;
        }

        public ArrayMesh Generate(bool saveToFile = false)
        {
            SurfaceTool surfaceTool = new();
            List<Vector3> verts = [.. new Vector3[VertexCount]];

            surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

            surfaceTool = GenerateMesh(surfaceTool);

            surfaceTool.GenerateNormals();
            surfaceTool.GenerateTangents();
            surfaceTool.Index();

            _m = surfaceTool.Commit();

            if (saveToFile)
                ResourceSaver.Save(_m, $"res://{MeshName}.res", ResourceSaver.SaverFlags.Compress);
            return _m;
        }

        public void Save()
        {
            ResourceSaver.Save(_m, $"res://{MeshName}.res", ResourceSaver.SaverFlags.Compress);
        }

        private SurfaceTool GenerateMesh(SurfaceTool surfaceTool)
        {
            for (int i = 0; i < JobLength; i++)
            {
                int u = i / Sides;
                Side side = GetSide(i - Sides * u);

                Vector3 uA = side.uvOrigin + side.uVector * (u) / Resolution;
                Vector3 uB = side.uvOrigin + side.uVector * (u + 1) / Resolution;
                Vector3 pA = uA.Normalized();
                Vector3 pB = uB.Normalized();

                int vi = 4 * Resolution * (Resolution * side.id + u);
                int ti = 2 * Resolution * (Resolution * side.id + u);


                for (int v = 1; v <= Resolution; v++, vi += 4, ti += 2)
                {
                    Vector3 pC = (uA + side.vVector * v / Resolution).Normalized();
                    Vector3 pD = (uB + side.vVector * v / Resolution).Normalized();

                    // verts[vi + 1] = pA;
                    // uvs.Add(Vector2.Zero);
                    // verts[vi] = pB;
                    // uvs.Add(new Vector2(1f, 0f));
                    // verts[vi + 2] = pC;
                    // uvs.Add(Vector2.One);
                    // verts[vi + 3] = pD;
                    // uvs.Add(new Vector2(0f, 1f));
                    surfaceTool.SetUV(Vector2.Zero);
                    surfaceTool.AddVertex(pB * Scale);
                    surfaceTool.SetUV(new Vector2(1f, 0f));
                    surfaceTool.AddVertex(pA * Scale);
                    surfaceTool.SetUV(Vector2.One);
                    surfaceTool.AddVertex(pC * Scale);
                    surfaceTool.SetUV(new Vector2(0f, 1f));
                    surfaceTool.AddVertex(pD * Scale);
                    surfaceTool.AddIndex(vi + 3);
                    surfaceTool.AddIndex(vi + 2);
                    surfaceTool.AddIndex(vi);
                    surfaceTool.AddIndex(vi + 2);
                    surfaceTool.AddIndex(vi + 1);
                    surfaceTool.AddIndex(vi);

                    pA = pC;
                    pB = pD;

                    // List<int> t1 = [vi + 3, vi + 2, vi];
                    // List<int> t2 = [vi + 2, vi + 1, vi];


                    // indices.InsertRange(ti, t1.Concat(t2));
                }
                // surfaceTool.AddTriangleFan([.. verts], [.. uvs]);
            }
            return surfaceTool;
        }

        static Side GetSide(int id) => id switch
        {
            0 => new Side
            {
                id = id,
                uvOrigin = new Vector3(-1f, -1f, -1f),
                uVector = Vector3.Right * 2f,
                vVector = Vector3.Up * 2f
            },
            1 => new Side
            {
                id = id,
                uvOrigin = new Vector3(1f, -1f, -1f),
                uVector = Vector3.Back * 2f,
                vVector = Vector3.Up * 2f,

            },
            2 => new Side
            {
                id = id,
                uvOrigin = new Vector3(-1f, -1f, -1f),
                uVector = Vector3.Back * 2f,
                vVector = Vector3.Right * 2f,

            },
            3 => new Side
            {
                id = id,
                uvOrigin = new Vector3(-1f, -1f, 1f),
                uVector = Vector3.Up * 2f,
                vVector = Vector3.Right * 2f,

            },
            4 => new Side
            {
                id = id,
                uvOrigin = new Vector3(-1f, -1f, -1f),
                uVector = Vector3.Up * 2f,
                vVector = Vector3.Back * 2f,

            },
            _ => new Side
            {
                id = id,
                uvOrigin = new Vector3(-1f, 1f, -1f),
                uVector = Vector3.Right * 2f,
                vVector = Vector3.Back * 2f,

            }
        };

    }
}