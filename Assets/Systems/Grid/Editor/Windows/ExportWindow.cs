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

		private Action onDestroy;


		public static void OpenWindow(GridData grid, Action onDestroy)
		{
			ExportWindow window = GetWindow<ExportWindow>();
			window.titleContent = new GUIContent("Export Grid");
			window.minSize = new Vector2(500, 200);

			window.grid = grid;

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
			if (!pathExists) Directory.CreateDirectory(Constants.PATH);

			bool fileExists = File.Exists(Path.Combine(Constants.PATH, Constants.EXPORT_SETTINGS_FILE_NAME));
			if (fileExists)
			{
				exportSettings = Load.LoadExportSettingsFile();
			}
			else
			{
				if (exportSettings == null) exportSettings = new();

				Save.SaveExportSettingsFile(exportSettings);

				Debug.Log("Successfully created export settings file");
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
				exportSettings.assemblyQualifiedName = string.Empty;
				return;
			}

			if (type == null ||
				!typeof(MonoScript).IsAssignableFrom(type) ||
				!type.IsClass ||
				type.IsAbstract ||
				type.ContainsGenericParameters ||
				type.GetConstructor(Type.EmptyTypes) == null ||
				type.IsInterface)
			{
				Debug.LogWarning("Exporter type changed or is malformed. Choose again an exporter");
				_exporter = null;
				exportSettings.assemblyQualifiedName = string.Empty;
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

			try
			{
				//int[][] parsedGrid = ParseGridTerrainData(grid);
				_exporter.ExportGrid(exportSettings.fileName, exportSettings.folderPath, grid.grid);
			}
			catch (Exception exception)
			{
				Debug.LogError("Unexpected error when creating grid data");
				Debug.LogException(exception);
				return;
			}
		}

		private int[][] ParseGridTerrainData(GridData grid)
		{
			int[][] result = new int[grid.columns][];

			for (int c = 0, C = grid.columns - 1; c < grid.columns; c++, --C)
			{
				result[c] = new int[grid.rows];
				for (int r = 0, R = grid.rows - 1; r < grid.rows; r++, --R)
				{
					result[c][r] = grid.grid[c][R];
				}
			}

			return result;
		}
	}
}
