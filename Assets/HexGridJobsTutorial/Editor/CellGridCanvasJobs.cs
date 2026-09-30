using UnityEngine;
using UnityEngine.UIElements;

// Optional complete reference canvas. This distinct class name lets you compare it
// with the original CellGridCanvas without replacing the original file.
public sealed class CellGridCanvasJobs : VisualElement
{
	public Vector2Int gridSize = Vector2Int.zero;
	public float cellSize = 100f;
	public uint[][] grid;

	// Toggle these in the demo to compare exactly the same renderer.
	public bool useJobs = true;
	public bool cullOffscreen = true;

	private Vector2 gridPosition;
	private Vector2 mousePosition;
	private Vector2 dragOffset;
	private int activePointer = -1;
	private int activeButton = -1;
	private bool pointerInside;
	private readonly VisualElement pointerOverlay;

	public CellGridCanvasJobs()
	{
		focusable = true;
		style.flexGrow = 1;
		style.minWidth = 0;
		style.minHeight = 0;
		style.marginLeft = 4;
		style.marginRight = 4;
		style.marginTop = 4;
		style.marginBottom = 4;
		style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
		style.overflow = Overflow.Hidden;

		generateVisualContent += DrawGrid;

		// Independent visual content: hovering only invalidates this small element.
		pointerOverlay = new VisualElement { pickingMode = PickingMode.Ignore };
		pointerOverlay.style.position = Position.Absolute;
		pointerOverlay.style.left = 0;
		pointerOverlay.style.right = 0;
		pointerOverlay.style.top = 0;
		pointerOverlay.style.bottom = 0;
		pointerOverlay.generateVisualContent += DrawPointerOverlay;
		Add(pointerOverlay);

		RegisterCallback<GeometryChangedEvent>(_ => MarkDirtyRepaint());
		RegisterCallback<PointerDownEvent>(OnPointerDown);
		RegisterCallback<PointerMoveEvent>(OnPointerMove);
		RegisterCallback<PointerUpEvent>(OnPointerUp);
		RegisterCallback<PointerCaptureOutEvent>(evt =>
		{
			if (evt.pointerId == activePointer) ClearInteraction();
		});
		RegisterCallback<PointerEnterEvent>(_ =>
		{
			pointerInside = true;
			pointerOverlay.MarkDirtyRepaint();
		});
		RegisterCallback<PointerLeaveEvent>(_ =>
		{
			pointerInside = false;
			pointerOverlay.MarkDirtyRepaint();
		});
		RegisterCallback<WheelEvent>(OnWheel);
	}

	// Call this before a repaint after changing dimensions; data is reset when dimensions change.
	public void SetGridSize(Vector2Int size)
	{
		gridSize = size;
		EnsureGrid();
		MarkDirtyRepaint();
	}

	public void ResetView()
	{
		gridPosition = Vector2.zero;
		cellSize = 100f;
		dragOffset = mousePosition - gridPosition;
		MarkDirtyRepaint();
		pointerOverlay.MarkDirtyRepaint();
	}

	private void DrawGrid(MeshGenerationContext context)
	{
		HexGridMeshJobs.DrawGrid(context, grid, gridSize, cellSize, gridPosition, contentRect, useJobs, cullOffscreen);
	}

	private bool EnsureGrid()
	{
		if (gridSize.x <= 0 || gridSize.y <= 0)
		{
			grid = null;
			return false;
		}

		bool rebuild = grid == null || grid.Length != gridSize.x;
		if (!rebuild)
		{
			for (int column = 0; column < grid.Length; column++)
			{
				if (grid[column] == null || grid[column].Length != gridSize.y)
				{
					rebuild = true;
					break;
				}
			}
		}
		if (!rebuild) return true;

		grid = new uint[gridSize.x][];
		for (int column = 0; column < grid.Length; column++)
			grid[column] = new uint[gridSize.y];
		return true;
	}

	// Retains your nearest-even/nearest-odd-column picking method.
	// Return true ONLY if a stored value actually changed.
	private bool EditGridCells()
	{
		if (grid == null || cellSize <= 0f || !contentRect.Contains(mousePosition) || (activeButton != 0 && activeButton != 1)) return false;

		float radius = cellSize * 0.5f;
		float apothem = radius * HexGridMeshJobs.HeightFactor;
		float stepX = cellSize * HexGridMeshJobs.ColumnStepFactor;
		float stepY = cellSize * HexGridMeshJobs.HeightFactor;
		Vector2 mouse = mousePosition - gridPosition - new Vector2(radius + HexGridMeshJobs.DrawingPadding, apothem + HexGridMeshJobs.DrawingPadding);

		int evenColumn = 2 * Mathf.RoundToInt(mouse.x / (2f * stepX));
		int evenRow = Mathf.RoundToInt(mouse.y / stepY);
		int oddColumn = 2 * Mathf.RoundToInt((mouse.x - stepX) / (2f * stepX)) + 1;
		int oddRow = Mathf.RoundToInt((mouse.y - stepY * 0.5f) / stepY);
		Vector2 evenCenter = new Vector2(evenColumn * stepX, evenRow * stepY);
		Vector2 oddCenter = new Vector2(oddColumn * stepX, (oddRow + 0.5f) * stepY);
		bool even = (mouse - evenCenter).sqrMagnitude <= (mouse - oddCenter).sqrMagnitude;
		int column = even ? evenColumn : oddColumn;
		int row = even ? evenRow : oddRow;

		if (column < 0 || column >= grid.Length || grid[column] == null || row < 0 || row >= grid[column].Length) return false;

		uint value = activeButton == 0 ? 1u : 0u;
		if (grid[column][row] == value) return false;

		grid[column][row] = value;
		return true;
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		if (activePointer != -1 || evt.button < 0 || evt.button > 2 || cellSize <= 0f || !EnsureGrid()) return;

		Focus();
		mousePosition = evt.localPosition;
		dragOffset = mousePosition - gridPosition;
		activePointer = evt.pointerId;
		activeButton = evt.button;
		pointerInside = true;
		this.CapturePointer(evt.pointerId);

		if (EditGridCells()) MarkDirtyRepaint();
		pointerOverlay.MarkDirtyRepaint();
		evt.StopPropagation();
	}

	private void OnPointerMove(PointerMoveEvent evt)
	{
		mousePosition = evt.localPosition;
		pointerInside = contentRect.Contains(mousePosition);
		pointerOverlay.MarkDirtyRepaint();

		if (evt.pointerId != activePointer) return;

		if (activeButton == 2)
		{
			Vector2 nextPosition = mousePosition - dragOffset;
			if (nextPosition != gridPosition)
			{
				gridPosition = nextPosition;
				MarkDirtyRepaint();
			}
		}
		else if (EditGridCells())
		{
			MarkDirtyRepaint();
		}
		evt.StopPropagation();
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		if (evt.pointerId != activePointer || evt.button != activeButton) return;
		int pointer = activePointer;
		ClearInteraction();
		this.ReleasePointer(pointer);
		evt.StopPropagation();
	}

	private void ClearInteraction()
	{
		activePointer = -1;
		activeButton = -1;
	}

	private void OnWheel(WheelEvent evt)
	{
		if (evt.delta.y == 0f || cellSize <= 0f) return;

		mousePosition = evt.localMousePosition;
		float oldSize = cellSize;
		float nextSize = Mathf.Clamp(oldSize + (evt.delta.y > 0f ? -10f : 1f), 1f, 300f);
		if (nextSize != oldSize)
		{
			Vector2 mouse = mousePosition - Vector2.one * HexGridMeshJobs.DrawingPadding;
			Vector2 gridPoint = (mouse - gridPosition) / oldSize;
			cellSize = nextSize;
			gridPosition = mouse - gridPoint * cellSize;

			// Preserve panning continuity if the wheel is used while middle-dragging.
			dragOffset = mousePosition - gridPosition;
			MarkDirtyRepaint();
			pointerOverlay.MarkDirtyRepaint();
		}
		evt.StopPropagation();
	}

	private void DrawPointerOverlay(MeshGenerationContext context)
	{
		if (!pointerInside) return;

		Painter2D painter = context.painter2D;
		// Convert from canvas coordinates to overlay coordinates explicitly.
		Vector2 mouse = this.ChangeCoordinatesTo(pointerOverlay, mousePosition);
		Rect bounds = pointerOverlay.contentRect;
		painter.strokeColor = Color.red;
		painter.lineWidth = 1f;
		painter.BeginPath();
		painter.MoveTo(new Vector2(mouse.x, bounds.yMin));
		painter.LineTo(new Vector2(mouse.x, bounds.yMax));
		painter.MoveTo(new Vector2(bounds.xMin, mouse.y));
		painter.LineTo(new Vector2(bounds.xMax, mouse.y));
		painter.Stroke();

		float halfWidth = cellSize * 0.5f;
		float halfHeight = cellSize * HexGridMeshJobs.HeightFactor * 0.5f;
		painter.lineWidth = 2f;
		painter.BeginPath();
		painter.MoveTo(mouse + new Vector2(-halfWidth, -halfHeight));
		painter.LineTo(mouse + new Vector2(halfWidth, -halfHeight));
		painter.LineTo(mouse + new Vector2(halfWidth, halfHeight));
		painter.LineTo(mouse + new Vector2(-halfWidth, halfHeight));
		painter.ClosePath();
		painter.Stroke();
	}
}
