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

		private event Action<string> NewGridData;


		public EditorTab(string label, SessionData session, TerrainColorData terrainColors, GridData currentGrid, GridEditorWindow owner, int maxVector2IntFieldSize)
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
			ValidateGridSize();

			///	CREATE UI
			_toolsView = new(session, this.currentGrid, terrainColors, owner, maxVector2IntFieldSize);
			_canvasView = new(session, this.currentGrid, terrainColors);

			///	ADD UI
			contentContainer.Add(_toolsView);
			contentContainer.Add(_canvasView);

			///	ADD LOGIC
			_toolsView.OnLeftTerrainChanged += (terrain) => session.leftColor = (uint)terrain;
			_toolsView.OnRightTerrainChanged += (terrain) => session.rightColor = (uint)terrain;
			_toolsView.OnLeftColorChanged += (value, color) => _canvasView.MarkDirtyRepaint();
			_toolsView.OnRightColorChanged += (value, color) => _canvasView.MarkDirtyRepaint();
			_toolsView.OnGridSizeChanged += _canvasView.SetGridSize;
			_toolsView.OnDeleteCell += _canvasView.DeleteCell;
			_toolsView.OnResetGrid += _canvasView.ResetGrid;
			_toolsView.OnFillGrid += _canvasView.FillGrid;
			_toolsView.OnExportGrid += OnExportGrid;
			_toolsView.OnNewGrid += (value) => NewGridData?.Invoke(value);
		}


		#region ACTIONS

		public void RegisterCallbackNewGridData(Action<string> action) => NewGridData += action;

		private void OnExportGrid()
		{
			owner.tabView.SetEnabled(false);

			ExportWindow.OpenWindow(currentGrid, owner);
		}

		#endregion

		public void LoadGrid(GridData newGrid, int index)
		{
			currentGrid = newGrid;
			session.saveFileIndex = index;

			ValidateGridSize();

			_canvasView.SetCurrentGrid(currentGrid);
			_toolsView.SetCurrentGrid(currentGrid);
		}

		private void ValidateGridSize()
		{
			if (currentGrid.columns > maxVector2IntFieldSize)
				currentGrid.columns = maxVector2IntFieldSize;

			if (currentGrid.rows > maxVector2IntFieldSize)
				currentGrid.rows = maxVector2IntFieldSize;
		}
	}
}
