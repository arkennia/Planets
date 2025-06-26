using Godot;
using Planets.SystemGenerator;
using System;

namespace Planets.SystemGenerator
{
    public partial class PlanetGenerator : GodotObject
    {
        public static Node3D GeneratePlanet()
        {
            var planet = new Planet("TestPlanet");
            var mI = planet.Generate();
            return mI;
        }
    }
}
