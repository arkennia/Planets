using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace Planets.Server;

public sealed partial class Server : Node
{
    [Export]
    public int Port { get; set; } = 7000;
    [Export]
    public string ServerIp { get; set; } = "127.0.0.1"; // IPv4 localhost
    [Export]
    public int MaxConnections { get; set; } = 20;

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

    private int _numConnections = 0;

    public override void _EnterTree()
    {
        GetTree().SetMultiplayer(MultiplayerApi.CreateDefaultInterface(), "/root/Main");
    }


    public override void _Ready()
    {
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

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void PlayerLoaded()
    {
        if (Multiplayer.IsServer())
        {
            _numConnections += 1;
            if (_numConnections == _players.Count)
            {
                // GetNode<Game>("/root/Game").StartGame();
                _numConnections = 0;
            }
        }
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RegisterPlayer(Dictionary<string, string> newPlayerInfo)
    {
        int newPlayerId = Multiplayer.GetRemoteSenderId();
        _players[newPlayerId] = newPlayerInfo;
        EmitSignal(SignalName.PlayerConnected, newPlayerId, newPlayerInfo);
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
        int peerId = Multiplayer.GetUniqueId();
        _players[peerId] = _playerInfo;
        EmitSignal(SignalName.PlayerConnected, peerId, _playerInfo);
    }
}
