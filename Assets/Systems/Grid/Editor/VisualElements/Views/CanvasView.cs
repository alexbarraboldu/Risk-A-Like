using ABB.Grid.Editor.Data;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class CanvasView : VisualElement
	{
		private SessionData session = null;
		private TerrainColorData terrainColors = null;
		private GridData gridData = new();

		public Vector2Int GridSize => new(gridData.columns, gridData.rows);


		private bool _deleteCell;
		private bool isPainting = false;
		private int button = -1;

		private Vector2 gridPosition = Vector2.zero;
		private Vector2 localMousePosition = Vector2.zero;
		private Vector2 localMouseStartPosition = Vector2.zero;
		private Vector2 localMouseDeltaPosition = Vector2.zero;

		private const float xOffset = 0.75f;
		private const float yOffset = 0.4330127f;
		private const float cellWidth = 1;
		private const float cellHeight = yOffset * 2f;


		public CanvasView(ref SessionData session, ref TerrainColorData terrainColors)
		{
			focusable = true;

			SetStyle();

			generateVisualContent += OnGenerateVisualContent;

			RegisterCallback<PointerDownEvent>(OnPointerDown);
			RegisterCallback<PointerMoveEvent>(OnPointerMove);
			RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
			RegisterCallback<PointerUpEvent>(OnPointerUp);
			RegisterCallback<WheelEvent>(OnWheelScroll);
			RegisterCallback<GeometryChangedEvent>(OnResizeWindow);

			this.session = session;
			this.terrainColors = terrainColors;
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

		private void OnGenerateVisualContent(MeshGenerationContext context)
		{
			DrawGrid(context);

			DrawPointerGridLines(context, localMousePosition);
			DrawPointerRectangle(context, localMousePosition);
		}

		private bool EnsureGrid()
		{
			if (gridData.columns <= 0 || gridData.rows <= 0)
			{
				gridData.grid = new int[1][];
				gridData.grid[0] = new int[1];
				return false;
			}

			bool needsRebuild = gridData.grid == null || gridData.grid.Length != gridData.columns;
			if (!needsRebuild)
			{
				for (int x = 0; x < gridData.columns; x++)
				{
					if (gridData.grid[x] == null || gridData.grid[x].Length != gridData.rows)
					{
						needsRebuild = true;
						break;
					}
				}
			}
			if (!needsRebuild) return true;

			int[][] aux = new int[gridData.columns][];
			int gridColumns = gridData.grid.Length, gridRows = gridData.grid[0].Length;
			for (int auxC = 0, c = 0; auxC < gridData.columns; auxC++, c++)
			{
				aux[auxC] = new int[gridData.rows];
				for (int auxR = 0, r = 0; auxR < gridData.rows; auxR++, r++)
				{
					if (auxC >= gridColumns)
					{
						aux[auxC][auxR] = -1;
						continue;
					}
					else if (auxR >= gridRows)
					{
						aux[auxC][auxR] = -1;
						continue;
					}

					aux[auxC][auxR] = gridData.grid[c][r];
				}
			}

			gridData.grid = aux;

			return true;
		}

		public void SetGridData(GridData gridData)
		{
			this.gridData = gridData;

			EnsureGrid();
			MarkDirtyRepaint();
		}

		public void SetGridSize(Vector2Int value)
		{
			gridData.columns = value.x;
			gridData.rows = value.y;

			EnsureGrid();
			MarkDirtyRepaint();
		}

		#region RENDERING

		private void DrawGrid(MeshGenerationContext context)
		{
			for (int c = 0; c < gridData.columns; c++)
			{
				for (int r = 0; r < gridData.rows; r++)
				{
					float oddColumnOffset = (c & 1) != 0 ? yOffset * session.cellSize : 0f;

					float xPos = c * xOffset * session.cellSize;
					float yPos = r * cellHeight * session.cellSize + oddColumnOffset;

					Vector2 position = gridPosition + new Vector2(xPos, yPos);

					int cellValue = gridData.grid[c][r];
					if (cellValue != -1)
						AllocateHexagon(context, position, cellValue);
					else
						DrawHexagonOutline(context, position);
				}
			}
		}

		private void DrawHexagonOutline(MeshGenerationContext context, Vector2 position)
		{
			if (session.cellSize <= 0f)
			{
				Debug.LogError("Cell size must be positive");
				return;
			}

			float radius = session.cellSize / 2f;
			float inRadius = Mathf.Sqrt(radius * radius - (radius / 2f) * (radius / 2f));

			float lineWidth = 2f;

			/// Offset so the outline is not clipped at the top/left edge.
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

		private void AllocateHexagon(MeshGenerationContext context, Vector2 position, int cellValue)
		{
			if (session.cellSize <= 0f)
			{
				Debug.LogError("Cell size must be positive");
				return;
			}

			float radius = session.cellSize / 2f;
			float inRadius = Mathf.Sqrt((radius * radius) - ((radius / 2) * (radius / 2)));

			Vector2 center = new Vector2(radius + 2f, inRadius + 2f);

			MeshWriteData mesh = context.Allocate(7, 18);

			Color tint = SelectCellTint(cellValue);

			///	Calculate vertices
			Vertex[] vertices = new Vertex[7]
			{
			///	0 bottom left
			new Vertex { position = new Vector3(position.x + center.x - radius / 2, position.y + center.y - inRadius,   Vertex.nearZ), tint = tint },
			///	1 mid left
			new Vertex { position = new Vector3(position.x + center.x - radius,     position.y + center.y,              Vertex.nearZ), tint = tint },
			///	2 top left
			new Vertex { position = new Vector3(position.x + center.x - radius / 2, position.y + center.y + inRadius,   Vertex.nearZ), tint = tint },
			///	3 top right
			new Vertex { position = new Vector3(position.x + center.x + radius / 2, position.y + center.y + inRadius,   Vertex.nearZ), tint = tint },
			///	4 mid right
			new Vertex { position = new Vector3(position.x + center.x + radius,     position.y + center.y,              Vertex.nearZ), tint = tint },
			///	5 bottom right
			new Vertex { position = new Vector3(position.x + center.x + radius / 2, position.y + center.y - inRadius,   Vertex.nearZ), tint = tint },
			///	6 middle
			new Vertex { position = new Vector3(position.x + center.x,              position.y + center.y,              Vertex.nearZ), tint = tint },
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

		private Color SelectCellTint(int cellValue)
		{
			Color color = terrainColors.terrainColors[cellValue].color;

			return color;
		}

		#endregion

		#region ACTIONS

		private void EditGridCells()
		{
			if (session.cellSize <= 0f)
			{
				Debug.LogError("Cell size must be positive");
				return;
			}

			float radius = session.cellSize * 0.5f;
			float inRadius = Mathf.Sqrt(radius * radius * 0.75f);
			float spacingX = xOffset * session.cellSize;
			float spacingY = cellHeight * session.cellSize;

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

			if (column < 0 || column >= gridData.grid.Length || row < 0 || row >= gridData.grid[column].Length)
				return;

			if (button == 0) gridData.grid[column][row] = _deleteCell ? -1 : (int)session.leftColor;
			else if (button == 1) gridData.grid[column][row] = _deleteCell ? -1 : (int)session.rightColor;
		}

		private void Zoom(float scrollWheelDelta)
		{
			if (session.cellSize <= 0f)
			{
				Debug.LogError("Cell size must be positive");
				return;
			}

			float previousCellSize = session.cellSize;

			session.cellSize += scrollWheelDelta > 0 ? -10 : 10;
			session.cellSize = Mathf.Clamp(session.cellSize, 10f, 300f);

			Vector2 mouse = localMousePosition - Vector2.one * 2f;
			Vector2 gridPoint = (mouse - gridPosition) / previousCellSize;

			gridPosition = mouse - gridPoint * session.cellSize;
		}

		private void Pan()
		{
			gridPosition = localMouseDeltaPosition;
		}

		public void DeleteCell()
		{
			_deleteCell = !_deleteCell;
		}

		public void FillGrid()
		{
			EnsureGrid();

			for (int c = 0; c < gridData.columns; c++)
			{
				for (int r = 0; r < gridData.rows; r++)
				{
					gridData.grid[c][r] = (int)session.leftColor;
				}
			}

			MarkDirtyRepaint();
		}

		public void ResetGrid()
		{
			EnsureGrid();

			for (int c = 0; c < gridData.columns; c++)
			{
				for (int r = 0; r < gridData.rows; r++)
				{
					gridData.grid[c][r] = -1;
				}
			}

			MarkDirtyRepaint();
		}

		#endregion

		#region EVENTS

		private void OnPointerDown(PointerDownEvent evt)
		{
			if (session.cellSize <= 0f || gridData.columns <= 0f || gridData.rows <= 0f /*|| !EnsureGrid()*/) return;

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
				Pan();
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
			if (gridData.columns <= 0 || gridData.rows <= 0) return;

			float width = evt.newRect.width;
			float height = evt.newRect.height;
			gridPosition.x = (width / 2f) - ((gridData.grid.Length * session.cellSize) / 2f) * xOffset;
			gridPosition.y = (height / 2f) - ((gridData.grid[0].Length * session.cellSize) / 2f) * cellHeight;
		}

		private void OnPointerLeave(PointerLeaveEvent evt)
		{
			isPainting = false;
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

			float halfWidth = (cellWidth / 2f) * session.cellSize;
			float halfHeight = (cellHeight / 2f) * session.cellSize;

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
}
