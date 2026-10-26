using System;
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
	public class ExportWindow : EditorWindow
	{
		private bool guiBuilt = false;

		private ExportSettingsData exportSettings;
		private GridData grid;

		private IExportGrid _exporter;


		private Button exportButton = null;

		private string destinationPath = string.Empty;
		private string fileName = string.Empty;

		private Action onDestroy;


		public static void OpenWindow(GridData grid, Action onDestroy)
		{
			ExportWindow window = GetWindow<ExportWindow>();
			window.titleContent = new GUIContent("Export Grid");
			window.minSize = new Vector2(500, 200);

			window.grid = grid;
			window.fileName = grid.fileName;

			window.onDestroy = onDestroy;
		}

		public void CreateGUI()
		{
			LoadExportSettings();

			TypeField<IExportGrid> typeField = CreateTypeField();

			RestoreExporterInstance(typeField);

			DirectoryPathBrowser directoryPathBrowser = new("Export folder", "Choose export folder")
			{
				style = {
					paddingLeft = 4,
					paddingRight = 4,
					paddingTop = 4
				}
			};
			directoryPathBrowser.FolderPath = exportSettings?.folderPath ?? string.Empty;
			directoryPathBrowser.OnFolderFieldValueChanged += OnFolderFieldValueChanged;

			TextField fileNameField = new("File name")
			{
				style =
				{
					paddingLeft = 4,
					paddingRight = 4,
					paddingTop = 4
				}
			};
			fileNameField.Children().ToArray()[0].style.minWidth = 100;
			fileNameField.value = exportSettings?.fileName ?? string.Empty;
			fileNameField.RegisterValueChangedCallback(OnFileNameFieldValueChanged);

			VisualElement spacer = new();
			spacer.style.flexGrow = 0.5f;

			///	Export Grid Button
			exportButton = new()
			{
				name = "export-window--export-grid__button",
				tooltip = "Export grid",
				style =
				{
					alignSelf = Align.Center,
					minWidth = 100,
					maxWidth = 100,
					minHeight = 50,
					maxHeight = 50,
				}
			};

			Label exportGridButtonLabel = new("Export")
			{
				style =
				{
					flexGrow = 1,
					fontSize = 24
				}
			};

			exportButton.Add(exportGridButtonLabel);

			exportButton.clicked += Export;


			rootVisualElement.Add(typeField);
			rootVisualElement.Add(directoryPathBrowser);
			rootVisualElement.Add(fileNameField);
			rootVisualElement.Add(spacer);
			rootVisualElement.Add(exportButton);

			guiBuilt = true;
		}

		private void OnDisable()
		{
			if (!guiBuilt) return;

			Save.SaveExportSettingsFile(exportSettings);
		}

		private void OnDestroy()
		{
			if (!guiBuilt) return;

			Save.SaveExportSettingsFile(exportSettings);

			onDestroy?.Invoke();
		}


		private void LoadExportSettings()
		{
			bool pathExists = Directory.Exists(Constants.PATH);
			if (!pathExists)
			{
				Directory.CreateDirectory(Constants.PATH);
			}

			try
			{
				if (pathExists)
					exportSettings = Load.LoadExportSettingsFile();
			}
			catch
			{
				if (exportSettings == null) exportSettings = new();

				Save.SaveExportSettingsFile(exportSettings);

				Debug.Log("Successfully created save files");
			}
		}

		private TypeField<IExportGrid> CreateTypeField()
		{
			TypeField<IExportGrid> typeField = new("Exporter")
			{
				style =
				{
					paddingTop = 4,
					paddingLeft = 4,
					paddingRight = 4,
					paddingBottom = 4
				},
			};

			typeField.RegisterValueChanged((monoScript) => CreateExporterInstance(monoScript?.GetClass()));

			return typeField;
		}

		private void CreateExporterInstance(Type type)
		{
			if (type == null)
			{
				_exporter = null;
				return;
			}

			_exporter = (IExportGrid)Activator.CreateInstance(type);
			exportSettings.assemblyQualifiedName = _exporter.GetType().AssemblyQualifiedName;

			Debug.Log($"Successfully assigned {type} as the exporter");
		}

		private void RestoreExporterInstance(TypeField<IExportGrid> typeField)
		{
			string assemblyQualifiedName = exportSettings?.assemblyQualifiedName ?? string.Empty;

			int commaIndex = assemblyQualifiedName.IndexOf(',');
			string typeName = commaIndex >= 0 ? assemblyQualifiedName.Substring(0, commaIndex) : assemblyQualifiedName;

			typeField.SetTypeName(typeName);

			if (string.IsNullOrEmpty(assemblyQualifiedName)) return;

			Type exporterType = Type.GetType(assemblyQualifiedName);
			CreateExporterInstance(exporterType);
		}

		private void OnFolderFieldValueChanged(ChangeEvent<string> folderPath)
		{
			exportSettings.folderPath = folderPath.newValue;
		}

		private void OnFileNameFieldValueChanged(ChangeEvent<string> fileName)
		{
			exportSettings.fileName = fileName.newValue;
		}

		private void Export()
		{
			if (string.IsNullOrEmpty(exportSettings.folderPath))
			{
				Debug.LogError("Folder path is null or empty. Please introduce a export path");
				return;
			}
			if (string.IsNullOrEmpty(exportSettings.fileName))
			{
				Debug.LogError("File name is null or empty. Please introduce a file name");
				return;
			}

			destinationPath = ParseFolderPath();
			fileName = exportSettings.fileName;
			fileName += ".asset";

			string finalPath = $"{destinationPath}/{fileName}";

			try
			{
				if (AssetDatabase.AssetPathExists(destinationPath))
				{
					int[][] parsedGrid = ParseGridTerrainData(grid.grid);
					_exporter.ExportGrid(finalPath, parsedGrid);
				}
				else
				{
					Debug.LogError($"Asset path does not exists. {destinationPath}");
					return;
				}
			}
			catch (Exception exception)
			{
				Debug.LogError("Unexpected error when creating grid data");
				Debug.LogException(exception);
				return;
			}
		}

		///	Divide into FindString() & ParseFolderPath()
		private string ParseFolderPath()
		{
			string toFind = "Assets";
			string cStr = exportSettings.folderPath;
			int toFindIndex = 0;
			int startIndex = 0;
			foreach (char c in cStr)
			{
				if (c == toFind[toFindIndex])
				{
					toFindIndex++;

					if (toFindIndex == toFind.Length)
						break;
				}
				else
				{
					toFindIndex = 0;
				}

				startIndex++;
			}
			startIndex++;
			startIndex -= toFind.Length;

			int subStrLength = cStr.Length - startIndex;
			string subStr = cStr.Substring(startIndex, subStrLength);

			return subStr;
		}

		private int[][] ParseGridTerrainData(int[][] grid)
		{
			int columns = grid.Length;
			int rows = grid[0].Length;

			int[][] result = new int[columns][];

			for (int c = 0, C = columns - 1; c < columns; c++, --C)
			{
				result[c] = new int[rows];
				for (int r = 0, R = rows - 1; r < rows; r++, --R)
				{
					result[c][r] = grid[c][R];
				}
			}

			return result;
		}
	}
}
