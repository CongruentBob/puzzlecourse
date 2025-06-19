using Game.Resources.Building;
using Game.UI;
using Godot;

namespace Game.Manager;

public partial class BuildingManager : Node
{
	[Export]
	private GridManager _gridManager;
	[Export]
    private GameUI _gameUI;
	[Export]
    private Node2D _ySortRoot;
	[Export]
	private Sprite2D _cursorSprite;
	
    private Vector2I? _hoveredGridCell;
	private BuildingResource _toPlaceBuildingResource;
	private int _currentResourceCount;
	private int _startingResourceCount = 4;
	private int _currentlyUsedResourceCount;

	private int AvailableResourceCount => _startingResourceCount + _currentResourceCount - _currentlyUsedResourceCount;
	public override void _Ready()
	{
		_gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
		_gameUI.BuildingResourceSelected += OnBuildingResourceSelectedPressed;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_hoveredGridCell.HasValue
			&& _toPlaceBuildingResource != null
			&& Input.IsActionJustPressed("left_click")
			&& _gridManager.IsTilePositionBuildable(_hoveredGridCell.Value)
			&& AvailableResourceCount >= _toPlaceBuildingResource.ResourceCost)
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

		_currentlyUsedResourceCount += _toPlaceBuildingResource.ResourceCost;
		GD.Print(AvailableResourceCount);
	}
	
	private void OnResourceTilesUpdated(int collectedTilesCount)
	{
		_currentResourceCount = collectedTilesCount;
	}

	private void OnBuildingResourceSelectedPressed(BuildingResource buildingResource)
	{
		_toPlaceBuildingResource = buildingResource;
		_cursorSprite.Visible = true;
		_gridManager.HighlightBuildableTiles();	
    }

}
