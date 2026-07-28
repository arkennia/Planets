using Godot;

namespace Planets.SystemGenerator.Terrain;

[GlobalClass]
public partial class TerrainColor : Resource
{
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
}