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
		/// <summary>
		///	Maximum grid memory allocation in MB
		/// </summary>
		private const int maxMemoryAllocationMB = 1000;
		private const int minWindowWidth = 400;
		private const int minWindowHeight = 400;
		private const string StylePath = "Assets/";
		public StyleSheet uss;

		private EditorTab _editorTab = null;
		private SaveFilesTab _saveFilesTab = null;

		public TabView tabView = null;

		///	DATA
		private SessionData session = null;
		private TerrainColorData terrainColors = null;
		private GridData currentGrid = new();
		private GridData[] grids = null;

		private bool closeEditor = false;


		[MenuItem("Window/Custom Tools/Grid Editor")]
		private static void OpenWindow()
		{
			GridEditorWindow window = CreateInstance<GridEditorWindow>();

			window.titleContent = new GUIContent("Grid Editor");
			window.minSize = new Vector2(minWindowWidth, minWindowHeight);

			window.Show();

			if (window.closeEditor) window.Close();
		}

		public void CreateGUI()
		{
			if (uss == null) uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
			if (uss != null) rootVisualElement.styleSheets.Add(uss);

			rootVisualElement.style.minWidth = minWindowWidth;
			rootVisualElement.style.minHeight = minWindowHeight;

			if (!LoadAllData())
			{
				closeEditor = true;
				return;
			}

			///	BUILD WINDOW UI
			tabView = new() { name = "grid-editor-window__tab-view" };
			tabView.style.flexGrow = 1;
			tabView.contentContainer.style.flexGrow = 1;

			int maxVector2IntFieldSize = CalculateVector2IntFieldMaxIntegerSize(maxMemoryAllocationMB);

			_editorTab = new("Editor", session, terrainColors, currentGrid, this, maxVector2IntFieldSize);
			_saveFilesTab = new("Save Files", grids);

			///	ADD LOGIC
			_editorTab.RegisterCallbackNewGridData(NewEntry);
			_saveFilesTab.OnGridDataSelected += ActivateGrid;

			///	ADD UI
			rootVisualElement.Add(tabView);

			tabView.contentContainer.Add(_editorTab);
			tabView.contentContainer.Add(_saveFilesTab);

			tabView.activeTab = _editorTab;

			///	RELOAD GRID
			int index = session.saveFileIndex;
			if (index >= 0 && index < grids.Length) ActivateGrid(grids[index], index);
			else ActivateGrid(new GridData(), -1);
		}

		private void OnDestroy()
		{
			if (closeEditor) return;

			if (!Directory.Exists(Constants.PATH))
			{
				Directory.CreateDirectory(Constants.PATH);
				Debug.Log($"Successfully created parent directory at {Constants.PATH}");
			}

			if (!Save.SaveGridDataFile(grids)) Debug.LogError("Could not save grids.bin, all information has been lost. Make sure you have space, another process has not locked the file or that storage device is working properlly.");
			EditorApplication.delayCall += ResetSessionIfClosed;
		}

		private void OnDisable()
		{
			if (closeEditor) return;

			if (!Directory.Exists(Constants.PATH))
			{
				Directory.CreateDirectory(Constants.PATH);
				Debug.Log($"Successfully created parent directory at {Constants.PATH}");
			}

			if (!Save.SaveGridDataFile(grids)) Debug.LogError("Could not save grids.bin, all information has been lost. Make sure you have space, another process has not locked the file or that storage device is working properlly.");
			Save.SaveTerrainColorsFile(terrainColors);
			Save.SaveSessionFile(session);
		}


		private static void ResetSessionIfClosed()
		{
			if (!EditorWindow.HasOpenInstances<GridEditorWindow>())
				Save.SaveSessionFile(new());
		}

		private bool LoadAllData()
		{
			bool pathExists = Directory.Exists(Constants.PATH);
			if (!pathExists)
			{
				Directory.CreateDirectory(Constants.PATH);
				Debug.Log($"Successfully created parent directory at {Constants.PATH}");
			}


			if (File.Exists(Path.Combine(Constants.PATH, Constants.TERRAIN_COLORS_FILE_NAME)))
				terrainColors = Load.LoadTerrainColorsFile();
			else
			{
				terrainColors = new();
				Save.SaveTerrainColorsFile(terrainColors);
			}

			if (File.Exists(Path.Combine(Constants.PATH, Constants.SESSION_FILE_NAME)))
				session = Load.LoadSessionFile();
			else
			{
				session = new();
				Save.SaveSessionFile(session);
			}

			if (File.Exists(Path.Combine(Constants.PATH, Constants.GRIDS_FILE_NAME)))
				grids = Load.LoadGridsDataFile();
			else
			{
				grids = Array.Empty<GridData>();
				Save.SaveGridDataFile(grids);
			}

			if (terrainColors == null || session == null || grids == null)
			{
				string affectedFiles = "Affected files: ";
				affectedFiles += terrainColors == null ? "TerrainColorData file, " : "";
				affectedFiles += session == null ? "SessionData file, " : "";
				affectedFiles += grids == null ? "GridData file," : "";
				affectedFiles = affectedFiles.TrimEnd(',');

				LoadErrorWindow.OpenWindow("Error on load data files", out bool closeParent, out bool regenerate, affectedFiles);

				if (regenerate)
				{
					if (terrainColors == null) terrainColors = new();
					if (session == null) session = new();
					if (grids == null) grids = Array.Empty<GridData>();
				}

				if (closeParent)
				{
					return false;
				}
			}

			return true;
		}

		private void ActivateGrid(GridData grid, int index)
		{
			currentGrid = grid;
			session.saveFileIndex = index;

			_editorTab.LoadGrid(grid, index);
			tabView.activeTab = _editorTab;
		}

		private void NewEntry(string newFileName)
		{
			///	SAVE CURRENT GRID INTO GRIDS
			int index = session.saveFileIndex;
			if (session.saveFileIndex < 0)
			{
				currentGrid.fileName = newFileName;
				index = grids.Length;
				Array.Resize(ref grids, index + 1);
			}
			grids[index] = currentGrid;

			///	GENERATE NEW GRID & SAVE NEW GRID INTO GRIDS
			if (session.saveFileIndex != -1)
			{
				currentGrid = new();
				currentGrid.fileName = newFileName;
				index = grids.Length;
				Array.Resize(ref grids, index + 1);
				grids[index] = currentGrid;
			}

			_editorTab.LoadGrid(currentGrid, index);

			///	REFRESH & SAVE
			_saveFilesTab.SetEntries(grids);

			if (!Save.SaveGridDataFile(grids))
				return;

			Save.SaveSessionFile(session);
		}

		private int CalculateVector2IntFieldMaxIntegerSize(int maxMemoryAllocationMB)
		{
			long maxVectorCount = (long)maxMemoryAllocationMB * 1_000_000 / sizeof(int);
			return (int)System.Math.Sqrt(maxVectorCount);
		}

		private int CalculateCanvasMaxMemmoryAllocation(int maxMemoryAllocationMB)
		{
			return (int)System.Math.Sqrt(maxMemoryAllocationMB);
		}

		///	This is for the future
		///	i.e.: GridEditor will have a maximum memory size alloaction and from that it will calculate
		///	the Vector2IntField.maxIntegerSize & CanvasView.maxGridSize
		private void DistributeMemoryLimits(out int maxVector2IntFieldMemory, out int maxCanvasMemory)
		{
			maxVector2IntFieldMemory = 2;
			maxCanvasMemory = 2;
		}
	}
}

internal class LoadErrorWindow : EditorWindow
{
	private Label affectedFilesLabel = null;
	private Button continueButton = null;
	private Button closeEditorButton = null;

	private string affectedFiles = string.Empty;

	private bool closeParent = true;
	private bool regenerate = false;

	public static LoadErrorWindow OpenWindow(string titleContent, out bool closeParent, out bool regenerate, string affectedFiles)
	{
		LoadErrorWindow window = CreateInstance<LoadErrorWindow>();
		window.titleContent = new GUIContent(titleContent);
		window.minSize = new Vector2(400, 100);
		window.maxSize = new Vector2(400, 100);

		Rect unityWindow = EditorGUIUtility.GetMainWindowPosition();
		window.position = new Rect(unityWindow.center - window.minSize * 0.5f, window.minSize);

		window.affectedFiles = affectedFiles;

		window.ShowModalUtility();

		closeParent = window.closeParent;
		regenerate = window.regenerate;

		return window;
	}

	private void CreateGUI()
	{
		rootVisualElement.style.paddingLeft = 4;
		rootVisualElement.style.paddingRight = 4;
		rootVisualElement.style.paddingBottom = 4;
		rootVisualElement.style.paddingTop = 4;

		affectedFilesLabel = new(affectedFiles);
		affectedFilesLabel.style.marginTop = 4;

		closeEditorButton = new()
		{
			text = "Close editor",
			tooltip = "Close editor"
		};
		closeEditorButton.style.marginTop = 4;
		closeEditorButton.style.minHeight = 30;

		continueButton = new()
		{
			text = "Continue (will regenerate files)",
			tooltip = "Continue (will regenerate files)"
		};
		continueButton.style.marginTop = 4;
		continueButton.style.minHeight = 30;

		rootVisualElement.Add(affectedFilesLabel);
		rootVisualElement.Add(closeEditorButton);
		rootVisualElement.Add(continueButton);

		closeEditorButton.clicked += () =>
		{
			closeParent = true;
			Close();
		};

		continueButton.clicked += () =>
		{
			closeParent = false;
			regenerate = true;
			Close();
		};


	}
}
