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



    // private Dictionary<long, Dictionary<string, string>> _players = [];

    // [Signal]
    // public delegate void PlayerConnectedEventHandler(int peerId, Dictionary<string, string> playerInfo);
    // [Signal]
    // public delegate void PlayerDisconnectedEventHandler(int peerId);
    // [Signal]
    // public delegate void ServerDisconnectedEventHandler();

    // private int _numConnections = 0;
    public override void _Ready()
    {
        GD.Print(GetPath());
        if (LoadingScreenScene?.Instantiate() is Control node)
        {
            AddChild(node);
            _loadingScreen = node;
        }
        Multiplayer.ConnectedToServer += () =>
        {
            if (_gameNode is null)
                _LoadGame();
            // Networking.Instance.RpcId(MultiplayerPeer.TargetPeerServer, Networking.MethodName.PlayerLoaded, Multiplayer.GetUniqueId());
        };
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