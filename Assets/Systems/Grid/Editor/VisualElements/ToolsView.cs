using System;

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

		public Vector2Int GridSize
		{
			get => gridSizeElement.Value;
			set => gridSizeElement.Value = value;
		}

		Action<string> OnCellGameObjectIdChanged;


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
			leftOptionsElement.Add(gridSizeElement);
			rootOptionsElement.Add(leftOptionsElement);

			rootOptionsElement.Add(middleOptionsElement);

			rightOptionsElement.Add(resetGridButton);
			rightOptionsElement.Add(fillGridButton);
			rootOptionsElement.Add(rightOptionsElement);

			Add(rootOptionsElement);
		}

		public void AddLogic(Action<string> onCellGameObjectIdChanged, Action<Vector2Int> onGridSizeChanged, Action resetGrid, Action fillGrid)
		{
			OnCellGameObjectIdChanged += onCellGameObjectIdChanged;
			gridSizeElement.ValueChanged += onGridSizeChanged;
			resetGridButton.clicked += resetGrid;
			fillGridButton.clicked += fillGrid;
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
	}
}
