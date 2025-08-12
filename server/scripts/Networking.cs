using Godot;
using Godot.Collections;
using Google.Protobuf;
using Planets.SystemGenerator;
using Planets.Util;

namespace Planets.Server;

public partial class Networking : Node
{
    [Export]
    public int Port { get; set; } = 7000;
    [Export]
    public string ServerIp { get; set; } = "127.0.0.1"; // IPv4 localhost
    [Export]
    public int MaxConnections { get; set; } = 20;

    public static Networking Instance { get; private set; }

    // These signals can be connected to by a UI lobby scene or the game scene.
    [Signal]
    public delegate void PlayerConnectedEventHandler(int peerId);
    [Signal]
    public delegate void PlayerDisconnectedEventHandler(int peerId);
    [Signal]
    public delegate void ServerDisconnectedEventHandler();

    // This will contain player info for every player,
    // with the keys being each player's unique IDs.
    // private Dictionary<long, Dictionary<string, string>> _players = [];


    // private Dictionary<long, Player> _playerObjects = ServerManager.ConnectedPlayers;

    private int _numConnections = 0;

    public override void _EnterTree()
    {
        // GetTree().SetMultiplayer(MultiplayerApi.CreateDefaultInterface(), "/root/Main");

    }


    public override void _Ready()
    {
        Instance ??= this;
        // GD.Print("Server ID: " + Multiplayer.GetUniqueId());
        if (Multiplayer is SceneMultiplayer mp)
        {
            mp.AllowObjectDecoding = true;
        }
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
        if (_numConnections == ServerManager.ConnectedPlayers.Count)
        {
            _numConnections = 0;
        }

    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RegisterPlayer(long id)
    {
        // int newPlayerId = Multiplayer.GetRemoteSenderId();
        // _players[newPlayerId] = newPlayerInfo;
        Player p = new(id);
        ServerManager.AddPlayer(id, p);
        GetTree().CurrentScene.GetNode<Node>("Game").AddChild(p);
        GD.Print("Currently connected players: " + ServerManager.ConnectedPlayers.ToString());
        EmitSignal(SignalName.PlayerConnected, id);
        // #if DEBUG
        //         GetTree().CurrentScene.PrintTreePretty();
        // #endif
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void GetPlanets(byte[] guid, int seed, Array<float> heights)
    {
        GD.Print("Method called by: " + Multiplayer.GetRemoteSenderId());
        RpcId(Multiplayer.GetRemoteSenderId(), MethodName.NumPlanets, ServerManager.Planets.Count);
        // for (int i = 0; i < ServerManager.Planets.Count; i++)
        // {
        //     GD.Print("Sent GUID: " + ServerManager.Planets[i].Planet.Guid.ToString());
        //     RpcId(Multiplayer.GetRemoteSenderId(), MethodName.GetPlanets,
        //         ServerManager.Planets[i].Planet.Guid.ToByteArray(),
        //         ServerManager.Planets[i].PlanetTerrain.Seed,
        //         new Array<float>(ServerManager.Planets[i].PlanetTerrain.Heights));
        // }
        foreach (PlanetNode p in ServerManager.Planets.Values)
        {
            GD.Print("Sent GUID: " + p.Planet.Guid.ToString());
            RpcId(Multiplayer.GetRemoteSenderId(), MethodName.GetPlanets,
                p.Planet.Guid.ToByteArray(),
                p.PlanetTerrain.Seed,
                new Array<float>(p.PlanetTerrain.Heights));
        }
        RpcId(Multiplayer.GetRemoteSenderId(), MethodName.GetPlanets, 1, 0, new Array<float>());

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NumPlanets(int numPlanets)
    {
        GD.Print("NumPlanets Method called by: " + Multiplayer.GetRemoteSenderId());

        GD.Print($"NumPlanets called. Value sent: {ServerManager.Planets.Count}");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendPlayerData()
    {
        int id = Multiplayer.GetRemoteSenderId();
        RpcId(id, MethodName.SendPlayerData, ServerManager.ConnectedPlayers[id].PlayerData);
        GD.Print($"Sending player data {ServerManager.ConnectedPlayers[id].PlayerData} to {id}");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestMovement(byte[] movementBytes)
    {
        PlayerMovement movement = new(PlayerMovementProto.Parser.ParseFrom(movementBytes));
        Player p = ServerManager.ConnectedPlayers[Multiplayer.GetRemoteSenderId()];
        p.Velocity = movement.Velocity;
        p.Rotation = movement.Rotation;
        // p.UpDirection = movement.Up;
        p.GlobalPosition = movement.CurrentGlobalPosition;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendMovement(long id, byte[] movementBytes)
    {
        //RpcId(id, MethodName.SendMovement, id, movementBytes);
        //Rpc(MethodName.SendMovement, id, movementBytes);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestSpawn()
    {
        long id = Multiplayer.GetRemoteSenderId();
        GD.Print($"Spawn requested by: {id}");
        if (ServerManager.ConnectedPlayers.TryGetValue(id, out Player value))
        {
            value.Spawn();
        }
    }

    private void OnPlayerConnected(long id)
    {
        GD.Print("Player connected at ID: " + id);
        //RpcId(MultiplayerPeer.TargetPeerServer, MethodName.SyncPlanets, null);
        // Error e = RpcId(id, MethodName.RegisterPlayer, _playerInfo);
        // if (e != Error.Ok)
        //     GD.Print(e.ToString());
        RpcId(1, MethodName.RegisterPlayer, id);
    }

    private void OnPlayerDisconnected(long id)
    {
        GD.Print("Disconnect: " + id);
        // _players.Remove(id);
        ServerManager.RemovePlayer(id);
        EmitSignal(SignalName.PlayerDisconnected, id);
    }

    private void OnConnectOk()
    {
        int peerId = Multiplayer.GetUniqueId();
        // _players[peerId] = _playerInfo;
        EmitSignal(SignalName.PlayerConnected, peerId);
    }
}
