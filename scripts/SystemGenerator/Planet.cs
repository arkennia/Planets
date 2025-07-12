using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Planets.SystemGenerator
{
    [GlobalClass]
    public partial class Planet : Resource, ICelestialBody
    {
        [Export]
        public string Name { get; set; } = "Earth";
        [Export]
        public string Area3DName { get; private set; }
        [Export]
        public int Area3DExtraSpace { get; set; } = 3000;
        public MeshInstance3D MeshInstance { get; set; }
        [Export]
        public int Scale { get; set; }
        [Export]
        public int Resolution { get; set; }
        [Export]
        public bool Generated { get; private set; }
        [Export]
        public float Gravity { get; set; } = 9.8f;
        [Export]
        public Vector2 Sector { get; private set; } = Vector2.Zero;
        [Export]
        public Vector3 SectorLocation { get; private set; } = Vector3.Zero;
        [Export]
        public NoiseTexture3D NoiseTexture { get; set; }
        [Export]
        public ShaderMaterial ShaderMaterial { get; set; } = ResourceLoader.Load<ShaderMaterial>("res://materials/shader_materials/planet_material.tres");

        public const int NOISE_SIZE = 128;

        public Guid Guid { get; private set; } = Guid.Empty;

        private Mesh _mesh;




        public Planet()
        {
            if (Guid == Guid.Empty)
            {
                Guid = Guid.NewGuid();
            }
        }

        public Planet(string name = "Earth", Mesh mesh = null, int scale = 20000, int resolution = 128)
        {
            Name = name;
            _mesh = mesh;
            Scale = scale;
            Resolution = resolution;
            Guid = new Guid();
        }

        public PlanetNode Generate()
        {
            if (MeshInstance == null)
            {
                CubeSphere cs = new()
                {
                    Resolution = Resolution,
                    Scale = Scale
                };
                ArrayMesh arrayMesh = cs.Generate();
                GD.Print("Mesh loaded.");
                _mesh = GenerateNoise((ArrayMesh)arrayMesh.Duplicate());
                _mesh.SurfaceSetMaterial(0, ShaderMaterial);
                GD.Print("Noise generated");
            }
            PlanetNode rootNode = new();

            MeshInstance3D mI = new()
            {
                Mesh = _mesh,
                Name = Name,
            };
            MeshInstance = mI;
            mI.SetSurfaceOverrideMaterial(0, ShaderMaterial);
            mI.SetInstanceShaderParameter("noiseTexture", NoiseTexture);

            StaticBody3D sB = new();

            Area3D area = new()
            {
                GravitySpaceOverride = Area3D.SpaceOverride.Replace,
                GravityPoint = true,
                GravityPointUnitDistance = Scale,
                Gravity = Gravity,
                GravityDirection = new Vector3(0, -1, 0)
            };

            SphereShape3D areaColliderShape = new()
            {
                Radius = Scale + Area3DExtraSpace
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
            colliderShape.SetFaces(_mesh.GetFaces());
            CollisionShape3D collider = new()
            {
                Shape = colliderShape,
            };

            rootNode.Planet = this;
            rootNode.AddChild(mI);
            rootNode.AddChild(area);
            area.Owner = rootNode;
            area.AddChild(areaCollider);
            areaCollider.Owner = rootNode;

            mI.Owner = rootNode;
            mI.AddChild(sB);

            sB.Owner = rootNode;
            sB.AddChild(collider);

            collider.Owner = rootNode;
            Generated = true;

            Area3DName = area.Name;
            GD.Print("Planet generation complete.");
            return rootNode;
        }

        public void Save(string path = "res://resources")
        {
            ResourceSaver.Save(this, $"{path}/{Guid}.res", ResourceSaver.SaverFlags.Compress);
        }

        private ArrayMesh GenerateNoise(ArrayMesh arrayMesh)
        {
            Vector3 vert;
            float n;
            Vector3 vert_n;
            FastNoiseLite noise = new();
            NoiseTexture = new NoiseTexture3D() { Noise = noise };
            RandomNumberGenerator rng = new();
            // var x = noise.GetNoise2D(5, 5);
            // var x_norm = x - (-1) / (1 - (-1));
            MeshDataTool mdt = new();
            mdt.CreateFromSurface(arrayMesh, 0);

            for (int i = 0; i < mdt.GetVertexCount(); i++)
            {
                vert = mdt.GetVertex(i);
                n = 1.0f * SampleNoise(noise, 1 * vert) + 0.5f * SampleNoise(noise, 2 * vert) + 0.25f * SampleNoise(noise, 4 * vert);
                n /= (1.0f + 0.5f + 0.25f);
                vert_n = mdt.GetVertexNormal(i);
                vert += vert_n * Mathf.Pow(n * 1.2f, 5.0f) * 20f;
                mdt.SetVertex(i, vert);
                // mdt.SetVertexNormal(i, Vector3.Zero);
                if (n <= 0.4)
                {
                    mdt.SetVertexColor(i, new Color(0.1f, 0.3f, 0.5f));
                }
                else
                {
                    mdt.SetVertexColor(i, new Color("GREEN"));
                }
            }

            // for (int i = 0; i < mdt.GetVertexCount() - 1; i++)
            // {
            //     var v = mdt.GetVertex(i);
            //     var faces = mdt.GetVertexFaces(i);
            //     Vector3[] normals = new Vector3[faces.Length];
            //     for (int j = 0; j < faces.Length; j++)
            //     {
            //         var a = mdt.GetFaceVertex(faces[j], 0);
            //         var b = mdt.GetFaceVertex(faces[j], 1);
            //         var c = mdt.GetFaceVertex(faces[j], 2);

            //         var ap = mdt.GetVertex(a);
            //         var bp = mdt.GetVertex(b);
            //         var cp = mdt.GetVertex(c);

            //         normals[j] = (bp - cp).Cross(ap - bp).Normalized();
            //     }
            //     vert_n = Enumerable.Aggregate(normals, Vector3.Zero, (sum, x) => sum + x) / normals.Length;
            //     mdt.SetVertexNormal(i, vert_n.Normalized());
            // }
            Godot.Collections.Array arrays = [];
            arrays.Resize((int)Mesh.ArrayType.Max);
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            for (int i = 0; i < mdt.GetFaceCount(); i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int v = mdt.GetFaceVertex(i, j);
                    verts.Add(mdt.GetVertex(v));
                    uvs.Add(mdt.GetVertexUV(v));
                    normals.Add(mdt.GetFaceNormal(i));
                    colors.Add(mdt.GetVertexColor(v));
                }
            }
            arrayMesh.ClearSurfaces();

            arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
            arrays[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
            arrays[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
            arrays[(int)Mesh.ArrayType.Color] = colors.ToArray();
            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

            //mdt.CommitToSurface(arrayMesh);

            return arrayMesh;
        }

        private static float SampleNoise(FastNoiseLite noise, Vector3 v) => (noise.GetNoise3Dv(v) + 1) / 2;

    }
}
