using System;

using ABB.Gameplay.Data.Types;
using ABB.Grid.Editor.Data;
using ABB.Grid.Editor.Utils;
using ABB.Grid.Editor.Windows;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class EditorTab : Tab, IDisposable
	{
		public SessionData sessionData { get; private set; }
		public GridData gridSaveData { get; private set; }

		private ToolsView _toolsView = null;
		private CanvasView _cellGridCanvas = null;

		private event Action<GridData, int> SaveGridData;


		public EditorTab(string label)
		{
			base.label = label;
			closeable = false;

			///	STYLE
			style.flexGrow = 1;
			contentContainer.style.flexGrow = 1;

			///	CREATE UI
			_toolsView = new();
			_cellGridCanvas = new();
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
			try
			{
				LoadSessionFile();
			}
			catch (Exception exception)
			{
				SaveSessionFile();

				Debug.LogWarning("Could not deserialize session file data");
				Debug.LogException(exception);
			}
			gridSaveData = new();

			_cellGridCanvas.CellSize = sessionData.cellSize;
			_toolsView.LeftColor = sessionData.leftColor;
			_toolsView.RightColor = sessionData.rightColor;
		}

		public void RegisterCallbackSaveGridData(Action<GridData, int> saveGridDataEvent)
		{
			if (saveGridDataEvent == null) return;

			SaveGridData += (x, y) =>
			{
				saveGridDataEvent.Invoke(x, sessionData.saveFileIndex);
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
		}

		private void OnRightTerrainChanged(int terrain)
		{
			_cellGridCanvas.RightSelectedTerrain = (TerrainType)terrain;
		}

		private void OnGridSizeChanged(Vector2Int newValue)
		{
			gridSaveData.columns = newValue.x;
			gridSaveData.rows = newValue.y;
			_cellGridCanvas.GridSize = newValue;
		}

		#endregion

		#region LOAD & SAVE DATA

		private void LoadSessionFile()
		{
			sessionData = Load.LoadSessionFile();
		}

		private void SaveSessionFile()
		{
			sessionData.cellSize = _cellGridCanvas.CellSize;
			sessionData.leftColor = _toolsView.LeftColor;
			sessionData.rightColor = _toolsView.RightColor;

			Save.SaveSessionFile(sessionData);
		}

		public void LoadGridClicked(GridData gridData, int index)
		{
			gridSaveData = gridData;
			sessionData.saveFileIndex = index;

			_cellGridCanvas.Grid = gridData.grid;
			_toolsView.GridSize = _cellGridCanvas.GridSize;
		}

		public void LoadCurrentGrid(GridData[] gridsData)
		{
			int index = sessionData.saveFileIndex;
			if (index < 0 || index >= gridsData.Length) return;

			_cellGridCanvas.Grid = gridsData[index].grid;
			_toolsView.GridSize = _cellGridCanvas.GridSize;
		}

		public void OnSaveGridClicked()
		{
			gridSaveData.columns = _cellGridCanvas.GridSize.x;
			gridSaveData.rows = _cellGridCanvas.GridSize.y;
			gridSaveData.grid = _cellGridCanvas.Grid;
			gridSaveData.fileName = "grid";

			SaveGridData?.Invoke(gridSaveData, sessionData.saveFileIndex);
		}

		#endregion

		public void Dispose()
		{
			SaveSessionFile();
			_toolsView?.Dispose();
		}
	}
}
