namespace Planets.SystemGenerator;
/// <summary>
/// ICelestialBodyNode should only be inherited by classes derived from <c>Node</c> or its descendants. <br/>
/// Holds a reference to another celestial body to be placed into the scene tree.
/// </summary>
/// <typeparam name="T">A class that implements <c>ICelestialBody</c></typeparam>
public interface ICelestialBodyNode<T> where T : ICelestialBody
{
    ICelestialBody CelestialBody { get; }

    /// <summary>
    /// Saves this node to a file.
    /// </summary>
    /// <param name="path">The path to save to.</param>
    public void Save(string path);

    /// <summary>
    /// Generate the celestial body.
    /// </summary>
    public void Generate();
}