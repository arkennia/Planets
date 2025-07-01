using Godot;
using Planets.SystemGenerator;
using System;

public partial class PlanetNode : Node3D
{
    [Export]
    public Planet Planet { get; set; }
    [Export]
    public Area3D PlanetArea { get; set; }

    public override void _Ready()
    {
        PlanetArea = GetNode<Area3D>($"./{Planet.Area3DName}");
    }

    public void Save(string path = "res://scenes/planets")
    {
        PackedScene ps = new();
        ps.Pack(this);
        ResourceSaver.Save(ps, $"{path}/{Planet.Name}.tscn", ResourceSaver.SaverFlags.Compress);
    }
}
