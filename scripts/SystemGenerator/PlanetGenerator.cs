using Godot;


namespace Planets.SystemGenerator
{
    public partial class PlanetGenerator : GodotObject
    {
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
