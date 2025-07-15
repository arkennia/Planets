using Godot;
using System;
using System.Threading.Tasks;

namespace Planets.SystemGenerator
{
    [GlobalClass]
    public partial class Planet : Resource, ICelestialBody
    {
        [Export] public string Name { get; set; } = "Earth";
        [Export] public string Area3DName { get; private set; }
        [Export] public int Area3DExtraSpace { get; set; } = 3000;
        public MeshInstance3D MeshInstance { get; private set; }
        [Export] public int Scale { get; set; }
        [Export] public int Resolution { get; set; }
        [Export] public bool Generated { get; private set; }
        [Export] public float Gravity { get; set; } = 9.8f;
        [Export] public Vector2 Sector { get; private set; } = Vector2.Zero;
        [Export] public Vector3 SectorLocation { get; private set; } = Vector3.Zero;
        [Export] public NoiseTexture3D NoiseTexture { get; set; }

        [Export]
        public ShaderMaterial ShaderMaterial { get; set; } =
            ResourceLoader.Load<ShaderMaterial>("res://materials/shader_materials/planet_material.tres");

        public const int NOISE_SIZE = 128;

        public Guid Guid { get; private set; } = Guid.Empty;

        // private Mesh _mesh;


        public Planet()
        {
            if (Guid == Guid.Empty)
            {
                Guid = Guid.NewGuid();
            }
        }

        public Planet(string name = "Earth", Mesh mesh = null, int scale = 500, int resolution = 128)
        {
            Name = name;
            if (mesh is not null)
            {
                MeshInstance = new MeshInstance3D
                {
                    Mesh = mesh,
                    Name = name
                };
            }

            Scale = scale;
            Resolution = resolution;
            Guid = Guid.Empty;
        }

        public PlanetNode Generate()
        {
            if (MeshInstance == null)
            {
                CubeSphere cs = new CubeSphere
                {
                    Resolution = Resolution,
                    Scale = Scale
                };
                ArrayMesh arrayMesh = cs.Generate();
                GD.Print("Mesh loaded.");
                Mesh m = GenerateNoise((ArrayMesh)arrayMesh.Duplicate());
                m.SurfaceSetMaterial(0, ShaderMaterial);
                GD.Print("Noise generated");
                MeshInstance = new MeshInstance3D
                {
                    Name = Name,
                    Mesh = m,
                    //Scale = new Vector3(Scale, Scale, Scale),
                };
            }

            PlanetNode rootNode = new PlanetNode();

            // MeshInstance3D mI = new()
            // {
            //     Mesh = _mesh,
            //     Name = Name,
            // };
            // MeshInstance = mI;
            MeshInstance.SetSurfaceOverrideMaterial(0, ShaderMaterial);
            // MeshInstance.SetInstanceShaderParameter("noiseTexture", NoiseTexture);

            StaticBody3D sB = new StaticBody3D
            {
                CollisionLayer = 0b10,
            };

            Area3D area = new Area3D
            {
                GravitySpaceOverride = Area3D.SpaceOverride.Replace,
                GravityPoint = true,
                GravityPointUnitDistance = Scale,
                Gravity = Gravity,
                GravityDirection = new Vector3(0, -1, 0)
            };

            SphereShape3D areaColliderShape = new SphereShape3D
            {
                Radius = Scale + Area3DExtraSpace
            };
            CollisionShape3D areaCollider = new CollisionShape3D
            {
                Shape = areaColliderShape
            };


            // SphereShape3D colliderShape = new()
            // {
            //     Radius = Scale + 8
            // };
            ConcavePolygonShape3D colliderShape = new ConcavePolygonShape3D();
            colliderShape.SetFaces(MeshInstance.Mesh.GetFaces());
            CollisionShape3D collider = new CollisionShape3D
            {
                Shape = colliderShape,
            };

            rootNode.Planet = this;
            rootNode.AddChild(MeshInstance);
            rootNode.AddChild(area);
            area.Owner = rootNode;
            area.AddChild(areaCollider);
            areaCollider.Owner = rootNode;

            MeshInstance.Owner = rootNode;
            MeshInstance.AddChild(sB);

            sB.Owner = rootNode;
            sB.AddChild(collider);

            collider.Owner = rootNode;
            Generated = true;

            Area3DName = area.Name;
            GD.Print("Planet generation complete.");
            // _mesh = null;
            return rootNode;
        }

        public static Vector3 GetRandomSurfacePosition(int scale)
        {
            RandomNumberGenerator rng = new RandomNumberGenerator();
            // var vert = rng.RandiRange(0, _mesh)
            Vector3 v = new Vector3(rng.Randfn(), rng.Randfn(), rng.Randfn());
            v = v.Normalized();
            return v * scale;
        }

        public void Save(string path = "res://resources")
        {
            ResourceSaver.Save(this, $"{path}/{Guid}.res", ResourceSaver.SaverFlags.Compress);
        }

        private ArrayMesh GenerateNoise(ArrayMesh arrayMesh)
        {
            RandomNumberGenerator rng = new ();
            FastNoiseLite noise1 = new()
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex,
                FractalGain = 0.4f,
                FractalOctaves = 6,
                Seed = (int)rng.Randi(),
            };
            FastNoiseLite noise2 = new()
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
                DomainWarpEnabled = true,
                Seed = (int)rng.Randi(),
            };
            FastNoiseLite noise3 = new()
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
                DomainWarpEnabled = true,
                Seed = (int)rng.Randi(),
            };
            // NoiseTexture = new NoiseTexture3D() { Noise = noise1 }

            MeshDataTool mdt = new MeshDataTool();
            mdt.CreateFromSurface(arrayMesh, 0);
            Parallel.For(0, mdt.GetVertexCount(), i =>
            {
                Vector3 vert = mdt.GetVertex(i);
                float n = SampleNoise(noise1, vert);
                // n /= (1.0f + 0.5f + 0.25f);
                Vector3 vertN = mdt.GetVertexNormal(i);
                vert += vertN * Mathf.Pow(n * 1.2f, 8.0f) * 15f;
                //vert += vertN * n;
                mdt.SetVertex(i, vert);
                mdt.SetVertexNormal(i, Vector3.Zero);
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (n <= 0.4)
                {
                    mdt.SetVertexColor(i, new Color(0.1f, 0.3f, 0.5f));
                }
                else
                {
                    mdt.SetVertexColor(i, new Color("GREEN"));
                }
            });
            // for (int i = 0; i < mdt.GetVertexCount(); i++)
            // {
            //     Vector3 vert = mdt.GetVertex(i);
            //     float n = SampleNoise(noise1, vert);
            //     // n /= (1.0f + 0.5f + 0.25f);
            //     Vector3 vertN = mdt.GetVertexNormal(i);
            //     vert += vertN * Mathf.Pow(n * 1.5f, 8.0f) * 10f;
            //     //vert += vertN * n;
            //     mdt.SetVertex(i, vert);
            //     mdt.SetVertexNormal(i, Vector3.Zero);
            //     // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            //     if (n <= 0.4)
            //     {
            //         mdt.SetVertexColor(i, new Color(0.1f, 0.3f, 0.5f));
            //     }
            //     else
            //     {
            //         mdt.SetVertexColor(i, new Color("GREEN"));
            //     }
            // }
            
            // for (int i = 0; i < mdt.GetVertexCount(); i++)
            // {
            //     //var v = mdt.GetVertex(i);
            //     int[] faces = mdt.GetVertexFaces(i);
            //     Vector3[] vNormals = new Vector3[faces.Length];
            //     for (int j = 0; j < faces.Length; j++)
            //     {
            //         vNormals[j] = mdt.GetFaceNormal(faces[j]);
            //     }
            //     Vector3 vertN = vNormals.Aggregate(Vector3.Zero, (sum, x) => sum + x);
            //     mdt.SetVertexNormal(i, vertN.Normalized());
            // }
            Parallel.For(0, mdt.GetFaceCount(), i =>
            {
                int ia = mdt.GetFaceVertex(i, 0);
                int ib = mdt.GetFaceVertex(i, 1);
                int ic = mdt.GetFaceVertex(i, 2);
                
                Vector3 e1 = mdt.GetVertex(ia) - mdt.GetVertex(ib);
                Vector3 e2 = mdt.GetVertex(ic) - mdt.GetVertex(ib);
                Vector3 normal = e1.Cross(e2);
                
                mdt.SetVertexNormal(ia, (mdt.GetVertexNormal(ia) + normal).Normalized());
                mdt.SetVertexNormal(ib, (mdt.GetVertexNormal(ib) + normal).Normalized());
                mdt.SetVertexNormal(ic, (mdt.GetVertexNormal(ic) + normal).Normalized());
            });
            // for (int i = 0; i < mdt.GetFaceCount(); i++)
            // {
            //     
            // }

            arrayMesh.ClearSurfaces();
            mdt.CommitToSurface(arrayMesh);
            return arrayMesh;
        }

        private static float SampleNoise(FastNoiseLite noise, Vector3 v) => (noise.GetNoise3Dv(v) + 1) / 2;
    }
}