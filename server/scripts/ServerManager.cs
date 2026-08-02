using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Planets.SystemGenerator;
using Planets.SystemGenerator.Terrain;

namespace Planets.Server;

public partial class ServerManager : Node
{
    public static Dictionary<string, SolarSystemNode> Systems { get => _systems; }
    private readonly static Dictionary<string, SolarSystemNode> _systems = [];

    public static Dictionary<long, Player> ConnectedPlayers { get => _playerObjects; }
    private static Dictionary<long, Player> _playerObjects = [];

    public static Dictionary<long, string> ClientUuids { get => _clientUuids; }
    private static Dictionary<long, string> _clientUuids = [];

    public static ServerManager Instance { get => _instance; }

    private static readonly ServerManager _instance = new();

    private ServerManager()
    {

    }

    public override void _Ready()
    {

    }

    public static void AddSolarSystem(SolarSystemNode system)
    {
        _systems.Add(system.Guid.ToString(), system);
        //PlanetSpawner.AddSpawnableScene(planet.SaveLocation);
        // GD.Print(PlanetSpawner.GetSpawnableScene(0));
    }

    public static void AddPlayer(long id, Player p)
    {
        if (!_playerObjects.ContainsKey(id))
        {
            _playerObjects.Add(id, p);
            p.Uuid = _clientUuids.ContainsKey(id) ? Guid.Parse(_clientUuids[id]) : Guid.Empty;
            if (FileAccess.FileExists($"{p.PlayerDataFolder}/{p.Uuid}.dat"))
            {
                using FileAccess file = FileAccess.Open($"{p.PlayerDataFolder}/{p.Uuid}.dat", FileAccess.ModeFlags.Read);
                byte[] bytes = file.GetBuffer((int)file.GetLength());
                PlayerDataProto proto = PlayerDataProto.Parser.ParseFrom(bytes);
                p.PlayerData = new(proto);
                GD.Print("Loaded player data for " + id + " with UUID: " + p.PlayerData.Uuid);
            }
            else
            {
                GD.Print($"No player data found for {id}, creating new player data.");
                p.PlayerData = new();
                p.PlayerData.Uuid = p.Uuid;
            }

            if (p.PlayerData.SpawnPlanet == string.Empty)
            {
                SetPlayerSpawn(p, _systems.Values.ToArray()[0].Planets.Values.ToArray()[0]);
            }
        }
    }

    public static void RemovePlayer(long id)
    {
        _playerObjects[id].QueueFree();
        _playerObjects.Remove(id);
    }

    public static void AddClientUUID(long id, string uuid)
    {
        if (!_clientUuids.ContainsKey(id))
        {
            _clientUuids.Add(id, uuid);
        }
    }

    public static void RemoveClientUUID(long id)
    {
        _clientUuids.Remove(id);
    }

    public static void SetPlayerSpawn(Player player, PlanetNode planet)
    {
        Terrain3D.SpawnPoint spawn = planet.GetSpawnPoint();
        GD.Print($"Spawn Local Position: {spawn.Node.Position}");
        GD.Print($"Spawn Global Position:{spawn.Node.GlobalPosition}");
        // spawn.mI.GlobalPosition = spawn.Node.GlobalPosition;
        // Player.GlobalPosition = spawn;
        // Player.Spawn(spawn, planet);
        player.PlayerData.SpawnPlanet = planet.Planet.Guid.ToString();
        player.PlayerData.SpawnPosition = spawn.Node.GlobalPosition;
        player.PlayerData.Up = spawn.Normal;
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
