using System;

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

			_editorTab = new("Editor", ref session, ref terrainColors);
			_saveFilesTab = new("Save Files", ref grids);

			///	ADD LOGIC
			_editorTab.LoadCurrentGrid(Array.AsReadOnly(grids));
			_editorTab.RegisterCallbackSaveGridData(AddEntry);
			_saveFilesTab.OnGridDataSelected += _editorTab.LoadGridClicked;
			_saveFilesTab.OnGridDataSelected += (gridData, index) =>
			{
				tabView.activeTab = _editorTab;
			};

			///	ADD UI
			rootVisualElement.Add(tabView);

			tabView.contentContainer.Add(_editorTab);
			tabView.contentContainer.Add(_saveFilesTab);

			tabView.activeTab = _editorTab;
		}

		private void OnDestroy()
		{
			SaveTerrainColorsFile();
			SaveSessionFile();
			SaveGridSavesFiles();
		}


		private void LoadAllData()
		{
			try
			{
				LoadTerrainColorsFile();
			}
			catch (Exception exception)
			{
				SaveTerrainColorsFile();

				Debug.LogWarning("Could not deserialize terrain colors file data");
				Debug.LogException(exception);
			}

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

			try
			{
				LoadGridsSaveFile();
			}
			catch (Exception exception)
			{
				SaveGridSavesFiles();

				Debug.LogWarning("Could not deserialize grid saves file data");
				Debug.LogException(exception);
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


		#region LOAD & SAVE DATA

		private void LoadTerrainColorsFile()
		{
			terrainColors = Utils.Load.LoadTerrainColorsFile();
		}

		private void SaveTerrainColorsFile()
		{
			Utils.Save.SaveTerrainColorsFile(terrainColors);
		}

		private void LoadSessionFile()
		{
			session = Load.LoadSessionFile();
		}

		private void SaveSessionFile()
		{
			Save.SaveSessionFile(session);
		}

		private void LoadGridsSaveFile()
		{
			grids = Load.LoadGridsDataFile();
		}

		private void SaveGridSavesFiles()
		{
			Save.SaveGridDataFile(grids);
		}


		#endregion
	}
}
