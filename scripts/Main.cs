using System;
using System.Threading.Tasks;
using Godot;
using Planets.UI;

namespace Planets;

public partial class Main : Node
{
    [Export]
    public PackedScene LoadingScreen { get; set; }
    [Export]
    public PackedScene MainMenu { get; set; }
    [Export(PropertyHint.FilePath, "*.tscn,*.scn,")]
    public string Game { get; private set; }

    public MainUi Ui { get; private set; } = null;

    public override void _Ready()
    {
        if (LoadingScreen?.Instantiate() is Node3D node)
            AddChild(node);
        // var awaiter = Task.Run();
    }

    private void _LoadUI()
    {
        Ui = (MainUi)GetNode<InstancePlaceholder>("%UI").CreateInstance();
        UiManager.Instance.Ui = Ui;
    }

    private void _LoadGame()
    {
        Node3D game = _LoadPackedScene()
    }

    private static Node3D _LoadPackedScene(string path)
    {
        Error sceneLoader =
                ResourceLoader.LoadThreadedRequest(path, useSubThreads: true);
        if (sceneLoader != Error.Ok)
            GD.PrintErr(sceneLoader);
        PackedScene scene =
            ResourceLoader.LoadThreadedGet(path) as PackedScene;
        if (scene?.Instantiate() is not Node3D sceneNode) return null;
        sceneNode.Position = new Vector3(0, 0, -500);
        return sceneNode;
        // GetNode("%World").AddChild(sceneNode);
        // GD.Print("Planet loaded");
    }
}