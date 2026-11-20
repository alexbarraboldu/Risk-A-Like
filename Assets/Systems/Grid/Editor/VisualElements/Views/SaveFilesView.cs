using System;
using System.Linq;

using ABB.Grid.Editor.Data;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class SaveFilesView : VisualElement
	{
		private GridData[] grids;

		private ListView listView = null;

		public event Action<GridData, int> OnGridDataSelected;
		public event Action<int> OnGridDataDeleted;


		public SaveFilesView(GridData[] gridsData)
		{
			this.grids = gridsData;

			listView = new();
			listView.reorderable = true;

			Add(listView);

			listView.itemsChosen += (x) =>
			{
				object obj = x.First();
				GridData gridData = (GridData)obj;

				int index = listView.itemsSource.IndexOf(obj);

				OnGridDataSelected?.Invoke(gridData, index);
			};
			listView.makeItem = () =>
			{
				SaveFileEntry saveFileEntry = new();
				saveFileEntry.OnDeleteClicked += () =>
				{
					int index = listView.itemsSource.IndexOf(saveFileEntry.userData);
					if (index >= 0)
						OnGridDataDeleted?.Invoke(index);
				};
				return saveFileEntry;
			};
			listView.bindItem = (element, index) =>
			{
				var gridSave = this.grids[index];

				element.userData = gridSave;

				Image icon = element.Q<Image>("icon");
				icon.image = EditorGUIUtility.IconContent("DefaultAsset Icon").image;

				element.name = gridSave.fileName;
				element.Q<Label>("label").text = gridSave.fileName;
			};

			listView.itemsSource = gridsData;
			listView.RefreshItems();
		}

		public void SetEntries(GridData[] grids)
		{
			this.grids = grids;
			listView.itemsSource = grids;
			listView.RefreshItems();
		}


		internal class SaveFileEntry : VisualElement
		{
			private Image icon = null;
			private Label label = null;
			private Button deleteButton = null;

			public event Action OnDeleteClicked;

			public SaveFileEntry()
			{
				style.flexDirection = FlexDirection.Row;
				style.alignItems = Align.Center;
				style.paddingLeft = 18;

				icon = new() { name = "icon" };
				icon.style.width = 16;
				icon.style.height = 16;
				icon.style.flexShrink = 0;

				label = new() { name = "label" };

				VisualElement spacer = new();
				spacer.style.flexGrow = 1;

				deleteButton = new() { name = "delete", tooltip = "Delete grid", iconImage = EditorGUIUtility.FindTexture("TreeEditor.Trash") };
				Color noColor = new Color(0f, 0f, 0f, 0f);
				deleteButton.style.borderBottomColor = noColor;
				deleteButton.style.borderRightColor = noColor;
				deleteButton.style.borderLeftColor = noColor;
				deleteButton.style.borderTopColor = noColor;
				deleteButton.clicked += () => OnDeleteClicked?.Invoke();

				Add(icon);
				Add(label);
				Add(spacer);
				Add(deleteButton);
			}
		}
	}
}
