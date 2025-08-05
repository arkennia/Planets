using System.Linq;
using Godot;
using Godot.Collections;
using Planets.SystemGenerator;
using Planets.SystemGenerator.Terrain;
using Array = Godot.Collections.Array;

namespace Planets.Server;

public partial class Networking : Node
{
    [Export]
    public int Port { get; set; } = 7000;
    [Export]
    public string ServerIp { get; set; } = "127.0.0.1"; // IPv4 localhost
    [Export]
    public int MaxConnections { get; set; } = 20;

    public static Networking Instance;

    // These signals can be connected to by a UI lobby scene or the game scene.
    [Signal]
    public delegate void PlayerConnectedEventHandler(int peerId, Dictionary<string, string> playerInfo);
    [Signal]
    public delegate void PlayerDisconnectedEventHandler(int peerId);
    [Signal]
    public delegate void ServerDisconnectedEventHandler();

    // This will contain player info for every player,
    // with the keys being each player's unique IDs.
    private Dictionary<long, Dictionary<string, string>> _players = [];

    private Dictionary<string, string> _playerInfo = new()
    {
        {"Name", "PlayerName"}
    };

    private Dictionary<long, Player> _playerObjects = [];

    private int _numConnections = 0;

    public override void _EnterTree()
    {
        // GetTree().SetMultiplayer(MultiplayerApi.CreateDefaultInterface(), "/root/Main");
        GD.Print("Server ID: " + Multiplayer.GetUniqueId());
        if (Multiplayer is SceneMultiplayer mp)
        {
            mp.AllowObjectDecoding = true;
        }
    }


    public override void _Ready()
    {
        Instance ??= this;
        GD.Print("Server Multiplayer instance ID: " + Multiplayer.GetInstanceId());
        ENetMultiplayerPeer peer = new();
        peer.CreateServer(Port, MaxConnections);
        Multiplayer.MultiplayerPeer = peer;
        Multiplayer.PeerConnected += OnPlayerConnected;
        Multiplayer.PeerDisconnected += OnPlayerDisconnected;
        // Multiplayer.ConnectedToServer += OnConnectOk;
        // Multiplayer.ConnectionFailed += OnConnectionFail;
        // Multiplayer.ServerDisconnected += OnServerDisconnected;
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void PlayerLoaded(long id)
    {
        _numConnections += 1;
        GD.Print("Peer loaded: " + id);
        if (_numConnections == _players.Count)
        {
            _numConnections = 0;
        }

    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RegisterPlayer(Dictionary<string, string> newPlayerInfo)
    {
        int newPlayerId = Multiplayer.GetRemoteSenderId();
        _players[newPlayerId] = newPlayerInfo;
        Player p = new Player(newPlayerId);
        _playerObjects[newPlayerId] = p;
        GD.Print(_playerObjects.ToString());
        EmitSignal(SignalName.PlayerConnected, newPlayerId, newPlayerInfo);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void GetPlanets(long id, int seed, Array<float> heights)
    {
        // PackedScene scene = ResourceLoader.Load<PackedScene>("res://scenes/planets/00000000-0000-0000-0000-000000000000.scn");
        GD.Print("Method called by: " + Multiplayer.GetRemoteSenderId());
        RpcId(Multiplayer.GetRemoteSenderId(), MethodName.NumPlanets, ServerManager.Planets.Count);
        for (int i = 0; i < ServerManager.Planets.Count; i++)
            RpcId(Multiplayer.GetRemoteSenderId(), MethodName.GetPlanets, 1,
                ServerManager.Planets[0].PlanetTerrain.Seed,
                new Array<float>(ServerManager.Planets[0].PlanetTerrain.Heights));
        RpcId(Multiplayer.GetRemoteSenderId(), MethodName.GetPlanets, 1, 0, new Array<float>());

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NumPlanets(int numPlanets)
    {
        GD.Print("NumPlanets Method called by: " + Multiplayer.GetRemoteSenderId());

        GD.Print($"NumPlanets called. Value sent: {ServerManager.Planets.Count}");
    }

    // [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    // public void SyncPlanets(Array<PlanetNode> nodes)
    // {
    //     if (Multiplayer.IsServer())
    //     {
    //         Error e = RpcId(MultiplayerPeer.TargetPeerBroadcast, MethodName.SyncPlanets, ServerManager.Instance.planets);
    //         if (e != Error.Ok)
    //         {
    //             GD.Print($"Error sending data: {e}");
    //         }
    //     }
    //     else
    //     {
    //         foreach (PlanetNode n in nodes)
    //         {
    //             GetNode("%World").AddChild(n);
    //         }
    //     }
    // }

    private void OnPlayerConnected(long id)
    {
        GD.Print("Player connected at ID: " + id);
        //RpcId(MultiplayerPeer.TargetPeerServer, MethodName.SyncPlanets, null);
        // Error e = RpcId(id, MethodName.RegisterPlayer, _playerInfo);
        // if (e != Error.Ok)
        //     GD.Print(e.ToString());
    }

    private void OnPlayerDisconnected(long id)
    {
        GD.Print("Disconnect: " + id);
        _players.Remove(id);
        _playerObjects.Remove(id);
        EmitSignal(SignalName.PlayerDisconnected, id);
    }

    private void OnConnectOk()
    {
        int peerId = Multiplayer.GetUniqueId();
        _players[peerId] = _playerInfo;
        EmitSignal(SignalName.PlayerConnected, peerId, _playerInfo);
    }
}
