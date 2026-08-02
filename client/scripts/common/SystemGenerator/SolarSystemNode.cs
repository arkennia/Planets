using Godot;
using Godot.Collections;
using System;

namespace Planets.SystemGenerator;

/// <summary>
/// The <c>SolarSystemNode</c> class inherits Node3D, and is a container for all solar system objects. <br/>
/// It is used to generate a solar system in the scene tree.
/// </summary>
public partial class SolarSystemNode : Node3D
{
	/// <summary>
	/// The <c>SunNode</c> object that is the center of the solar system. <br/>
	/// </summary>
	[Export]
	public SunNode Sun { get; private set; }

	/// <summary>
	/// The <c>Planets</c> dictionary contains all of the <c>PlanetNode</c> objects in the solar system. <br/>
	/// The key is the GUID of the planet, and the value is the <c>PlanetNode</c> object. <br/>
	/// </summary>
	[Export]
	public Dictionary<string, PlanetNode> Planets { get; private set; } = [];

	/// <summary>
	/// The <c>Sector</c> property is the sector of the solar system. <br/>
	/// It is used to determine the location of the solar system in the universe. <br/>
	/// </summary>
	[Export]
	public Vector3 Sector { get; private set; } = Vector3.Zero;

	public Guid Guid { get; private set; } = Guid.Empty;


	public void Generate(Guid? guid = null)
	{
		Guid = guid ?? Guid.NewGuid();

		Name = "SolarSystem";
		Sun = new SunNode();
		AddChild(Sun);
		Sun.Generate();

		PlanetNode p = PlanetGenerator.GeneratePlanet(scale: 50);
		// p = PlanetGenerator.GeneratePlanet(scale: 50, seed: (int)p.PlanetTerrain.Seed, heights: new Array<double>(p.PlanetTerrain.Heights), guid: p.Guid);
		p.Position = new Vector3(10000, 0, 0);
		Planets[p.Guid.ToString()] = p;
		AddChild(p);
		p.Save();
	}
}
