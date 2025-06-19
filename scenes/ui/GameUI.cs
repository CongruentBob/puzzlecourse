using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class GameUI : MarginContainer
{
	[Signal]
	public delegate void BuildingResourceSelectedEventHandler(BuildingResource buildingResource);

	[Export]
	private BuildingResource[] _buildingResources;
    private HBoxContainer _hBoxContainer;

	public override void _Ready()
	{
		_hBoxContainer = GetNode<HBoxContainer>("HBoxContainer");
		CreateBuildingButtons();
	}

	private void CreateBuildingButtons()
	{
		foreach (var buildingResource in _buildingResources)
		{
			var buildingButton = new Button()
			{
				Text = $"Place {buildingResource.DisplayName}",
			};
			buildingButton.Pressed += () => EmitSignal(SignalName.BuildingResourceSelected, buildingResource);
			_hBoxContainer.AddChild(buildingButton);
		}
	}
}
