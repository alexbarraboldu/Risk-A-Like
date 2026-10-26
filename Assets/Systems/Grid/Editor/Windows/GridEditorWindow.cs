using System;
using System.IO;

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
		private const string StylePath = "Assets/";
		public StyleSheet uss;

		private EditorTab _editorTab = null;
		private SaveFilesTab _saveFilesTab = null;

		public TabView tabView = null;

		///	DATA
		private SessionData session;
		private TerrainColorData terrainColors;
		private GridData currentGrid;
		private GridData[] grids;


		[MenuItem("Window/Custom Tools/Grid Editor")]
		private static void OpenWindow()
		{
			GridEditorWindow window = GetWindow<GridEditorWindow>();
			window.titleContent = new GUIContent("Grid Editor");
			window.minSize = new Vector2(500, 500);
		}

		public void CreateGUI()
		{
			if (uss == null) uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
			if (uss != null) rootVisualElement.styleSheets.Add(uss);

			LoadAllData();

			///	BUILD WINDOW UI
			tabView = new() { name = "grid-editor-window__tab-view" };
			tabView.style.flexGrow = 1;
			tabView.contentContainer.style.flexGrow = 1;

			currentGrid = grids.Length != 0 && session.saveFileIndex < grids.Length ? grids[session.saveFileIndex] : new GridData();
			_editorTab = new("Editor", ref session, ref terrainColors, currentGrid);
			_saveFilesTab = new("Save Files", ref grids);

			///	ADD LOGIC
			_editorTab.RegisterCallbackSaveGridData(AddEntry);
			_saveFilesTab.OnGridDataSelected += _editorTab.LoadGridClicked;
			_saveFilesTab.OnGridDataSelected += (gridData, index) =>
			{
				tabView.activeTab = _editorTab;
			};

			///	SET DATA


			///	ADD UI
			rootVisualElement.Add(tabView);

			tabView.contentContainer.Add(_editorTab);
			tabView.contentContainer.Add(_saveFilesTab);

			tabView.activeTab = _editorTab;
		}

		private void OnDestroy()
		{
			Save.SaveTerrainColorsFile(terrainColors);
			Save.SaveSessionFile(session);
			Save.SaveGridDataFile(grids);
		}


		private void LoadAllData()
		{
			bool pathExists = Directory.Exists(Constants.PATH);
			if (!pathExists)
			{
				Directory.CreateDirectory(Constants.PATH);
				Debug.Log($"Successfully created parent directory at {Constants.PATH}");
			}


			if (pathExists)
			{
				terrainColors = Load.LoadTerrainColorsFile();
			}
			else
			{
				//Debug.LogWarning("Could not deserialize terrain colors file data");

				if (terrainColors == null) terrainColors = new();
				Save.SaveTerrainColorsFile(terrainColors);
			}

			if (pathExists)
			{
				session = Load.LoadSessionFile();
			}
			else
			{
				//Debug.LogWarning("Could not deserialize session file data");

				if (session == null) session = new();
				Save.SaveSessionFile(session);
			}

			if (pathExists)
			{
				grids = Load.LoadGridsDataFile();
			}
			else
			{
				//Debug.LogWarning("Could not deserialize grid saves file data");

				if (grids == null) grids = Array.Empty<GridData>();
				Save.SaveGridDataFile(grids);
			}
		}

		private void AddEntry(GridData gridSave, int index)
		{
			if (index < 0)
			{
				index = grids.Length + 1;

				Array.Resize(ref grids, index);
			}

			grids[index] = gridSave;
			session.saveFileIndex = index;

			_saveFilesTab.SetEntries(grids);
		}
	}
}
