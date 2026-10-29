using System;

using ABB.Grid.Editor.Data;
using ABB.Grid.Editor.Windows;

using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class EditorTab : Tab
	{
		private SessionData session = null;
		private GridData currentGrid = null;

		private ToolsView _toolsView = null;
		private CanvasView _canvasView = null;

		private event Action<string> NewGridData;

		private GridEditorWindow gridEditorWindow = null;


		public EditorTab(string label, SessionData session, TerrainColorData terrainColors, GridData currentGrid, int maxGridSizeAllocation, GridEditorWindow gridEditorWindow)
		{
			base.label = label;
			closeable = false;

			///	STYLE
			style.flexGrow = 1;
			contentContainer.style.flexGrow = 1;

			this.session = session;
			this.currentGrid = currentGrid;

			///	CREATE UI
			_toolsView = new(session, currentGrid, terrainColors, maxGridSizeAllocation);
			_canvasView = new(session, currentGrid, terrainColors);

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

			this.gridEditorWindow = gridEditorWindow;
		}


		#region ACTIONS

		public void RegisterCallbackNewGridData(Action<string> action) => NewGridData += action;

		private void OnExportGrid()
		{
			gridEditorWindow.tabView.SetEnabled(false);

			ExportWindow.OpenWindow(currentGrid, gridEditorWindow);
		}

		#endregion

		public void LoadGridClicked(GridData newGrid, int index)
		{
			this.currentGrid = newGrid;
			session.saveFileIndex = index;

			_canvasView.SetCurrentGrid(newGrid);
			_toolsView.SetCurrentGrid(newGrid);
		}
	}
}
