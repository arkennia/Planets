using Godot;
using Planets.SystemGenerator;
using System;

public partial class SunNode : Node3D, ICelestialBodyNode<Sun>
{
    public ICelestialBody CelestialBody => throw new NotImplementedException();

    public string SaveLocation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void Generate()
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

}
