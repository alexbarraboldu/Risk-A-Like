using System;
using System.Linq;

using ABB.Grid.Editor.Data;
using ABB.Grid.Editor.Windows;

using UnityEditor;

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
		private ColorSelectorDropdown leftColorSelector = null;
		private ColorSelectorDropdown rightColorSelector = null;

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
			add => leftColorSelector.OnColorChanged += value;
			remove => leftColorSelector.OnColorChanged -= value;
		}
		public event Action<int, Color> OnRightColorChanged
		{
			add => rightColorSelector.OnColorChanged += value;
			remove => rightColorSelector.OnColorChanged -= value;
		}
		public event Action<int> OnRemoveTerrain;

		public event Action<Vector2Int> OnGridSizeChanged
		{
			add => gridSizeField.ValueChanged += value;
			remove => gridSizeField.ValueChanged -= value;
		}
		public event Action OnEraserClicked;
		public event Action OnResetGrid
		{
			add => resetGridButton.clicked += value;
			remove => resetGridButton.clicked -= value;
		}
		public event Action OnFillGrid;
		public event Action OnExportGrid
		{
			add => exportGridButton.clicked += value;
			remove => exportGridButton.clicked -= value;
		}
		public event Func<string, bool> OnNewGrid;


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
		}


		private void BuildUI(int maxVector2IntFieldSize)
		{
			name = "tools_view";

			style.flexDirection = FlexDirection.Row;
			style.paddingLeft = 4;
			style.paddingTop = 4;
			style.paddingBottom = 4;
			style.paddingRight = 4;
			style.backgroundColor = Color.gray4;
			style.alignItems = Align.Center;

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
			leftColorSelector = new ColorSelectorDropdown(terrainColors.terrainColors.ToList());
			rightColorSelector = new ColorSelectorDropdown(terrainColors.terrainColors.ToList());
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
			///	Left
			Add(leftOptionsElement);

			VisualElement rowSelectorParent = new();
			rowSelectorParent.style.flexDirection = FlexDirection.Row;

			rowSelectorParent.Add(leftColorSelector);
			rowSelectorParent.Add(rightColorSelector);

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
			Add(middleOptionsElement);

			///	Right
			Add(rightOptionsElement);

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
			leftColorSelector.isDelayed = true;
			rightColorSelector.isDelayed = true;

			leftColorSelector.OnIndexChanged += LeftSelectorValueChanged;
			rightColorSelector.OnIndexChanged += RightSelectorValueChanged;

			leftColorSelector.OnColorChanged += LeftSelectorColorChanged;
			rightColorSelector.OnColorChanged += RightSelectorColorChanged;

			leftColorSelector.OnChoiceAdded += LeftSelectorChoiceAdded;
			rightColorSelector.OnChoiceAdded += RightSelectorChoiceAdded;

			leftColorSelector.OnChoiceRemoved += LeftSelectorChoiceRemoved;
			rightColorSelector.OnChoiceRemoved += RightSelectorChoiceRemoved;

			leftColorSelector.SetIndex((int)session.leftColor);
			rightColorSelector.SetIndex((int)session.rightColor);

			eraserButton.clicked += OnEraseButton;
			fillGridButton.clicked += OnFillButton;

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
				OnNewGridConfirmed,
				false
				);
		}

		private void OnNewGridConfirmed(string value)
		{
			bool result = OnNewGrid?.Invoke(value) ?? false;
			if (result) SetEnableUI(true);
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

		#region ERASE & FILL BUTTONS

		private void OnFillButton()
		{
			OnFillGrid?.Invoke();

			if (!fillGridButton.hasCheckedPseudoState)
			{
				fillGridButton.SetCheckedPseudoState(true);
				fillGridButton.style.backgroundColor = Color.gray5;

				if (eraserButton.hasCheckedPseudoState)
				{
					OnEraserClicked?.Invoke();
					eraserButton.SetCheckedPseudoState(false);
					eraserButton.style.backgroundColor = new Color(0.345098f, 0.345098f, 0.345098f, 1f);
				}
			}
			else
			{
				fillGridButton.SetCheckedPseudoState(false);
				fillGridButton.style.backgroundColor = new Color(0.345098f, 0.345098f, 0.345098f, 1f);
			}
		}

		private void OnEraseButton()
		{
			OnEraserClicked?.Invoke();

			if (!eraserButton.hasCheckedPseudoState)
			{
				eraserButton.SetCheckedPseudoState(true);
				eraserButton.style.backgroundColor = Color.gray5;

				if (fillGridButton.hasCheckedPseudoState)
				{
					OnFillGrid?.Invoke();
					fillGridButton.SetCheckedPseudoState(false);
					fillGridButton.style.backgroundColor = new Color(0.345098f, 0.345098f, 0.345098f, 1f);
				}
			}
			else
			{
				eraserButton.SetCheckedPseudoState(false);
				eraserButton.style.backgroundColor = new Color(0.345098f, 0.345098f, 0.345098f, 1f);
			}
		}

		#endregion

		#region COLOR SELECTORS

		private void LeftSelectorValueChanged(int value) => session.leftColor = (uint)value;
		private void RightSelectorValueChanged(int value) => session.rightColor = (uint)value;

		private void LeftSelectorColorChanged(int value, Color color)
		{
			rightColorSelector.SetColor(value, color);

			TerrainColor newTerrainColor = leftColorSelector.choices[value];
			terrainColors.terrainColors[value] = newTerrainColor;
		}
		private void RightSelectorColorChanged(int value, Color color)
		{
			leftColorSelector.SetColor(value, color);

			TerrainColor newTerrainColor = leftColorSelector.choices[value];
			terrainColors.terrainColors[value] = newTerrainColor;
		}

		private void LeftSelectorChoiceAdded(TerrainColor newChoice)
		{
			rightColorSelector.AddChoice(newChoice);
			if (rightColorSelector.index == 0)
				rightColorSelector.SetIndex(0);

			ArrayUtility.Add(ref terrainColors.terrainColors, newChoice);
		}
		private void RightSelectorChoiceAdded(TerrainColor newChoice)
		{
			leftColorSelector.AddChoice(newChoice);
			if (leftColorSelector.index == 0)
				leftColorSelector.SetIndex(0);

			ArrayUtility.Add(ref terrainColors.terrainColors, newChoice);
		}

		private void LeftSelectorChoiceRemoved(int index)
		{
			if (rightColorSelector.choices.Count == 0) return;

			rightColorSelector.RemoveChoice(index);
			if (leftColorSelector.index == index)
				rightColorSelector.SetIndex(index - 1 < 0 ? 0 : index - 1);

			ArrayUtility.RemoveAt(ref terrainColors.terrainColors, index);

			if (terrainColors.terrainColors.Length == 0)
				rightColorSelector.SetIndex(0);

			OnRemoveTerrain?.Invoke(index);
		}
		private void RightSelectorChoiceRemoved(int index)
		{
			if (leftColorSelector.choices.Count == 0) return;

			leftColorSelector.RemoveChoice(index);
			if (leftColorSelector.index == index)
				leftColorSelector.SetIndex(index - 1 < 0 ? 0 : index - 1);

			ArrayUtility.RemoveAt(ref terrainColors.terrainColors, index);

			if (terrainColors.terrainColors.Length == 0)
				leftColorSelector.SetIndex(0);

			OnRemoveTerrain?.Invoke(index);
		}

		#endregion
	}
}
