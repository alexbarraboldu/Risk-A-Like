using System;
using System.IO;

using ABB.Gameplay.Data.Types;
using ABB.Grid.Editor.Data;
using ABB.Grid.Editor.Utils;
using ABB.Grid.Editor.Windows;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class EditorTab : Tab
	{
		private readonly string FILE_NAME = "gridEditorSession.bin";

		private SessionData sessionFile;

		private ToolsView _toolsView = null;
		private CellGridCanvas _cellGridCanvas = null;


		public EditorTab(string label)
		{
			try
			{
				LoadSessionFile();
			}
			catch (Exception exception)
			{
				SaveSessionFile();

				Debug.Log("Could not deserialize session file data");
				Debug.LogException(exception);
			}

			base.label = label;
			closeable = false;

			///	STYLE
			style.flexGrow = 1;
			contentContainer.style.flexGrow = 1;

			///	CREATE UI
			_toolsView = new();
			Vector2Int gridSize = new Vector2Int((int)sessionFile.columns, (int)sessionFile.rows);
			_cellGridCanvas = new(gridSize);

			///	ADD UI
			contentContainer.Add(_toolsView);
			contentContainer.Add(_cellGridCanvas);

			///	ADD LOGIC
			_toolsView.OnLeftTerrainChanged += OnLeftTerrainChanged;
			_toolsView.OnRightTerrainChanged += OnRightTerrainChanged;
			_toolsView.OnGridSizeChanged += OnGridSizeChanged;
			_toolsView.OnResetGrid += _cellGridCanvas.ResetGrid;
			_toolsView.OnFillGrid += _cellGridCanvas.FillGrid;
			_toolsView.OnExportGrid += OnExportGrid;

			///	SET DATA
			_toolsView.GridSize = gridSize;

			_cellGridCanvas.Grid = sessionFile.grid;
			_cellGridCanvas.CellSize = sessionFile.cellSize;
		}


		private void OnExportGrid()
		{
			ExportWindow.OpenWindow(sessionFile);
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
			sessionFile.columns = (uint)newValue.x;
			sessionFile.rows = (uint)newValue.y;
			_cellGridCanvas.GridSize = newValue;
		}

		#region LOAD & SAVE DATA

		public void LoadSessionFile()
		{
			sessionFile = BinarySerialization.ReadFromBinaryFile<SessionData>($"{GridEditorWindow.PATH}\\{FILE_NAME}");
		}

		public void SaveSessionFile()
		{
			if (!Directory.Exists(GridEditorWindow.PATH))
			{
				Directory.CreateDirectory(GridEditorWindow.PATH);
			}

			try
			{
				if (sessionFile == null) sessionFile = new SessionData();

				sessionFile.cellSize = _cellGridCanvas.CellSize;
				sessionFile.grid = _cellGridCanvas.Grid;

				BinarySerialization.WriteToBinary<SessionData>($"{GridEditorWindow.PATH}\\{FILE_NAME}", sessionFile);
			}
			catch (Exception)
			{
				//Debug.Log("Could not serialize session file data");
				//Debug.LogException(exception);
			}
		}

		#endregion
	}
}
