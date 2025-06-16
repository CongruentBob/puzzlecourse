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
	private TileMapLayer highlightTilemapLayer;
	[Export]
	private TileMapLayer baseTerrainTilemapLayer;
	private readonly HashSet<Vector2I> validBuildableTiles = [];

	public override void _Ready()
	{
		GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
	}

    public bool IsTilePositionValid(Vector2I tilePosition)
	{
		var customData = baseTerrainTilemapLayer.GetCellTileData(tilePosition);
		if (customData == null) return false;
		return (bool)customData.GetCustomData("buildable");
	}

	public bool IsTilePositionBuildable(Vector2I tilePosition)
	{
		return validBuildableTiles.Contains(tilePosition);
	}

	public void HighlightBuildableTiles()
	{
		foreach (var tilePosition in validBuildableTiles)
		{
			highlightTilemapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
		}
	}

	public void ClearHighlightedTiles()
	{
		highlightTilemapLayer.Clear();
	}

	public Vector2I GetMouseGridCellPosition()
	{
		var mousePosition = highlightTilemapLayer.GetGlobalMousePosition();
		var gridPosition = mousePosition / 64;
		gridPosition = gridPosition.Floor();
		return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
	}

	private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
	{
		var rootCell = buildingComponent.GetGridCellPosition();
		var radius = buildingComponent.BuildableRadius;
		for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
		{
			for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
			{
				var tilePosition = new Vector2I(x, y);
				if (!IsTilePositionValid(tilePosition)) continue;
				validBuildableTiles.Add(tilePosition);
			}
		}

		validBuildableTiles.Remove(rootCell);
	}

    private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		UpdateValidBuildableTiles(buildingComponent);
	}
}
