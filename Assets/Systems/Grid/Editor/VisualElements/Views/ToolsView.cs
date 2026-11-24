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
		private static readonly GUID StyleSheetGuid = new("a0440a4f759e35543b5063dfec33da71");
		public StyleSheet uss;

		///	DATA
		private SessionData session = null;
		private GridData currentGrid = null;
		private TerrainColorData terrainColors = null;

		private readonly GridEditorWindow owner = null;

		///	VISUAL ELEMENTS
		private ColorSelectorDropdown leftColorSelector = null;
		private ColorSelectorDropdown rightColorSelector = null;
		private Button swapSelectorsColorsButton = null;

		private Vector2IntField gridSizeField = null;
		private Button blockXButton = null;
		private bool xLocked = false;
		private Button blockYButton = null;
		private bool yLocked = false;

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
			if (uss == null) uss = AssetDatabase.LoadAssetByGUID<StyleSheet>(StyleSheetGuid);
			if (uss != null) styleSheets.Add(uss);

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
			name = "tools-view";
			AddToClassList("abb-tools-view");

			VisualElement leftOptionsElement = new()
			{
				name = "editing-tools"
			};
			leftOptionsElement.AddToClassList("abb-tools-view__editing-tools");

			gridSizeField = new Vector2IntField("Grid Size", "Columns", "Rows", false, maxVector2IntFieldSize);

			blockXButton = new(EditorGUIUtility.FindTexture(xLocked ? "LockIcon-On" : "LockIcon"))
			{
				tooltip = xLocked ? "Unlock" : "Lock"
			};
			blockYButton = new(EditorGUIUtility.FindTexture(yLocked ? "LockIcon-On" : "LockIcon"))
			{
				tooltip = yLocked ? "Unlock" : "Lock"
			};

			VisualElement middleOptionsElement = new()
			{
				name = "toolbar-spacer"
			};
			middleOptionsElement.AddToClassList("abb-tools-view__spacer");

			VisualElement rightOptionsElement = new()
			{
				name = "file-tools"
			};
			rightOptionsElement.AddToClassList("abb-tools-view__file-tools");

			///	Terrain type selector
			leftColorSelector = new ColorSelectorDropdown(terrainColors.terrainColors.ToList());
			rightColorSelector = new ColorSelectorDropdown(terrainColors.terrainColors.ToList());
			swapSelectorsColorsButton = new Button(EditorGUIUtility.FindTexture("preAudioLoopOff"), SwapSelectorsColors);
			swapSelectorsColorsButton.tooltip = "Swap colors";
			///----------------------

			///	Eraser Button
			eraserButton = new()
			{
				name = "eraser-button",
				tooltip = "Eraser",
				iconImage = (Texture2D)EditorGUIUtility.IconContent("Grid.EraserTool").image
			};
			///	Reset Grid Button
			resetGridButton = new()
			{
				name = "reset-grid-button",
				tooltip = "Reset grid",
				iconImage = (Texture2D)EditorGUIUtility.IconContent("Refresh").image
			};
			///	Fill Grid Button
			fillGridButton = new()
			{
				name = "fill-grid-button",
				tooltip = "Fill grid",
				iconImage = (Texture2D)EditorGUIUtility.IconContent("Grid.FillTool").image
			};
			///	Export Grid Button
			exportGridButton = new()
			{
				name = "export-grid-button",
				tooltip = "Export grid",
				iconImage = (Texture2D)EditorGUIUtility.IconContent("SceneLoadOut").image
			};
			///	New Grid Button
			newGridButton = new()
			{
				name = "new-grid-button",
				tooltip = "New grid",
				iconImage = (Texture2D)EditorGUIUtility.IconContent("Toolbar Plus").image
			};
			///-------------------

			///	Grid Text Label
			gridTextLabel = new(currentGrid.fileName == string.Empty ? "No grid loaded" : currentGrid.fileName)
			{
				name = "grid-name-label"
			};
			gridTextLabel.AddToClassList("abb-tools-view__grid-name-label");

			///	ADD TO UI
			///	Left
			Add(leftOptionsElement);

			VisualElement selectorParent = new()
			{
				name = "color-selectors"
			};
			selectorParent.AddToClassList("abb-tools-view__color-selectors");

			selectorParent.Add(leftColorSelector);
			selectorParent.Add(swapSelectorsColorsButton);
			selectorParent.Add(rightColorSelector);

			VisualElement gridSizeButtonsParent = new VisualElement()
			{
				name = "grid-controls"
			};
			gridSizeButtonsParent.AddToClassList("abb-tools-view__grid-controls");
			gridSizeButtonsParent.Add(gridSizeField);

			VisualElement blockButtonsParent = new VisualElement()
			{
				name = "block-buttons-parent"
			};
			blockButtonsParent.AddToClassList("abb-tools-view__block-buttons-parent");
			blockButtonsParent.Add(blockXButton);
			blockButtonsParent.Add(blockYButton);

			gridSizeButtonsParent.Add(blockButtonsParent);
			gridSizeButtonsParent.Add(eraserButton);
			gridSizeButtonsParent.Add(resetGridButton);
			gridSizeButtonsParent.Add(fillGridButton);

			leftOptionsElement.Add(selectorParent);
			leftOptionsElement.Add(gridSizeButtonsParent);

			///	Middle
			Add(middleOptionsElement);

			///	Right
			Add(rightOptionsElement);

			VisualElement rightButtonsParent = new()
			{
				name = "file-actions"
			};
			rightButtonsParent.AddToClassList("abb-tools-view__file-actions");

			rightOptionsElement.Add(gridTextLabel);
			rightOptionsElement.Add(rightButtonsParent);

			rightButtonsParent.Add(newGridButton);
			rightButtonsParent.Add(exportGridButton);
		}

		private void AddStyle()
		{
			eraserButton.AddToClassList("abb-tools-view__button");
			resetGridButton.AddToClassList("abb-tools-view__button");
			fillGridButton.AddToClassList("abb-tools-view__button");
			exportGridButton.AddToClassList("abb-tools-view__button");
			newGridButton.AddToClassList("abb-tools-view__button");
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

			xLocked = !session.xLocked;
			yLocked = !session.yLocked;
			XFieldBlockClicked();
			YFieldBlockClicked();

			blockXButton.clicked += XFieldBlockClicked;
			blockYButton.clicked += YFieldBlockClicked;

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

		#region SWAP SELECTORS COLORS BUTTON

		private void SwapSelectorsColors()
		{
			if (terrainColors.terrainColors.Length == 0) return;

			int leftIndex = leftColorSelector.index;
			int rightIndex = rightColorSelector.index;

			leftColorSelector.SetIndex(rightIndex);
			rightColorSelector.SetIndex(leftIndex);
		}

		#endregion

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

		#region GRID SIZE

		private void XFieldBlockClicked()
		{
			xLocked = !xLocked;
			blockXButton.tooltip = xLocked ? "Unlock" : "Lock";
			blockXButton.iconImage = EditorGUIUtility.FindTexture(xLocked ? "LockIcon-On" : "LockIcon");
			gridSizeField.Q("x-field").Q("unity-text-input").SetEnabled(!xLocked);

			session.xLocked = xLocked;
		}
		private void YFieldBlockClicked()
		{
			yLocked = !yLocked;
			blockYButton.tooltip = yLocked ? "Unlock" : "Lock";
			blockYButton.iconImage = EditorGUIUtility.FindTexture(yLocked ? "LockIcon-On" : "LockIcon");
			gridSizeField.Q("y-field").Q("unity-text-input").SetEnabled(!yLocked);

			session.yLocked = yLocked;
		}

		#endregion

		#region COLOR SELECTORS

		private void LeftSelectorValueChanged(int value) => session.leftColor = value;
		private void RightSelectorValueChanged(int value) => session.rightColor = value;

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
			{
				session.leftColor = session.rightColor = -1;
				rightColorSelector.SetIndex(0);
			}

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
			{
				session.leftColor = session.rightColor = -1;
				leftColorSelector.SetIndex(0);
			}

			OnRemoveTerrain?.Invoke(index);
		}

		#endregion
	}
}
