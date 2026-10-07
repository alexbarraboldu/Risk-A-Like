using System;
using System.Collections.Generic;

using ABB.Gameplay.Data.Types;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class ToolsView : VisualElement
	{
		private Vector2IntField gridSizeElement = null;

		private Button resetGridButton = null;
		private Button fillGridButton = null;

		private Button exportGridButton = null;

		private TerrainType selectedTerrain = 0;

		public Vector2Int GridSize
		{
			get => gridSizeElement.Value;
			set => gridSizeElement.Value = value;
		}

		public event Action<int> OnTerrainChanged;
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
			var terrainTypeSelector = CreateTerrainTypeSelector();
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

			leftOptionsElement.Add(terrainTypeSelector);
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


		private DropdownField CreateTerrainTypeSelector()
		{
			DropdownField layerField = new DropdownField("Terrain type")
			{
				style =
				{
					paddingTop = 4
				}
			};

			List<string> choices = new();

			Type terrainType = typeof(TerrainType);
			foreach (var terrain in Enum.GetValues(terrainType))
			{
				string terrainStr = terrain.ToString();
				choices.Add(terrainStr);
			}

			layerField.choices = choices;
			layerField.index = 0;
			layerField.RegisterValueChangedCallback(evt =>
			{
				selectedTerrain = (TerrainType)Enum.Parse(terrainType, evt.newValue);
				OnTerrainChanged?.Invoke((int)selectedTerrain);
				Debug.Log(layerField.value);
			});

			return layerField;
		}
	}
}
