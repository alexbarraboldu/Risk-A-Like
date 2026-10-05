using System;
using System.Collections.Generic;

using ABB.Gameplay.Data.Types;

using UnityEditor.Search;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class ToolsView : VisualElement
	{
		private ObjectField cellGameObjectField = null;
		private Vector2IntElement gridSizeElement = null;
		private Button resetGridButton = null;
		private Button fillGridButton = null;

		private TerrainType selectedTerrain = 0;

		public Vector2Int GridSize
		{
			get => gridSizeElement.Value;
			set => gridSizeElement.Value = value;
		}

		public event Action<string> OnCellGameObjectIdChanged;
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


		public ToolsView()
		{
			BuildUI();
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
					backgroundColor = Color.gray,
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

			cellGameObjectField = CreateCellGameObjectField();
			cellGameObjectField.style.maxWidth = 240;

			gridSizeElement = new Vector2IntElement("Grid Size", "Columns", "Rows");

			VisualElement middleOptionsElement = new()
			{
				style =
				{
					flexGrow = 1
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


			///	ADD TO UI
			leftOptionsElement.Add(cellGameObjectField);

			var terrainTypeSelector = CreateTerrainTypeSelector();
			leftOptionsElement.Add(terrainTypeSelector);

			leftOptionsElement.Add(gridSizeElement);
			rootOptionsElement.Add(leftOptionsElement);

			rootOptionsElement.Add(middleOptionsElement);

			rightOptionsElement.Add(resetGridButton);
			rightOptionsElement.Add(fillGridButton);
			rootOptionsElement.Add(rightOptionsElement);

			Add(rootOptionsElement);
		}

		private ObjectField CreateCellGameObjectField()
		{
			ObjectField gameObjectField = new ObjectField("Cell")
			{
				objectType = typeof(GameObject)
			};
			gameObjectField.RegisterValueChangedCallback(OnCellGameObjectChanged);

			return gameObjectField;
		}

		private void OnCellGameObjectChanged(ChangeEvent<UnityEngine.Object> evt)
		{
			string cellGameObjectId = (evt.newValue as GameObject).GetInstanceID().ToString();
			OnCellGameObjectIdChanged?.Invoke(cellGameObjectId);
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
