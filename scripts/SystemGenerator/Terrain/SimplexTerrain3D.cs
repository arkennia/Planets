using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

// ReSharper disable NotAccessedField.Local

namespace Planets.SystemGenerator.Terrain;

[GlobalClass]
public partial class SimplexTerrain3D : Terrain3D
{
    [Export]
    public bool UseSeamless { get; set; } = true;

    [Export]
    public float WaterLevel { get; set; } = 0.2f;

    [Export]
    public float MountainLevel { get; set; } = 0.7f;


    [Export]
    public RDShaderFile ComputeShaderImages { get; set; } =
        ResourceLoader.Load<RDShaderFile>("res://materials/shader_materials/compute_heightmap.glsl");

    [Export]
    public RDShaderFile VertexHeightShader { get; set; } =
        ResourceLoader.Load<RDShaderFile>("res://materials/shader_materials/get_vertex_heights.glsl");

    [Export]
    public RDShaderFile ComputerNormalsShader { get; set; } =
        ResourceLoader.Load<RDShaderFile>("res://materials/shader_materials/compute_normalmap.glsl");


    [ExportGroup("Image Sizes")]
    [Export]
    public NoiseImageSize HeightmapSize { get; set; } = new();

    [Export]
    public NoiseImageSize Noise1ImageSize { get; set; } = new();

    [Export]
    public NoiseImageSize Noise2ImageSize { get; set; } = new();

    [Export]
    public NoiseImageSize Noise3ImageSize { get; set; } = new();

    [Export]
    public NoiseImageSize MoistureImageSize { get; set; } = new();

    // ReSharper disable once MemberCanBePrivate.Global
    public float[] Heights { get; private set; }
    private FastNoiseLite Noise1 { get; set; }
    private FastNoiseLite Noise2 { get; set; }
    private FastNoiseLite Noise3 { get; set; }
    private FastNoiseLite Moisture { get; set; }
    private NoiseImages Images { get; set; }
    private Gradient Gradient { get; set; } = _CreateDefaultGradient();

    private enum NoiseImage
    {
        Noise1 = 0,
        Noise2 = 1,
        Noise3 = 2,
        Moisture = 3
    }

    private RenderingDevice _rd;
    private int _wh;
    private ShaderMaterial _material;
    private bool _generateLods;
    private bool _generateCalled;
    private ArrayMesh _mesh;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (_generateCalled && !Generated)
        {
            // _Generate();
            Generated = true;
        }
    }
// #if DEBUG
//         else
//         {
//             GD.Print($"{Name} already generated!");
//         }
//     }
// #endif

    public override void Generate(bool generateLods, ShaderMaterial shaderMaterial)
    {
        if (Noise1 is null)
            _CreateNoise();
        _generateLods = generateLods;
        _material = shaderMaterial;
        _generateCalled = true;
        Images = new NoiseImages();
        _Generate();
    }

    private void _Generate()
    {
        // _material = shaderMaterial;
        _mesh = new ArrayMesh();
        _mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, Mesh.SurfaceGetArrays(0));
        GD.Print("Mesh loaded.");

        if (_generateLods) _mesh = _GenerateLoDs(_mesh);
        Mesh = _GenerateNoise(_mesh.Duplicate() as ArrayMesh);
        // m.SurfaceSetMaterial(0, _material);
        // MeshInstance.SetInstanceShaderParameter("noise1", NoiseTexture1);
        // MeshInstance.SetInstanceShaderParameter("noise2", NoiseTexture2);
        // MeshInstance.SetInstanceShaderParameter("noise3", NoiseTexture3);
        _material.SetShaderParameter("Moisture", Images.Moisture);
        _material.SetShaderParameter("Heights", Heights);
        GD.Print("Noise generated");
        // Mesh = m;
        SetSurfaceOverrideMaterial(0, _material);
        // _material.SetShaderParameter("heightMapDim", HeightmapSize.Depth);
        _generateCalled = false;
    }

    private static Gradient _CreateDefaultGradient()
    {
        Gradient g = new();
        g.AddPoint(0.5f, new Color(0.9f, 0.9f, 0.9f));
        g.AddPoint(0.8f, new Color(1.0f, 1.0f, 1.0f));
        // g.Reverse();
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

    // private static float _SampleNoise(Array<Image> img, Vector3 v, int width, int height = -1, int depth = -1)
    // {
    //     // float nx = v.X / width;
    //     // float ny = v.Y / (height != -1 ? height : width);
    //     // float nz = v.Z / (depth != -1 ? depth : width);
    //     // float texel = img[nz].GetPixel(nx, ny);
    //     // img[0].SetP
    //     return 0.0f;
    // }

    /*
        private static ComputeShaderImage _CreateComputeShaderImageSampler(RenderingDevice rd, Array<Image> img,
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
                            RenderingDevice.TextureUsageBits.CanCopyFromBit | RenderingDevice.TextureUsageBits.SamplingBit
            };

            Rid rid = rd.TextureCreate(format, new RDTextureView());
            RDUniform unif = new()
            {
                UniformType = RenderingDevice.UniformType.SamplerWithTexture,
                Binding = binding
            };
            RDSamplerState state = new();
            Rid samplerRid = rd.SamplerCreate(state);
            unif.AddId(samplerRid);
            unif.AddId(rid);
            return new ComputeShaderImage
            {
                Format = format,
                Rid = rid,
                Unif = unif,
                ImgBytes = imgBytes,
                Binding = binding,
            };
        }
    */

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

    private static void _UpdateNoiseTexture(RenderingDevice rd, ComputeShaderImage tex)
    {
        rd.TextureUpdate(tex.Rid, 0, tex.ImgBytes.ToArray());
    }

    // private Array<Image> _GetImageFromGPU(RenderingDevice rd, ComputeShaderImage noiseTex, uint layer = 0)
    // {
    //     byte[] bytes = rd.TextureGetData(noiseTex.Rid, layer);
    //     Array<Image> images = [];
    //     images.Resize(ImageSize.Depth);
    //     for (int z = 0; z < ImageSize.Depth; z++)
    //     {
    //         byte[] buffer = new byte[_wh];
    //         System.Array.Copy(bytes, z * _wh, buffer, 0, _wh);
    //         images[z] = Image.CreateFromData(ImageSize.Width, ImageSize.Height, false, Image.Format.L8, buffer);
    //     }
    // 
    //     return images;
    // }

    private Array<Image>[] _CreateNoiseImages()
    {
        Array<Image>[] imgs = new Array<Image>[4];
        if (UseSeamless)
        {
            var n1 = Task.Run(() => _CreateNoiseImageSeamless(Noise1, Noise1ImageSize));
            var n2 = Task.Run(() => _CreateNoiseImageSeamless(Noise2, Noise2ImageSize));
            var n3 = Task.Run(() => _CreateNoiseImageSeamless(Noise3, Noise3ImageSize));
            var m = Task.Run(() => _CreateNoiseImageSeamless(Moisture, MoistureImageSize));
            Task.WaitAll(n1, n2, n3, m);
            imgs[(int)NoiseImage.Noise1] = n1.Result;
            imgs[(int)NoiseImage.Noise2] = n2.Result;
            imgs[(int)NoiseImage.Noise3] = n3.Result;
            imgs[(int)NoiseImage.Moisture] = m.Result;
        }
        else
        {
            var n1 = Task.Run(() => _CreateNoiseImage(Noise1, Noise1ImageSize));
            var n2 = Task.Run(() => _CreateNoiseImage(Noise2, Noise2ImageSize));
            var n3 = Task.Run(() => _CreateNoiseImage(Noise3, Noise3ImageSize));
            var m = Task.Run(() => _CreateNoiseImage(Moisture, MoistureImageSize));
            Task.WaitAll(n1, n2, n3, m);
            imgs[(int)NoiseImage.Noise1] = n1.Result;
            imgs[(int)NoiseImage.Noise2] = n2.Result;
            imgs[(int)NoiseImage.Noise3] = n3.Result;
            imgs[(int)NoiseImage.Moisture] = m.Result;
        }

        return imgs;
    }

    private void _SetNoiseImages(Array<Image>[] imgs, Array<Image> heightMap, Array<Image> normalMap)
    {
        Images = new NoiseImages();
        Images.Noise1 = new ImageTexture3D();

        Images.Noise1.Create(Image.Format.L8, Noise1ImageSize.Width, Noise1ImageSize.Height, Noise1ImageSize.Depth,
            false, imgs[0]);

        Images.Noise2 = new ImageTexture3D();
        Images.Noise2.Create(Image.Format.L8, Noise2ImageSize.Width, Noise2ImageSize.Height, Noise2ImageSize.Depth,
            false, imgs[1]);

        Images.Noise3 = new ImageTexture3D();
        Images.Noise3.Create(Image.Format.L8, Noise3ImageSize.Width, Noise3ImageSize.Height, Noise3ImageSize.Depth,
            false, imgs[2]);

        Images.Moisture = new ImageTexture3D();
        Images.Moisture.Create(Image.Format.L8, MoistureImageSize.Width, MoistureImageSize.Height,
            MoistureImageSize.Depth, false, imgs[3]);

        Images.HeightMap = new ImageTexture3D();
        Images.HeightMap.Create(Image.Format.L8, HeightmapSize.Width, HeightmapSize.Height, HeightmapSize.Depth, false,
            heightMap);

        Images.NormalMap = new ImageTexture3D();
        Images.NormalMap.Create(Image.Format.Rgba8, HeightmapSize.Width, HeightmapSize.Height, HeightmapSize.Depth,
            false,
            normalMap);
    }


    private void _ComputeNoiseWithImages(Array<Image>[] noiseImages, Array<Image> heightMap)
    {
        RDShaderFile shader = ComputeShaderImages;
        RDShaderSpirV shaderSpirv = shader.GetSpirV();
        Rid shaderRid = _rd.ShaderCreateFromSpirV(shaderSpirv);

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
        Rid gradRid = _rd.TextureCreate(gradFormat, new RDTextureView(), [gradTex.GetImage().GetData()]);
        RDUniform gradUnif = new()
        {
            UniformType = RenderingDevice.UniformType.Image,
            Binding = 0
        };
        gradUnif.AddId(gradRid);
        ComputeShaderImage noiseTex1 =
            ComputeShaderImage.Create(_rd, noiseImages[(int)NoiseImage.Noise1], Noise1ImageSize, 1);
        ComputeShaderImage noiseTex2 =
            ComputeShaderImage.Create(_rd, noiseImages[(int)NoiseImage.Noise2], Noise2ImageSize, 2);
        ComputeShaderImage noiseTex3 =
            ComputeShaderImage.Create(_rd, noiseImages[(int)NoiseImage.Noise3], Noise3ImageSize, 3);
        // ComputeShaderImage moistureTex =
        //     _CreateComputeShaderImage(rd, noiseImages[(int)NoiseImage.Moisture], MoistureImageSize, 4);
        ComputeShaderImage heightmapTex =
            ComputeShaderImage.Create(_rd, heightMap, HeightmapSize, 4);

        Rid uniformSet =
            _rd.UniformSetCreate(
                [noiseTex1.Unif, noiseTex2.Unif, noiseTex3.Unif, heightmapTex.Unif, gradUnif],
                shaderRid, 0);
        Rid pipeline = _rd.ComputePipelineCreate(shaderRid);
        // The things above are expensive to make and should be stored for future runs....somehow.

        // rd.TextureUpdate(noise1Rid, 0, noise1Bytes);
        // rd.TextureUpdate(heightmapRid, 0, heightMap.GetImage().GetData());
        // imgTex1.Create(Image.Format.L8, 64, 64, 64, true, img1);

        // rd.TextureUpdate(noiseTex1.Rid, 0, noiseTex1.ImgBytes.ToArray());

        _UpdateNoiseTexture(_rd, noiseTex1);
        _UpdateNoiseTexture(_rd, noiseTex2);
        _UpdateNoiseTexture(_rd, noiseTex3);
        // _UpdateNoiseTexture(rd, moistureTex);
        _UpdateNoiseTexture(_rd, heightmapTex);


        long computeBegin = _rd.ComputeListBegin();
        _rd.ComputeListBindComputePipeline(computeBegin, pipeline);
        _rd.ComputeListBindUniformSet(computeBegin, uniformSet, 0);
        _rd.ComputeListDispatch(computeBegin, (uint)(HeightmapSize.Width / 8),
            (uint)(HeightmapSize.Height / 8),
            (uint)(HeightmapSize.Height / 8));
        _rd.ComputeListEnd();
        _rd.Submit();
        _rd.Sync();
        // byte[,] outBytes = new byte[_NUM_NOISE_IMAGES, wh];
        byte[] outBytes = _rd.TextureGetData(heightmapTex.Rid, 0);
        Array<Image> hImages = [];
        hImages.Resize(HeightmapSize.Depth);
        for (int z = 0; z < HeightmapSize.Depth; z++)
        {
            byte[] buffer = new byte[_wh];
            System.Array.Copy(outBytes, z * _wh, buffer, 0, _wh);
            hImages[z] =
                Image.CreateFromData(HeightmapSize.Width, HeightmapSize.Height, false, Image.Format.L8, buffer);
        }

        Images.HeightMap.Update(hImages);

        _rd.FreeRid(gradRid);
        _rd.FreeRid(noiseTex1.Rid);
        _rd.FreeRid(noiseTex2.Rid);
        _rd.FreeRid(noiseTex3.Rid);
        _rd.FreeRid(heightmapTex.Rid);
        _rd.FreeRid(shaderRid);
        // _rd.FreeRid(pipeline);
        // _rd.FreeRid(uniformSet);
    }


    private ArrayMesh _GenerateNoise(ArrayMesh arrayMesh)
    {
        // Array<Image> img1 = Noise1.GetImage3D(64, 64, 64);

        // List<byte> imgBytes = [];
        // foreach (Image imgX in img1) imgBytes.AddRange(imgX.GetData());

        // NoiseImageSize size = new(64);
        // Array<Image> img1 = _CreateNoiseImage(Noise1, size);

        // _wh = HeightmapSize.Width * HeightmapSize.Height;
        // Array<Image>[] noiseImages = _CreateNoiseImages();
        // GD.Print("Noise Images created.");
        // 
        // Array<Image> normalMap = new();
        // normalMap.Resize(HeightmapSize.Depth);
        // Parallel.For(0, HeightmapSize.Depth,
        //     i => normalMap[i] = Image.CreateEmpty(HeightmapSize.Width, HeightmapSize.Height,
        //         false, Image.Format.Rgba8));
        // 
        // 
        // Array<Image> heightMap = new();
        // heightMap.Resize(HeightmapSize.Depth);
        // Parallel.For(0, HeightmapSize.Depth,
        //     i => heightMap[i] = Image.CreateEmpty(HeightmapSize.Width, HeightmapSize.Height,
        //         false, Image.Format.L8));
        // _SetNoiseImages(noiseImages, heightMap, normalMap);
        // GD.Print("Noise and heightmap set.");
        // 
        // _rd = RenderingServer.CreateLocalRenderingDevice();
        MeshDataTool mdt = new();
        mdt.CreateFromSurface(arrayMesh, 0);
        int vCount = mdt.GetVertexCount();
        Images.Moisture = new ImageTexture3D();
        Images.Moisture.Create(Image.Format.L8, MoistureImageSize.Width, MoistureImageSize.Height,
            MoistureImageSize.Depth, false,
            Moisture.GetImage3D(MoistureImageSize.Width, MoistureImageSize.Height, MoistureImageSize.Depth));
        // _ComputeNoiseWithImages(noiseImages, heightMap);
        // GD.Print("Noise compute shader finished.");
        // Heights = _GetVertexHeights(vCount, mdt);
        // // normalMap = _ComputeNormals();
        // // ImageTexture3D nMap = new();
        // // nMap.Create(Image.Format.Rgba8, HeightmapSize.Width, HeightmapSize.Height, HeightmapSize.Depth,
        // //     false, normalMap
        // // );
        // 
        // // }
        // _ComputeNormals();
        // _material.SetShaderParameter("heightMap", Images.HeightMap);
        // _material.SetShaderParameter("moisture", Images.Moisture);
        // _material.SetShaderParameter("normalMap", Images.NormalMap);
        // _material.SetShaderParameter("waterLevel", WaterLevel);
        // _material.SetShaderParameter("mountainLevel", MountainLevel);

#if DEBUG
        GD.Print($"Num faces: {mdt.GetFaceCount()}");
        GD.Print($"Num Vertices: {vCount}");
#endif
        _material.SetShaderParameter("VertexCount", vCount);
        Heights = new float[vCount];
        Parallel.For(0, vCount, i =>
        {
            Vector3 vert = mdt.GetVertex(i);
            // float height = Heights[i];
            float n1 = _SampleNoise(Noise1, vert);
            float n2 = _SampleNoise(Noise2, vert * 2f);
            float n3 = _SampleNoise(Noise3, vert * 3f);
            // float m = _SampleNoise(Moisture, vert);
            float h = n1 * 1f + n2 * 0.2f + n3 * 0.1f;
            Vector3 vertN = mdt.GetVertexNormal(i);
            Heights[i] = h;

            vert += vertN * h * .5f;

            mdt.SetVertex(i, vert);
            mdt.SetVertexNormal(i, Vector3.Zero);
            // mdt.SetVertexColor(i, _GetColor(height, m));
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
        arrayMesh.ClearSurfaces();
        mdt.CommitToSurface(arrayMesh);
        return arrayMesh;
    }

    private float[] _GetVertexHeights(int vertexCount, MeshDataTool mdt)
    {
        float[] heights = new float[vertexCount];
        float[] verts = new float[vertexCount * 4];
        Array<RDUniform> uniforms = [];
        uniforms.Resize(4);
        for (int i = 0, j = 0; i < vertexCount; i++, j += 4)
        {
            Vector3 v = mdt.GetVertex(i);
            // verts[i] = new Vector4(v.X, v.Y, v.Z, 1.0f);
            verts[j] = v.X;
            verts[j + 1] = v.Y;
            verts[j + 2] = v.Z;
            verts[j + 3] = i;
        }

        RDShaderFile shader = VertexHeightShader;
        RDShaderSpirV shaderSpirv = shader.GetSpirV();
        Rid shaderRid = _rd.ShaderCreateFromSpirV(shaderSpirv);

        // Height Map Data and Sampler
        List<byte> imgBytes = _CreateImageByteArray(Images.HeightMap.GetData());
        RDTextureFormat format = new()
        {
            Format = RenderingDevice.DataFormat.R8Unorm,
            Width = (uint)HeightmapSize.Width,
            Height = (uint)HeightmapSize.Height,
            Depth = (uint)HeightmapSize.Depth,
            TextureType = RenderingDevice.TextureType.Type3D,
            UsageBits = RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.CanUpdateBit |
                        RenderingDevice.TextureUsageBits.CanCopyFromBit | RenderingDevice.TextureUsageBits.SamplingBit
        };

        Rid rid = _rd.TextureCreate(format, new RDTextureView());
        RDUniform unif = new()
        {
            UniformType = RenderingDevice.UniformType.SamplerWithTexture,
            Binding = 0
        };

        RDSamplerState sampler = new();
        Rid samplerRid = _rd.SamplerCreate(sampler);
        unif.AddId(samplerRid);
        unif.AddId(rid);

        _rd.TextureUpdate(rid, 0, imgBytes.ToArray());

        // Height buffer
        byte[] heightBytes = new byte[heights.Length * sizeof(float)];
        Buffer.BlockCopy(heights, 0, heightBytes, 0, heightBytes.Length);

        Rid hBuffer = _rd.StorageBufferCreate((uint)heightBytes.Length, heightBytes);

        RDUniform hBufferUnif = new()
        {
            UniformType = RenderingDevice.UniformType.StorageBuffer,
            Binding = 1
        };

        hBufferUnif.AddId(hBuffer);

        // Vertex Buffers
        byte[] vertBytes = new byte[verts.Length * sizeof(float)];

        Buffer.BlockCopy(verts, 0, vertBytes, 0, vertBytes.Length);

        Rid vBuffer = _rd.StorageBufferCreate((uint)vertBytes.Length, vertBytes);

        RDUniform vBufferUnif = new()
        {
            UniformType = RenderingDevice.UniformType.StorageBuffer,
            Binding = 2
        };

        vBufferUnif.AddId(vBuffer);

        Rid uniformSet = _rd.UniformSetCreate([unif, hBufferUnif, vBufferUnif], shaderRid, 0);

        Rid pipeline = _rd.ComputePipelineCreate(shaderRid);
        long computeBegin = _rd.ComputeListBegin();
        _rd.ComputeListBindComputePipeline(computeBegin, pipeline);
        _rd.ComputeListBindUniformSet(computeBegin, uniformSet, 0);
        _rd.ComputeListDispatch(computeBegin, (uint)(vertexCount / 32), 1, 1);
        _rd.ComputeListEnd();
        _rd.Submit();
        _rd.Sync();

        byte[] heightBufferOut = _rd.BufferGetData(hBuffer);
        Buffer.BlockCopy(heightBufferOut, 0, heights, 0, heightBufferOut.Length);

        _rd.FreeRid(samplerRid);
        _rd.FreeRid(rid);
        _rd.FreeRid(hBuffer);
        _rd.FreeRid(vBuffer);
        _rd.FreeRid(shaderRid);
        // _rd.FreeRid(pipeline);
        // _rd.FreeRid(uniformSet);

        return heights;
    }

    private void _ComputeNormals()
    {
        RDShaderFile shader = ComputerNormalsShader;
        RDShaderSpirV shaderSpirv = shader.GetSpirV();
        Rid shaderRid = _rd.ShaderCreateFromSpirV(shaderSpirv);

        // Height Map Data and Sampler
        List<byte> imgBytes = _CreateImageByteArray(Images.HeightMap.GetData());
        RDTextureFormat format = new()
        {
            Format = RenderingDevice.DataFormat.R8Unorm,
            Width = (uint)HeightmapSize.Width,
            Height = (uint)HeightmapSize.Height,
            Depth = (uint)HeightmapSize.Depth,
            TextureType = RenderingDevice.TextureType.Type3D,
            UsageBits = RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.CanUpdateBit |
                        RenderingDevice.TextureUsageBits.CanCopyFromBit
        };

        Rid rid = _rd.TextureCreate(format, new RDTextureView());
        RDUniform unif = new()
        {
            UniformType = RenderingDevice.UniformType.Image,
            Binding = 0
        };

        unif.AddId(rid);

        _rd.TextureUpdate(rid, 0, imgBytes.ToArray());
        Array<Image> normalMap = new();
        normalMap.Resize(HeightmapSize.Depth);
        Parallel.For(0, HeightmapSize.Depth,
            i => normalMap[i] = Image.CreateEmpty(HeightmapSize.Width, HeightmapSize.Height,
                false, Image.Format.Rgba8));
        List<byte> normalBytes = _CreateImageByteArray(normalMap);

        RDTextureFormat normalFormat = new()
        {
            Format = RenderingDevice.DataFormat.R8G8B8A8Unorm,
            Width = (uint)HeightmapSize.Width,
            Height = (uint)HeightmapSize.Height,
            Depth = (uint)HeightmapSize.Depth,
            TextureType = RenderingDevice.TextureType.Type3D,
            UsageBits = RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.CanUpdateBit |
                        RenderingDevice.TextureUsageBits.CanCopyFromBit
        };

        Rid nRid = _rd.TextureCreate(normalFormat, new RDTextureView());
        RDUniform nUnif = new()
        {
            UniformType = RenderingDevice.UniformType.Image,
            Binding = 1
        };
        nUnif.AddId(nRid);

        _rd.TextureUpdate(nRid, 0, normalBytes.ToArray());

        Rid uniformSet = _rd.UniformSetCreate([unif, nUnif], shaderRid, 0);

        Rid pipeline = _rd.ComputePipelineCreate(shaderRid);
        long computeBegin = _rd.ComputeListBegin();
        _rd.ComputeListBindComputePipeline(computeBegin, pipeline);
        _rd.ComputeListBindUniformSet(computeBegin, uniformSet, 0);
        _rd.ComputeListDispatch(
            computeBegin, (uint)HeightmapSize.Width / 4, (uint)HeightmapSize.Height / 4,
            (uint)HeightmapSize.Depth / 1
        );
        _rd.ComputeListEnd();
        _rd.Submit();
        _rd.Sync();

        byte[] outBytes = _rd.TextureGetData(nRid, 0);
        // hImages.Resize(HeightmapSize.Depth);
        for (int z = 0; z < HeightmapSize.Depth; z++)
        {
            byte[] buffer = new byte[_wh * 4];
            System.Array.Copy(outBytes, z * _wh, buffer, 0, _wh * 4);
            normalMap[z] =
                Image.CreateFromData(HeightmapSize.Width, HeightmapSize.Height, false, Image.Format.Rgba8, buffer);
        }

        Images.NormalMap.Update(normalMap);

        _rd.FreeRid(rid);
        _rd.FreeRid(nRid);
        _rd.FreeRid(shaderRid);
        // _rd.FreeRid(pipeline);
        // _rd.FreeRid(uniformSet);
    }

    /*private Color _GetColor(float height, float m)
    {
        if (height < 0.1)
            return Colors.DeepWater;
        if (height < 0.15)
            return Colors.Water;
        if (height < WaterLevel)
            return Colors.Savannah;

        if (height > MountainStart)
        {
            if (m < 0.1) return Colors.Savannah;
            if (m < 0.2) return Colors.MountainSide;
            return m < 0.6 ? Colors.Tundra : Colors.Snow;
        }

        if (height > 0.6)
        {
            if (m < 0.33) return Colors.Desert;
            return m < 0.66 ? Colors.Savannah : Colors.Forest;
        }

        if (height > 0.4)
        {
            if (m < 0.2) return Colors.Desert;
            if (m < 0.4) return Colors.Grassland;
            if (m < 0.83) return Colors.Jungle;
        }

        if (m < 0.16) return Colors.Savannah;
        return m < 0.33 ? Colors.Grassland : Colors.Jungle;
    }*/

    private void _CreateNoise()
    {
        RandomNumberGenerator rng = new();

        rng.Seed = Seed;

        Noise1 ??= new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            FractalGain = 0.4f,
            FractalOctaves = 6,
            FractalLacunarity = 2.0f,
            // DomainWarpEnabled = true,
            Frequency = 0.01f,
            Seed = (int)rng.Randi()
        };

        Noise2 ??= new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            // DomainWarpEnabled = true,
            // FractalLacunarity = 1.9f,
            Frequency = 0.03f,
            Seed = (int)rng.Randi()
        };
        Noise3 ??= new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            // DomainWarpEnabled = true,
            // FractalLacunarity = 1.9f,
            Frequency = 0.06f,
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