using Godot;
using Godot.Collections;
using System;

namespace Planets.SystemGenerator;

/// <summary>
/// The <c>SolarSystemNode</c> class inherits Node3D, and is a container for all solar system objects. <br/>
/// It is used to generate a solar system in the scene tree.
/// </summary>
public partial class SolarSystemNode : WorldEnvironment
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

	[Export]
	public string SaveLocation { get; set; } = "res://scenes/solarsystems";

	public Guid Guid { get; private set; } = Guid.Empty;

	public override void _Ready()
	{
		Environment = ResourceLoader.Load("res://scripts/common/environments/solar_system_environment.tres").Duplicate() as Godot.Environment;
	}

	public void Generate(Guid? guid = null)
	{
		Guid = guid ?? Guid.NewGuid();

		Name = "SolarSystem";
		Sun = new SunNode();
		AddChild(Sun);
		Sun.Generate(10000);

		PlanetNode p = PlanetGenerator.GeneratePlanet(scale: 50);
		p.Position = new Vector3(50000, 0, 0);
		Planets[p.Guid.ToString()] = p;
		AddChild(p);
	}

	public void Save()
	{
		ReparentChildren(this);

		PackedScene ps = new();
		ps.Pack(this);
		string fullPath = $"res://scenes/solarsystems/{Guid}.tscn";
		ResourceSaver.Save(ps, fullPath, ResourceSaver.SaverFlags.Compress);
	}

	private void ReparentChildren(Node node = null)
	{
		if (node == null)
		{
			return;
		}
		else
		{
			foreach (var child in node.GetChildren(true))
			{
				child.Owner = this;
				GD.Print("Reparented child: " + child.GetType().Name + " " + child.Name + " to owner: " + Name);
				ReparentChildren(child);
			}
		}
	}
}
