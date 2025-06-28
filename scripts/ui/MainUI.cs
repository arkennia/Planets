using Godot;
using System;

public partial class MainUI : Control
{

    private bool _gameMenuOpen = false;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    public override void _Input(InputEvent @event)
    {

        if (@event.IsActionReleased("ui_cancel"))
        {
            HandleGameMenu();
        }
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void HandleGameMenu()
    {
        Control gameMenu = GetNode<Control>("GameMenu");
        if (!_gameMenuOpen)
        {
            _gameMenuOpen = true;
            gameMenu.Show();
        }
        else
        {
            _gameMenuOpen = false;
            gameMenu.Hide();
        }
    }
}
