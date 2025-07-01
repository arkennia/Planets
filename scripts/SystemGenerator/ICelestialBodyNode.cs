namespace Planets.SystemGenerator
{
    public interface ICelestialBodyNode<T> where T : ICelestialBody
    {
        ICelestialBody CelestialBody { get; }

        public void Save(string path);
    }
}