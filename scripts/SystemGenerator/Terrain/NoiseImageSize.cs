using Godot;

namespace Planets.SystemGenerator.Terrain;

public partial class NoiseImageSize : Resource
{
    [Export] public int Width;
    [Export] public int Height;
    [Export] public int Depth;

    public NoiseImageSize(int dim)
    {
        Width = Height = Depth = dim;
    }

    public NoiseImageSize(Vector3I dims)
    {
        Width = dims.X;
        Height = dims.Y;
        Depth = dims.Z;
    }

    public NoiseImageSize()
    {
        Width = Height = Depth = 64;
    }
}