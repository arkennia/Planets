using System;
using Godot;
using Godot.Collections;
using Planets.SystemGenerator;
using Planets.SystemGenerator.Terrain;

public partial class Networking : Node
{
    public static bool Connected { get; private set; } = false;
    public static Networking Instance { get; private set; } = new();
    private Dictionary<long, Dictionary<string, string>> _players = [];
    private Dictionary<string, string> _playerInfo = new()
    {
        // {"Name", "PlayerName"}
    };

    public bool IsSyncing { get; private set; }
    public Array<PlanetNode> planets;

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
        // Instance ??= this;
        if (Multiplayer is SceneMultiplayer mp)
        {
            mp.AllowObjectDecoding = true;
        }
        GD.Print("Client Multiplayer instance ID: " + Multiplayer.GetInstanceId());
        ENetMultiplayerPeer peer = new();
        peer.CreateClient("127.0.0.1", 7000);
        PlanetLoaded += (planet) => GD.Print("Planet loaded! " + planet.ToString());
        SyncingFinished += () => IsSyncing = false;
        Multiplayer.MultiplayerPeer = peer;
        Multiplayer.PeerConnected += OnPlayerConnected;
        Multiplayer.PeerDisconnected += OnPlayerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectOk;
        Multiplayer.ConnectionFailed += OnConnectionFail;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
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

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void GetPlanets(long id, int seed, Array<float> heights)
    {
        GD.Print("Method called by: " + Multiplayer.GetRemoteSenderId());
        if (Multiplayer.GetRemoteSenderId() != 1) return;
        // GD.Print("Data Received: " + planetBytes.Length + " ");
        if (heights.Count > 0)
        {
            IsSyncing = true;
            GD.Print(heights[0..10]);
            GD.Print("Seed: " + seed);
            PlanetNode planet = PlanetGenerator.GeneratePlanet(heights: heights, seed: seed);
            GetTree().Root.GetNode<Node>("/root/Main/Game/World").AddChild(planet);
            planets.Add(planet);
            EmitSignal(SignalName.PlanetLoaded, planet);
        }
        else
        {
            EmitSignal(SignalName.SyncingFinished);
        }
    }

    private void OnPlayerConnected(long id)
    {
        GD.Print("Player connected at ID: " + id);
        Error e = RpcId(id, MethodName.RegisterPlayer, _playerInfo);
        if (e != Error.Ok)
            GD.Print(e.ToString());
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
