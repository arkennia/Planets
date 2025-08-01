using System;
using Godot;


namespace Planets.SystemGenerator;
/// <summary>
/// Used to create a common interface for all celestial bodies. <br/>
/// Examples: Suns, Planets, Moons
/// </summary>
/// <remarks>
/// Node based classes should not implement this. They should implement
/// <c>ICelestialBodyNode</c> instead. This will typically be inherited by <c>Resource</c> classes.
/// </remarks>
public interface ICelestialBody
{
    Guid Guid { get; }
    string Name { get; }
    Vector2 Sector { get; }
    Vector3 SectorLocation { get; }

    public void Save(string path);
}