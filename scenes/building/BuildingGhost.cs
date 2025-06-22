using Godot;

namespace Game.Building;

public partial class BuildingGhost : Node2D
{
	public void SetIsValid(bool isValid)
	{
		Modulate = isValid ? Colors.White : Colors.Red;
	}
}
