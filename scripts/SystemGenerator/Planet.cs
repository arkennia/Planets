using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable once RedundantUsingDirective
using Godot.Collections;

namespace Planets.SystemGenerator;

[GlobalClass]
public partial class Planet : Resource, ICelestialBody
{
    [Export]
    public string Name { get; set; } = "Earth";

    [Export]
    public string Area3DName { get; private set; }

    [Export]
    public int Area3DExtraSpace { get; set; } = 3000;

    [Export]
    public float Gravity { get; set; } = 9.8f;

    [Export]
    public int Radius { get; set; }

    [Export]
    public int Resolution { get; set; }

    [Export]
    public bool Generated { get; private set; }

    [Export]
    public bool GenerateLods { get; set; }

    [Export]
    public Vector2 Sector { get; private set; } = Vector2.Zero;

    [Export]
    public Vector3 SectorLocation { get; private set; } = Vector3.Zero;

    [Export]
    public ShaderMaterial ShaderMaterial { get; set; } =
        ResourceLoader.Load<ShaderMaterial>("res://materials/shader_materials/planet_material.tres");

    [ExportGroup("Biome Colors")]
    [Export]
    public Color DeepWater { get; set; } = new(0.0f, 0.04f, 0.17f);

    [Export]
    public Color Water { get; set; } = new(0.1f, 0.3f, 0.5f);

    [Export]
    public Color Forest { get; set; } = new(0.0f, 0.5f, 0.06f);

    [Export]
    public Color Jungle { get; set; } = new(0.0f, 0.2f, 0.01f);

    [Export]
    public Color Savannah { get; set; } = new(0.608f, 0.453f, 0.168f);

    [Export]
    public Color Shrubland { get; set; } = new(0.255f, 0.605f, 0.255f);

    [Export]
    public Color Grassland { get; set; } = new(0.15f, 0.42f, 0.137f);

    [Export]
    public Color Desert { get; set; } = new(0.7f, 0.54f, 0.21f);

    [Export]
    public Color MountainSide { get; set; } = new(0.2f, 0.2f, 0.1f);

    [Export]
    public Color Tundra { get; set; } = new(0.55f, 0.55f, 0.55f);

    [Export]
    public Color Snow { get; set; } = new(0.97f, 0.96f, 0.91f);

    [Export]
    public ImageTexture3D Before { get; private set; }

    [Export]
    public ImageTexture3D After { get; private set; }


    // public NoiseTexture3D NoiseTexture1 { get; private set; }
    // public NoiseTexture3D NoiseTexture2 { get; private set; }
    // public NoiseTexture3D NoiseTexture3 { get; private set; }
    // public NoiseTexture3D MoistureTexture { get; private set; }


    public MeshInstance3D MeshInstance { get; private set; }
    public Guid Guid { get; private set; } = Guid.Empty;

    private FastNoiseLite _noise1;
    private FastNoiseLite _noise2;
    private FastNoiseLite _noise3;
    private FastNoiseLite _moisture;

    private Mesh _mesh;

    public Planet()
    {
        // if (Guid == Guid.Empty) Guid = Guid.NewGuid();
    }

    public Planet(string name = "Earth", Mesh mesh = null, int radius = 500, int resolution = 128)
    {
        Name = name;
        _mesh = mesh;
        Radius = radius;
        Resolution = resolution;
        Guid = Guid.Empty;
    }

    public PlanetNode Generate()
    {
        ArrayMesh arrayMesh;
        if (_mesh == null)
        {
            CubeSphere cs = new()
            {
                Resolution = Resolution,
                Radius = Radius
            };
            arrayMesh = cs.Generate();
        }
        else
        {
            arrayMesh = new ArrayMesh();
            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, _mesh.SurfaceGetArrays(0));
        }

        GD.Print("Mesh loaded.");
        if (GenerateLods) arrayMesh = _GenerateLoDs(arrayMesh);
        Mesh m = _GenerateNoise((ArrayMesh)arrayMesh.Duplicate());
        m.SurfaceSetMaterial(0, ShaderMaterial);
        // MeshInstance.SetInstanceShaderParameter("noise1", NoiseTexture1);
        // MeshInstance.SetInstanceShaderParameter("noise2", NoiseTexture2);
        // MeshInstance.SetInstanceShaderParameter("noise3", NoiseTexture3);
        // MeshInstance.SetInstanceShaderParameter("moisture", MoistureTexture);
        GD.Print("Noise generated");

        MeshInstance = new MeshInstance3D
        {
            Name = Name,
            Mesh = m
            // Scale = new Vector3(Scale, Scale, Scale),
        };

        PlanetNode rootNode = new();

        // if (ShaderMaterial.Duplicate() is ShaderMaterial material)
        // {
        //     MeshInstance.SetSurfaceOverrideMaterial(0, material);
        //     material.SetShaderParameter("noise1", NoiseTexture1);
        //     material.SetShaderParameter("noise2", NoiseTexture2);
        //     material.SetShaderParameter("noise3", NoiseTexture3);
        //     material.SetShaderParameter("moisture", MoistureTexture);
        // }

        StaticBody3D sB = new()
        {
            CollisionLayer = 0b10
        };

        Area3D area = new()
        {
            GravitySpaceOverride = Area3D.SpaceOverride.Replace,
            GravityPoint = true,
            GravityPointUnitDistance = Radius,
            Gravity = Gravity,
            GravityDirection = new Vector3(0, -1, 0)
        };

        SphereShape3D areaColliderShape = new()
        {
            Radius = Radius + Area3DExtraSpace
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

        colliderShape.SetFaces(MeshInstance.Mesh.GetFaces());
        CollisionShape3D collider = new()
        {
            Shape = colliderShape
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

        // _mesh = null;
        return rootNode;
    }

    public static Vector3 GetRandomSurfacePosition(int scale)
    {
        RandomNumberGenerator rng = new();
        // var vert = rng.RandiRange(0, _mesh)
        Vector3 v = new(rng.Randfn(), rng.Randfn(), rng.Randfn());
        v = v.Normalized();
        return v * scale;
    }

    public void Save(string path = "res://resources")
    {
        ResourceSaver.Save(this, $"{path}/{Guid}.res", ResourceSaver.SaverFlags.Compress);
    }

    private static ArrayMesh _GenerateLoDs(ArrayMesh arrayMesh)
    {
        ImporterMesh im = new();
        im.AddSurface(Mesh.PrimitiveType.Triangles, arrayMesh.SurfaceGetArrays(0));
        im.GenerateLods(45.0f, 0, null);
        arrayMesh.ClearSurfaces();
        Dictionary lods = new();
        Parallel.For(0, im.GetSurfaceLodCount(0),
            i => lods[im.GetSurfaceLodSize(0, i)] = im.GetSurfaceLodIndices(0, i));
        GD.Print("LoDs Generated.");
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, im.GetSurfaceArrays(0), null, lods);
        return arrayMesh;
    }

    private ArrayMesh _GenerateNoise(ArrayMesh arrayMesh)
    {
        RandomNumberGenerator rng = new();

        rng.Seed = 42069;
        _noise1 = new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            FractalGain = 0.4f,
            FractalOctaves = 3,
            FractalLacunarity = 2.0f,
            DomainWarpEnabled = true,
            Frequency = 0.007f,
            Seed = (int)rng.Randi()
        };
        _noise2 = new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            DomainWarpEnabled = true,
            // FractalLacunarity = 1.9f,
            Frequency = 0.009f,
            Seed = (int)rng.Randi()
        };
        _noise3 = new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            DomainWarpEnabled = true,
            // FractalLacunarity = 1.9f,
            Frequency = 0.009f,
            Seed = (int)rng.Randi()
        };
        _moisture = new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            // DomainWarpEnabled = true,
            FractalOctaves = 4,
            // FractalGain = 0.5f,
            // Frequency = 0.007f,
            // FractalLacunarity = 1.9f,
            Seed = (int)rng.Randi()
        };
        // NoiseTexture = new NoiseTexture3D() { Noise = noise1 }

        // NoiseTexture1 = new NoiseTexture3D { Noise = noise1 };
        // NoiseTexture2 = new NoiseTexture3D { Noise = noise2 };
        // NoiseTexture3 = new NoiseTexture3D { Noise = noise3 };
        // MoistureTexture = new NoiseTexture3D { Noise = moisture };

        RenderingDevice rd = RenderingServer.CreateLocalRenderingDevice();
        Gradient g = new();
        g.AddPoint(0.6f, new Color(0.9f, 0.9f, 0.9f));
        g.AddPoint(0.8f, new Color(1.0f, 1.0f, 1.0f));
        g.Reverse();

        GradientTexture1D gradTex = new()
        {
            Gradient = g
        };

        RDShaderFile shader =
            ResourceLoader.Load<RDShaderFile>("res://materials/shader_materials/compute_heightmap.glsl");
        RDShaderSpirV shaderSpirv = shader.GetSpirV();
        Rid shaderRid = rd.ShaderCreateFromSpirV(shaderSpirv);

        RDTextureFormat heightmapFormat = new()
        {
            Format = RenderingDevice.DataFormat.R8Unorm,
            Width = 64,
            Height = 64,
            Depth = 64,
            TextureType = RenderingDevice.TextureType.Type3D,
            UsageBits = RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.CanUpdateBit |
                        RenderingDevice.TextureUsageBits.CanCopyFromBit
        };

        Rid heightmapRid = rd.TextureCreate(heightmapFormat, new RDTextureView());
        RDUniform heightmapUnif = new()
        {
            UniformType = RenderingDevice.UniformType.Image,
            Binding = 0
        };
        heightmapUnif.AddId(heightmapRid);

        RDTextureFormat gradFormat = new()
        {
            Format = RenderingDevice.DataFormat.R8G8B8A8Unorm,
            Width = (uint)gradTex.Width,
            Height = 1,
            UsageBits = RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.CanUpdateBit
        };
        Rid gradRid = rd.TextureCreate(gradFormat, new RDTextureView(), [gradTex.GetImage().GetData()]);
        RDUniform gradUnif = new()
        {
            UniformType = RenderingDevice.UniformType.Image,
            Binding = 1
        };
        gradUnif.AddId(gradRid);

        Rid uniformSet = rd.UniformSetCreate([heightmapUnif, gradUnif], shaderRid, 0);
        Rid pipeline = rd.ComputePipelineCreate(shaderRid);
        // The things above are expensive to make and should be stored for future runs....somehow.
        var img1 = _noise1.GetImage3D(64, 64, 64);
        Before = new ImageTexture3D();
        Before.Create(Image.Format.L8, 64, 64, 64, false, img1);
        List<byte> imgBytes = [];
        foreach (Image imgX in img1) imgBytes.AddRange(imgX.GetData());

        // imgTex1.Create(Image.Format.L8, 64, 64, 64, true, img1);

        rd.TextureUpdate(heightmapRid, 0, imgBytes.ToArray());

        long computeBegin = rd.ComputeListBegin();
        rd.ComputeListBindComputePipeline(computeBegin, pipeline);
        rd.ComputeListBindUniformSet(computeBegin, uniformSet, 0);
        rd.ComputeListDispatch(computeBegin, 8, 8, 8);
        rd.ComputeListEnd();
        rd.Submit();
        rd.Sync();
        byte[] outBytes = rd.TextureGetData(heightmapRid, 0);
        Array<Image> images = new();
        images.Resize(64);
        const int wh = 64 * 64;
        for (int z = 0; z < 64; z++)
        {
            byte[] buffer = new byte[wh];
            System.Array.Copy(outBytes, z * wh, buffer, 0, wh);
            images[z] = Image.CreateFromData(64, 64, false, Image.Format.L8, buffer);
        }

        After = new ImageTexture3D();
        After.Create(Image.Format.L8, 64, 64, 64, false, images);

        // var img1 = _noise1.GetImage3D(64, 64, 64);
        // var img2 = _noise2.GetImage3D(64, 64, 64);
        // var img3 = _noise3.GetImage3D(64, 64, 64);


        MeshDataTool mdt = new();
        mdt.CreateFromSurface(arrayMesh, 0);
        int vCount = mdt.GetVertexCount();

#if DEBUG
        GD.Print($"Num faces: {mdt.GetFaceCount()}");
        GD.Print($"Num Vertices: {vCount}");
#endif

        float[] heightMap = new float[vCount];
        Parallel.For(0, vCount, i =>
        {
            Vector3 vert = mdt.GetVertex(i);
            float n1 = _SampleNoise(_noise1, vert * 1.1f);
            float n2 = _SampleNoise(_noise2, vert * 0.9f);
            float n3 = _SampleNoise(_noise3, vert * 0.9f);

            float n = n1 * 1.0f + n2 * 0.33f + n3 * 0.1f;
            n /= 1.0f + 0.33f + 0.1f;
            float height = Mathf.Pow(n * 1.3f, 3.6f);
            heightMap[i] = height;
        });
        Parallel.For(0, vCount, i =>
        {
            Vector3 vert = mdt.GetVertex(i);
            float height = _AdjustHeight(mdt, i, heightMap[i]);
            float m = _SampleNoise(_moisture, vert);
            Vector3 vertN = mdt.GetVertexNormal(i);
            vert += vertN * height; // * 20f;
            // vert += vertN * n;
            mdt.SetVertex(i, vert);
            mdt.SetVertexNormal(i, Vector3.Zero);
            mdt.SetVertexColor(i, _GetColor(height, m));
        });

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

    private float _AdjustHeight(MeshDataTool mdt, int vIdx, float height)
    {
        // int[] faces = mdt.GetVertexFaces(vIdx);
        // Vector3 v = mdt.GetVertex(vIdx);
        // HashSet<int> vertices = [];
        // foreach (int t in faces)
        //     for (int j = 0; j < 3; j++)
        //         vertices.Add(mdt.GetFaceVertex(t, j));
        // 
        // 
        // GD.Print($"Num Faces: {faces.Length}");
        return height;
    }

    private Color _GetColor(float height, float m)
    {
        if (height < 0.1)
            return DeepWater;
        if (height < 0.15)
            return Water;
        if (height < 0.2)
            return Desert;

        if (height > 0.8)
        {
            if (m < 0.2) return MountainSide;
            return m < 0.6 ? Tundra : Snow;
        }

        if (height > 0.5)
        {
            if (m < 0.33) return Desert;
            return m < 0.66 ? Shrubland : Forest;
        }

        if (height > 0.4)
        {
            if (m < 0.2) return Desert;
            if (m < 0.4) return Grassland;
            if (m < 0.83) return Jungle;
        }

        if (m < 0.16) return Savannah;
        return m < 0.33 ? Grassland : Jungle;
    }

    private static float _SampleNoise(FastNoiseLite noise, Vector3 v)
    {
        return (noise.GetNoise3Dv(v) + 1) / 2;
    }

    private static float _SampleNoise(Array<Image> img, Vector3 v, int width, int height = -1, int depth = -1)
    {
        // float nx = v.X / width;
        // float ny = v.Y / (height != -1 ? height : width);
        // float nz = v.Z / (depth != -1 ? depth : width);
        // float texel = img[nz].GetPixel(nx, ny);
        // img[0].SetP
        return 0.0f;
    }

    // private static bool _IsPowerOfTwo(ulong x)
    // {
    //     return x != 0 && (x & (x - 1)) == 0;
    // }
}