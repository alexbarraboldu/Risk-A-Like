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
		}

		public void CreateGUI()
		{
			LoadExportSettings();

			TypeField<IExportGrid> typeField = CreateTypeField();

			DirectoryPathBrowser directoryPathBrowser = new("Export folder", "Choose export folder");
			directoryPathBrowser.FolderPath = exportSettings?.folderPath ?? string.Empty;
			directoryPathBrowser.OnFolderFieldValueChanged += OnFolderFieldValueChanged;

			fileNameField = new("File name")
			{
				style =
				{
					marginLeft = 6,
					marginRight = 6,
					marginTop = 6
				}
			};
			fileNameField.Children().ToArray()[0].style.minWidth = 100;
			fileNameField.value = currentGrid?.fileName ?? string.Empty;

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

			bool exporterExists = RestoreExporterInstance(typeField);
			exportButton.SetEnabled(exporterExists);

			guiBuilt = true;
		}

		private void OnEnable()
		{
			EditorWindow.windowFocusChanged += OnWindowFocusChanged;
		}

		private void OnDisable()
		{
			if (!guiBuilt) return;

			Save.SaveExportSettingsFile(exportSettings);
		}

		private void OnDestroy()
		{
			EditorWindow.windowFocusChanged -= OnWindowFocusChanged;
			EditorApplication.delayCall -= RestoreFocus;

			editorInstance?.tabView.SetEnabled(true);

			if (!guiBuilt) return;

			Save.SaveExportSettingsFile(exportSettings);
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
					marginLeft= 6,
					marginRight = 6,
					marginTop = 9
				},
			};

			typeField.RegisterValueChanged((monoScript) =>
			{
				bool exporterExists = CreateExporterInstance(monoScript?.GetClass());
				exportButton.SetEnabled(exporterExists);
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

				Debug.Log($"Successfully assigned {type} as the exporter");

				return true;
			}
			catch (Exception exception)
			{
				_exporter = null;
				exportSettings.assemblyQualifiedName = null;

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
				_exporter.ExportGrid(fileNameField.text, exportSettings.folderPath, currentGrid.grid);

				Debug.Log("Grid data exported successfully!");
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return;
			}
		}
	}
}
