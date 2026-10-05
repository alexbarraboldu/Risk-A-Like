using System;
using System.IO;

using ABB.Gameplay.Data.Types;
using ABB.Grid.Editor.Data;
using ABB.Grid.Editor.Utils;
using ABB.Grid.Editor.VisualElements;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.Windows
{
	public class GridEditorWindow : EditorWindow
	{
		private bool guiBuilt = false;

		private const string StylePath = "Assets/";
		public StyleSheet uss;

		private readonly string PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ABB", "GridEditor");
		private readonly string FILE_NAME = "gridEditorSessionSaveFile.bin";

		private SessionFileData sessionFile = null;

		private ToolsView _toolsView = null;
		private CellGridCanvas _cellGridCanvas = null;


		[MenuItem("Window/Custom Tools/Grid Editor")]
		private static void OpenWindow()
		{
			GridEditorWindow window = GetWindow<GridEditorWindow>();
			window.titleContent = new GUIContent("Grid Editor");
			window.minSize = new Vector2(500, 500);
		}


		private void OnDisable()
		{
			if (!guiBuilt) return;

			SaveSessionFile();
		}

		private void OnDestroy()
		{
			if (!guiBuilt) return;

			SaveSessionFile();
		}

		public void CreateGUI()
		{
			if (uss == null) uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
			if (uss != null) rootVisualElement.styleSheets.Add(uss);

			try
			{
				sessionFile = BinarySerialization.ReadFromBinaryFile<SessionFileData>($"{PATH}\\{FILE_NAME}");
			}
			catch (Exception exception)
			{
				SaveSessionFile();

				Debug.Log("Could not deserialize session file data");
				Debug.LogException(exception, this);
			}

			///	BUILD WINDOW UI
			TabView tabView = new();
			tabView.style.flexGrow = 1;
			tabView.contentContainer.style.flexGrow = 1;

			EditorTab editorTab = new("Editor");
			editorTab.style.flexGrow = 1;
			editorTab.contentContainer.style.flexGrow = 1;
			ExportTab exportTab = new("Export");

			_toolsView = new();
			VisualElement rootCellGridCanvas = BuildCellGridCanvasUI();

			///	ADD UI
			rootVisualElement.Add(tabView);

			tabView.contentContainer.Add(editorTab);
			tabView.contentContainer.Add(exportTab);

			editorTab.contentContainer.Add(_toolsView);
			editorTab.contentContainer.Add(rootCellGridCanvas);

			///	ADD LOGIC
			_toolsView.OnCellGameObjectIdChanged += OnCellGameObjectIdChanged;
			_toolsView.OnTerrainChanged += OnTerrainChanged;
			_toolsView.OnGridSizeChanged += OnGridSizeChanged;
			_toolsView.OnResetGrid += _cellGridCanvas.ResetGrid;
			_toolsView.OnFillGrid += _cellGridCanvas.FillGrid;

			///	UPDATE VALUES
			_toolsView.GridSize = new Vector2Int((int)sessionFile.columns, (int)sessionFile.rows);
			///	Get gameObject by ID
			//cellGameObjectField.value = sessionFile.cellGameObjectId;

			_cellGridCanvas.Grid = sessionFile.grid;
			_cellGridCanvas.CellSize = sessionFile.cellSize;

			guiBuilt = true;
		}


		private void SaveSessionFile()
		{
			if (!Directory.Exists(PATH))
			{
				Directory.CreateDirectory(PATH);
			}

			try
			{
				if (sessionFile == null) sessionFile = new SessionFileData();

				sessionFile.cellSize = _cellGridCanvas.CellSize;
				sessionFile.grid = _cellGridCanvas.Grid;

				BinarySerialization.WriteToBinary<SessionFileData>($"{PATH}\\{FILE_NAME}", sessionFile);
			}
			catch (Exception)
			{
				//Debug.Log("Could not serialize session file data");
				//Debug.LogException(exception);
			}
		}

		private void OnCellGameObjectIdChanged(string cellGameObjectId)
		{
			sessionFile.cellGameObjectId = cellGameObjectId;
		}

		private void OnTerrainChanged(int terrain)
		{
			_cellGridCanvas.SelectedTerrain = (TerrainType)terrain;
		}

		private void OnGridSizeChanged(Vector2Int newValue)
		{
			sessionFile.columns = (uint)newValue.x;
			sessionFile.rows = (uint)newValue.y;
			_cellGridCanvas.GridSize = newValue;
		}

		#region UI

		private VisualElement BuildCellGridCanvasUI()
		{
			Vector2Int gridSize = new Vector2Int((int)sessionFile.columns, (int)sessionFile.rows);
			_cellGridCanvas = new(gridSize);

			return _cellGridCanvas;
		}

		#endregion
	}
}
