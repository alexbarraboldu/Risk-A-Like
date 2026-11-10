using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

		private EditorTab _editorTab = null;
		private SaveFilesTab _saveFilesTab = null;

		public TabView tabView = null;

		///	DATA
		private SessionData session = null;
		private TerrainColorData terrainColors = null;
		private GridData currentGrid = new();
		private GridData[] grids = null;

		private bool closeEditor = true;


		[MenuItem("Window/Custom Tools/Grid Editor")]
		private static void OpenWindow()
		{
			if (HasOpenInstances<GridEditorWindow>())
				return;

			GridEditorWindow window = CreateInstance<GridEditorWindow>();

			window.titleContent = new GUIContent("Grid Editor");
			window.minSize = new Vector2(minWindowWidth, minWindowHeight);

			window.Show();
		}

		public void CreateGUI()
		{
			rootVisualElement.style.minWidth = minWindowWidth;
			rootVisualElement.style.minHeight = minWindowHeight;

			closeEditor = true;
			if (!LoadAllData())
			{
				EditorApplication.delayCall += Close;
				return;
			}
			CheckData();

			///	BUILD WINDOW UI
			tabView = new() { name = "grid-editor-window__tab-view" };
			tabView.style.flexGrow = 1;
			tabView.contentContainer.style.flexGrow = 1;

			int maxVector2IntFieldSize = Math.Min(CalculateVector2IntFieldMaxIntegerSize(maxMemoryAllocationMB), Constants.MAX_GRID_DIMENSION);

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

			closeEditor = false;
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

		private bool CheckData()
		{
			bool invalid = false;
			int paletteLength = terrainColors.terrainColors.Length;
			for (int i = 0; i < grids.Length; i++)
			{
				GridData grid = grids[i];
				List<int> values = new();
				for (int column = 0; column < grid.columns; column++)
				{
					values.AddRange(grid.grid[column].Distinct());
				}
				IEnumerable<int> uniqueValues = values.Distinct();


				foreach (var item in uniqueValues)
				{
					invalid = item < -1 || item >= paletteLength;
					if (invalid) break;
				}

				if (invalid)
				{
					Debug.LogWarning($"Grid: '{grid.fileName}' has as invalid palette value");
				}
			}

			if (session.leftColor >= paletteLength)
				session.leftColor = paletteLength > 0 ? (uint)paletteLength - 1 : 0;
			if (session.rightColor >= paletteLength)
				session.rightColor = paletteLength > 0 ? (uint)paletteLength - 1 : 0;

			return !invalid;
		}

		private void ActivateGrid(GridData grid, int index)
		{
			_editorTab.LoadGrid(grid, index);

			currentGrid = grid;
			session.saveFileIndex = index;

			tabView.activeTab = _editorTab;
		}

		private void NewEntry(string newFileName)
		{
			if (grids.Length >= Constants.MAX_SAVED_GRIDS)
			{
				Debug.LogError($"Maximum saved grids reached. Grid {newFileName} won't be created.");
				return;
			}

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
				GridData newGrid = new() { fileName = newFileName };
				index = grids.Length;
				Array.Resize(ref grids, index + 1);
				grids[index] = currentGrid = newGrid;
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
