using System;
using Game.Manager;
using Game.Resources.Building;
using Game.UI;
using Godot;

namespace Game;

public partial class Main : Node
{
	private BuildingResource _toPlaceBuildingResource;
	private GridManager _gridManager;
	private Sprite2D _cursorSprite;
    private Node2D _ySortRoot;
    private GameUI _gameUI;
    private Vector2I? _hoveredGridCell;

	public override void _Ready()
	{
		_cursorSprite = GetNode<Sprite2D>("Cursor");
		_gridManager = GetNode<GridManager>("GridManager");
		_ySortRoot = GetNode<Node2D>("YSortRoot");
		_gameUI = GetNode<GameUI>("GameUI");

		_cursorSprite.Visible = false;

		_gameUI.BuildingResourceSelected += OnBuildingResourceSelectedPressed;
		_gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
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
		if (_toPlaceBuildingResource != null && _cursorSprite.Visible & (!_hoveredGridCell.HasValue || _hoveredGridCell.Value != gridPosition))
		{
			_hoveredGridCell = gridPosition;
			_gridManager.ClearHighlightedTiles();
			_gridManager.HighlightExpandedBuildableTiles(gridPosition, _toPlaceBuildingResource);
			_gridManager.HighlightResourceTiles(gridPosition, _toPlaceBuildingResource);
		}
	}

	private void PlaceBuildingAtHoveredCellPosition()
	{
		if (!_hoveredGridCell.HasValue) return;

		var building = _toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
		_ySortRoot.AddChild(building);
		building.GlobalPosition = 64 * _hoveredGridCell.Value;

		_hoveredGridCell = null;
		_gridManager.ClearHighlightedTiles();
	}

	private void OnBuildingResourceSelectedPressed(BuildingResource buildingResource)
	{
		_toPlaceBuildingResource = buildingResource;
		_cursorSprite.Visible = true;
		_gridManager.HighlightBuildableTiles();	
    }

    private void OnResourceTilesUpdated(int collectedTilesCount)
    {
		GD.Print(collectedTilesCount);
    }
}
