using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Planets.SystemGenerator.Terrain;
/// <summary>
/// Holds the necessary objects for using image3D in a compute shader.
/// </summary>
public struct ComputeShaderImage
{
    public Rid Rid;
    public RDTextureFormat Format;
    public RDUniform Unif;
    public List<byte> ImgBytes;
    public int Binding;

    public static ComputeShaderImage Create(RenderingDevice rd, Array<Image> img,
        NoiseImageSize size, int binding)
    {
        List<byte> imgBytes = [];
        foreach (Image imgX in img) imgBytes.AddRange(imgX.GetData());
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

    public static ComputeShaderImage Create(RenderingDevice rd, Image img,
        NoiseImageSize size, int binding)
    {
        List<byte> imgBytes = img.GetData().ToList();
        RDTextureFormat format = new()
        {
            Format = RenderingDevice.DataFormat.R8Unorm,
            Width = (uint)size.Width,
            Height = (uint)size.Height,
            Depth = (uint)size.Depth,
            TextureType = RenderingDevice.TextureType.Type2D,
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

    public static ComputeShaderImage CreateWithSampler(RenderingDevice rd, Image img,
        NoiseImageSize size, int binding)
    {
        List<byte> imgBytes = img.GetData().ToList();
        RDTextureFormat format = new()
        {
            Format = RenderingDevice.DataFormat.R8Unorm,
            Width = (uint)size.Width,
            Height = (uint)size.Height,
            TextureType = RenderingDevice.TextureType.Type2D,
            UsageBits = RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.CanUpdateBit |
                        RenderingDevice.TextureUsageBits.CanCopyFromBit | RenderingDevice.TextureUsageBits.SamplingBit
        };

        Rid rid = rd.TextureCreate(format, new RDTextureView());
        RDUniform unif = new()
        {
            UniformType = RenderingDevice.UniformType.SamplerWithTexture,
            Binding = binding
        };
        return new ComputeShaderImage
        {
            Format = format,
            Rid = rid,
            Unif = unif,
            ImgBytes = imgBytes,
            Binding = binding
        };
    }
}