using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Planets;
using Planets.SystemGenerator;
using Planets.SystemGenerator.Terrain;

public partial class Networking : Node
{
    public static Player Player { get; set; }
    public static bool Connected { get; private set; } = false;
    public static Networking Instance { get; private set; }
    private Dictionary<long, Dictionary<string, string>> _players = [];
    private Dictionary<string, string> _playerInfo = new()
    {
        // {"Name", "PlayerName"}
    };
    public Array<PlanetNode> planets = [];

    private int _numPlanets = 0;

    [Signal]
    public delegate void PlayerConnectedEventHandler(int peerId, Dictionary<string, string> playerInfo);
    [Signal]
    public delegate void PlayerDisconnectedEventHandler(int peerId);
    [Signal]
    public delegate void ServerDisconnectedEventHandler();
    [Signal]
    public delegate void PlanetLoadedEventHandler(PlanetNode planet);
    [Signal]
    public delegate void SyncingFinishedEventHandler();

    private int _numConnections = 0;

    // private bool _isSyncing = false;

    private Networking()
    {
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance ??= this;
        if (Multiplayer is SceneMultiplayer mp)
        {
            mp.AllowObjectDecoding = true;
        }
        GD.Print("Client Multiplayer instance ID: " + Multiplayer.GetInstanceId());
    }

    public Error ConnectToServer()
    {
        ENetMultiplayerPeer peer = new();
        Error e = peer.CreateClient("127.0.0.1", 7000);
        PlanetLoaded += (planet) => GD.Print("Planet loaded! " + planet.ToString());
        // SyncingFinished += GameManager.Instance.WorldLoaded;
        Multiplayer.MultiplayerPeer = peer;
        Multiplayer.PeerConnected += OnPlayerConnected;
        Multiplayer.PeerDisconnected += OnPlayerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectOk;
        Multiplayer.ConnectionFailed += OnConnectionFail;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
        return e;
    }

    public void BeginPlanetSync()
    {
        Error e = RpcId(1, MethodName.NumPlanets, -1);
        if (e != Error.Ok)
            GD.Print($"Error getting number of planets: {e}");
        e = RpcId(1, MethodName.GetPlanets, 0, 0, new Array<float>());
        if (e != Error.Ok)
            GD.Print($"Error getting planets: {e}");
    }

    public void BeginPlayerDataSync()
    {
        Error e = RpcId(1, MethodName.SendPlayerData, null);
        GD.Print("Requesting player data. Code: " + e.ToString());
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void PlayerLoaded()
    {

    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RegisterPlayer(Dictionary<string, string> newPlayerInfo)
    {
        int newPlayerId = Multiplayer.GetRemoteSenderId();
        _players[newPlayerId] = newPlayerInfo;
        EmitSignal(SignalName.PlayerConnected, newPlayerId, newPlayerInfo);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void GetPlanets(byte[] id, int seed, Array<float> heights)
    {
        GD.Print("Method called by: " + Multiplayer.GetRemoteSenderId());
        if (Multiplayer.GetRemoteSenderId() != 1) return;
        // GD.Print("Data Received: " + planetBytes.Length + " ");
        if (heights.Count > 0)
        {
            // Guid uid = Guid.Parse(id);
            GD.Print(heights[0..10]);
            GD.Print("Seed: " + seed);
            Guid guid = new Guid(id);
            GD.Print("Received GUID: " + guid.ToString());
            PlanetNode planet = PlanetGenerator.GeneratePlanet(heights: heights, seed: seed);
            GetTree().Root.GetNode<Node>("/root/Main/Game/World").AddChild(planet);
            planets.Add(planet);
            EmitSignal(SignalName.PlanetLoaded, planet);
        }
        else
        {
            EmitSignal(SignalName.SyncingFinished);
            GD.Print($"Number of Planets received:  {planets.Count}");
            GameManager.Instance.WorldLoaded();
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NumPlanets(int numPlanets)
    {
        if (Multiplayer.GetRemoteSenderId() != 1) return;
        _numPlanets = numPlanets;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendPlayerData(PlayerData data)
    {
        GetTree().CurrentScene.GetNode<Player>("Game/Player").PlayerData = data;
        GD.Print("PlayerData: " + data.ToString());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RequestMovement(PlayerMovement movement)
    {

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendMovement(PlayerMovement movement)
    {
        float distance = Mathf.Abs(movement.CurrentGlobalPosition.DistanceTo(Player.GlobalPosition));
        if (distance >= 0.5f)
        {
            Player.GlobalPosition = movement.CurrentGlobalPosition;
        }
        GD.Print($"Distance from Server to Client: {distance}");
    }

    private void OnPlayerConnected(long id)
    {
        GD.Print("Player connected at ID: " + id);
        Error e = RpcId(id, MethodName.RegisterPlayer, _playerInfo);
        if (e != Error.Ok)
            GD.Print(e.ToString());
        if (Multiplayer.GetRemoteSenderId() == 1)
        {
            BeginPlanetSync();
        }
    }
    private void OnPlayerDisconnected(long id)
    {
        _players.Remove(id);
        EmitSignal(SignalName.PlayerDisconnected, id);
    }

    private void OnConnectOk()
    {
        Connected = true;
        int peerId = Multiplayer.GetUniqueId();
        GD.Print("My ID: " + peerId);
        _players[peerId] = _playerInfo;
        RpcId(1, MethodName.GetPlanets, peerId, 0, new());
        EmitSignal(SignalName.PlayerConnected, peerId, _playerInfo);
    }

    private void OnConnectionFail()
    {
        Multiplayer.MultiplayerPeer = null;
    }

    private void OnServerDisconnected()
    {
        Connected = false;
        Multiplayer.MultiplayerPeer = null;
        _players.Clear();
        EmitSignal(SignalName.ServerDisconnected);
    }

}
