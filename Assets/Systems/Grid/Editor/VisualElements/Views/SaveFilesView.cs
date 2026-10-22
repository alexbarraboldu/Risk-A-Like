using System;
using System.Linq;

using ABB.Grid.Editor.Data;

using UnityEditor;

using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class SaveFilesView : VisualElement
	{
		private GridData[] grids;

		private ListView listView = null;

		public event Action<GridData, int> OnGridDataSelected;


		public SaveFilesView(ref GridData[] gridsData)
		{
			this.grids = gridsData;

			listView = new();

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
				VisualElement parent = new();
				parent.style.flexDirection = FlexDirection.Row;
				parent.style.alignItems = Align.Center;
				parent.style.paddingLeft = 18;

				Image icon = new() { name = "icon" };
				icon.style.width = 16;
				icon.style.height = 16;
				icon.style.flexShrink = 0;

				Label label = new() { name = "label" };

				parent.Add(icon);
				parent.Add(label);

				return parent;
			};
			listView.bindItem = (element, index) =>
			{
				var gridSave = this.grids[index];

				Image icon = element.Q<Image>("icon");
				icon.image = EditorGUIUtility.IconContent("DefaultAsset Icon").image;

				element.name = gridSave.fileName;
				element.Q<Label>("label").text = gridSave.fileName;
			};

			listView.itemsSource = gridsData;
			listView.RefreshItems();
		}


		public void LoadGridData(out GridData gridSave, out int index)
		{
			int selectedIndex = 0;

			gridSave = grids[selectedIndex];
			index = selectedIndex;
		}

		public void SetEntries(GridData[] grids)
		{
			this.grids = grids;
			listView.itemsSource = grids;
			listView.RefreshItems();
		}
	}
}
