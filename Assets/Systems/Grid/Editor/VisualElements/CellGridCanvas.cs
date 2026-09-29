using UnityEngine;
using UnityEngine.UIElements;

public class CellGridCanvas : VisualElement
{
	private Vector2 gridPosition = Vector2.zero;

	private Vector2Int _gridSize = Vector2Int.zero;
	public Vector2Int GridSize
	{
		get => _gridSize;
		set
		{
			_gridSize = value;
			EnsureGrid();
		}
	}

	public float CellSize { get; set; }

	private uint[][] _grid = null;
	public uint[][] Grid
	{
		get => _grid;
		set
		{
			_grid = value;
			EnsureGrid();
		}
	}

	private Vector2 localMousePosition = Vector2.zero;
	private Vector2 localMouseStartPosition = Vector2.zero;
	private Vector2 localMouseDeltaPosition = Vector2.zero;
	private bool isPainting = false;
	private int button = -1;

	private const float xOffset = 0.75f;
	private const float yOffset = 0.4330127f;
	private const float cellWidth = 1;
	private const float cellHeight = yOffset * 2f;


	public CellGridCanvas()
	{
		focusable = true;

		SetStyle();

		generateVisualContent += OnGenerateVisualContent;

		//gridPosition.x = localBound.width / 2f;

		RegisterCallback<PointerDownEvent>(OnPointerDown);
		RegisterCallback<PointerMoveEvent>(OnPointerMove);
		RegisterCallback<PointerUpEvent>(OnPointerUp);
		RegisterCallback<WheelEvent>(OnWheelScroll);
		RegisterCallback<GeometryChangedEvent>(OnResizeWindow);
	}

	private void OnGenerateVisualContent(MeshGenerationContext context)
	{
		///	Draw cells, grid lines, selection, brush preview, etc.
		DrawGrid(context);

		DrawPointerGridLines(context, localMousePosition);
		DrawPointerRectangle(context, localMousePosition);
	}


	public void LoadGrid(uint[][] grid, float cellSize)
	{
		this.Grid = grid;
		this.CellSize = cellSize;
	}


	private void SetStyle()
	{
		style.flexGrow = 1;
		style.overflow = Overflow.Hidden;
		style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
		style.marginBottom = 4;
		style.marginLeft = 4;
		style.marginRight = 4;
		style.marginTop = 4;
	}


	private void DrawGrid(MeshGenerationContext context)
	{
		for (uint x = 0; x < _gridSize.x; x++)
		{
			for (uint y = 0; y < _gridSize.y; y++)
			{
				float oddColumnOffset = (x & 1) != 0 ? yOffset * CellSize : 0f;

				float xPos = x * xOffset * CellSize;
				float yPos = y * cellHeight * CellSize + oddColumnOffset;

				Vector2 position = gridPosition + new Vector2(xPos, yPos);

				if (Grid[x][y] == 1)
					AllocateHexagon(context, position);
				else
					DrawHexagonOutline(context, position);
			}
		}
	}

	private bool EnsureGrid()
	{
		if (_gridSize.x <= 0 || _gridSize.y <= 0)
		{
			_grid = null;
			return false;
		}

		uint columns = (uint)_gridSize.x;
		uint rows = (uint)_gridSize.y;

		bool needsRebuild = Grid == null || Grid.Length != columns;
		if (!needsRebuild)
		{
			for (int x = 0; x < columns; x++)
			{
				if (_grid[x] == null || _grid[x].Length != rows)
				{
					needsRebuild = true;
					break;
				}
			}
		}
		if (!needsRebuild) return true;

		_grid = new uint[columns][];
		for (int x = 0; x < columns; x++)
		{
			_grid[x] = new uint[rows];
		}

		return true;
	}

	private void EditGridCells()
	{
		float radius = CellSize * 0.5f;
		float inRadius = Mathf.Sqrt(radius * radius * 0.75f);
		float spacingX = xOffset * CellSize;
		float spacingY = cellHeight * CellSize;

		Vector2 mouse = localMousePosition - new Vector2(radius + 2f, inRadius + 2f) - gridPosition;

		int evenColumn = 2 * Mathf.RoundToInt(mouse.x / (2f * spacingX));
		int evenRow = Mathf.RoundToInt(mouse.y / spacingY);

		int oddColumn = 2 * Mathf.RoundToInt((mouse.x - spacingX) / (2f * spacingX)) + 1;
		int oddRow = Mathf.RoundToInt((mouse.y - spacingY * 0.5f) / spacingY);

		Vector2 evenCenter = new(evenColumn * spacingX, evenRow * spacingY);
		Vector2 oddCenter = new(oddColumn * spacingX, (oddRow + 0.5f) * spacingY);

		bool evenIsCloser = (mouse - evenCenter).sqrMagnitude <= (mouse - oddCenter).sqrMagnitude;

		int column = evenIsCloser ? evenColumn : oddColumn;
		int row = evenIsCloser ? evenRow : oddRow;

		if (column < 0 || column >= _grid.Length || row < 0 || row >= _grid[column].Length)
			return;

		//Debug.Log($"Clicked cell: column: {column}, row: {row}");

		if (button == 0) _grid[column][row] = 1;
		else if (button == 1) _grid[column][row] = 0;
	}

	#region RENDERING

	private void DrawHexagonOutline(MeshGenerationContext context, Vector2 position)
	{
		if (CellSize <= 0f)
		{
			Debug.LogError("Cell size must be positive");
			return;
		}

		float radius = CellSize / 2f;
		float inRadius = Mathf.Sqrt(radius * radius - (radius / 2f) * (radius / 2f));

		float lineWidth = 2f;

		// Offset so the outline is not clipped at the top/left edge.
		Vector2 center = new Vector2(radius + lineWidth, inRadius + lineWidth);

		Vector2[] points =
		{
			new(position.x + center.x - radius / 2f,    position.y + center.y - inRadius),
			new(position.x + center.x - radius,         position.y + center.y),
			new(position.x + center.x - radius / 2f,    position.y + center.y + inRadius),
			new(position.x + center.x + radius / 2f,    position.y + center.y + inRadius),
			new(position.x + center.x + radius,         position.y + center.y),
			new(position.x + center.x + radius / 2f,    position.y + center.y - inRadius),
		};

		Painter2D painter = context.painter2D;

		painter.strokeColor = Color.white;
		painter.lineWidth = lineWidth;

		painter.BeginPath();
		painter.MoveTo(points[0]);

		for (int i = 1; i < points.Length; i++)
		{
			painter.LineTo(points[i]);
		}

		painter.ClosePath();
		painter.Stroke();
	}

	private void AllocateHexagon(MeshGenerationContext context, Vector2 position)
	{
		if (CellSize <= 0f)
		{
			Debug.LogError("Cell size must be positive");
			return;
		}

		float radius = CellSize / 2f;
		float inRadius = Mathf.Sqrt((radius * radius) - ((radius / 2) * (radius / 2)));

		Vector2 center = new Vector2(radius + 2f, inRadius + 2f);

		MeshWriteData mesh = context.Allocate(7, 18);

		///	Calculate vertices
		Vertex[] vertices = new Vertex[7]
		{
			///	0 bottom left
			new Vertex { position = new Vector3(position.x + center.x - radius / 2, position.y + center.y - inRadius,   Vertex.nearZ), tint = Color.white },
			///	1 mid left
			new Vertex { position = new Vector3(position.x + center.x - radius,     position.y + center.y,              Vertex.nearZ), tint = Color.white },
			///	2 top left
			new Vertex { position = new Vector3(position.x + center.x - radius / 2, position.y + center.y + inRadius,   Vertex.nearZ), tint = Color.white },
			///	3 top right
			new Vertex { position = new Vector3(position.x + center.x + radius / 2, position.y + center.y + inRadius,   Vertex.nearZ), tint = Color.white },
			///	4 mid right
			new Vertex { position = new Vector3(position.x + center.x + radius,     position.y + center.y,              Vertex.nearZ), tint = Color.white },
			///	5 bottom right
			new Vertex { position = new Vector3(position.x + center.x + radius / 2, position.y + center.y - inRadius,   Vertex.nearZ), tint = Color.white },
			///	6 middle
			new Vertex { position = new Vector3(position.x + center.x,              position.y + center.y,              Vertex.nearZ), tint = Color.white },
		};
		mesh.SetAllVertices(vertices);

		///	Calculate triangles
		ushort[] tris = new ushort[18]
		{
				/// bottom middle
				0, 5, 6,
				/// bottom right
				5, 4, 6,
				/// top right
				4, 3, 6,
				/// top middle
				3, 2, 6,
				/// top left
				2, 1, 6,
				/// bottom left
				1, 0, 6
		};
		mesh.SetAllIndices(tris);
	}

	#endregion

	private void Zoom(float scrollWheelDelta)
	{
		float previousCellSize = CellSize;

		CellSize += scrollWheelDelta > 0 ? -10 : 10;
		CellSize = Mathf.Clamp(CellSize, 10f, 300f);

		Vector2 mouse = localMousePosition - Vector2.one * 2f;
		Vector2 gridPoint = (mouse - gridPosition) / previousCellSize;

		gridPosition = mouse - gridPoint * CellSize;
	}

	private void Move()
	{
		gridPosition = localMouseDeltaPosition;
	}

	#region EVENTS

	private void OnPointerDown(PointerDownEvent evt)
	{
		if (CellSize <= 0f || _gridSize.x <= 0f || _gridSize.y <= 0f /*|| !EnsureGrid()*/) return;

		MarkDirtyRepaint();

		localMousePosition = evt.localPosition;
		localMouseStartPosition = (Vector2)evt.localPosition - gridPosition;

		isPainting = true;
		button = evt.button;

		if (button != 2)
			EditGridCells();
	}

	private void OnPointerMove(PointerMoveEvent evt)
	{
		if (!isPainting) return;

		MarkDirtyRepaint();

		localMousePosition = evt.localPosition;
		localMouseDeltaPosition = localMousePosition - localMouseStartPosition;

		if (button == 2)
			Move();
		else
			EditGridCells();
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		isPainting = false;
		button = -1;
	}

	private void OnWheelScroll(WheelEvent evt)
	{
		localMousePosition = evt.localMousePosition;

		Zoom(evt.delta.y);

		MarkDirtyRepaint();
		evt.StopPropagation();
	}

	private void OnResizeWindow(GeometryChangedEvent evt)
	{
		float width = evt.newRect.width;
		float height = evt.newRect.height;
		gridPosition.x = (width / 2f) - ((_grid.Length * CellSize) / 2f) * xOffset;
		gridPosition.y = (height / 2f) - ((_grid[0].Length * CellSize) / 2f) * cellHeight;
	}

	#endregion

	#region VISUAL HELPERS

	private void DrawPointerGridLines(MeshGenerationContext context, Vector2 pointerPos)
	{
		float lineWidth = 1f;
		Painter2D painter = context.painter2D;

		painter.strokeColor = Color.red;
		painter.lineWidth = lineWidth;

		painter.BeginPath();

		Vector2 verticalStart = new(pointerPos.x, 0f);
		Vector2 verticalEnd = new(pointerPos.x, localBound.yMax);
		painter.MoveTo(verticalStart);
		painter.LineTo(verticalEnd);

		Vector2 horizontalStart = new(0f, pointerPos.y);
		Vector2 horizontalEnd = new(localBound.xMax, pointerPos.y);
		painter.MoveTo(horizontalStart);
		painter.LineTo(horizontalEnd);

		painter.ClosePath();
		painter.Stroke();
	}

	private void DrawPointerRectangle(MeshGenerationContext context, Vector2 mousePos)
	{
		float lineWidth = 2f;
		Painter2D painter = context.painter2D;

		painter.strokeColor = Color.red;
		painter.lineWidth = lineWidth;

		painter.BeginPath();

		float halfWidth = (cellWidth / 2f) * CellSize;
		float halfHeight = (cellHeight / 2f) * CellSize;

		Vector2 topLeft = new(localMousePosition.x - halfWidth, localMousePosition.y + halfHeight);
		Vector2 topRight = new(localMousePosition.x + halfWidth, localMousePosition.y + halfHeight);
		Vector2 bottomLeft = new(localMousePosition.x - halfWidth, localMousePosition.y - halfHeight);
		Vector2 bottomRight = new(localMousePosition.x + halfWidth, localMousePosition.y - halfHeight);

		painter.MoveTo(topLeft);
		painter.LineTo(topRight);
		painter.LineTo(bottomRight);
		painter.LineTo(bottomLeft);
		painter.LineTo(topLeft);

		painter.ClosePath();
		painter.Stroke();
	}

	#endregion
}
