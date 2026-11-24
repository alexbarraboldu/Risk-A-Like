using System;
using System.Collections.Generic;

using ABB.Grid.Editor.Data;

using Unity.Collections.LowLevel.Unsafe;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class CanvasView : VisualElement
	{
		private static readonly GUID StyleSheetGuid = new("3992a330f4f35004eb95a1f7a35d619c");
		public StyleSheet uss;

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

		private RectInt renderedCells;
		private Rect LayerViewport => new Rect(contentRect.position - gridPosition, contentRect.size);

		private Vector2 gridPosition = Vector2.zero;
		private Vector2 localMousePosition = Vector2.zero;
		private Vector2 localMouseStartPosition = Vector2.zero;
		private Vector2 localMouseDeltaPosition = Vector2.zero;

		private const float xOffset = 0.75f;
		private const float yOffset = 0.4330127f;
		private const float cellHeight = yOffset * 2f;

		///	ZOOM
		private float zoomBaseStep = 0.1f;
		private float zoomMaxStep = 20f;
		private float maxCellSize = 300f;

		private long renderMemorySizeBytes = 0;
		private long totalStaticCanvasMemorySizeBytes = 0;

		///	VISUAL ELEMENTS
		private Label memoryLabel = null;

		private int maxMemoryAllocationMB = 0;

		private readonly VisualElement gridLayer = new()
		{
			pickingMode = PickingMode.Ignore,
			name = "grid-layer"
		};


		public CanvasView(SessionData session, GridData currentGrid, TerrainColorData terrainColors, int maxMemoryAllocationMB)
		{
			if (uss == null) uss = AssetDatabase.LoadAssetByGUID<StyleSheet>(StyleSheetGuid);
			if (uss != null) styleSheets.Add(uss);

			name = "canvas-view";
			AddToClassList("abb-canvas-view");
			focusable = true;

			this.session = session;
			this.currentGrid = currentGrid;
			this.terrainColors = terrainColors;

			this.maxMemoryAllocationMB = maxMemoryAllocationMB;

			memoryLabel = new()
			{
				name = "memory-label"
			};
			memoryLabel.AddToClassList("abb-canvas-view__memory-label");

			gridLayer.AddToClassList("abb-canvas-view__grid-layer");

			AddLogic();

			Add(gridLayer);
			Add(memoryLabel);

			SetGridPosition(session.gridPosition);
		}


		private void AddLogic()
		{
			gridLayer.usageHints |= UsageHints.DynamicTransform;
			gridLayer.generateVisualContent += OnGenerateVisualContent;

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
				CanvasMarkDirtyRepaint("KeyDownEvent");

				string state = isRenderEnabled ? "enabled" : "disabled";
				Debug.Log($"Render state: {state}");
			});
		}

		private void OnGenerateVisualContent(MeshGenerationContext context)
		{
			if (session.cellSize <= 0f)
			{
				Debug.LogError("Cell size must be positive");
				return;
			}

			if (isRenderEnabled)
			{
				Rect view = LayerViewport;
				Vector2 margin = view.size * 0.1f;
				Rect drawView = new Rect(view.position - margin, view.size + 2f * margin);

				var (filled, outlined) = HexGridJobs.DrawGrid(context, currentGrid.grid, GridSize, session.cellSize, Vector2.zero, drawView, terrainColors.terrainColors);

				renderedCells = HexGridJobs.GetLayerCellRange(GridSize, session.cellSize, drawView);

				renderMemorySizeBytes = CalculateSizeOfFilledHexagons(filled) + CalculateSizeOfOutlinedHexagons(outlined);
				schedule.Execute(SetMemoryLabelText);
			}
		}

		private void EnsureGrid(ref GridData gridData, int columns, int rows)
		{
			int oldColumns = gridData.columns, oldRows = gridData.rows;
			int[] oldGrid = gridData.grid ?? Array.Empty<int>();
			int[] newGrid = oldGrid;

			if (columns <= 0 || rows <= 0)
			{
				newGrid = Array.Empty<int>();
			}
			else if (columns != oldColumns || rows != oldRows || oldGrid.Length != columns * rows)
			{
				newGrid = new int[columns * rows];
				Array.Fill(newGrid, -1);

				int columnsToCopy = Math.Min(oldColumns, columns);
				int rowsToCopy = Math.Min(oldRows, rows);

				for (int column = 0; column < columnsToCopy && rowsToCopy > 0; column++)
				{
					int oldStart = column * oldRows;
					if (oldStart >= oldGrid.Length) break;

					int count = Math.Min(rowsToCopy, oldGrid.Length - oldStart);
					Array.Copy(oldGrid, oldStart, newGrid, column * rows, count);
				}
			}

			gridData.columns = columns;
			gridData.rows = rows;
			gridData.grid = newGrid;

			schedule.Execute(SetMemoryLabelText);
		}

		public void CanvasMarkDirtyRepaint(string where = "")
		{
			gridLayer.MarkDirtyRepaint();
			//Debug.Log($"Mark Dirty Repaint: {where}");
		}

		public void SetCurrentGrid(GridData gridData, int columns, int rows)
		{
			EnsureGrid(ref gridData, columns, rows);
			currentGrid = gridData;
			CanvasMarkDirtyRepaint("SetCurrentGrid");
		}

		public void SetGridSize(Vector2Int value)
		{
			EnsureGrid(ref currentGrid, value.x, value.y);
			CanvasMarkDirtyRepaint("SetGridSize");
		}

		private void SetMemoryLabelText(TimerState timerState)
		{
			CalculateStaticCanvasMemory();

			long renderBytes = isRenderEnabled ? renderMemorySizeBytes : 0L;

			memoryLabel.text =
				$"Max memory: {maxMemoryAllocationMB} MB\n" +
				$"Estimated render memory:\n" +
				$"{(renderBytes + totalStaticCanvasMemorySizeBytes) / 1000L} KB\n" +
				$"{(renderBytes + totalStaticCanvasMemorySizeBytes) / 1000000L} MB";
		}

		private void SetGridPosition(Vector2 position)
		{
			gridPosition = position;
			gridLayer.style.translate = position;

			session.gridPosition = position;
			session.hasGridPosition = true;
		}

		#region ACTIONS 

		private bool EditGridCells(Vector2 pointerPosition)
		{
			if (session.cellSize <= 0f)
			{
				Debug.LogError("Cell size must be positive");
				return false;
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

			if (column < 0 || column >= currentGrid.columns || row < 0 || row >= currentGrid.rows)
				return false;

			bool hasEdited = false;
			if (isFilling)
			{
				hasEdited = FillGrid(column, row);
			}
			else
			{
				if (mouseButton == 0)
				{
					int cellValue = currentGrid.grid[column * currentGrid.rows + row];
					int paintValue = isErasing ? -1 : (int)session.leftColor;
					if (cellValue != paintValue)
					{
						currentGrid.grid[column * currentGrid.rows + row] = paintValue;
						hasEdited = true;
					}
				}
				else if (mouseButton == 1)
				{
					int cellValue = currentGrid.grid[column * currentGrid.rows + row];
					int paintValue = isErasing ? -1 : (int)session.rightColor;
					if (cellValue != paintValue)
					{
						currentGrid.grid[column * currentGrid.rows + row] = paintValue;
						hasEdited = true;
					}
				}
			}

			return hasEdited;
		}

		private void PaintStroke(Vector2 from, Vector2 to)
		{
			if (session.cellSize <= 0f) return;

			float spacing = session.cellSize * 0.25f;
			int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(from, to) / spacing));

			bool hasEdited = false;
			for (int i = 1; i <= steps; i++)
			{
				Vector2 position = Vector2.Lerp(from, to, i / (float)steps);
				hasEdited |= EditGridCells(position);
			}

			if (hasEdited) CanvasMarkDirtyRepaint("EditGridCells");
		}

		private void Zoom(float scrollWheelDelta)
		{
			if (session.cellSize <= 0f)
			{
				Debug.LogError("Cell size must be positive");
				return;
			}

			float previousCellSize = session.cellSize;

			float zoomFactor = 1f + (zoomMaxStep - zoomBaseStep) / maxCellSize;
			float nextCellSize = scrollWheelDelta > 0f
				? (previousCellSize - zoomBaseStep) / zoomFactor
				: previousCellSize * zoomFactor + zoomBaseStep;

			session.cellSize = Mathf.Clamp(nextCellSize, 1f, 300f);

			Vector2 mouse = localMousePosition - Vector2.one * 2f;
			Vector2 gridPoint = (mouse - gridPosition) / previousCellSize;

			SetGridPosition(mouse - gridPoint * session.cellSize);

			if (previousCellSize != session.cellSize)
			{
				CanvasMarkDirtyRepaint("Zoom");
			}
		}

		private void Pan()
		{
			SetGridPosition(localMouseDeltaPosition);
		}

		public void EraseClicked() => isErasing = !isErasing;

		public void FillClicked() => isFilling = !isFilling;

		private bool FillGrid(int column, int row)
		{
			int cellValueToCheck = currentGrid.grid[column * currentGrid.rows + row];
			int newValue = mouseButton == 0 ? (int)session.leftColor : mouseButton == 1 ? (int)session.rightColor : -1;

			if (cellValueToCheck == newValue) return false;
			///				RIGHT		LEFT	BOTTOM		TOP		RIGHT-TOP	RIGHT-BOTTOM	LEFT-TOP	LEFT-BOTTOM
			int[,] dir = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 }, { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } };
			Queue<(int, int)> queue = new Queue<(int, int)>();
			queue.Enqueue((column, row));

			int oldColor = currentGrid.grid[column * currentGrid.rows + row];
			currentGrid.grid[column * currentGrid.rows + row] = newValue;

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
						currentGrid.grid[nC * currentGrid.rows + nR] == oldColor)
					{
						currentGrid.grid[nC * currentGrid.rows + nR] = newValue;
						queue.Enqueue((nC, nR));
					}
				}
			}

			return true;
		}

		public void ResetGrid()
		{
			EnsureGrid(ref currentGrid, currentGrid.columns, currentGrid.rows);

			for (int column = 0; column < currentGrid.columns; column++)
			{
				for (int row = 0; row < currentGrid.rows; row++)
				{
					currentGrid.grid[column * currentGrid.rows + row] = -1;
				}
			}

			CanvasMarkDirtyRepaint("ResetGrid");
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
			{
				if (EditGridCells(localMousePosition))
					CanvasMarkDirtyRepaint("OnPointerDown: EditGridCells");
			}
		}

		private void OnPointerMove(PointerMoveEvent evt)
		{
			if (!isPainting) return;

			Vector2 previousPosition = localMousePosition;

			localMousePosition = evt.localPosition;
			localMouseDeltaPosition = localMousePosition - localMouseStartPosition;

			if (mouseButton == 2)
			{
				Pan();
				RectInt needed = HexGridJobs.GetLayerCellRange(GridSize, session.cellSize, LayerViewport);

				if (needed.width > 0 && needed.height > 0 && (needed.xMin < renderedCells.xMin || needed.xMax > renderedCells.xMax || needed.yMin < renderedCells.yMin || needed.yMax > renderedCells.yMax))
				{
					CanvasMarkDirtyRepaint("OnPointerMove: Pan");
				}
			}
			else if (!isFilling)
			{
				PaintStroke(previousPosition, localMousePosition);
			}
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

			SetGridPosition(session.hasGridPosition ? session.gridPosition : (evt.newRect.size - gridExtent) / 2f - drawnOffset);

			CanvasMarkDirtyRepaint("OnResizeWindow");
		}

		private void OnPointerLeave(PointerLeaveEvent evt)
		{
			isPainting = false;
		}

		#endregion

		#region	MEMORY CALCULATIONS

		private long CalculateSizeOfOutlinedHexagons(int count)
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
				//outlinedHexVertices.Length * UnsafeUtility.SizeOf<Vertex>() +
				///	Outlined Tris
				//outlinedHexTris.Length * sizeof(ushort) +
				///	Filled Vertices
				//filledHexVertices.Length * UnsafeUtility.SizeOf<Vertex>() +
				///	Filled Tris
				//filledHexTris.Length * sizeof(ushort) +
				///	Current Grid
				currentGrid.columns * currentGrid.rows * sizeof(int) +
				2 * sizeof(int) +
				///	Grid & Mouse Positions
				4 * UnsafeUtility.SizeOf<Vector2>() +
				///	Cell Drawing
				4 * sizeof(float) + 2 * UnsafeUtility.SizeOf<Vector2>();
		}

		internal void RemoveCellValue(int value)
		{
			int length = currentGrid.columns * currentGrid.rows;
			for (int i = 0; i < length; i++)
			{
				if (currentGrid.grid[i] == value) currentGrid.grid[i] = -1;
			}

			CanvasMarkDirtyRepaint();
		}

		#endregion
	}
}
