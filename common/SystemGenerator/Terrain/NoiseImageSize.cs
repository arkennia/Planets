using Godot;

namespace Planets.SystemGenerator.Terrain;

[GlobalClass]
public partial class NoiseImageSize : Resource
{
    [Export]
    public int Width { get; set; } = 64;

    [Export]
    public int Height { get; set; } = 64;

    [Export]
    public int Depth { get; set; } = 64;

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
    }
}