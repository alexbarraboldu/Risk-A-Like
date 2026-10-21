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
		private SessionData sessionData;
		private GridData gridSaveData;

		private ToolsView _toolsView = null;
		private CanvasView _cellGridCanvas = null;

		private event Action<GridData> SaveGridData;


		public EditorTab(string label)
		{
			try
			{
				LoadSessionFile();
				gridSaveData = new();
			}
			catch (Exception exception)
			{
				SaveSessionFile();

				Debug.LogWarning("Could not deserialize session file data");
				Debug.LogException(exception);
			}


			base.label = label;
			closeable = false;

			///	STYLE
			style.flexGrow = 1;
			contentContainer.style.flexGrow = 1;

			///	CREATE UI
			_toolsView = new();
			_cellGridCanvas = new();

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
			Vector2Int gridSize = new Vector2Int(gridSaveData?.columns ?? 0, gridSaveData?.rows ?? 0);
			_toolsView.GridSize = gridSize;

			_cellGridCanvas.Grid = gridSaveData.grid;
			_cellGridCanvas.CellSize = sessionData.cellSize;
			_toolsView.LeftColor = sessionData.leftColor;
			_toolsView.RightColor = sessionData.rightColor;
		}

		public void RegisterCallbackSaveGridData(Action<GridData> saveGridDataEvent)
		{
			if (saveGridDataEvent == null) return;

			SaveGridData += saveGridDataEvent;
		}

		#region ACTIONS

		private void OnExportGrid()
		{
			GridEditorWindow gridEditorWindow = EditorWindow.GetWindow<GridEditorWindow>();
			gridEditorWindow.tabView.SetEnabled(false);

			ExportWindow.OpenWindow(gridSaveData.grid, OnDestroyExportWindow);

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

		public void LoadGridClicked(GridData gridData)
		{
			gridSaveData = gridData;
			_cellGridCanvas.Grid = gridData.grid;

			Vector2Int gridSize = new Vector2Int(gridData.columns, gridData.rows);
			_cellGridCanvas.GridSize = gridSize;
			_toolsView.GridSize = gridSize;
		}

		public void OnSaveGridClicked()
		{
			gridSaveData.columns = _cellGridCanvas.GridSize.x;
			gridSaveData.rows = _cellGridCanvas.GridSize.y;
			gridSaveData.grid = _cellGridCanvas.Grid;
			gridSaveData.fileName = "grid";

			SaveGridData?.Invoke(gridSaveData);
		}

		#endregion

		public void Dispose()
		{
			SaveSessionFile();
			_toolsView?.Dispose();
		}
	}
}
