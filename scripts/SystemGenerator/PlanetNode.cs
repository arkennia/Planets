using Godot;

namespace Planets.SystemGenerator
{
    public partial class PlanetNode : Node3D, ICelestialBodyNode<Planet>
    {
        [Export]
        public Planet Planet { get; set; }
        [Export]
        public Area3D PlanetArea { get; set; }

        public ICelestialBody CelestialBody => Planet;




        public override void _Ready()
        {
            PlanetArea = GetNode<Area3D>($"./{Planet.Area3DName}");
        }

        public void Save(string path = "res://scenes/planets")
        {
            PackedScene ps = new();
            ps.Pack(this);
            ResourceSaver.Save(ps, $"{path}/{CelestialBody.Guid}.tscn", ResourceSaver.SaverFlags.Compress);
        }
    }
}