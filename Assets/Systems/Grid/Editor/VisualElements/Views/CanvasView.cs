using System;
using System.Collections.Generic;

using ABB.Grid.Editor.Data;

using Unity.Collections.LowLevel.Unsafe;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class CanvasView : VisualElement
	{
		///	DATA
		private SessionData session = null;
		private GridData currentGrid = new();
		private TerrainColorData terrainColors = null;

		public Vector2Int GridSize => new(currentGrid.columns, currentGrid.rows);

		private bool isRenderEnabled = true;
		private bool isErasing = false;
		private bool isFilling = false;
		private bool isPainting = false;
		private int mouseButton = -1;

		private Vector2 gridPosition = Vector2.zero;
		private Vector2 localMousePosition = Vector2.zero;
		private Vector2 localMouseStartPosition = Vector2.zero;
		private Vector2 localMouseDeltaPosition = Vector2.zero;

		private const float xOffset = 0.75f;
		private const float yOffset = 0.4330127f;
		private const float cellWidth = 1;
		private const float cellHeight = yOffset * 2f;

		private long renderMemorySizeBytes = 0;
		private long totalStaticCanvasMemorySizeBytes = 0;

		///	OUTLINED HEXAGON
		Vertex[] outlinedHexVertices = new Vertex[12]
		{
				///	0 outer bottom left
				new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
				///	1 outer mid left
				new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
				///	2 outer top left
				new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
				///	3 outer top right
				new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
				///	4 outer mid right
				new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
				///	5 outer bottom right
				new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },

				///	6 inner bottom left
				new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
				///	7 inner mid left
				new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
				///	8 inner top left
				new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
				///	9 inner top right
				new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
				///	10 inner mid right
				new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
				///	11 inner bottom right
				new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
		};
		private readonly ushort[] outlinedHexTris = new ushort[36]
			{
				///	outter -> inner
				/// bottom middle
				0, 5, 6,
				5, 11, 6,
				/// bottom right
				5, 4, 11,
				11, 4, 10,
				/// top right
				10, 4, 9,
				4, 3, 9,
				/// top middle
				8, 9, 3,
				8, 3, 2,
				/// top left
				1, 7, 2,
				7, 8, 2,
				/// bottom left
				0, 7, 1,
				0, 6, 7
			};

		///	FILLED HEXAGON
		Vertex[] filledHexVertices = new Vertex[7]
		{
			///	0 bottom left
			new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
			///	1 mid left
			new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
			///	2 top left
			new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
			///	3 top right
			new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
			///	4 mid right
			new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
			///	5 bottom right
			new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
			///	6 middle
			new Vertex { position = new Vector3(0, 0, Vertex.nearZ), tint = Color.white },
		};
		private readonly ushort[] filledHexTris = new ushort[18]
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

		///	DRAW HEXAGON
		private Vector2 cellPosition = Vector2.zero;
		private float radius = 0;
		private float inRadius = 0;
		private Vector2 center = Vector2.zero;
		private float innerRadius = 0;
		private float innerInRadius = 0;

		///	VISUAL ELEMENTS
		private Label memoryLabel = null;


		public CanvasView(SessionData session, GridData currentGrid, TerrainColorData terrainColors)
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
			RegisterCallback<KeyDownEvent>(evt =>
			{
				if (evt.keyCode != KeyCode.R) return;

				isRenderEnabled = !isRenderEnabled;

				schedule.Execute(SetMemoryLabelText);
				MarkDirtyRepaint();

				string state = isRenderEnabled ? "enabled" : "disabled";
				Debug.Log($"Render state: {state}");
			});

			this.session = session;
			this.currentGrid = currentGrid;
			this.terrainColors = terrainColors;

			memoryLabel = new();
			memoryLabel.style.alignSelf = Align.FlexEnd;
			memoryLabel.style.color = Color.white;
			Add(memoryLabel);
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
			if (session.cellSize <= 0f)
			{
				Debug.LogError("Cell size must be positive");
				return;
			}

			if (isRenderEnabled)
				DrawGrid(context);
		}

		private void EnsureGrid(ref GridData gridData, int columns, int rows)
		{
			int[][] oldGrid = gridData.grid;
			int[][] preparedGrid = oldGrid;

			if (columns <= 0 || rows <= 0)
			{
				preparedGrid = Array.Empty<int[]>();
			}
			else if (oldGrid == null || oldGrid.Length != columns || Array.Exists(oldGrid, column => column == null || column.Length != rows))
			{
				preparedGrid = new int[columns][];

				for (int c = 0; c < columns; c++)
				{
					int[] oldColumn = oldGrid != null && c < oldGrid.Length ? oldGrid[c] : null;

					int[] newColumn = new int[rows];
					int rowsToCopy = Math.Min(oldColumn?.Length ?? 0, rows);

					if (rowsToCopy > 0)
						Array.Copy(oldColumn, newColumn, rowsToCopy);

					for (int r = rowsToCopy; r < rows; r++)
						newColumn[r] = -1;

					preparedGrid[c] = newColumn;
				}
			}

			gridData.columns = columns;
			gridData.rows = rows;
			gridData.grid = preparedGrid;

			schedule.Execute(SetMemoryLabelText);
		}

		public void SetCurrentGrid(GridData gridData, int columns, int rows)
		{
			EnsureGrid(ref gridData, columns, rows);
			currentGrid = gridData;
			MarkDirtyRepaint();
		}

		public void SetGridSize(Vector2Int value)
		{
			EnsureGrid(ref currentGrid, value.x, value.y);
			MarkDirtyRepaint();
		}

		private void SetMemoryLabelText(TimerState timerState)
		{
			CalculateStaticCanvasMemory();

			long renderBytes = isRenderEnabled ? renderMemorySizeBytes : 0L;

			memoryLabel.text =
				$"Estimated render memory:\n" +
				$"{(renderBytes + totalStaticCanvasMemorySizeBytes) / 1000L} KB\n" +
				$"{(renderBytes + totalStaticCanvasMemorySizeBytes) / 1000000L} MB";
		}

		#region RENDERING

		private void DrawGrid(MeshGenerationContext context)
		{
			///	HEXAGON PRE-CALCULATIONS
			radius = session.cellSize / 2f;
			inRadius = Mathf.Sqrt((radius * radius) - ((radius / 2f) * (radius / 2f)));
			center.x = radius + 2f; center.y = inRadius + 2f;

			innerRadius = session.cellSize / 2.25f;
			innerInRadius = Mathf.Sqrt((innerRadius * innerRadius) - ((innerRadius / 2f) * (innerRadius / 2f)));

			int outlineHexagons = 0, filledHexagons = 0, cellValue = 0;
			float oddColumnOffset = 0, xPos = 0, yPos = 0;
			for (int c = 0; c < currentGrid.columns; c++)
			{
				for (int r = 0; r < currentGrid.rows; r++)
				{
					oddColumnOffset = (c & 1) != 0 ? yOffset * session.cellSize : 0f;

					xPos = c * xOffset * session.cellSize;
					yPos = r * cellHeight * session.cellSize + oddColumnOffset;

					cellPosition = gridPosition;
					cellPosition.x += xPos; cellPosition.y += yPos;

					cellValue = currentGrid.grid[c][r];
					if (cellValue != -1)
					{
						AllocateFilledHexagon(context, cellPosition, cellValue);
						filledHexagons++;
					}
					else
					{
						AllocateOutlinedHexagon(context, cellPosition);
						outlineHexagons++;
					}
				}
			}

			this.renderMemorySizeBytes = CalculateSizeOfFilledHexagons(filledHexagons) + CalculateSizeOfOutlineHexagons(outlineHexagons);

			schedule.Execute(SetMemoryLabelText);
		}

		private void AllocateOutlinedHexagon(MeshGenerationContext context, Vector2 position)
		{
			MeshWriteData mesh = context.Allocate(12, 36);

			///	Calculate vertices
			outlinedHexVertices[0].position.x = position.x + center.x - radius / 2;
			outlinedHexVertices[0].position.y = position.y + center.y - inRadius;

			outlinedHexVertices[1].position.x = position.x + center.x - radius;
			outlinedHexVertices[1].position.y = position.y + center.y;

			outlinedHexVertices[2].position.x = position.x + center.x - radius / 2;
			outlinedHexVertices[2].position.y = position.y + center.y + inRadius;

			outlinedHexVertices[3].position.x = position.x + center.x + radius / 2;
			outlinedHexVertices[3].position.y = position.y + center.y + inRadius;

			outlinedHexVertices[4].position.x = position.x + center.x + radius;
			outlinedHexVertices[4].position.y = position.y + center.y;

			outlinedHexVertices[5].position.x = position.x + center.x + radius / 2;
			outlinedHexVertices[5].position.y = position.y + center.y - inRadius;

			outlinedHexVertices[6].position.x = position.x + center.x - innerRadius / 2;
			outlinedHexVertices[6].position.y = position.y + center.y - innerInRadius;

			outlinedHexVertices[7].position.x = position.x + center.x - innerRadius;
			outlinedHexVertices[7].position.y = position.y + center.y;

			outlinedHexVertices[8].position.x = position.x + center.x - innerRadius / 2;
			outlinedHexVertices[8].position.y = position.y + center.y + innerInRadius;

			outlinedHexVertices[9].position.x = position.x + center.x + innerRadius / 2;
			outlinedHexVertices[9].position.y = position.y + center.y + innerInRadius;

			outlinedHexVertices[10].position.x = position.x + center.x + innerRadius;
			outlinedHexVertices[10].position.y = position.y + center.y;

			outlinedHexVertices[11].position.x = position.x + center.x + innerRadius / 2;
			outlinedHexVertices[11].position.y = position.y + center.y - innerInRadius;

			mesh.SetAllVertices(outlinedHexVertices);
			mesh.SetAllIndices(outlinedHexTris);
		}

		private void AllocateFilledHexagon(MeshGenerationContext context, Vector2 position, int cellValue)
		{
			MeshWriteData mesh = context.Allocate(7, 18);

			Color tint = SelectCellTint(cellValue);

			///	Calculate vertices
			filledHexVertices[0].position.x = position.x + center.x - radius / 2;
			filledHexVertices[0].position.y = position.y + center.y - inRadius;
			filledHexVertices[0].tint = tint;

			filledHexVertices[1].position.x = position.x + center.x - radius;
			filledHexVertices[1].position.y = position.y + center.y;
			filledHexVertices[1].tint = tint;

			filledHexVertices[2].position.x = position.x + center.x - radius / 2;
			filledHexVertices[2].position.y = position.y + center.y + inRadius;
			filledHexVertices[2].tint = tint;

			filledHexVertices[3].position.x = position.x + center.x + radius / 2;
			filledHexVertices[3].position.y = position.y + center.y + inRadius;
			filledHexVertices[3].tint = tint;

			filledHexVertices[4].position.x = position.x + center.x + radius;
			filledHexVertices[4].position.y = position.y + center.y;
			filledHexVertices[4].tint = tint;

			filledHexVertices[5].position.x = position.x + center.x + radius / 2;
			filledHexVertices[5].position.y = position.y + center.y - inRadius;
			filledHexVertices[5].tint = tint;

			filledHexVertices[6].position.x = position.x + center.x;
			filledHexVertices[6].position.y = position.y + center.y;
			filledHexVertices[6].tint = tint;

			mesh.SetAllVertices(filledHexVertices);
			mesh.SetAllIndices(filledHexTris);
		}

		private Color SelectCellTint(int cellValue)
		{
			Color color = terrainColors.terrainColors[cellValue].color;

			return color;
		}

		#endregion

		#region ACTIONS 

		private void EditGridCells(Vector2 pointerPosition)
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

			Vector2 mouse = pointerPosition - new Vector2(radius + 2f, inRadius + 2f) - gridPosition;

			int evenColumn = 2 * Mathf.RoundToInt(mouse.x / (2f * spacingX));
			int evenRow = Mathf.RoundToInt(mouse.y / spacingY);

			int oddColumn = 2 * Mathf.RoundToInt((mouse.x - spacingX) / (2f * spacingX)) + 1;
			int oddRow = Mathf.RoundToInt((mouse.y - spacingY * 0.5f) / spacingY);

			Vector2 evenCenter = new(evenColumn * spacingX, evenRow * spacingY);
			Vector2 oddCenter = new(oddColumn * spacingX, (oddRow + 0.5f) * spacingY);

			bool evenIsCloser = (mouse - evenCenter).sqrMagnitude <= (mouse - oddCenter).sqrMagnitude;

			int column = evenIsCloser ? evenColumn : oddColumn;
			int row = evenIsCloser ? evenRow : oddRow;

			if (column < 0 || column >= currentGrid.grid.Length || row < 0 || row >= currentGrid.grid[column].Length)
				return;

			if (isFilling)
			{
				FillGrid(column, row);
			}
			else
			{
				if (mouseButton == 0) currentGrid.grid[column][row] = isErasing ? -1 : (int)session.leftColor;
				else if (mouseButton == 1) currentGrid.grid[column][row] = isErasing ? -1 : (int)session.rightColor;
			}
		}

		private void PaintStroke(Vector2 from, Vector2 to)
		{
			if (session.cellSize <= 0f) return;

			float spacing = session.cellSize * 0.25f;
			int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(from, to) / spacing));

			for (int i = 1; i <= steps; i++)
			{
				Vector2 position = Vector2.Lerp(from, to, i / (float)steps);
				EditGridCells(position);
			}
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
			session.cellSize = Mathf.Clamp(session.cellSize, 5f, 300f);

			Vector2 mouse = localMousePosition - Vector2.one * 2f;
			Vector2 gridPoint = (mouse - gridPosition) / previousCellSize;

			gridPosition = mouse - gridPoint * session.cellSize;
		}

		private void Pan()
		{
			gridPosition = localMouseDeltaPosition;
		}

		public void EraseClicked() => isErasing = !isErasing;

		public void FillClicked() => isFilling = !isFilling;

		private void FillGrid(int column, int row)
		{
			int cellValueToCheck = currentGrid.grid[column][row];
			int newValue = mouseButton == 0 ? (int)session.leftColor : mouseButton == 1 ? (int)session.rightColor : -1;

			if (cellValueToCheck == newValue) return;
			///				RIGHT		LEFT	BOTTOM		TOP		RIGHT-TOP	RIGHT-BOTTOM	LEFT-TOP	LEFT-BOTTOM
			int[,] dir = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 }, { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } };
			Queue<(int, int)> queue = new Queue<(int, int)>();
			queue.Enqueue((column, row));

			int oldColor = currentGrid.grid[column][row];
			currentGrid.grid[column][row] = newValue;

			int columns = currentGrid.columns;
			int rows = currentGrid.rows;

			int dirLength = dir.Length / 2;
			while (queue.Count > 0)
			{
				var (c, r) = queue.Dequeue();
				for (int i = 0; i < dirLength; i++)
				{
					int nC = c + dir[i, 0];
					int nR = r + dir[i, 1];

					int diagonalRowOffset = (c & 1) == 0 ? -1 : 1;
					if (i >= 4 && dir[i, 1] != diagonalRowOffset) continue;

					if (nC >= 0 && nC < columns &&
						nR >= 0 && nR < rows &&
						currentGrid.grid[nC][nR] == oldColor)
					{
						currentGrid.grid[nC][nR] = newValue;
						queue.Enqueue((nC, nR));
					}
				}
			}
		}

		public void ResetGrid()
		{
			EnsureGrid(ref currentGrid, currentGrid.columns, currentGrid.rows);

			for (int c = 0; c < currentGrid.columns; c++)
			{
				for (int r = 0; r < currentGrid.rows; r++)
				{
					currentGrid.grid[c][r] = -1;
				}
			}

			MarkDirtyRepaint();
		}

		#endregion

		#region EVENTS

		private void OnPointerDown(PointerDownEvent evt)
		{
			if (session.cellSize <= 0f || currentGrid.columns <= 0f || currentGrid.rows <= 0f || evt.button > 2) return;

			localMousePosition = evt.localPosition;
			localMouseStartPosition = (Vector2)evt.localPosition - gridPosition;

			isPainting = true;
			mouseButton = evt.button;

			if (mouseButton != 2)
				EditGridCells(localMousePosition);

			MarkDirtyRepaint();
		}

		private void OnPointerMove(PointerMoveEvent evt)
		{
			if (!isPainting) return;

			Vector2 previousPosition = localMousePosition;

			localMousePosition = evt.localPosition;
			localMouseDeltaPosition = localMousePosition - localMouseStartPosition;

			if (mouseButton == 2)
				Pan();
			else if (!isFilling)
				PaintStroke(previousPosition, localMousePosition);

			MarkDirtyRepaint();
		}

		private void OnPointerUp(PointerUpEvent evt)
		{
			isPainting = false;
			mouseButton = -1;
		}

		private void OnWheelScroll(WheelEvent evt)
		{
			localMousePosition = evt.localMousePosition;

			Zoom(evt.delta.y);

			if (isPainting && mouseButton == 2)
				localMouseStartPosition = localMousePosition - gridPosition;

			MarkDirtyRepaint();
			evt.StopPropagation();
		}

		private void OnResizeWindow(GeometryChangedEvent evt)
		{
			if (currentGrid.columns <= 0 || currentGrid.rows <= 0) return;

			float size = session.cellSize;

			float gridWidth = (currentGrid.columns - 1) * xOffset * size + size;
			float gridHeight = currentGrid.rows * cellHeight * size;

			// Odd columns extend half a hex further down.
			if (currentGrid.columns > 1) gridHeight += yOffset * size;

			Vector2 drawnOffset = new(2f, 2f);
			Vector2 gridExtent = new(gridWidth, gridHeight);

			gridPosition = (evt.newRect.size - gridExtent) / 2f - drawnOffset;

			MarkDirtyRepaint();

			//float width = evt.newRect.width;
			//float height = evt.newRect.height;
			//gridPosition.x = (width / 2f) - ((currentGrid.grid.Length * session.cellSize) / 2f) * xOffset;
			//gridPosition.y = (height / 2f) - ((currentGrid.grid[0].Length * session.cellSize) / 2f) * cellHeight;
		}

		private void OnPointerLeave(PointerLeaveEvent evt)
		{
			isPainting = false;
		}

		#endregion

		#region VISUAL HELPERS

		[Obsolete("This method is not longer used", true)]
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

		[Obsolete("This method is not longer used", true)]
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

		#region	MEMOERY CALCULATIONS

		private long CalculateSizeOfOutlineHexagons(int count)
		{
			if (count == 0) return 0;

			long vertexSize = 12 * UnsafeUtility.SizeOf<Vertex>();
			long trianglesSize = 36 * sizeof(ushort);

			return (vertexSize + trianglesSize) * count;
		}

		private long CalculateSizeOfFilledHexagons(int count)
		{
			if (count == 0) return 0;

			long vertexSize = 7 * UnsafeUtility.SizeOf<Vertex>();
			long trianglesSize = 18 * sizeof(ushort);

			return (vertexSize + trianglesSize) * count;
		}

		private void CalculateStaticCanvasMemory()
		{
			totalStaticCanvasMemorySizeBytes =
				///	Outlined Vertices
				outlinedHexVertices.Length * UnsafeUtility.SizeOf<Vertex>() +
				///	Outlined Tris
				outlinedHexTris.Length * sizeof(ushort) +
				///	Filled Vertices
				filledHexVertices.Length * UnsafeUtility.SizeOf<Vertex>() +
				///	Filled Tris
				filledHexTris.Length * sizeof(ushort) +
				///	Current Grid
				currentGrid.columns * currentGrid.rows * sizeof(int) +
				2 * sizeof(int) +
				///	Grid & Mouse Positions
				4 * UnsafeUtility.SizeOf<Vector2>() +
				///	Cell Drawing
				4 * sizeof(float) + 2 * UnsafeUtility.SizeOf<Vector2>();
		}

		#endregion
	}
}
