using Godot;


namespace Planets.SystemGenerator
{
    [Tool]
    public partial class PlanetGenerator : EditorScript
    {
        [Export]
        public string PlanetName { get; set; } = "Earth";
        [Export]
        public Mesh Mesh { get; set; } = null;
        [Export]
        public int Scale { get; set; } = 1000;
        public int Resolution { get; set; } = 64;
        public override void _Run()
        {
            PlanetNode p = GeneratePlanet(PlanetName, Mesh, Scale, Resolution);
            p.Save();
        }
        public static PlanetNode GeneratePlanet(string name = "Earth", Mesh mesh = null, int scale = 1000, int resolution = 64)
        {
            Planet planet = new(name, mesh, scale, resolution);
            PlanetNode mI = planet.Generate();
            return mI;
        }

        public static PlanetNode GeneratePlanet(Planet p)
        {
            return p.Generate();
        }
    }
}
