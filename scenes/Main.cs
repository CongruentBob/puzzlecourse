using System.Collections.Generic;
using Godot;

namespace Game;

public partial class Main : Node
{
    private PackedScene buildingScene = GD.Load<PackedScene>("res://scenes/building/building.tscn");
	private Sprite2D cursorSprite;
    private Button button;
    private TileMapLayer highlightTileMapLayer;
	private Vector2? hoveredGridCell;
	private readonly HashSet<Vector2> occupiedCells = [];

	public override void _Ready()
	{
		cursorSprite = GetNode<Sprite2D>("Cursor");
		button = GetNode<Button>("PlaceBuildingButton");
		highlightTileMapLayer = GetNode<TileMapLayer>("HighlightTileMapLayer");

		cursorSprite.Visible = false;

		button.Pressed += OnButtonPressed;
	}

    public override void _UnhandledInput(InputEvent @event)
	{
		if (hoveredGridCell.HasValue && Input.IsActionJustPressed("left_click") && !occupiedCells.Contains(hoveredGridCell.Value))
		{
			PlaceBuildingAtHoveredCellPosition();
			cursorSprite.Visible = false;
		}
	}

	public override void _Process(double delta)
	{
		var gridPosition = GetMouseGridCellPosition();
		cursorSprite.GlobalPosition = 64 * gridPosition;
		if (cursorSprite.Visible & (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
		{
			hoveredGridCell = gridPosition;
			UpdateHighlightTileMapLayer();
		}	
	}

	private void PlaceBuildingAtHoveredCellPosition()
	{
		if (!hoveredGridCell.HasValue) return;

		var building = buildingScene.Instantiate<Node2D>();
		
		building.GlobalPosition = 64 * hoveredGridCell.Value;
		occupiedCells.Add(hoveredGridCell.Value);
		AddChild(building);

		hoveredGridCell = null;
		highlightTileMapLayer.Clear();
	}

	private void UpdateHighlightTileMapLayer()
	{
		highlightTileMapLayer.Clear();
		if (!hoveredGridCell.HasValue) return;

		for (var x = hoveredGridCell.Value.X - 3; x <= hoveredGridCell.Value.X + 3; x++)
		{
			for (var y = hoveredGridCell.Value.Y - 3; y <= hoveredGridCell.Value.Y + 3; y++)
			{
				highlightTileMapLayer.SetCell(new Vector2I((int)x, (int)y), 0, new Vector2I(0, 0), 0);
			}
		}
	}

	private Vector2 GetMouseGridCellPosition()
	{
		var mousePosition = highlightTileMapLayer.GetGlobalMousePosition();
		var gridPosition = mousePosition / 64;
		return gridPosition.Floor();
	}

    private void OnButtonPressed()
    {
		cursorSprite.Visible = true;	
    }
}
