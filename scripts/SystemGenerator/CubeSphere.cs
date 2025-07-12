using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Planets.SystemGenerator
{
    public partial class CubeSphere(int scale = 1000, int resolution = 32, int sides = 6) : Mesh
    {

        public int Scale { get; set; } = scale;

        public int Resolution { get; set; } = resolution;

        public int Sides { get; set; } = sides;
        public const string FOLDER_PATH = "res://meshes/planets";

        //public string MeshName { get; set; } = meshName;

        static List<GeneratedCubeSphere> _generatedCubeSpheres = null;



        struct Side
        {
            public int id;
            public Vector3 uvOrigin, uVector, vVector;
        }

        struct GeneratedCubeSphere
        {
            public int scale;
            public int resolution;
            public string path;

            public static bool operator ==(GeneratedCubeSphere left, GeneratedCubeSphere right)
            {
                return left.Equals(right);
            }
            public static bool operator !=(GeneratedCubeSphere left, GeneratedCubeSphere right)
            {
                return !left.Equals(right);
            }

            public override readonly bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                    return false;

                var comp = (GeneratedCubeSphere)obj;
                return scale == comp.scale && resolution == comp.resolution;
            }

            public override readonly int GetHashCode()
            {
                return HashCode.Combine(scale, resolution);
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

            var csg = Exists(Scale, Resolution);
            if (csg != default)
            {
                _m = (ArrayMesh)ResourceLoader.Load(csg.path);
                GD.Print($"Cubesphere mesh found with {Scale} scale and {Resolution} resolution.");
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
                //_m = GenerateNoise(_m);
                GD.Print($"Cubesphere mesh generated with {Scale} scale and {Resolution} resolution.");
            }
            Save();
            return _m;
        }

        public void Save()
        {
            ResourceSaver.Save(_m, $"{FOLDER_PATH}/{Resolution}_{Scale}_CubeSphere.res", ResourceSaver.SaverFlags.Compress);
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
                }

            }
            return surfaceTool;
        }

        private static GeneratedCubeSphere Exists(int scale, int resolution)
        {
            var tmp = new GeneratedCubeSphere() { scale = scale, resolution = resolution };
            var csg = _generatedCubeSpheres.FindIndex(x => x.Equals(tmp));
            if (csg != -1)
            {
                return _generatedCubeSpheres[csg];
            }
            else return default;
        }

        private static void LoadGeneratedCubeSpheres()
        {
            var dir = DirAccess.Open(FOLDER_PATH);
            foreach (var cs in dir.GetFiles())
            {
                if (cs.EndsWith("res"))
                {
                    var split = cs.Split("_");
                    int res = split[0].ToInt();
                    int scale = split[1].ToInt();
                    _generatedCubeSpheres.Add(new GeneratedCubeSphere
                    {
                        scale = scale,
                        resolution = res,
                        path = $"{dir.GetCurrentDir()}/{cs}"
                    });
                }
            }
            // GD.Print($"Generated cubespheres: \n {_generatedCubeSpheres}");
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