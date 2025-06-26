using Godot;
using System;

public partial class MainUI : Control
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            Control gameMenu = GetNode<Control>("GameMenu");
            gameMenu.Show();
        }
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
