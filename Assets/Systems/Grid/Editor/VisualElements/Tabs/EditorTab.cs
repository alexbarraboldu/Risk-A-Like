using System;

using ABB.Grid.Editor.Data;
using ABB.Grid.Editor.Windows;

using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class EditorTab : Tab
	{
		///	DATA
		private SessionData session = null;
		private GridData currentGrid = null;

		private GridEditorWindow owner = null;

		///	VISUAL ELEMENTS
		private ToolsView _toolsView = null;
		private CanvasView _canvasView = null;

		///	FIELDS
		private int maxVector2IntFieldSize = 0;

		private event Func<string, bool> NewGridData;


		public EditorTab(string label, SessionData session, TerrainColorData terrainColors, GridData currentGrid, GridEditorWindow owner, int maxVector2IntFieldSize, int maxMemoryAllocationMB)
		{
			base.label = label;
			closeable = false;

			///	STYLE
			style.flexGrow = 1;
			contentContainer.style.flexGrow = 1;

			this.session = session;
			this.currentGrid = currentGrid;

			this.owner = owner;

			this.maxVector2IntFieldSize = maxVector2IntFieldSize;

			///	CREATE UI
			_toolsView = new(session, this.currentGrid, terrainColors, owner, maxVector2IntFieldSize);
			_canvasView = new(session, this.currentGrid, terrainColors, maxMemoryAllocationMB);

			///	ADD UI
			contentContainer.Add(_toolsView);
			contentContainer.Add(_canvasView);

			///	ADD LOGIC
			_toolsView.OnLeftColorChanged += (value, color) => _canvasView.CanvasMarkDirtyRepaint();
			_toolsView.OnRightColorChanged += (value, color) => _canvasView.CanvasMarkDirtyRepaint();
			_toolsView.OnGridSizeChanged += _canvasView.SetGridSize;
			_toolsView.OnEraserClicked += _canvasView.EraseClicked;
			_toolsView.OnResetGrid += _canvasView.ResetGrid;
			_toolsView.OnFillGrid += _canvasView.FillClicked;
			_toolsView.OnExportGrid += OnExportGrid;
			_toolsView.OnNewGrid += OnNewGrid;
			_toolsView.OnRemoveTerrain += _canvasView.RemoveCellValue;
		}


		#region ACTIONS

		public void RegisterCallbackNewGridData(Func<string, bool> action) => NewGridData += action;

		private void OnExportGrid()
		{
			ExportWindow.OpenWindow(currentGrid, owner);
		}

		private bool OnNewGrid(string value) => NewGridData?.Invoke(value) ?? false;

		#endregion

		public bool LoadGrid(GridData newGrid, int index)
		{
			int columns = Math.Min(newGrid.columns, maxVector2IntFieldSize);
			int rows = Math.Min(newGrid.rows, maxVector2IntFieldSize);

			if (newGrid.columns > maxVector2IntFieldSize || newGrid.rows > maxVector2IntFieldSize)
			{
				LoadErrorWindow.OpenWindow(
					$"{newGrid.fileName} grid size is bigger than what the memory budget allows.",
					out bool closeParent,
					out bool regenerate,
					"Cancel opening",
					"Continue (resize grid to actual limit)",
					$"{newGrid.fileName}"
				);

				if (!regenerate && closeParent)
				{
					return false;
				}
			}

			_canvasView.SetCurrentGrid(newGrid, columns, rows);
			_toolsView.SetCurrentGrid(newGrid);

			currentGrid = newGrid;
			session.saveFileIndex = index;

			return true;
		}
	}
}
