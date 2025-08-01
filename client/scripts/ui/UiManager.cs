using System;
using Godot;

namespace Planets.UI;

/// <summary>
/// This class is an autoload. It enables connections from the UI to other nodes as needed.
/// </summary>
public partial class UiManager : Node
{
    private Client MainNode { get; set; } = null;

    public GameUi Ui { get; set; } = null;

    public static UiManager Instance { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance ??= this;
        MainNode ??= GetNode<Client>("/root/Main/Main");
        Ui ??= MainNode.Ui;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}