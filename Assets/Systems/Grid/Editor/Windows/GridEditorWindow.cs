using System;
using System.IO;

using ABB.Grid.Editor.Data;
using ABB.Grid.Editor.Utils;

using UnityEditor;
using UnityEditor.Search;

using UnityEngine;
using UnityEngine.UIElements;

public class GridEditorWindow : EditorWindow
{
	private const string StylePath = "Assets/";
	public StyleSheet uss;

	private readonly string PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ABB", "GridEditor");
	private readonly string FILE_NAME = "gridEditorSessionSaveFile.bin";

	private Toggle _listenSceneClicksToggle = null;
	private Vector2IntElement gridSizeElement = null;
	private ObjectField cellGameObjectField = null;
	private static bool _listenSceneClicks = false;
	private CellGridCanvas _cellGridCanvas = null;


	/// <summary>
	/// Data to be saved by each session
	/// </summary>
	private SessionFileData sessionFile = null;


	[MenuItem("Window/Custom Tools/Grid Editor")]
	private static void OpenWindow()
	{
		GridEditorWindow window = GetWindow<GridEditorWindow>();
		window.titleContent = new GUIContent("Grid Editor");
		window.minSize = new Vector2(500, 500);
	}


	private void OnDisable()
	{
		SaveSessionFile();
	}

	private void OnDestroy()
	{
		SaveSessionFile();
		_listenSceneClicks = false;
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
			sessionFile = new SessionFileData();
			SaveSessionFile();

			Debug.Log("Could not deserialize session file data");
			Debug.LogException(exception, this);
		}


		///	BUILD WINDOW UI
		VisualElement rootOptionsElement = new()
		{
			style =
			{
				flexDirection = FlexDirection.Column,
				minHeight = 100,
				paddingLeft = 4
			}
		};

		_listenSceneClicksToggle = CreateToggleField_ListenSceneClicks("Listen Scene Clicks", true);

		cellGameObjectField = CreateCellGameObjectField();

		gridSizeElement = new Vector2IntElement("Grid Size", "Columns", "Rows");
		gridSizeElement.ValueChanged += OnGridSizeChanged;

		VisualElement rootCellGridCanvas = new()
		{
			style =
			{
				flexGrow = 1
			}
		};

		_cellGridCanvas = new();


		///	ADD TO UI
		rootOptionsElement.Add(_listenSceneClicksToggle);
		rootOptionsElement.Add(cellGameObjectField);
		rootOptionsElement.Add(gridSizeElement);

		rootCellGridCanvas.Add(_cellGridCanvas);

		rootVisualElement.Add(rootOptionsElement);
		rootVisualElement.Add(rootCellGridCanvas);


		///	UPDATE VALUES
		///	Get gameObject by ID
		//cellGameObjectField.value = sessionFile.cellGameObjectId;

		gridSizeElement.Value = new Vector2Int((int)sessionFile.columns, (int)sessionFile.rows);

		_cellGridCanvas.Grid = sessionFile.grid;
		_cellGridCanvas.CellSize = sessionFile.cellSize;
	}

	private void SaveSessionFile()
	{
		if (!Directory.Exists(PATH))
		{
			Directory.CreateDirectory(PATH);
		}

		try
		{
			sessionFile.cellSize = _cellGridCanvas.CellSize;
			sessionFile.grid = _cellGridCanvas.Grid;

			BinarySerialization.WriteToBinary<SessionFileData>($"{PATH}\\{FILE_NAME}", sessionFile);
		}
		catch (Exception exception)
		{
			Debug.Log("Could not serialize session file data");
			Debug.LogException(exception);
		}
	}

	private void OnGridSizeChanged(Vector2Int newValue)
	{
		sessionFile.columns = (uint)newValue.x;
		sessionFile.rows = (uint)newValue.y;
		_cellGridCanvas.GridSize = newValue;
	}

	#region Visual Elements

	private Toggle CreateToggleField_ListenSceneClicks(string name, bool value)
	{
		_listenSceneClicks = value;

		Toggle result = new(name);
		result.SetValueWithoutNotify(value);
		result.RegisterValueChangedCallback(evt =>
		{
			_listenSceneClicks = evt.newValue;
		});

		return result;
	}

	private ObjectField CreateCellGameObjectField()
	{
		ObjectField gameObjectField = new ObjectField("Cell")
		{
			objectType = typeof(GameObject)
		};

		gameObjectField.RegisterValueChangedCallback(evt =>
		{
			sessionFile.cellGameObjectId = (evt.newValue as GameObject).GetInstanceID().ToString();

			if (sessionFile.cellGameObjectId != null) Debug.Log($"Cell GameObject: {evt.newValue.name}", evt.newValue);
		});

		return gameObjectField;
	}

	#endregion
}
