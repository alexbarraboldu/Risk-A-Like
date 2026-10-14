using System;
using System.IO;
using System.Linq;

using ABB.Grid.Editor.Data;
using ABB.Grid.Editor.Utils;
using ABB.Grid.Editor.VisualElements;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.Windows
{
	public class ExportWindow : EditorWindow
	{
		private bool guiBuilt = false;

		private readonly string FILE_NAME = "exportSettings.bin";

		private int[][] _grid;
		public int[][] Grid
		{
			get => _grid;
			set => _grid = value;
		}

		private ExportSettingsData exportSettings;

		private IExportGrid _exporter;


		private ObjectField cellGameObjectField = null;
		private Button exportButton = null;

		///	Add directory save path field
		string soPath = string.Empty;

		///	Add file name save field
		string soName = string.Empty;


		public static void OpenWindow()
		{
			ExportWindow window = GetWindow<ExportWindow>();
			window.titleContent = new GUIContent("Export Grid");
			window.minSize = new Vector2(500, 200);
		}

		public static void OpenWindow(SessionData session)
		{
			ExportWindow window = GetWindow<ExportWindow>();
			window.titleContent = new GUIContent("Export Grid");
			window.minSize = new Vector2(500, 200);

			window._grid = session.grid;
		}


		public void CreateGUI()
		{
			try
			{
				LoadExportSettingsFile();
			}
			catch (Exception)
			{
				SaveExportSettingsFile();
			}

			cellGameObjectField = CreateCellGameObjectField();
			cellGameObjectField.style.maxWidth = 240;

			ObjectField exporterScriptField = CreateExporterScriptField();

			TypeField<IExportGrid> typeField = CreateTypeField();

			DirectoryPathBrowser directoryPathBrowser = new("Export folder", "Choose export folder");
			directoryPathBrowser.FolderPath = exportSettings?.folderPath ?? string.Empty;
			directoryPathBrowser.OnFolderFieldValueChanged += OnFolderFieldValueChanged;

			TextField fileNameField = new("File name");
			fileNameField.style.paddingLeft = 4;
			fileNameField.style.paddingRight = 4;
			fileNameField.Children().ToArray()[0].style.minWidth = 100;

			fileNameField.value = exportSettings?.fileName ?? string.Empty;
			fileNameField.RegisterValueChangedCallback(OnFileNameFieldValueChanged);

			VisualElement spacer = new();
			spacer.style.flexGrow = 1;

			///	Export Grid Button
			exportButton = new()
			{
				name = "export-grid",
				tooltip = "Export grid",
				style =
				{
					minWidth = 100,
					maxWidth = 100,
					minHeight = 50,
					maxHeight = 50,
					paddingLeft = 4,
					bottom = 8,
					alignSelf = Align.Center,
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


			rootVisualElement.Add(cellGameObjectField);
			rootVisualElement.Add(exporterScriptField);
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

			SaveExportSettingsFile();
		}

		private void OnDestroy()
		{
			if (!guiBuilt) return;

			SaveExportSettingsFile();
		}


		private ObjectField CreateCellGameObjectField()
		{
			ObjectField gameObjectField = new ObjectField("Cell")
			{
				style =
			{
				paddingTop = 4,
				paddingLeft = 4,
				paddingBottom = 8
			},
				objectType = typeof(GameObject)
			};
			gameObjectField.Children().ToArray()[0].style.minWidth = 100;
			gameObjectField.RegisterValueChangedCallback(OnCellGameObjectChanged);

			if (GUID.TryParse(exportSettings.cellGUID, out GUID result))
				gameObjectField.value = AssetDatabase.LoadAssetByGUID(result, typeof(GameObject));

			return gameObjectField;
		}

		private ObjectField CreateExporterScriptField()
		{
			ObjectField scriptField = new ObjectField("Exporter")
			{
				style =
				{
					paddingTop = 4,
					paddingLeft = 4,
					paddingBottom = 8
				},
				objectType = typeof(MonoScript),
				allowSceneObjects = false
			};

			scriptField.Children().ToArray()[0].style.minWidth = 100;

			scriptField.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue is not MonoScript script)
					return;

				Type type = script.GetClass();

				if (type == null || !typeof(IExportGrid).IsAssignableFrom(type))
				{
					Debug.LogWarning($"The selected script must implement {typeof(IExportGrid)}");
					scriptField.SetValueWithoutNotify(null);
					return;
				}

				_exporter = (IExportGrid)Activator.CreateInstance(type);
			});

			return scriptField;
		}


		private TypeField<IExportGrid> CreateTypeField()
		{
			TypeField<IExportGrid> typeField = new("Exporter");

			return typeField;
		}

		private void OnCellGameObjectChanged(ChangeEvent<UnityEngine.Object> evt)
		{
			string path = AssetDatabase.GetAssetPath(evt.newValue);
			string gUID = AssetDatabase.AssetPathToGUID(path);
			exportSettings.cellGUID = gUID;
		}

		private void OnFolderFieldValueChanged(ChangeEvent<string> folderPath)
		{
			exportSettings.folderPath = folderPath.newValue;
		}

		private void OnFileNameFieldValueChanged(ChangeEvent<string> fileName)
		{
			exportSettings.fileName = fileName.newValue;
		}

		#region LOAD & SAVE DATA

		public void LoadExportSettingsFile()
		{
			exportSettings = BinarySerialization.ReadFromBinaryFile<ExportSettingsData>($"{Constants.PATH}\\{FILE_NAME}");
		}

		public void SaveExportSettingsFile()
		{
			if (!Directory.Exists(Constants.PATH))
			{
				Directory.CreateDirectory(Constants.PATH);
			}

			try
			{
				if (exportSettings == null) exportSettings = new ExportSettingsData();

				BinarySerialization.WriteToBinary<ExportSettingsData>($"{Constants.PATH}\\{FILE_NAME}", exportSettings);
			}
			catch (Exception)
			{
				//Debug.Log("Could not serialize session file data");
				//Debug.LogException(exception);
			}
		}

		#endregion

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

			soPath = ParseFolderPath();
			soName = exportSettings.fileName;
			soName += ".asset";

			string finalPath = $"{soPath}/{soName}";

			try
			{
				if (AssetDatabase.AssetPathExists(soPath))
				{
					int[][] parsedGrid = ParseGridTerrainData(_grid);
					_exporter.ExportGrid(finalPath, parsedGrid);
				}
				else
				{
					Debug.LogError($"Asset path does not exists. {soPath}");
					return;
				}
			}
			catch
			{
				Debug.LogError("Unexpected error when creating gridTerrainDataSO");
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
