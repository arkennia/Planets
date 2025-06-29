using Godot;
using Planets.SystemGenerator;
using System;

namespace Planets.SystemGenerator
{
    public partial class PlanetGenerator : GodotObject
    {
        public static PlanetNode GeneratePlanet(string name = "Earth", Mesh mesh = null, int scale = 20000, int resolution = 128)
        {
            Planet planet = new Planet(name, mesh, scale, resolution);
            PlanetNode mI = planet.Generate();
            return mI;
        }
    }
}
