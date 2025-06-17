using Game.Manager;
using Godot;

namespace Game;

public partial class Main : Node
{
    private PackedScene __towerScene = GD.Load<PackedScene>("res://scenes/building/tower.tscn");
    private PackedScene __villageScene = GD.Load<PackedScene>("res://scenes/building/village.tscn");
	private PackedScene _toPlaceBuildingScene;
	private GridManager _gridManager;
	private Sprite2D _cursorSprite;
    private Button _towerButton;
    private Button _villageButton;
    private Node2D _ySortRoot;

	private Vector2I? _hoveredGridCell;

	public override void _Ready()
	{
		_cursorSprite = GetNode<Sprite2D>("Cursor");
		_gridManager = GetNode<GridManager>("GridManager");
		_towerButton = GetNode<Button>("PlaceTowerButton");
		_villageButton = GetNode<Button>("PlaceVillageButton");
		_ySortRoot = GetNode<Node2D>("YSortRoot");

		_cursorSprite.Visible = false;

		_towerButton.Pressed += OnPlaceTowerButtonPressed;
		_villageButton.Pressed += OnPlaceVillageButtonPressed;
		_toPlaceBuildingScene = __towerScene;
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

		var building = _toPlaceBuildingScene.Instantiate<Node2D>();
		_ySortRoot.AddChild(building);
		building.GlobalPosition = 64 * _hoveredGridCell.Value;

		_hoveredGridCell = null;
		_gridManager.ClearHighlightedTiles();
	}

	private void OnPlaceTowerButtonPressed()
	{
		_toPlaceBuildingScene = __towerScene;
		_cursorSprite.Visible = true;
		_gridManager.HighlightBuildableTiles();	
    }

	private void OnPlaceVillageButtonPressed()
	{
		_toPlaceBuildingScene = __villageScene;
		_cursorSprite.Visible = true;
		_gridManager.HighlightBuildableTiles();	
    }
}
