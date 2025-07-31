using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Planets.UI;
using Array = Godot.Collections.Array;

namespace Planets;

public partial class Client : Node
{
    [Export]
    public PackedScene LoadingScreenScene { get; set; }
    [Export]
    public PackedScene MainMenuScene { get; set; }

    [Export]
    public PackedScene GameUiScene { get; set; }

    [Export(PropertyHint.FilePath, "*.tscn,*.scn,")]
    public string Game { get; private set; }

    public GameUi Ui { get; private set; } = null;

    private bool _loading;

    private Control _loadingScreen;

    private Node _gameNode;

    private Dictionary<string, string> _playerInfo = new()
    {
        {"Name", "PlayerName"}
    };

    private Dictionary<long, Dictionary<string, string>> _players = [];

    [Signal]
    public delegate void PlayerConnectedEventHandler(int peerId, Dictionary<string, string> playerInfo);
    [Signal]
    public delegate void PlayerDisconnectedEventHandler(int peerId);
    [Signal]
    public delegate void ServerDisconnectedEventHandler();

    private int _numConnections = 0;
    public override void _Ready()
    {
        GD.Print(GetPath());
        if (LoadingScreenScene?.Instantiate() is Control node)
        {
            AddChild(node);
            _loadingScreen = node;
        }
        GD.Print("Client Multiplayer instance ID: " + Multiplayer.GetInstanceId());
        ENetMultiplayerPeer peer = new();
        peer.CreateClient("127.0.0.1", 7000);
        Multiplayer.MultiplayerPeer = peer;
        // Multiplayer.PeerConnected += OnPlayerConnected;
        Multiplayer.PeerDisconnected += OnPlayerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectOk;
        Multiplayer.ConnectionFailed += OnConnectionFail;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
        if (_gameNode is null)
            _LoadGame();
    }

    public override void _Process(double _)
    {
        if (_loading)
        {
            Array progress = [];
            if (ResourceLoader.LoadThreadedGetStatus(Game, progress) == ResourceLoader.ThreadLoadStatus.Loaded)
            {
                GD.Print($"Progress: {progress}");
                if (_GetLoadedPackedScene(Game) is not Node sceneNode)
                {
                    GD.PrintErr($"Failed to load scene {Game}.");
                }
                else
                {
                    AddChild(sceneNode);
                    RemoveChild(_loadingScreen);
                    GD.Print("Game Scene loaded");
                    _gameNode = sceneNode;
                    _LoadGameUi();
                }
                _loading = false;
            }
        }
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

    private void OnConnectionFail()
    {
        Multiplayer.MultiplayerPeer = null;
    }

    private void OnServerDisconnected()
    {
        Multiplayer.MultiplayerPeer = null;
        _players.Clear();
        EmitSignal(SignalName.ServerDisconnected);
    }

    private void _LoadGameUi()
    {
        Ui = GameUiScene.Instantiate<GameUi>();
        AddChild(Ui);
        UiManager.Instance.Ui = Ui;
    }

    private void _LoadGame()
    {
        _BeginLoadPackedScene(Game);
        _loading = true;
    }

    private static void _BeginLoadPackedScene(string path)
    {
        Error sceneLoader =
                ResourceLoader.LoadThreadedRequest(path);
        if (sceneLoader != Error.Ok)
            GD.PrintErr(sceneLoader);
    }

    private static Node _GetLoadedPackedScene(string path)
    {
        PackedScene scene =
                ResourceLoader.LoadThreadedGet(path) as
                    PackedScene;
        if (scene?.Instantiate() is not Node sceneNode) return null;
        return sceneNode;
    }
}