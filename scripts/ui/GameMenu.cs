using Godot;
using System;
using System.Threading.Tasks;

public partial class GameMenu : Control
{

    private ConfirmationDialog _exitDialog;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _exitDialog = null;
        Hide();
        Button exitButton = GetNode<Button>("ExitButton");
        exitButton.Pressed += OnExitButtonPressed;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void OnExitButtonPressed()
    {
        GD.Print("Exit button pressed");
        if (_exitDialog == null)
            _ = ShowExitDialog();
    }

    private async Task ShowExitDialog()
    {
        CreateExitDialog();
        AddSibling(_exitDialog);
        _exitDialog.PopupCentered();
        _exitDialog.GrabFocus();
        await ToSignal(_exitDialog, ConfirmationDialog.SignalName.VisibilityChanged);
        _exitDialog = null;
    }

    private void CreateExitDialog()
    {
        ConfirmationDialog dialog = new();
        dialog.AlwaysOnTop = true;
        dialog.DialogText = "Are you sure you want to exit?";
        dialog.OkButtonText = "Yes";
        dialog.CancelButtonText = "No";
        dialog.Transient = true;
        dialog.Exclusive = true;
        _exitDialog = dialog;
    }
}
