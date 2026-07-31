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
	public SunNode Sun { get; private set; }

	/// <summary>
	/// The <c>Planets</c> dictionary contains all of the <c>PlanetNode</c> objects in the solar system. <br/>
	/// The key is the GUID of the planet, and the value is the <c>PlanetNode</c> object. <br/>
	/// </summary>
	public Dictionary<string, PlanetNode> Planets { get; private set; } = new();

	/// <summary>
	/// The <c>Sector</c> property is the sector of the solar system. <br/>
	/// It is used to determine the location of the solar system in the universe. <br/>
	/// </summary>
	public Vector3 Sector { get; private set; } = Vector3.Zero;


	public void Generate()
	{
		Sun = new SunNode();

		PlanetNode p = PlanetGenerator.GeneratePlanet(scale: 250);
		Planets[p.Guid.ToString()] = p;

	}
}
