using Godot;

namespace Planets;

public partial class Main : Node
{
    // Called when the node enters the scene tree for the first time.

    public override void _EnterTree()
    {
        GetTree().SetMultiplayer(MultiplayerApi.CreateDefaultInterface(), "/root/Main/Client0");
    }


    public override void _Ready()
    {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
