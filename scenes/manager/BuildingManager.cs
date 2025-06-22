using Game.Building;
using Game.Resources.Building;
using Game.UI;
using Godot;

namespace Game.Manager;

public partial class BuildingManager : Node
{
	private readonly StringName ACTION_LEFT_CLICK = "left_click";
	private readonly StringName ACTION_CANCEL = "cancel";

	[Export]
	private GridManager _gridManager;
	[Export]
    private GameUI _gameUI;
	[Export]
    private Node2D _ySortRoot;
	[Export]
	private PackedScene _buildingGhostScene;
	
    private Vector2I? _hoveredGridCell;
	private BuildingResource _toPlaceBuildingResource;
	private int _currentResourceCount;
	private int _startingResourceCount = 4;
	private int _currentlyUsedResourceCount;
	private BuildingGhost _buildingGhost;

	private int AvailableResourceCount => _startingResourceCount + _currentResourceCount - _currentlyUsedResourceCount;
	public override void _Ready()
	{
		_gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
		_gameUI.BuildingResourceSelected += OnBuildingResourceSelected;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed(ACTION_CANCEL))
		{
			ClearBuildingGhost();
		}
		else if (_hoveredGridCell.HasValue
				&& _toPlaceBuildingResource != null
				&& Input.IsActionJustPressed(ACTION_LEFT_CLICK)
				&& IsBuildingPlacableAtTile(_hoveredGridCell.Value))
		{
			PlaceBuildingAtHoveredCellPosition();
		}
	}

	public override void _Process(double delta)
	{
		if (!IsInstanceValid(_buildingGhost)) return;

		var gridPosition = _gridManager.GetMouseGridCellPosition();
		_buildingGhost.GlobalPosition = 64 * gridPosition;
		if (_toPlaceBuildingResource != null && (!_hoveredGridCell.HasValue || _hoveredGridCell.Value != gridPosition))
		{
			_hoveredGridCell = gridPosition;
			UpdateGridDisplay();
		}
	}

	private void UpdateGridDisplay()
	{
		if (!_hoveredGridCell.HasValue) return;

		_gridManager.ClearHighlightedTiles();
		_gridManager.HighlightBuildableTiles();
		if (IsBuildingPlacableAtTile(_hoveredGridCell.Value))
		{
			_gridManager.HighlightExpandedBuildableTiles(_hoveredGridCell.Value, _toPlaceBuildingResource);
			_gridManager.HighlightResourceTiles(_hoveredGridCell.Value, _toPlaceBuildingResource);
			_buildingGhost.SetIsValid(true);
		}
		else
		{
			_buildingGhost.SetIsValid(false);
		}
	}

	private void PlaceBuildingAtHoveredCellPosition()
	{
		if (!_hoveredGridCell.HasValue) return;

		var building = _toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
		_ySortRoot.AddChild(building);
		building.GlobalPosition = 64 * _hoveredGridCell.Value;

		_currentlyUsedResourceCount += _toPlaceBuildingResource.ResourceCost;
		ClearBuildingGhost();
	}

	private void ClearBuildingGhost()
	{
		_hoveredGridCell = null;
		_gridManager.ClearHighlightedTiles();

		if (IsInstanceValid(_buildingGhost))
		{
			_buildingGhost.QueueFree();
		}
		_buildingGhost = null;
	}

	private bool IsBuildingPlacableAtTile(Vector2I tilePosition)
	{
		return _gridManager.IsTilePositionBuildable(tilePosition)
			&& AvailableResourceCount >= _toPlaceBuildingResource.ResourceCost;
	}
	
	private void OnResourceTilesUpdated(int collectedTilesCount)
	{
		_currentResourceCount = collectedTilesCount;
	}

	private void OnBuildingResourceSelected(BuildingResource buildingResource)
	{
		if (IsInstanceValid(_buildingGhost))
		{
			_buildingGhost.QueueFree();
		}

		_buildingGhost = _buildingGhostScene.Instantiate<BuildingGhost>();
		_ySortRoot.AddChild(_buildingGhost);

		var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
		_buildingGhost.AddChild(buildingSprite);

		_toPlaceBuildingResource = buildingResource;
		UpdateGridDisplay();
    }

}
