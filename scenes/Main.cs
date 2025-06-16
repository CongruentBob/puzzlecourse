using Game.Manager;
using Godot;

namespace Game;

public partial class Main : Node
{
    private PackedScene buildingScene = GD.Load<PackedScene>("res://scenes/building/building.tscn");
	private GridManager gridManager;
	private Sprite2D cursorSprite;
    private Button button;
	private Vector2I? hoveredGridCell;

	public override void _Ready()
	{
		cursorSprite = GetNode<Sprite2D>("Cursor");
		gridManager = GetNode<GridManager>("GridManager");
		button = GetNode<Button>("PlaceBuildingButton");

		cursorSprite.Visible = false;

		button.Pressed += OnButtonPressed;
	}

    public override void _UnhandledInput(InputEvent @event)
	{
		if (hoveredGridCell.HasValue
			&& Input.IsActionJustPressed("left_click")
			&& gridManager.IsTilePositionValid(hoveredGridCell.Value))
		{
			PlaceBuildingAtHoveredCellPosition();
			cursorSprite.Visible = false;
		}
	}

	public override void _Process(double delta)
	{
		var gridPosition = gridManager.GetMouseGridCellPosition();
		cursorSprite.GlobalPosition = 64 * gridPosition;
		if (cursorSprite.Visible & (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
		{
			hoveredGridCell = gridPosition;
			gridManager.HighlightBuildableTiles();
		}
	}

	private void PlaceBuildingAtHoveredCellPosition()
	{
		if (!hoveredGridCell.HasValue) return;

		var building = buildingScene.Instantiate<Node2D>();
		
		building.GlobalPosition = 64 * hoveredGridCell.Value;
		gridManager.MarkTileAsOccupied(hoveredGridCell.Value);
		AddChild(building);

		hoveredGridCell = null;
		gridManager.ClearHighlightedTiles();
	}

    private void OnButtonPressed()
    {
		cursorSprite.Visible = true;	
    }
}
