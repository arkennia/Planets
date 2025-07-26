using System;
using System.Threading.Tasks;
using Godot;


namespace Planets.SystemGenerator.Terrain;

[GlobalClass]
public partial class Noise2DTerrain : Terrain3D
{
    [Export]
    public bool UseSeamless { get; set; } = true;

    [Export]
    public bool GenerateLods { get; set; } = false;

    [Export]
    public float WaterLevel { get; set; } = 0.2f;

    [Export]
    public float MountainLevel { get; set; } = 0.7f;

    [Export]
    public VisualShader Shader { get; set; } =
        ResourceLoader.Load<VisualShader>("res://materials/shader_materials/2Dto3D/noise2d.tres");

    [Export]
    public ShaderMaterial Material { get; set; } = null;

    [Export]
    public RDShaderFile VertexHeightShader { get; set; } =
        ResourceLoader.Load<RDShaderFile>("res://materials/shader_materials/get_vertex_heights_2d.glsl");

    [Export]
    public RDShaderFile HeightMapGeneratorShader { get; set; } =
        ResourceLoader.Load<RDShaderFile>("res://materials/shader_materials/generate_heightmap.glsl");

    [Export]
    public int HeightMapDimensions { get; set; } = 128;

    [Export]
    public int MoistureMapDimensions { get; set; } = 128;

    [Export]
    public ImageTexture HeightMap { get; private set; }


    private ComputeShaderImage _hShaderImage;//_nShaderImage1, _nShaderImage2;
    private NoiseImageSize _hSize, _mSize;
    private FastNoiseLite _noise1, _noise2, _mNoise;
    private ImageTexture _heightMap, _moistureMap, _nImage1, _nImage2;
    private float[] _heights;
    private int[] _vIdxes;
    private ArrayMesh _mesh;
    private RenderingDevice _rd;

    public override void _EnterTree()
    {
        RandomNumberGenerator rng = new();

        rng.Seed = Seed;
        _noise1 ??= new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            FractalGain = 0.4f,
            FractalOctaves = 6,
            FractalLacunarity = 2.0f,
            // DomainWarpEnabled = true,
            Frequency = 0.01f,
            Seed = (int)rng.Randi()
        };
        _noise2 ??= new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            // FractalGain = 0.4f,
            // FractalOctaves = 6,
            // FractalLacunarity = 2.0f,
            // DomainWarpEnabled = true,
            Frequency = 0.03f,
            Seed = (int)rng.Randi()
        };

        _mNoise ??= new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            // DomainWarpEnabled = true,
            FractalOctaves = 4,
            // FractalGain = 0.5f,
            // Frequency = 0.007f,
            // FractalLacunarity = 1.9f,
            Seed = (int)rng.Randi()
        };
        _Generate();
    }

    public override void Generate(bool generateLods, ShaderMaterial shaderMaterial)
    {
        GenerateLods = generateLods;
        Material = shaderMaterial;
        // _generateCalled = true;
        _Generate();
    }

    private void _Generate()
    {
        _mesh = new ArrayMesh();
        _mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, Mesh.SurfaceGetArrays(0));
        if (GenerateLods) _mesh = _GenerateLoDs(_mesh);
        Mesh = _GenerateNoise(_mesh);
        if (Material is null)
        {
            Material = new ShaderMaterial();
            Material.Shader = Shader;
        }

        SetSurfaceOverrideMaterial(0, Material);
    }

    private ArrayMesh _GenerateNoise(ArrayMesh mesh)
    {
        _rd = RenderingServer.CreateLocalRenderingDevice();
        _hSize = new NoiseImageSize(HeightMapDimensions);
        _mSize = new NoiseImageSize(MoistureMapDimensions);
        _InitImages();
        MeshDataTool mdt = new();
        mdt.CreateFromSurface(mesh, 0);
        int vCount = mdt.GetVertexCount();
        // _GenerateHeightmap();
        _GenerateVertexHeights(vCount, mdt);

        // Parallel.For(0, vCount, i =>
        // {
        //     Vector3 vert = mdt.GetVertex(i);
        //     float height = _heights[i];
        //     // float m = _SampleNoise(Moisture, vert);
        //     Vector3 vertN = mdt.GetVertexNormal(i);
        //
        //     vert += vertN * height * .5f;
        //
        //     mdt.SetVertex(i, vert);
        //     mdt.SetVertexNormal(i, Vector3.Zero);
        //     // mdt.SetVertexColor(i, _GetColor(height, m));
        // });
        // // //
        // Parallel.For(0, mdt.GetFaceCount(), i =>
        // {
        //     int ia = mdt.GetFaceVertex(i, 0);
        //     int ib = mdt.GetFaceVertex(i, 1);
        //     int ic = mdt.GetFaceVertex(i, 2);
        //
        //     Vector3 e1 = mdt.GetVertex(ia) - mdt.GetVertex(ib);
        //     Vector3 e2 = mdt.GetVertex(ic) - mdt.GetVertex(ib);
        //     Vector3 normal = e1.Cross(e2);
        //
        //     mdt.SetVertexNormal(ia, (mdt.GetVertexNormal(ia) + normal).Normalized());
        //     mdt.SetVertexNormal(ib, (mdt.GetVertexNormal(ib) + normal).Normalized());
        //     mdt.SetVertexNormal(ic, (mdt.GetVertexNormal(ic) + normal).Normalized());
        // });
        mesh.ClearSurfaces();
        mdt.CommitToSurface(mesh);
        HeightMap = _heightMap;
        return mesh;
    }

    private void _InitImages()
    {
        _heightMap = ImageTexture.CreateFromImage(_noise1.GetImage(_hSize.Width, _hSize.Height, in3DSpace: true));
        _nImage1 = ImageTexture.CreateFromImage(_noise1.GetImage(_hSize.Width, _hSize.Height, in3DSpace: true));
        _nImage2 = ImageTexture.CreateFromImage(_noise2.GetImage(_hSize.Width, _hSize.Height, in3DSpace: true));

        _moistureMap = ImageTexture.CreateFromImage(_mNoise.GetImage(_hSize.Width, _hSize.Height, in3DSpace: true));
    }

    private void _GenerateVertexHeights(int vertexCount, MeshDataTool mdt)
    {
        int binding = 0;
        float[] heights = new float[vertexCount];
        float[] verts = new float[vertexCount * 4];
        int[] vertIndexes = new int[vertexCount];
        for (int i = 0, j = 0; i < vertexCount; i++, j += 4)
        {
            Vector3 v = mdt.GetVertex(i);
            // verts[i] = new Vector4(v.X, v.Y, v.Z, 1.0f);
            verts[j] = v.X;
            verts[j + 1] = v.Y;
            verts[j + 2] = v.Z;
            verts[j + 3] = i;
            vertIndexes[i] = i;
        }

        RDShaderFile shader = VertexHeightShader;
        RDShaderSpirV shaderSpirv = shader.GetSpirV();
        Rid shaderRid = _rd.ShaderCreateFromSpirV(shaderSpirv);

        // Height Map Data and Sampler
        // byte[] imgBytes = _heightMap.GetImage().GetData();
        _hShaderImage = ComputeShaderImage.CreateWithSampler(_rd, _heightMap.GetImage(), _hSize, binding++);

        RDSamplerState hSampler = new();
        Rid samplerRid = _rd.SamplerCreate(hSampler);
        _hShaderImage.Unif.AddId(samplerRid);
        _hShaderImage.Unif.AddId(_hShaderImage.Rid);
        _rd.TextureUpdate(_hShaderImage.Rid, 0, _hShaderImage.ImgBytes.ToArray());


        // Height buffer
        byte[] heightBytes = new byte[heights.Length * sizeof(float)];
        Buffer.BlockCopy(heights, 0, heightBytes, 0, heightBytes.Length);

        Rid hBuffer = _rd.StorageBufferCreate((uint)heightBytes.Length, heightBytes);

        RDUniform hBufferUnif = new()
        {
            UniformType = RenderingDevice.UniformType.StorageBuffer,
            Binding = binding++,
        };

        hBufferUnif.AddId(hBuffer);

        // Vertex Buffers
        byte[] vertBytes = new byte[verts.Length * sizeof(float)];

        Buffer.BlockCopy(verts, 0, vertBytes, 0, vertBytes.Length);

        Rid vBuffer = _rd.StorageBufferCreate((uint)vertBytes.Length, vertBytes);

        RDUniform vBufferUnif = new()
        {
            UniformType = RenderingDevice.UniformType.StorageBuffer,
            Binding = binding++,
        };

        vBufferUnif.AddId(vBuffer);

        Rid uniformSet = _rd.UniformSetCreate(
            [_hShaderImage.Unif, hBufferUnif, vBufferUnif],
            shaderRid, 0);

        Rid pipeline = _rd.ComputePipelineCreate(shaderRid);
        long computeBegin = _rd.ComputeListBegin();
        _rd.ComputeListBindComputePipeline(computeBegin, pipeline);
        _rd.ComputeListBindUniformSet(computeBegin, uniformSet, 0);
        _rd.ComputeListDispatch(computeBegin, (uint)_hSize.Width / 32, (uint)_hSize.Height, 1);
        _rd.ComputeListEnd();
        _rd.Submit();
        _rd.Sync();

        byte[] heightBufferOut = _rd.BufferGetData(hBuffer);
        Buffer.BlockCopy(heightBufferOut, 0, heights, 0, heightBufferOut.Length);

        byte[] hMapBytes = _rd.TextureGetData(_hShaderImage.Rid, 0);
        _heightMap = ImageTexture.CreateFromImage(Image.CreateFromData(_hSize.Width, _hSize.Height, false,
            Image.Format.L8, hMapBytes));
        HeightMap = _heightMap;


        // _rd.FreeRid(sampler1Rid);
        // _rd.FreeRid(sampler2RId);
        // _rd.FreeRid(_nShaderImage1.Rid);
        // _rd.FreeRid(_nShaderImage2.Rid);
        _rd.FreeRid(hBuffer);
        _rd.FreeRid(vBuffer);
        _rd.FreeRid(shaderRid);
        _heights = heights;
        _vIdxes = vertIndexes;
    }

    private void _GenerateHeightmap()
    {
        int binding = 0;
        RDShaderFile shader = HeightMapGeneratorShader;
        RDShaderSpirV shaderSpirv = shader.GetSpirV();
        Rid shaderRid = _rd.ShaderCreateFromSpirV(shaderSpirv);

        _hShaderImage = ComputeShaderImage.Create(_rd, _heightMap.GetImage(), _hSize, binding++);
        // _hShaderImage.Unif.AddId(_hShaderImage.Rid);

        byte[] heightBytes = _heightMap.GetImage().GetData();

        // Buffer.BlockCopy(_heights, 0, heightBytes, 0, heightBytes.Length);
        // _heightMap =
        //     ImageTexture.CreateFromImage(Image.CreateFromData(_hSize.Width, _hSize.Height, false, Image.Format.L8,
        //         heightBytes));
        // HeightMap = _heightMap;

        Rid hBuffer = _rd.StorageBufferCreate((uint)heightBytes.Length, heightBytes);

        RDUniform hBufferUnif = new()
        {
            UniformType = RenderingDevice.UniformType.StorageBuffer,
            Binding = binding++,
        };

        hBufferUnif.AddId(hBuffer);

        byte[] vIdxBVytes = new byte[_heights.Length * sizeof(int)];

        Buffer.BlockCopy(_vIdxes, 0, vIdxBVytes, 0, vIdxBVytes.Length);
        // _heightMap =
        //     ImageTexture.CreateFromImage(Image.CreateFromData(_hSize.Width, _hSize.Height, false, Image.Format.L8,
        //         heightBytes));
        // HeightMap = _heightMap;

        Rid vBuffer = _rd.StorageBufferCreate((uint)vIdxBVytes.Length, vIdxBVytes);

        RDUniform vBufferUnif = new()
        {
            UniformType = RenderingDevice.UniformType.StorageBuffer,
            Binding = binding++,
        };

        vBufferUnif.AddId(vBuffer);
        Rid uniformSet = _rd.UniformSetCreate([_hShaderImage.Unif, hBufferUnif, vBufferUnif],
            shaderRid, 0);

        Rid pipeline = _rd.ComputePipelineCreate(shaderRid);
        long computeBegin = _rd.ComputeListBegin();
        _rd.ComputeListBindComputePipeline(computeBegin, pipeline);
        _rd.ComputeListBindUniformSet(computeBegin, uniformSet, 0);
        _rd.ComputeListDispatch(computeBegin, (uint)_hSize.Width / 32, (uint)_hSize.Height, 1);
        _rd.ComputeListEnd();
        _rd.Submit();
        _rd.Sync();

        byte[] hMapBytes = _rd.TextureGetData(_hShaderImage.Rid, 0);
        _heightMap = ImageTexture.CreateFromImage(Image.CreateFromData(_hSize.Width, _hSize.Height, false,
            Image.Format.L8, hMapBytes));
        HeightMap = _heightMap;

        _rd.FreeRid(_hShaderImage.Rid);
        _rd.FreeRid(hBuffer);
        _rd.FreeRid(shaderRid);
    }

    private ArrayMesh _GenerateLoDs(ArrayMesh mesh)
    {
        ImporterMesh im = new();
        im.AddSurface(Mesh.PrimitiveType.Triangles, mesh.SurfaceGetArrays(0));
        im.GenerateLods(45.0f, 0, null);
        mesh.ClearSurfaces();
        Godot.Collections.Dictionary lods = new();
        Parallel.For(0, im.GetSurfaceLodCount(0),
            i => lods[im.GetSurfaceLodSize(0, i)] = im.GetSurfaceLodIndices(0, i));
        GD.Print("LoDs Generated.");
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, im.GetSurfaceArrays(0), null, lods);
        return mesh;
    }
}