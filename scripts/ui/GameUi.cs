using System;
using Godot;

namespace Planets.UI;

/// <summary>
/// The main UI object for the game.
/// </summary>
public partial class GameUi : Control
{
    [Export]
    public PackedScene GameMenu { get; set; }

    [Signal]
    public delegate void GameMenuOpenedEventHandler();

    [Signal]
    public delegate void GameMenuClosedEventHandler();

    private bool _gameMenuOpen = false;

    private Control _gameMenu = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // GameMenu = ResourceLoader.Load<PackedScene>("res://scene.tscn");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionReleased("ui_cancel")) HandleGameMenu();
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void HandleGameMenu()
    {
        if (!_gameMenuOpen)
        {
            _gameMenu = (Control)GameMenu.Instantiate();
            AddChild(_gameMenu);
            EmitSignal(SignalName.GameMenuOpened);
            _gameMenuOpen = true;
            _gameMenu.Show();
        }
        else
        {
            EmitSignal(SignalName.GameMenuClosed);
            _gameMenuOpen = false;
            _gameMenu.QueueFree();
        }
    }
}