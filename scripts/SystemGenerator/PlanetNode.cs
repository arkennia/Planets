using Godot;
using Planets.SystemGenerator;
using System;

public partial class PlanetNode : Node3D
{
    [Export]
    public Planet Planet { get; set; }

    public override void _Ready()
    {
    }
}
