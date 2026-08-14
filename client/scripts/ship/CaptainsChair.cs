using Godot;
using Planets;

/// <summary>
/// The captain's chair in the ship. Used to pilot the ship.
/// </summary>
public partial class CaptainsChair : MeshInstance3D, IInteractable
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Interact()
	{
		GD.Print($"Interacted with {GetType()}.");
	}

}
