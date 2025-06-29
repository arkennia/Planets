using Godot;
using System;

namespace Planets.UI
{
    public partial class UiManager : Node
    {
        private Main MainNode { get; set; } = null;

        public MainUi Ui { get; set; } = null;

        public static UiManager Instance { get; private set; }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Instance ??= this;
            MainNode ??= GetNode<Main>("/root/Main");
            Ui ??= MainNode.Ui;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }
    }
}