using System;
using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Component;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
	[Export]
	private TileMapLayer _highlightTilemapLayer;
	[Export]
	private TileMapLayer _baseTerrainTilemapLayer;
	private readonly HashSet<Vector2I> _validBuildableTiles = [];

	public override void _Ready()
	{
		GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
	}

    public bool IsTilePositionValid(Vector2I tilePosition)
	{
		var customData = _baseTerrainTilemapLayer.GetCellTileData(tilePosition);
		if (customData == null) return false;
		return (bool)customData.GetCustomData("buildable");
	}

	public bool IsTilePositionBuildable(Vector2I tilePosition)
	{
		return _validBuildableTiles.Contains(tilePosition);
	}

	public void HighlightBuildableTiles()
	{
		foreach (var tilePosition in _validBuildableTiles)
		{
			_highlightTilemapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
		}
	}

	public void HighlightExpandedBuildableTiles(Vector2I rootCell, int radius)
	{
		ClearHighlightedTiles();
		HighlightBuildableTiles();

		var validTiles = GetValidTilesInRadius(rootCell, radius).ToHashSet();
		var expandedTiles = validTiles.Except(_validBuildableTiles).Except(GetOccupiedTiles());
		foreach (var tilePosition in expandedTiles)
		{
			_highlightTilemapLayer.SetCell(tilePosition, 0, Vector2I.Right);
		}
	}

	public void ClearHighlightedTiles()
	{
		_highlightTilemapLayer.Clear();
	}

	public Vector2I GetMouseGridCellPosition()
	{
		var mousePosition = _highlightTilemapLayer.GetGlobalMousePosition();
		var gridPosition = mousePosition / 64;
		gridPosition = gridPosition.Floor();
		return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
	}

	private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
	{
		var rootCell = buildingComponent.GetGridCellPosition();
		var validTiles = GetValidTilesInRadius(rootCell, buildingComponent.BuildableRadius);
		_validBuildableTiles.UnionWith(validTiles);
		_validBuildableTiles.ExceptWith(GetOccupiedTiles());
	}

	private IEnumerable<Vector2I> GetOccupiedTiles()
	{
		var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();
		return buildingComponents.Select(c => c.GetGridCellPosition());
	}

	private IEnumerable<Vector2I> GetValidTilesInRadius(Vector2I rootCell, int radius)
	{
		for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
		{
			for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
			{
				var tilePosition = new Vector2I(x, y);
				if (!IsTilePositionValid(tilePosition)) continue;
				yield return tilePosition;
			}
		}
	}

    private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		UpdateValidBuildableTiles(buildingComponent);
	}
}
