using Godot;
using System;


namespace Planets.SystemGenerator
{
    public interface ICelestialBody
    {
        Guid Guid { get; }
        string Name { get; }
        bool Generated { get; }
        Vector2 Sector { get; }
        Vector3 SectorLocation { get; }

        public void Save(string path);
    }
}