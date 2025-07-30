using System.Threading.Tasks;
using Godot;
using Planets.UI;
using Array = Godot.Collections.Array;

namespace Planets;

public partial class Main : Node
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

    public override void _Ready()
    {
        if (LoadingScreenScene?.Instantiate() is Control node)
        {
            AddChild(node);
            _loadingScreen = node;
        }
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