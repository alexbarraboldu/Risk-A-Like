using System;
using System.Collections.Generic;

using ABB.Gameplay.Data.Types;
using ABB.Grid.Editor.Data;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class ToolsView : VisualElement
	{
		private TerrainColorData terrainColorsFile;

		private static ColorSelectorDropdown<TerrainType> leftTerrainTypeSelector = null;
		private static ColorSelectorDropdown<TerrainType> rightTerrainTypeSelector = null;

		private Vector2IntField gridSizeElement = null;

		private Button resetGridButton = null;
		private Button fillGridButton = null;

		private Button exportGridButton = null;

		public static Dictionary<TerrainType, Color> TerrainColors => leftTerrainTypeSelector.TerrainColors;

		public uint LeftColor
		{
			get => (uint)leftTerrainTypeSelector.Value;
			set => leftTerrainTypeSelector.Value = (int)value;
		}
		public uint RightColor
		{
			get => (uint)rightTerrainTypeSelector.Value;
			set => rightTerrainTypeSelector.Value = (int)value;
		}

		public Vector2Int GridSize
		{
			get => gridSizeElement.Value;
			set => gridSizeElement.Value = value;
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


		public ToolsView()
		{
			try
			{
				LoadTerrainColorsFile();
			}
			catch (Exception exception)
			{
				SaveTerrainColorsFile();

				Debug.LogWarning("Could not deserialize terrain colors file data");
				Debug.LogException(exception);
			}


			BuildUI();
			AddStyle();
			AddLogic();
		}


		private void BuildUI()
		{
			VisualElement rootOptionsElement = new()
			{
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
				style =
				{
					flexDirection = FlexDirection.Column,
					maxWidth = 300,
					paddingRight = 4
				}
			};

			gridSizeElement = new Vector2IntField("Grid Size", "Columns", "Rows");

			VisualElement middleOptionsElement = new()
			{
				style =
				{
					flexGrow = 1,
					flexDirection = FlexDirection.Row,
					justifyContent = Justify.Center
				}
			};

			VisualElement rightOptionsElement = new()
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					maxWidth = 300
				}
			};


			///	Terrain type selector
			leftTerrainTypeSelector = new ColorSelectorDropdown<TerrainType>();
			rightTerrainTypeSelector = new ColorSelectorDropdown<TerrainType>();

			leftTerrainTypeSelector.SetColors(terrainColorsFile.terrainColors);
			rightTerrainTypeSelector.SetColors(terrainColorsFile.terrainColors);

			leftTerrainTypeSelector.OnColorChanged += (value, color) => rightTerrainTypeSelector.ChangeColor(value, color);

			rightTerrainTypeSelector.OnColorChanged += (value, color) => leftTerrainTypeSelector.ChangeColor(value, color);
			///----------------------

			///	Reset Grid Button
			resetGridButton = new()
			{
				name = "reset-grid",
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
				name = "fill-grid",
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
				name = "export-grid",
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
				style =
				{
					flexGrow = 1,
					fontSize = 24
				}
			};

			exportGridButton.Add(exportGridButtonLabel);
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

			leftOptionsElement.Add(rowSelectorParent);
			leftOptionsElement.Add(gridSizeElement);

			///	Middle
			rootOptionsElement.Add(middleOptionsElement);

			middleOptionsElement.Add(resetGridButton);
			middleOptionsElement.Add(fillGridButton);

			///	Right
			rootOptionsElement.Add(rightOptionsElement);

			rightOptionsElement.Add(exportGridButton);
		}

		private void AddStyle()
		{

		}

		private void AddLogic()
		{

		}


		#region LOAD & SAVE DATA

		public void LoadTerrainColorsFile()
		{
			terrainColorsFile = Utils.Load.LoadTerrainColorsFile();
		}

		public void SaveTerrainColorsFile()
		{
			TerrainColor[] terrainColors = new TerrainColor[leftTerrainTypeSelector.Length];
			int i = 0;
			foreach (var terrainColor in leftTerrainTypeSelector.TerrainColors)
			{
				terrainColors[i].terrain = (int)terrainColor.Key;

				var value = terrainColor.Value;
				terrainColors[i].r = value.r;
				terrainColors[i].g = value.g;
				terrainColors[i].b = value.b;
				terrainColors[i].a = value.a;

				i++;
			}
			terrainColorsFile.terrainColors = terrainColors;

			Utils.Save.SaveTerrainColorsFile(terrainColorsFile);
		}

		#endregion
	}
}
