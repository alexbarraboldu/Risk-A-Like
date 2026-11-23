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
		private static readonly GUID StyleSheetGuid = new("cc31d2ce52787024ab4c53a83a1ee223");
		[SerializeField] public StyleSheet saveFileEntryUSS;

		private GridData[] grids;

		private ListView listView = null;

		public event Action<GridData, int> OnGridDataSelected;
		public event Action<int> OnGridDataDeleted;


		public SaveFilesView(GridData[] gridsData)
		{
			if (saveFileEntryUSS == null) saveFileEntryUSS = AssetDatabase.LoadAssetByGUID<StyleSheet>(StyleSheetGuid);

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
				SaveFileEntry saveFileEntry = new(saveFileEntryUSS);
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

				Image icon = element.Q<Image>(className: SaveFileEntry.IconClass);
				icon.image = EditorGUIUtility.IconContent("DefaultAsset Icon").image;

				element.Q<Label>(className: SaveFileEntry.FileNameClass).text = gridSave.fileName;
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

			internal const string IconClass = "abb-save-file-entry__icon";
			internal const string FileNameClass = "abb-save-file-entry__file-name";

			public event Action OnDeleteClicked;

			public SaveFileEntry(StyleSheet uss)
			{
				if (uss != null) styleSheets.Add(uss);

				name = "save-file-entry";
				AddToClassList("abb-save-file-entry");

				icon = new()
				{
					name = "icon"
				};
				icon.AddToClassList(IconClass);
				label = new()
				{
					name = "label"
				};
				label.AddToClassList(FileNameClass);

				VisualElement spacer = new();
				spacer.style.flexGrow = 1;

				deleteButton = new() { tooltip = "Delete grid", iconImage = EditorGUIUtility.FindTexture("TreeEditor.Trash") };
				deleteButton.AddToClassList("abb-save-file-entry__delete-button");
				deleteButton.clicked += () => OnDeleteClicked?.Invoke();

				Add(icon);
				Add(label);
				Add(spacer);
				Add(deleteButton);
			}
		}
	}
}
