using System;

using ABB.Gameplay.Data.Types;
using ABB.Grid.Editor.Data;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class ToolsView : VisualElement
	{
		private SessionData session;
		private TerrainColorData terrainColors;

		private ColorSelectorDropdown<TerrainType> leftTerrainTypeSelector = null;
		private ColorSelectorDropdown<TerrainType> rightTerrainTypeSelector = null;

		private Vector2IntField gridSizeElement = null;

		private Button deleteCellButton = null;

		private Button resetGridButton = null;
		private Button fillGridButton = null;

		private Button exportGridButton = null;
		private Button saveGridButton = null;


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
			add => gridSizeElement.ValueChanged += value;
			remove => gridSizeElement.ValueChanged -= value;
		}
		public event Action OnDeleteCell
		{
			add => deleteCellButton.clicked += value;
			remove => deleteCellButton.clicked -= value;
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
		public event Action OnSaveGrid
		{
			add => saveGridButton.clicked += value;
			remove => saveGridButton.clicked -= value;
		}


		public ToolsView(ref SessionData session, ref TerrainColorData terrainColors)
		{
			BuildUI();
			AddStyle();
			AddLogic();

			///	INJECT DATA
			this.session = session;
			this.terrainColors = terrainColors;

			leftTerrainTypeSelector.SetColorsPalette(this.terrainColors.terrainColors);
			rightTerrainTypeSelector.SetColorsPalette(this.terrainColors.terrainColors);

			leftTerrainTypeSelector.Value = (int)session.leftColor;
			rightTerrainTypeSelector.Value = (int)session.rightColor;
		}


		private void BuildUI()
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

			gridSizeElement = new Vector2IntField("Grid Size", "Columns", "Rows", false);
			gridSizeElement.Q<Label>("vector2int-field__label").style.display = DisplayStyle.None;
			gridSizeElement.Q("vector2int-field").style.flexGrow = 1;

			VisualElement visualElement = new();
			gridSizeElement.Q("vector2int-field__fields-xField").style.width = 118;
			gridSizeElement.Q("vector2int-field__fields-yField").style.width = 118;

			VisualElement middleOptionsElement = new()
			{
				name = "tools-view--middle-options",
				style =
				{
					flexGrow = 1,
					flexDirection = FlexDirection.Row,
					justifyContent = Justify.Center
				}
			};

			VisualElement rightOptionsElement = new()
			{
				name = "tools-view--right-options",
				style =
				{
					flexDirection = FlexDirection.Row,
					maxWidth = 300
				}
			};


			///	Terrain type selector
			leftTerrainTypeSelector = new ColorSelectorDropdown<TerrainType>();
			rightTerrainTypeSelector = new ColorSelectorDropdown<TerrainType>();
			///----------------------

			///	Delete Cell Button
			deleteCellButton = new()
			{
				name = "tools-view--delete-grid__button",
				tooltip = "Delete grid",
				style =
				{
					minWidth = 80,
					maxWidth = 80,
					minHeight = 35,
					maxHeight = 35,
				}
			};

			Label deleteCellButtonLabel = new("Delete")
			{
				style =
				{
					flexGrow = 1,
					fontSize = 22
				}
			};

			deleteCellButton.Add(deleteCellButtonLabel);
			///------------------

			///	Reset Grid Button
			resetGridButton = new()
			{
				name = "tools-view--reset-grid__button",
				tooltip = "Reset grid",
				style =
				{
					minWidth = 100,
					maxWidth = 100,
					minHeight = 50,
					maxHeight = 50,
					paddingLeft = 4
				}
			};

			Label resetGridButtonLabel = new("Reset")
			{
				style =
				{
					flexGrow = 1,
					fontSize = 24
				}
			};

			resetGridButton.Add(resetGridButtonLabel);
			///------------------

			///	Fill Grid Button
			fillGridButton = new()
			{
				name = "tools-view--fill-grid__button",
				tooltip = "Fill grid",
				style =
				{
					minWidth = 100,
					maxWidth = 100,
					minHeight = 50,
					maxHeight = 50,
					paddingLeft = 4
				}
			};

			Label fillGridButtonLabel = new("Fill")
			{
				style =
				{
					flexGrow = 1,
					fontSize = 24
				}
			};

			fillGridButton.Add(fillGridButtonLabel);
			///------------------

			///	Export Grid Button
			exportGridButton = new()
			{
				name = "tools-view--export-grid__button",
				tooltip = "Export grid",
				style =
				{
					minWidth = 100,
					maxWidth = 100,
					minHeight = 50,
					maxHeight = 50,
					paddingLeft = 4
				}
			};

			Label exportGridButtonLabel = new("Export")
			{
				name = "tools-view--export-grid__label",
				style =
				{
					flexGrow = 1,
					fontSize = 24
				}
			};

			exportGridButton.Add(exportGridButtonLabel);
			///-------------------

			///	Export Grid Button
			saveGridButton = new()
			{
				name = "tools-view--save-grid__button",
				tooltip = "Save grid",
				style =
				{
					minWidth = 100,
					maxWidth = 100,
					minHeight = 50,
					maxHeight = 50,
					paddingLeft = 4
				}
			};

			Label saveGridButtonLabel = new("Save")
			{
				name = "tools-view--export-grid__label",
				style =
				{
					flexGrow = 1,
					fontSize = 24
				}
			};

			saveGridButton.Add(saveGridButtonLabel);
			///-------------------


			///	ADD TO UI
			Add(rootOptionsElement);

			///	Left
			rootOptionsElement.Add(leftOptionsElement);

			VisualElement rowSelectorParent = new();
			rowSelectorParent.style.flexDirection = FlexDirection.Row;
			rowSelectorParent.style.alignSelf = Align.Center;

			rowSelectorParent.Add(leftTerrainTypeSelector);
			rowSelectorParent.Add(rightTerrainTypeSelector);

			VisualElement rowGridSizeDeleteParent = new VisualElement()
			{
				style =
				{
					paddingTop = 4
				}
			};
			rowGridSizeDeleteParent.style.flexDirection = FlexDirection.Row;
			rowGridSizeDeleteParent.style.alignItems = Align.Center;

			rowGridSizeDeleteParent.Add(gridSizeElement);
			rowGridSizeDeleteParent.Add(deleteCellButton);

			leftOptionsElement.Add(rowSelectorParent);
			leftOptionsElement.Add(rowGridSizeDeleteParent);

			///	Middle
			rootOptionsElement.Add(middleOptionsElement);

			middleOptionsElement.Add(resetGridButton);
			middleOptionsElement.Add(fillGridButton);

			///	Right
			rootOptionsElement.Add(rightOptionsElement);

			rightOptionsElement.Add(exportGridButton);
			rightOptionsElement.Add(saveGridButton);
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

			deleteCellButton.clicked += () =>
			{
				if (deleteCellButton.style.backgroundColor == Color.gray3)
					deleteCellButton.style.backgroundColor = new Color(0.345098f, 0.345098f, 0.345098f, 1f);
				else
					deleteCellButton.style.backgroundColor = Color.gray3;
			};
		}

		public void SetGridSize(Vector2Int size) => gridSizeElement.Value = size;

		private void LeftSelectorValueChanged(int value) => session.leftColor = (uint)value;
		private void RightSelectorValueChanged(int value) => session.rightColor = (uint)value;

		private void LeftSelectorColorChanged(int value, Color color)
		{
			rightTerrainTypeSelector.SetPaletteColor(value, color);
			if (rightTerrainTypeSelector.Value == value)
				rightTerrainTypeSelector.SetColor(value, color);

			TerrainColor newTerrainColor = new((TerrainType)value, color);
			terrainColors.terrainColors[value] = newTerrainColor;
		}
		private void RightSelectorColorChanged(int value, Color color)
		{
			leftTerrainTypeSelector.SetPaletteColor(value, color);
			if (leftTerrainTypeSelector.Value == value)
				leftTerrainTypeSelector.SetColor(value, color);

			TerrainColor newTerrainColor = new((TerrainType)value, color);
			terrainColors.terrainColors[value] = newTerrainColor;
		}
	}
}
