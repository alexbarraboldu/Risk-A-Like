using System;

using ABB.Grid.Editor.Data;
using ABB.Grid.Editor.Windows;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class EditorTab : Tab
	{
		private SessionData session = null;
		private GridData gridData = null;

		private ToolsView _toolsView = null;
		private CanvasView _canvasView = null;

		private event Action<GridData, int> SaveGridData;
		private event Action<GridData, int> NewGridData;


		public EditorTab(string label, ref SessionData session, ref TerrainColorData terrainColors, GridData gridData, int maxGridSizeAllocation)
		{
			base.label = label;
			closeable = false;

			///	STYLE
			style.flexGrow = 1;
			contentContainer.style.flexGrow = 1;

			this.session = session;
			this.gridData = gridData;

			///	CREATE UI
			_toolsView = new(ref session, ref terrainColors, maxGridSizeAllocation);
			_canvasView = new(ref session, ref terrainColors);

			///	ADD UI
			contentContainer.Add(_toolsView);
			contentContainer.Add(_canvasView);

			///	ADD LOGIC
			_toolsView.OnLeftTerrainChanged += OnLeftTerrainChanged;
			_toolsView.OnRightTerrainChanged += OnRightTerrainChanged;
			_toolsView.OnLeftColorChanged += (value, color) => _canvasView.MarkDirtyRepaint();
			_toolsView.OnRightColorChanged += (value, color) => _canvasView.MarkDirtyRepaint();
			_toolsView.OnGridSizeChanged += _canvasView.SetGridSize;
			_toolsView.OnDeleteCell += _canvasView.DeleteCell;
			_toolsView.OnResetGrid += _canvasView.ResetGrid;
			_toolsView.OnFillGrid += _canvasView.FillGrid;
			_toolsView.OnExportGrid += OnExportGrid;
			_toolsView.OnSaveGrid += OnSaveGrid;
			_toolsView.OnNewGrid += OnNewGrid;

			///	ADD DATA
			_canvasView.SetGridData(gridData);
			_toolsView.SetGridSize(_canvasView.GridSize);
		}


		public void RegisterCallbackSaveGridData(Action<GridData, int> action) => SaveGridData += action;
		public void RegisterCallbackNewGridData(Action<GridData, int> action) => NewGridData += action;

		#region ACTIONS

		private void OnExportGrid()
		{
			GridEditorWindow gridEditorWindow = EditorWindow.GetWindow<GridEditorWindow>();
			gridEditorWindow.tabView.SetEnabled(false);

			//parent.Q<GridEditorWindow>();
			Debug.Log(panel.visualTree.name);

			ExportWindow.OpenWindow(gridData, gridEditorWindow);
		}

		private void OnSaveGrid()
		{
			///	FIRST OPTION
			/// Swap to SaveFiles tab and add an entrance with the name change enabled.
			///	SECOND OPTION
			///	Open SaveWindow and write there the name then save the file.

			SaveGridData?.Invoke(gridData, session.saveFileIndex);
		}

		private void OnNewGrid()
		{
			NewGridData?.Invoke(gridData, -1);
		}

		private void OnLeftTerrainChanged(int terrain) => session.leftColor = (uint)terrain;

		private void OnRightTerrainChanged(int terrain) => session.rightColor = (uint)terrain;

		#endregion

		public void LoadGridClicked(GridData gridData, int index)
		{
			this.gridData = gridData;
			session.saveFileIndex = index;

			_canvasView.SetGridData(gridData);
			_toolsView.SetGridSize(_canvasView.GridSize);
		}
	}
}
