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
	public class ExportWindow : EditorWindow
	{
		private const string StylePath = "Assets/Systems/Grid/Editor/StyleSheets/ExportWindow.uss";
		public StyleSheet uss;

		private bool guiBuilt = false;
		private bool closeExportWindow = true;

		private ExportSettingsData exportSettings;
		private GridData currentGrid;

		private GridEditorWindow editorInstance = null;

		private IExportGrid _exporter;

		TextField fileNameField = null;
		private Button exportButton = null;


		public static void OpenWindow(GridData currentGrid, GridEditorWindow gridEditorWindow)
		{
			ExportWindow window = CreateInstance<ExportWindow>();
			window.titleContent = new GUIContent("Export Grid");
			window.minSize = new Vector2(500, 200);
			window.maxSize = new Vector2(500, 200);

			window.currentGrid = currentGrid;
			window.editorInstance = gridEditorWindow;

			window.ShowUtility();

			window.position = new Rect(gridEditorWindow.position.center - window.minSize * 0.5f, window.minSize);
		}

		public void CreateGUI()
		{
			if (uss == null) uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
			if (uss != null) rootVisualElement.styleSheets.Add(uss);

			closeExportWindow = true;
			closeExportWindow = !LoadExportSettings();
			if (closeExportWindow)
			{
				EditorApplication.delayCall += Close;
				return;
			}

			exportSettings.fileName = !string.IsNullOrWhiteSpace(exportSettings?.fileName) ? exportSettings?.fileName : currentGrid?.fileName ?? string.Empty;

			TypeField<IExportGrid> typeField = CreateTypeField();

			DirectoryPathBrowser directoryPathBrowser = new("Export folder", "Choose export folder");
			directoryPathBrowser.FolderPath = exportSettings?.folderPath ?? string.Empty;
			directoryPathBrowser.OnFolderFieldValueChanged += OnFolderFieldValueChanged;

			fileNameField = new("File name")
			{
				name = "export-window-file-name-field",
				value = exportSettings.fileName
			};
			fileNameField.RegisterValueChangedCallback(evt =>
			{
				exportSettings.fileName = evt.newValue;
				SetEnabledExportButton();
			});

			VisualElement spacer = new()
			{
				name = "export-window-spacer"
			};

			///	Export Grid Button
			exportButton = new()
			{
				name = "export-window--export-grid__button",
				tooltip = "Export grid",
				text = "Export",
			};
			exportButton.clicked += Export;


			rootVisualElement.Add(typeField);
			rootVisualElement.Add(directoryPathBrowser);
			rootVisualElement.Add(fileNameField);
			rootVisualElement.Add(spacer);
			rootVisualElement.Add(exportButton);

			bool exporterExists = RestoreExporterInstance(typeField);
			SetEnabledExportButton();

			guiBuilt = true;
		}

		private void OnEnable()
		{
			EditorApplication.delayCall += DisableTabView;
			EditorWindow.windowFocusChanged += OnWindowFocusChanged;
		}

		private void OnDisable()
		{
			EditorWindow.windowFocusChanged -= OnWindowFocusChanged;
			EditorApplication.delayCall -= RestoreFocus;
			EditorApplication.delayCall -= DisableTabView;

			if (!guiBuilt || closeExportWindow) return;

			Save.SaveExportSettingsFile(exportSettings);
		}

		private void OnDestroy()
		{
			if (editorInstance != null)
				editorInstance.tabView?.SetEnabled(true);

			if (!guiBuilt || closeExportWindow) return;

			exportSettings.fileName = string.Empty;
			Save.SaveExportSettingsFile(exportSettings);
		}


		private void SetEnabledExportButton()
		{
			exportButton.SetEnabled(_exporter != null
				&& !string.IsNullOrEmpty(exportSettings.folderPath)
				&& !string.IsNullOrWhiteSpace(exportSettings.fileName));
		}

		private void OnWindowFocusChanged()
		{
			EditorApplication.delayCall -= RestoreFocus;
			EditorApplication.delayCall += RestoreFocus;
		}

		private void RestoreFocus()
		{
			if (this == null || editorInstance == null)
				return;

			if (EditorWindow.focusedWindow == editorInstance)
				Focus();
		}

		private void DisableTabView()
		{
			if (this != null && editorInstance != null)
				editorInstance.tabView?.SetEnabled(false);
		}

		private bool LoadExportSettings()
		{
			bool pathExists = Directory.Exists(Constants.PATH);
			if (!pathExists) Directory.CreateDirectory(Constants.PATH);

			if (File.Exists(Path.Combine(Constants.PATH, Constants.EXPORT_SETTINGS_FILE_NAME)))
			{
				exportSettings = Load.LoadExportSettingsFile();
			}
			else
			{
				if (exportSettings == null) exportSettings = new();

				if (Save.SaveExportSettingsFile(exportSettings))
					Debug.Log("Successfully created export settings file");
			}

			if (exportSettings == null)
			{
				string affectedFiles = "Affected files: ExportSettings file";

				LoadErrorWindow.OpenWindow(
					"Error on load data files",
					out bool closeParent,
					out bool regenerate,
					"Close editor",
					"Continue (will regenerate files)",
					affectedFiles
				);

				if (regenerate)
				{
					exportSettings = new();
				}
				if (closeParent)
				{
					return false;
				}
			}

			return true;
		}

		private TypeField<IExportGrid> CreateTypeField()
		{
			TypeField<IExportGrid> typeField = new("Exporter")
			{
				style =
				{
					marginLeft= 6,
					marginRight = 6,
					marginTop = 9
				},
			};

			typeField.RegisterValueChanged((monoScript) =>
			{
				bool exporterExists = CreateExporterInstance(monoScript?.GetClass());
				if (exporterExists)
					Debug.Log($"Successfully assigned {_exporter} as the exporter");

				SetEnabledExportButton();
			});

			return typeField;
		}

		private bool CreateExporterInstance(Type type)
		{
			if (type == null ||
				!typeof(IExportGrid).IsAssignableFrom(type) ||
				!type.IsClass ||
				type.IsAbstract ||
				type.ContainsGenericParameters ||
				type.GetConstructor(Type.EmptyTypes) == null ||
				type.IsInterface)
			{
				_exporter = null;
				exportSettings.assemblyQualifiedName = string.Empty;

				return false;
			}

			try
			{
				_exporter = (IExportGrid)Activator.CreateInstance(type);
				exportSettings.assemblyQualifiedName = _exporter.GetType().AssemblyQualifiedName;

				return true;
			}
			catch (Exception exception)
			{
				_exporter = null;
				exportSettings.assemblyQualifiedName = string.Empty;

				Debug.LogException(exception);

				return false;
			}
		}

		private bool RestoreExporterInstance(TypeField<IExportGrid> typeField)
		{
			string assemblyQualifiedName = exportSettings?.assemblyQualifiedName ?? string.Empty;

			int commaIndex = assemblyQualifiedName.IndexOf(',');
			string typeName = commaIndex >= 0 ? assemblyQualifiedName.Substring(0, commaIndex) : assemblyQualifiedName;

			if (string.IsNullOrEmpty(assemblyQualifiedName)) return false;

			Type exporterType = Type.GetType(assemblyQualifiedName);
			bool result = CreateExporterInstance(exporterType);

			if (result) typeField.SetTypeName(typeName);

			return result;
		}

		private void OnFolderFieldValueChanged(ChangeEvent<string> folderPath)
		{
			exportSettings.folderPath = folderPath.newValue;

			SetEnabledExportButton();
		}

		private void Export()
		{
			if (string.IsNullOrEmpty(exportSettings.folderPath))
			{
				Debug.LogError("Folder path is null or empty. Please introduce a export path");
				return;
			}
			if (string.IsNullOrEmpty(fileNameField.text))
			{
				Debug.LogError("File name is null or empty. Please introduce a file name");
				return;
			}
			if (_exporter == null)
			{
				Debug.LogError("Exporter is null and this should have not happened");
				return;
			}

			try
			{
				if (_exporter.ExportGrid(fileNameField.text, exportSettings.folderPath, currentGrid.grid, currentGrid.columns, currentGrid.rows))
					Debug.Log("Grid data exported successfully!");
				else
				{
					Debug.LogError("Grid export failed");
					return;
				}

			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return;
			}
		}
	}
}
