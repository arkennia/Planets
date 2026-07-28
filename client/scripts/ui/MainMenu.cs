using System;
using Godot;
using Planets;

public partial class MainMenu : Control
{
    [Export]
    public Button StartButton { get; set; }
    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        StartButton.Pressed += GameManager.Instance.ConnectToServer;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
