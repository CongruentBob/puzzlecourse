using Godot;

namespace Game.UI;

public partial class GameUI : MarginContainer
{
	[Signal]
	public delegate void PlaceTowerButtonPressedEventHandler();
	[Signal]
	public delegate void PlaceVillageButtonPressedEventHandler();
    private Button _placeTowerButton;
    private Button _placeVillageButton;
	public override void _Ready()
	{
		_placeTowerButton = GetNode<Button>("%PlaceTowerButton");
		_placeVillageButton = GetNode<Button>("%PlaceVillageButton");

		_placeTowerButton.Pressed += OnPlaceTowerButtonPressed;
		_placeVillageButton.Pressed += OnPlaceVillageButtonPressed;
	}

	private void OnPlaceTowerButtonPressed()
	{
		EmitSignal(SignalName.PlaceTowerButtonPressed);
	}

	private void OnPlaceVillageButtonPressed()
	{
		EmitSignal(SignalName.PlaceVillageButtonPressed);
    }
}
