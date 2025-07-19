using Godot;
using System;
using System.Collections.Generic;
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
    public Vector2 Sector { get; private set; } = Vector2.Zero;

    [Export]
    public Vector3 SectorLocation { get; private set; } = Vector3.Zero;

    [Export]
    public ShaderMaterial ShaderMaterial { get; set; } =
        ResourceLoader.Load<ShaderMaterial>("res://materials/shader_materials/planet_material.tres");

    [Export]
    public Terrain.TerrainColor Colors { get; set; } = new();


    // public NoiseTexture3D NoiseTexture1 { get; private set; }
    // public NoiseTexture3D NoiseTexture2 { get; private set; }
    // public NoiseTexture3D NoiseTexture3 { get; private set; }
    // public NoiseTexture3D MoistureTexture { get; private set; }


    // public MeshInstance3D MeshInstance { get; private set; }
    public Guid Guid { get; private set; } = Guid.Empty;


    public Planet()
    {
        // if (Guid == Guid.Empty) Guid = Guid.NewGuid();
    }

    public Planet(string name = "Earth", Mesh mesh = null, int radius = 500, int resolution = 128)
    {
        Name = name;
        Radius = radius;
        Resolution = resolution;
        Guid = Guid.Empty;
    }

    // public PlanetNode Generate()
    // {
    // }

    // public static Vector3 GetRandomSurfacePosition(int scale)
    // {
    //     RandomNumberGenerator rng = new();
    //     // var vert = rng.RandiRange(0, _mesh)
    //     Vector3 v = new(rng.Randfn(), rng.Randfn(), rng.Randfn());
    //     v = v.Normalized();
    //     return v * scale;
    // }

    public void Save(string path = "res://resources")
    {
        ResourceSaver.Save(this, $"{path}/{Guid}.res", ResourceSaver.SaverFlags.Compress);
    }

//     private ArrayMesh _GenerateNoise(ArrayMesh arrayMesh)
//     {
//         RandomNumberGenerator rng = new();
// 
//         rng.Seed = 42069;
//         _noise1 = new FastNoiseLite
//         {
//             NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
//             FractalGain = 0.4f,
//             FractalOctaves = 3,
//             FractalLacunarity = 2.0f,
//             DomainWarpEnabled = true,
//             Frequency = 0.007f,
//             Seed = (int)rng.Randi()
//         };
//         _noise2 = new FastNoiseLite
//         {
//             NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
//             DomainWarpEnabled = true,
//             // FractalLacunarity = 1.9f,
//             Frequency = 0.009f,
//             Seed = (int)rng.Randi()
//         };
//         _noise3 = new FastNoiseLite
//         {
//             NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
//             DomainWarpEnabled = true,
//             // FractalLacunarity = 1.9f,
//             Frequency = 0.009f,
//             Seed = (int)rng.Randi()
//         };
//         _moisture = new FastNoiseLite
//         {
//             NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
//             // DomainWarpEnabled = true,
//             FractalOctaves = 4,
//             // FractalGain = 0.5f,
//             // Frequency = 0.007f,
//             // FractalLacunarity = 1.9f,
//             Seed = (int)rng.Randi()
//         };
//         // NoiseTexture = new NoiseTexture3D() { Noise = noise1 }
// 
//         // NoiseTexture1 = new NoiseTexture3D { Noise = noise1 };
//         // NoiseTexture2 = new NoiseTexture3D { Noise = noise2 };
//         // NoiseTexture3 = new NoiseTexture3D { Noise = noise3 };
//         // MoistureTexture = new NoiseTexture3D { Noise = moisture };
// 
//         RenderingDevice rd = RenderingServer.CreateLocalRenderingDevice();
//         Gradient g = new();
//         g.AddPoint(0.6f, new Color(0.9f, 0.9f, 0.9f));
//         g.AddPoint(0.8f, new Color(1.0f, 1.0f, 1.0f));
//         g.Reverse();
// 
//         GradientTexture1D gradTex = new()
//         {
//             Gradient = g
//         };
// 
//         RDShaderFile shader =
//             ResourceLoader.Load<RDShaderFile>("res://materials/shader_materials/compute_heightmap.glsl");
//         RDShaderSpirV shaderSpirv = shader.GetSpirV();
//         Rid shaderRid = rd.ShaderCreateFromSpirV(shaderSpirv);
// 
//         RDTextureFormat heightmapFormat = new()
//         {
//             Format = RenderingDevice.DataFormat.R8Unorm,
//             Width = 64,
//             Height = 64,
//             Depth = 64,
//             TextureType = RenderingDevice.TextureType.Type3D,
//             UsageBits = RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.CanUpdateBit |
//                         RenderingDevice.TextureUsageBits.CanCopyFromBit
//         };
// 
//         Rid heightmapRid = rd.TextureCreate(heightmapFormat, new RDTextureView());
//         RDUniform heightmapUnif = new()
//         {
//             UniformType = RenderingDevice.UniformType.Image,
//             Binding = 0
//         };
//         heightmapUnif.AddId(heightmapRid);
// 
//         RDTextureFormat gradFormat = new()
//         {
//             Format = RenderingDevice.DataFormat.R8G8B8A8Unorm,
//             Width = (uint)gradTex.Width,
//             Height = 1,
//             UsageBits = RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.CanUpdateBit
//         };
//         Rid gradRid = rd.TextureCreate(gradFormat, new RDTextureView(), [gradTex.GetImage().GetData()]);
//         RDUniform gradUnif = new()
//         {
//             UniformType = RenderingDevice.UniformType.Image,
//             Binding = 1
//         };
//         gradUnif.AddId(gradRid);
// 
//         Rid uniformSet = rd.UniformSetCreate([heightmapUnif, gradUnif], shaderRid, 0);
//         Rid pipeline = rd.ComputePipelineCreate(shaderRid);
//         // The things above are expensive to make and should be stored for future runs....somehow.
//         var img1 = _noise1.GetImage3D(64, 64, 64);
//         Before = new ImageTexture3D();
//         Before.Create(Image.Format.L8, 64, 64, 64, false, img1);
//         List<byte> imgBytes = [];
//         foreach (Image imgX in img1) imgBytes.AddRange(imgX.GetData());
// 
//         // imgTex1.Create(Image.Format.L8, 64, 64, 64, true, img1);
// 
//         rd.TextureUpdate(heightmapRid, 0, imgBytes.ToArray());
// 
//         long computeBegin = rd.ComputeListBegin();
//         rd.ComputeListBindComputePipeline(computeBegin, pipeline);
//         rd.ComputeListBindUniformSet(computeBegin, uniformSet, 0);
//         rd.ComputeListDispatch(computeBegin, 8, 8, 8);
//         rd.ComputeListEnd();
//         rd.Submit();
//         rd.Sync();
//         byte[] outBytes = rd.TextureGetData(heightmapRid, 0);
//         Array<Image> images = new();
//         images.Resize(64);
//         const int wh = 64 * 64;
//         for (int z = 0; z < 64; z++)
//         {
//             byte[] buffer = new byte[wh];
//             System.Array.Copy(outBytes, z * wh, buffer, 0, wh);
//             images[z] = Image.CreateFromData(64, 64, false, Image.Format.L8, buffer);
//         }
// 
//         After = new ImageTexture3D();
//         After.Create(Image.Format.L8, 64, 64, 64, false, images);
// 
//         // var img1 = _noise1.GetImage3D(64, 64, 64);
//         // var img2 = _noise2.GetImage3D(64, 64, 64);
//         // var img3 = _noise3.GetImage3D(64, 64, 64);
// 
// 
//         MeshDataTool mdt = new();
//         mdt.CreateFromSurface(arrayMesh, 0);
//         int vCount = mdt.GetVertexCount();
// 
// #if DEBUG
//         GD.Print($"Num faces: {mdt.GetFaceCount()}");
//         GD.Print($"Num Vertices: {vCount}");
// #endif
// 
//         float[] heightMap = new float[vCount];
//         Parallel.For(0, vCount, i =>
//         {
//             Vector3 vert = mdt.GetVertex(i);
//             float n1 = _SampleNoise(_noise1, vert * 1.1f);
//             float n2 = _SampleNoise(_noise2, vert * 0.9f);
//             float n3 = _SampleNoise(_noise3, vert * 0.9f);
// 
//             float n = n1 * 1.0f + n2 * 0.33f + n3 * 0.1f;
//             n /= 1.0f + 0.33f + 0.1f;
//             float height = Mathf.Pow(n * 1.3f, 3.6f);
//             heightMap[i] = height;
//         });
//         Parallel.For(0, vCount, i =>
//         {
//             Vector3 vert = mdt.GetVertex(i);
//             float height = _AdjustHeight(mdt, i, heightMap[i]);
//             float m = _SampleNoise(_moisture, vert);
//             Vector3 vertN = mdt.GetVertexNormal(i);
//             vert += vertN * height; // * 20f;
//             // vert += vertN * n;
//             mdt.SetVertex(i, vert);
//             mdt.SetVertexNormal(i, Vector3.Zero);
//             mdt.SetVertexColor(i, _GetColor(height, m));
//         });
// 
//         Parallel.For(0, mdt.GetFaceCount(), i =>
//         {
//             int ia = mdt.GetFaceVertex(i, 0);
//             int ib = mdt.GetFaceVertex(i, 1);
//             int ic = mdt.GetFaceVertex(i, 2);
// 
//             Vector3 e1 = mdt.GetVertex(ia) - mdt.GetVertex(ib);
//             Vector3 e2 = mdt.GetVertex(ic) - mdt.GetVertex(ib);
//             Vector3 normal = e1.Cross(e2);
// 
//             mdt.SetVertexNormal(ia, (mdt.GetVertexNormal(ia) + normal).Normalized());
//             mdt.SetVertexNormal(ib, (mdt.GetVertexNormal(ib) + normal).Normalized());
//             mdt.SetVertexNormal(ic, (mdt.GetVertexNormal(ic) + normal).Normalized());
//         });
//         // for (int i = 0; i < mdt.GetFaceCount(); i++)
//         // {
//         //     
//         // }
//         arrayMesh.ClearSurfaces();
//         mdt.CommitToSurface(arrayMesh);
//         return arrayMesh;
//     }

    // private static bool _IsPowerOfTwo(ulong x)
    // {
    //     return x != 0 && (x & (x - 1)) == 0;
    // }
}