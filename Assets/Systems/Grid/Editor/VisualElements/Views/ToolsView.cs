using System;

using ABB.Gameplay.Data.Types;
using ABB.Grid.Editor.Data;
using ABB.Grid.Editor.Windows;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class ToolsView : VisualElement
	{
		///	DATA
		private SessionData session = null;
		private GridData currentGrid = null;
		private TerrainColorData terrainColors = null;

		private readonly GridEditorWindow owner = null;

		///	VISUAL ELEMENTS
		private ColorSelectorDropdown<TerrainType> leftTerrainTypeSelector = null;
		private ColorSelectorDropdown<TerrainType> rightTerrainTypeSelector = null;

		private Vector2IntField gridSizeField = null;

		private Button eraserButton = null;
		private Button resetGridButton = null;
		private Button fillGridButton = null;

		private Button exportGridButton = null;
		private Button newGridButton = null;

		private Label gridTextLabel = null;

		///	ACTIONS
		public event Action<int, Color> OnLeftColorChanged
		{
			add => leftTerrainTypeSelector.OnColorChanged += value;
			remove => leftTerrainTypeSelector.OnColorChanged -= value;
		}
		public event Action<int, Color> OnRightColorChanged
		{
			add => rightTerrainTypeSelector.OnColorChanged += value;
			remove => rightTerrainTypeSelector.OnColorChanged -= value;
		}
		public event Action<int> OnLeftTerrainChanged
		{
			add => leftTerrainTypeSelector.OnValueChanged += value;
			remove => leftTerrainTypeSelector.OnValueChanged -= value;
		}
		public event Action<int> OnRightTerrainChanged
		{
			add => rightTerrainTypeSelector.OnValueChanged += value;
			remove => rightTerrainTypeSelector.OnValueChanged -= value;
		}
		public event Action<Vector2Int> OnGridSizeChanged
		{
			add => gridSizeField.ValueChanged += value;
			remove => gridSizeField.ValueChanged -= value;
		}
		public event Action OnDeleteCell
		{
			add => eraserButton.clicked += value;
			remove => eraserButton.clicked -= value;
		}
		public event Action OnResetGrid
		{
			add => resetGridButton.clicked += value;
			remove => resetGridButton.clicked -= value;
		}
		public event Action OnFillGrid
		{
			add => fillGridButton.clicked += value;
			remove => fillGridButton.clicked -= value;
		}
		public event Action OnExportGrid
		{
			add => exportGridButton.clicked += value;
			remove => exportGridButton.clicked -= value;
		}
		public event Action<string> OnNewGrid;


		public ToolsView(SessionData session, GridData currentGrid, TerrainColorData terrainColors, GridEditorWindow owner, int maxVector2IntFieldSize)
		{
			///	INJECT DATA
			this.session = session;
			this.currentGrid = currentGrid;
			this.terrainColors = terrainColors;

			this.owner = owner;

			BuildUI(maxVector2IntFieldSize);
			AddStyle();
			AddLogic();

			leftTerrainTypeSelector.SetColorsPalette(this.terrainColors.terrainColors);
			rightTerrainTypeSelector.SetColorsPalette(this.terrainColors.terrainColors);

			leftTerrainTypeSelector.Value = (int)session.leftColor;
			rightTerrainTypeSelector.Value = (int)session.rightColor;
		}


		private void BuildUI(int maxVector2IntFieldSize)
		{
			name = "tools_view";

			VisualElement rootOptionsElement = new()
			{
				name = "tools-view--root",
				style =
				{
					flexDirection = FlexDirection.Row,
					paddingLeft = 4,
					paddingTop = 4,
					paddingBottom = 4,
					paddingRight = 4,
					backgroundColor = Color.gray4,
					alignItems = Align.Center
				}
			};

			VisualElement leftOptionsElement = new()
			{
				name = "tools-view--left-options",
				style =
				{
					flexDirection = FlexDirection.Column,
					maxWidth = 300,
					paddingRight = 4
				}
			};

			gridSizeField = new Vector2IntField("Grid Size", "Columns", "Rows", false, maxVector2IntFieldSize);
			gridSizeField.Q<Label>("vector2int-field__label").style.display = DisplayStyle.None;
			gridSizeField.Q("vector2int-field").style.flexGrow = 1;

			VisualElement visualElement = new();
			gridSizeField.Q("vector2int-field__fields-xField").style.width = 118;
			gridSizeField.Q("vector2int-field__fields-yField").style.width = 118;

			VisualElement middleOptionsElement = new()
			{
				name = "tools-view--middle-options",
				style =
				{
					flexGrow = 1,
					flexDirection = FlexDirection.Column,
					justifyContent = Justify.Center
				}
			};

			VisualElement rightOptionsElement = new()
			{
				name = "tools-view--right-options",
				style =
				{
					flexDirection = FlexDirection.Column,
					alignItems = Align.Center,
					alignSelf = Align.Stretch,
					justifyContent = Justify.SpaceAround,
					maxWidth = 300
				}
			};

			///	Terrain type selector
			leftTerrainTypeSelector = new ColorSelectorDropdown<TerrainType>();
			rightTerrainTypeSelector = new ColorSelectorDropdown<TerrainType>();
			///----------------------

			///	Eraser Button
			eraserButton = new()
			{
				name = "tools-view--eraser__button",
				tooltip = "Eraser",
				style =
				{
					minWidth = 35,
					maxWidth = 35,
					minHeight = 35,
					maxHeight = 35,
				}
			};
			eraserButton.iconImage = (Texture2D)UnityEditor.EditorGUIUtility.IconContent("Grid.EraserTool").image;
			///	Reset Grid Button
			resetGridButton = new()
			{
				name = "tools-view--reset-grid__button",
				tooltip = "Reset grid",
				style =
				{
					minWidth = 35,
					maxWidth = 35,
					minHeight = 35,
					maxHeight = 35,
				}
			};
			resetGridButton.iconImage = (Texture2D)UnityEditor.EditorGUIUtility.IconContent("Refresh").image;
			///	Fill Grid Button
			fillGridButton = new()
			{
				name = "tools-view--fill-grid__button",
				tooltip = "Fill grid",
				style =
				{
					minWidth = 35,
					maxWidth = 35,
					minHeight = 35,
					maxHeight = 35,
				}
			};
			fillGridButton.iconImage = (Texture2D)UnityEditor.EditorGUIUtility.IconContent("Grid.FillTool").image;
			///	Export Grid Button
			exportGridButton = new()
			{
				name = "tools-view--export-grid__button",
				tooltip = "Export grid",
				style =
				{
					minWidth = 35,
					maxWidth = 35,
					minHeight = 35,
					maxHeight = 35,
				}
			};
			exportGridButton.iconImage = (Texture2D)UnityEditor.EditorGUIUtility.IconContent("SceneLoadOut").image;
			///	New Grid Button
			newGridButton = new()
			{
				name = "tools-view--new-grid__button",
				tooltip = "New grid",
				style =
				{
					minWidth = 35,
					maxWidth = 35,
					minHeight = 35,
					maxHeight = 35,
				}
			};
			newGridButton.iconImage = (Texture2D)UnityEditor.EditorGUIUtility.IconContent("Toolbar Plus").image;
			///-------------------

			///	Grid Text Label
			gridTextLabel = new(currentGrid.fileName == string.Empty ? "No grid loaded" : currentGrid.fileName);
			gridTextLabel.style.alignSelf = Align.Stretch;

			///	ADD TO UI
			Add(rootOptionsElement);

			///	Left
			rootOptionsElement.Add(leftOptionsElement);

			VisualElement rowSelectorParent = new();
			rowSelectorParent.style.flexDirection = FlexDirection.Row;

			rowSelectorParent.Add(leftTerrainTypeSelector);
			rowSelectorParent.Add(rightTerrainTypeSelector);

			VisualElement rowGridSizeDeleteParent = new VisualElement()
			{
				style =
				{
					paddingTop = 4,
					alignSelf = Align.FlexStart,
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center
				}
			};
			rowGridSizeDeleteParent.Add(gridSizeField);
			rowGridSizeDeleteParent.Add(eraserButton);
			rowGridSizeDeleteParent.Add(resetGridButton);
			rowGridSizeDeleteParent.Add(fillGridButton);

			leftOptionsElement.Add(rowSelectorParent);
			leftOptionsElement.Add(rowGridSizeDeleteParent);

			///	Middle
			rootOptionsElement.Add(middleOptionsElement);

			///	Right
			rootOptionsElement.Add(rightOptionsElement);

			VisualElement rightButtonsParent = new();
			rightButtonsParent.style.flexDirection = FlexDirection.Row;

			rightOptionsElement.Add(gridTextLabel);
			rightOptionsElement.Add(rightButtonsParent);

			rightButtonsParent.Add(newGridButton);
			rightButtonsParent.Add(exportGridButton);
		}

		private void AddStyle()
		{

		}

		private void AddLogic()
		{
			leftTerrainTypeSelector.OnValueChanged += LeftSelectorValueChanged;
			rightTerrainTypeSelector.OnValueChanged += RightSelectorValueChanged;

			leftTerrainTypeSelector.OnColorChanged += LeftSelectorColorChanged;
			rightTerrainTypeSelector.OnColorChanged += RightSelectorColorChanged;

			eraserButton.clicked += () =>
			{
				if (eraserButton.style.backgroundColor == Color.gray5)
					eraserButton.style.backgroundColor = new Color(0.345098f, 0.345098f, 0.345098f, 1f);
				else
					eraserButton.style.backgroundColor = Color.gray5;
			};

			SetEnableUI(false);

			newGridButton.clicked += OnNewGridButtonClicked;
		}

		private void OnNewGridButtonClicked()
		{
			TextInputDialog.OpenWindow(
				"Create new grid",
				"Grid name",
				"Create & Edit",
				owner.position,
				OnNewGridConfirmed
				);
		}

		private void OnNewGridConfirmed(string value)
		{
			SetEnableUI(true);

			OnNewGrid?.Invoke(value);
		}

		private void SetEnableUI(bool value)
		{
			gridSizeField.SetEnabled(value);

			eraserButton.SetEnabled(value);
			resetGridButton.SetEnabled(value);
			fillGridButton.SetEnabled(value);

			exportGridButton.SetEnabled(value);
		}

		public void SetCurrentGrid(GridData currentGrid)
		{
			this.currentGrid = currentGrid;
			gridSizeField.SetValueWithoutNotify(new Vector2Int(currentGrid.columns, currentGrid.rows));
			gridTextLabel.text = currentGrid.fileName;

			SetEnableUI(!(currentGrid.fileName == string.Empty));
		}

		private void LeftSelectorValueChanged(int value) => session.leftColor = (uint)value;
		private void RightSelectorValueChanged(int value) => session.rightColor = (uint)value;

		private void LeftSelectorColorChanged(int value, Color color)
		{
			rightTerrainTypeSelector.SetPaletteColor(value, color);
			if (rightTerrainTypeSelector.Value == value)
				rightTerrainTypeSelector.SetColorByValue(value);

			TerrainColor newTerrainColor = new((TerrainType)value, color);
			terrainColors.terrainColors[value] = newTerrainColor;
		}
		private void RightSelectorColorChanged(int value, Color color)
		{
			leftTerrainTypeSelector.SetPaletteColor(value, color);
			if (leftTerrainTypeSelector.Value == value)
				leftTerrainTypeSelector.SetColorByValue(value);

			TerrainColor newTerrainColor = new((TerrainType)value, color);
			terrainColors.terrainColors[value] = newTerrainColor;
		}
	}
}
