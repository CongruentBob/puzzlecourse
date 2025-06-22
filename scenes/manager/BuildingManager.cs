using Game.Building;
using Game.Resources.Building;
using Game.UI;
using Godot;

namespace Game.Manager;

public partial class BuildingManager : Node
{
	private readonly StringName ACTION_LEFT_CLICK = "left_click";
	private readonly StringName ACTION_RIGHT_CLICK = "right_click";
	private readonly StringName ACTION_CANCEL = "cancel";

	[Export]
	private GridManager _gridManager;
	[Export]
    private GameUI _gameUI;
	[Export]
    private Node2D _ySortRoot;
	[Export]
	private PackedScene _buildingGhostScene;

	private enum State
	{
		Normal,
		PlacingBuilding,
	}

    private Vector2I _hoveredGridCell;
	private BuildingResource _toPlaceBuildingResource;
	private int _currentResourceCount;
	private int _startingResourceCount = 4;
	private int _currentlyUsedResourceCount;
	private BuildingGhost _buildingGhost;
	private State _currentState = State.Normal;

	private int AvailableResourceCount => _startingResourceCount + _currentResourceCount - _currentlyUsedResourceCount;
	public override void _Ready()
	{
		_gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
		_gameUI.BuildingResourceSelected += OnBuildingResourceSelected;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		switch (_currentState)
		{
			case State.Normal:
				if (@event.IsActionPressed(ACTION_RIGHT_CLICK))
				{
					DestroyBuildingAtHoveredCellPosition();
				}
				break;
			case State.PlacingBuilding:
				if (@event.IsActionPressed(ACTION_CANCEL))
				{
					ChangeState(State.Normal);
				}
				else if (_toPlaceBuildingResource != null
						&& Input.IsActionJustPressed(ACTION_LEFT_CLICK)
						&& IsBuildingPlacableAtTile(_hoveredGridCell))
				{
					PlaceBuildingAtHoveredCellPosition();
				}
				break;
			default:
				break;
		}
	}

	public override void _Process(double delta)
	{
		var gridPosition = _gridManager.GetMouseGridCellPosition();
		if (_hoveredGridCell != gridPosition)
		{
			_hoveredGridCell = gridPosition;
			UpdateHoveredGridCell();
		}
		
		switch (_currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				_buildingGhost.GlobalPosition = 64 * gridPosition;
				break;
		}
	}

	private void UpdateGridDisplay()
	{
		_gridManager.ClearHighlightedTiles();
		_gridManager.HighlightBuildableTiles();
		if (IsBuildingPlacableAtTile(_hoveredGridCell))
		{
			_gridManager.HighlightExpandedBuildableTiles(_hoveredGridCell, _toPlaceBuildingResource);
			_gridManager.HighlightResourceTiles(_hoveredGridCell, _toPlaceBuildingResource);
			_buildingGhost.SetIsValid(true);
		}
		else
		{
			_buildingGhost.SetIsValid(false);
		}
	}

	private void PlaceBuildingAtHoveredCellPosition()
	{
		var building = _toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
		_ySortRoot.AddChild(building);
		building.GlobalPosition = 64 * _hoveredGridCell;

		_currentlyUsedResourceCount += _toPlaceBuildingResource.ResourceCost;
		
		ChangeState(State.Normal);
	}

	private void DestroyBuildingAtHoveredCellPosition()
	{

	}

	private void ClearBuildingGhost()
	{
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

	private void UpdateHoveredGridCell()
	{
		switch (_currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				UpdateGridDisplay();
				break;
		}
	}

	private void ChangeState(State toState)
	{
		switch (_currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				ClearBuildingGhost();
				_toPlaceBuildingResource = null;
				break;
		}

		_currentState = toState;
		
		switch (_currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				_buildingGhost = _buildingGhostScene.Instantiate<BuildingGhost>();
				_ySortRoot.AddChild(_buildingGhost);

				break;
		}
	}
	
	private void OnResourceTilesUpdated(int collectedTilesCount)
	{
		_currentResourceCount = collectedTilesCount;
	}


	private void OnBuildingResourceSelected(BuildingResource buildingResource)
	{
		ChangeState(State.PlacingBuilding);

		var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
		_buildingGhost.AddChild(buildingSprite);

		_toPlaceBuildingResource = buildingResource;
		UpdateGridDisplay();
	}
}
