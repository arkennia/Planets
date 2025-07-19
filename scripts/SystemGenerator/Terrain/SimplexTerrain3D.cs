using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace Planets.SystemGenerator.Terrain;

[GlobalClass]
public partial class SimplexTerrain3D : Terrain3D
{
    [Export]
    public bool UseSeamless { get; set; } = true;

    [Export]
    public FastNoiseLite Noise1 { get; set; }

    [Export]
    public FastNoiseLite Noise2 { get; set; }


    [Export]
    public FastNoiseLite Noise3 { get; set; }

    [Export]
    public FastNoiseLite Moisture { get; set; }

    [Export]
    public NoiseImageSize NoiseImageSize { get; set; }

    [Export]
    public Gradient Gradient { get; set; } = _CreateDefaultGradient();

    [Export]
    private RDShaderFile ComputeShader { get; set; } =
        ResourceLoader.Load<RDShaderFile>("res://materials/shader_materials/compute_heightmap.glsl");

    private struct ComputeShaderImage
    {
        public Rid Rid;
        public RDTextureFormat Format;
        public RDUniform Unif;
        public List<byte> ImgBytes;
        public int Binding;
    }

    public SimplexTerrain3D()
    {
        // Colors = new TerrainColor();
        _CreateNoise();
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    public override void Generate(bool generateLods, ShaderMaterial shaderMaterial)
    {
        ArrayMesh arrayMesh = new();
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, Mesh.SurfaceGetArrays(0));

        GD.Print("Mesh loaded.");
        if (generateLods) arrayMesh = _GenerateLoDs(arrayMesh);
        Mesh m = _GenerateNoise((ArrayMesh)arrayMesh.Duplicate());
        m.SurfaceSetMaterial(0, shaderMaterial);
        // MeshInstance.SetInstanceShaderParameter("noise1", NoiseTexture1);
        // MeshInstance.SetInstanceShaderParameter("noise2", NoiseTexture2);
        // MeshInstance.SetInstanceShaderParameter("noise3", NoiseTexture3);
        // MeshInstance.SetInstanceShaderParameter("moisture", MoistureTexture);
        GD.Print("Noise generated");
        Mesh = m;
    }

    private static Gradient _CreateDefaultGradient()
    {
        Gradient g = new();
        g.AddPoint(0.6f, new Color(0.9f, 0.9f, 0.9f));
        g.AddPoint(0.8f, new Color(1.0f, 1.0f, 1.0f));
        g.Reverse();
        return g;
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

    private static ComputeShaderImage _CreateComputeShaderImage(RenderingDevice rd, Array<Image> img,
        NoiseImageSize size, int binding)
    {
        List<byte> imgBytes = _CreateImageByteArray(img);
        RDTextureFormat format = new()
        {
            Format = RenderingDevice.DataFormat.R8Unorm,
            Width = (uint)size.Width,
            Height = (uint)size.Height,
            Depth = (uint)size.Depth,
            TextureType = RenderingDevice.TextureType.Type3D,
            UsageBits = RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.CanUpdateBit |
                        RenderingDevice.TextureUsageBits.CanCopyFromBit
        };

        Rid rid = rd.TextureCreate(format, new RDTextureView());
        RDUniform unif = new()
        {
            UniformType = RenderingDevice.UniformType.Image,
            Binding = binding
        };
        unif.AddId(rid);
        return new ComputeShaderImage
        {
            Format = format,
            Rid = rid,
            Unif = unif,
            ImgBytes = imgBytes,
            Binding = binding
        };
    }

    private static List<byte> _CreateImageByteArray(Array<Image> img)
    {
        List<byte> imgBytes = [];
        foreach (Image imgX in img) imgBytes.AddRange(imgX.GetData());
        return imgBytes;
    }

    private static Array<Image> _CreateNoiseImage(FastNoiseLite noise, NoiseImageSize size)
    {
        return noise.GetImage3D(size.Width, size.Height, size.Depth);
    }

    private static Array<Image> _CreateNoiseImageSeamless(FastNoiseLite noise, NoiseImageSize size)
    {
        return noise.GetSeamlessImage3D(size.Width, size.Height, size.Depth);
    }


    private ArrayMesh _GenerateNoise(ArrayMesh arrayMesh)
    {
        // Array<Image> img1 = Noise1.GetImage3D(64, 64, 64);

        // List<byte> imgBytes = [];
        // foreach (Image imgX in img1) imgBytes.AddRange(imgX.GetData());

        NoiseImageSize size = new(64);
        Array<Image> img1 = _CreateNoiseImage(Noise1, size);
        Before = new ImageTexture3D();
        Before.Create(Image.Format.L8, 64, 64, 64, false, img1);

        RenderingDevice rd = RenderingServer.CreateLocalRenderingDevice();
        RDShaderFile shader = ComputeShader;
        RDShaderSpirV shaderSpirv = shader.GetSpirV();
        Rid shaderRid = rd.ShaderCreateFromSpirV(shaderSpirv);

        GradientTexture1D gradTex = new()
        {
            Gradient = Gradient
        };

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

        ComputeShaderImage csi = _CreateComputeShaderImage(rd, img1, size, 0);

        Rid uniformSet = rd.UniformSetCreate([csi.Unif, gradUnif], shaderRid, 0);
        Rid pipeline = rd.ComputePipelineCreate(shaderRid);
        // The things above are expensive to make and should be stored for future runs....somehow.

        // imgTex1.Create(Image.Format.L8, 64, 64, 64, true, img1);

        rd.TextureUpdate(csi.Rid, 0, csi.ImgBytes.ToArray());

        long computeBegin = rd.ComputeListBegin();
        rd.ComputeListBindComputePipeline(computeBegin, pipeline);
        rd.ComputeListBindUniformSet(computeBegin, uniformSet, 0);
        rd.ComputeListDispatch(computeBegin, 8, 8, 8);
        rd.ComputeListEnd();
        rd.Submit();
        rd.Sync();
        byte[] outBytes = rd.TextureGetData(csi.Rid, 0);
        Array<Image> images = [];
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
            float n1 = _SampleNoise(Noise1, vert * 1.1f);
            float n2 = _SampleNoise(Noise2, vert * 0.9f);
            float n3 = _SampleNoise(Noise3, vert * 0.9f);

            float n = n1 * 1.0f + n2 * 0.33f + n3 * 0.1f;
            n /= 1.0f + 0.33f + 0.1f;
            float height = Mathf.Pow(n * 1.3f, 3.6f);
            heightMap[i] = height;
        });
        Parallel.For(0, vCount, i =>
        {
            Vector3 vert = mdt.GetVertex(i);
            float height = _AdjustHeight(mdt, i, heightMap[i]);
            float m = _SampleNoise(Moisture, vert);
            Vector3 vertN = mdt.GetVertexNormal(i);
            vert += vertN * height * 20f;
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
            return Colors.DeepWater;
        if (height < 0.15)
            return Colors.Water;
        if (height < 0.2)
            return Colors.Desert;

        if (height > 0.8)
        {
            if (m < 0.2) return Colors.MountainSide;
            return m < 0.6 ? Colors.Tundra : Colors.Snow;
        }

        if (height > 0.5)
        {
            if (m < 0.33) return Colors.Desert;
            return m < 0.66 ? Colors.Shrubland : Colors.Forest;
        }

        if (height > 0.4)
        {
            if (m < 0.2) return Colors.Desert;
            if (m < 0.4) return Colors.Grassland;
            if (m < 0.83) return Colors.Jungle;
        }

        if (m < 0.16) return Colors.Savannah;
        return m < 0.33 ? Colors.Grassland : Colors.Jungle;
    }

    private void _CreateNoise()
    {
        RandomNumberGenerator rng = new();

        rng.Seed = Seed;

        Noise1 ??= new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            FractalGain = 0.4f,
            FractalOctaves = 3,
            FractalLacunarity = 2.0f,
            DomainWarpEnabled = true,
            Frequency = 0.007f,
            Seed = (int)rng.Randi()
        };

        Noise2 ??= new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            DomainWarpEnabled = true,
            // FractalLacunarity = 1.9f,
            Frequency = 0.009f,
            Seed = (int)rng.Randi()
        };
        Noise3 ??= new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            DomainWarpEnabled = true,
            // FractalLacunarity = 1.9f,
            Frequency = 0.009f,
            Seed = (int)rng.Randi()
        };
        Moisture ??= new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            // DomainWarpEnabled = true,
            FractalOctaves = 4,
            // FractalGain = 0.5f,
            // Frequency = 0.007f,
            // FractalLacunarity = 1.9f,
            Seed = (int)rng.Randi()
        };
    }
}