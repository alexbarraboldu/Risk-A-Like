using System;
using System.Collections.ObjectModel;

using ABB.Gameplay.Data.Types;
using ABB.Grid.Editor.Data;
using ABB.Grid.Editor.Windows;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class EditorTab : Tab
	{
		private SessionData session;
		public GridData gridSaveData { get; private set; }

		private ToolsView _toolsView = null;
		private CanvasView _cellGridCanvas = null;

		private event Action<GridData, int> SaveGridData;


		public EditorTab(string label, ref SessionData session, ref TerrainColorData terrainColors)
		{
			base.label = label;
			closeable = false;

			///	STYLE
			style.flexGrow = 1;
			contentContainer.style.flexGrow = 1;

			this.session = session;

			///	CREATE UI
			_toolsView = new(ref session, ref terrainColors);
			_cellGridCanvas = new(ref session/*terrainColors*/);
			_cellGridCanvas.toolsView = _toolsView;

			///	ADD UI
			contentContainer.Add(_toolsView);
			contentContainer.Add(_cellGridCanvas);

			///	ADD LOGIC
			_toolsView.OnLeftTerrainChanged += OnLeftTerrainChanged;
			_toolsView.OnRightTerrainChanged += OnRightTerrainChanged;
			_toolsView.OnGridSizeChanged += OnGridSizeChanged;
			_toolsView.OnDeleteCell += _cellGridCanvas.DeleteCell;
			_toolsView.OnResetGrid += _cellGridCanvas.ResetGrid;
			_toolsView.OnFillGrid += _cellGridCanvas.FillGrid;
			_toolsView.OnExportGrid += OnExportGrid;
			_toolsView.OnSaveGrid += OnSaveGridClicked;

			///	INJECT DATA
			gridSaveData = new();
		}


		public void RegisterCallbackSaveGridData(Action<GridData, int> saveGridDataEvent)
		{
			if (saveGridDataEvent == null) return;

			SaveGridData += (x, y) =>
			{
				saveGridDataEvent.Invoke(x, session.saveFileIndex);
			};
		}

		#region ACTIONS

		private void OnExportGrid()
		{
			GridEditorWindow gridEditorWindow = EditorWindow.GetWindow<GridEditorWindow>();
			gridEditorWindow.tabView.SetEnabled(false);

			ExportWindow.OpenWindow(_cellGridCanvas.Grid, OnDestroyExportWindow);

			void OnDestroyExportWindow()
			{
				GridEditorWindow gridEditorWindow = EditorWindow.GetWindow<GridEditorWindow>();
				gridEditorWindow.tabView.SetEnabled(true);
			}
		}

		private void OnLeftTerrainChanged(int terrain)
		{
			_cellGridCanvas.LeftSelectedTerrain = (TerrainType)terrain;
			session.leftColor = (uint)terrain;
		}

		private void OnRightTerrainChanged(int terrain)
		{
			_cellGridCanvas.RightSelectedTerrain = (TerrainType)terrain;
			session.rightColor = (uint)terrain;
		}

		private void OnGridSizeChanged(Vector2Int newValue)
		{
			gridSaveData.columns = newValue.x;
			gridSaveData.rows = newValue.y;
			_cellGridCanvas.GridSize = newValue;
		}

		#endregion

		public void LoadGridClicked(GridData gridData, int index)
		{
			gridSaveData = gridData;
			session.saveFileIndex = index;

			_cellGridCanvas.Grid = gridData.grid;
			_toolsView.GridSize = _cellGridCanvas.GridSize;
		}

		public void LoadCurrentGrid(ReadOnlyCollection<GridData> gridsData)
		{
			int index = session.saveFileIndex;
			if (index < 0 || index >= gridsData.Count) return;

			_cellGridCanvas.Grid = gridsData[index].grid;
			_toolsView.GridSize = _cellGridCanvas.GridSize;
		}

		public void OnSaveGridClicked()
		{
			gridSaveData.columns = _cellGridCanvas.GridSize.x;
			gridSaveData.rows = _cellGridCanvas.GridSize.y;
			gridSaveData.grid = _cellGridCanvas.Grid;
			gridSaveData.fileName = "grid";

			SaveGridData?.Invoke(gridSaveData, session.saveFileIndex);
		}
	}
}
