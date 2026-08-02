using Godot;

namespace Planets.SystemGenerator.Terrain;

[GlobalClass]
public partial class NoiseImages : Resource
{
    [Export]
    public ImageTexture3D Noise1 { get; set; }

    [Export]
    public ImageTexture3D Noise2 { get; set; }

    [Export]
    public ImageTexture3D Noise3 { get; set; }

    [Export]
    public ImageTexture3D Moisture { get; set; }

    [Export]
    public ImageTexture3D HeightMap { get; set; }

    [Export]
    public ImageTexture3D NormalMap { get; set; }
}