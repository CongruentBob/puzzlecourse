using Game.Manager;
using Godot;

namespace Game;

public partial class Main : Node
{
    private PackedScene __buildingScene = GD.Load<PackedScene>("res://scenes/building/building.tscn");
	private GridManager _gridManager;
	private Sprite2D _cursorSprite;
    private Button _button;
	private Vector2I? _hoveredGridCell;

	public override void _Ready()
	{
		_cursorSprite = GetNode<Sprite2D>("Cursor");
		_gridManager = GetNode<GridManager>("GridManager");
		_button = GetNode<Button>("PlaceBuildingButton");

		_cursorSprite.Visible = false;

		_button.Pressed += OnButtonPressed;
	}

    public override void _UnhandledInput(InputEvent @event)
	{
		if (_hoveredGridCell.HasValue
			&& Input.IsActionJustPressed("left_click")
			&& _gridManager.IsTilePositionBuildable(_hoveredGridCell.Value))
		{
			PlaceBuildingAtHoveredCellPosition();
			_cursorSprite.Visible = false;
		}
	}

	public override void _Process(double delta)
	{
		var gridPosition = _gridManager.GetMouseGridCellPosition();
		_cursorSprite.GlobalPosition = 64 * gridPosition;
		if (_cursorSprite.Visible & (!_hoveredGridCell.HasValue || _hoveredGridCell.Value != gridPosition))
		{
			_hoveredGridCell = gridPosition;
			_gridManager.HighlightExpandedBuildableTiles(gridPosition, 3);
		}
	}

	private void PlaceBuildingAtHoveredCellPosition()
	{
		if (!_hoveredGridCell.HasValue) return;

		var building = __buildingScene.Instantiate<Node2D>();
		AddChild(building);
		building.GlobalPosition = 64 * _hoveredGridCell.Value;

		_hoveredGridCell = null;
		_gridManager.ClearHighlightedTiles();
	}

	private void OnButtonPressed()
	{
		_cursorSprite.Visible = true;
		_gridManager.HighlightBuildableTiles();	
    }
}
