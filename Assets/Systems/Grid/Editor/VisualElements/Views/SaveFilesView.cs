using System;
using System.Linq;

using ABB.Grid.Editor.Data;
using ABB.Grid.Editor.Utils;

using UnityEditor;

using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class SaveFilesView : VisualElement, IDisposable
	{
		private GridData[] gridsData;

		private ListView listView = null;

		public event Action<GridData> OnSelected;


		public SaveFilesView()
		{
			try
			{
				LoadGridsSaveFile();
			}
			catch
			{
				SaveGridSavesFiles();
			}

			listView = new();

			Add(listView);

			listView.itemsChosen += (x) =>
			{
				object obj = x.First();
				GridData gridData = (GridData)obj;

				OnSelected?.Invoke(gridData);
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
				var gridSave = gridsData[index];

				Image icon = element.Q<Image>("icon");
				icon.image = EditorGUIUtility.IconContent("DefaultAsset Icon").image;

				element.name = gridSave.fileName;
				element.Q<Label>("label").text = gridSave.fileName;
			};

			listView.itemsSource = gridsData;
		}


		public void LoadGridData(out GridData gridSave, out int index)
		{
			int selectedIndex = 0;

			gridSave = gridsData[selectedIndex];
			index = selectedIndex;
		}

		public void SaveGridData(GridData gridSave, int index)
		{
			if (index < 0)
			{
				int newLength = gridsData.Length + 1;

				GridData[] aux = gridsData;
				gridsData = new GridData[newLength];

				for (int i = 0; i < aux.Length; i++)
				{
					gridsData[i] = aux[i];
				}
				gridsData[newLength - 1] = gridSave;
			}
			else
			{
				gridsData[index] = gridSave;
			}

			listView.itemsSource = gridsData;
		}


		#region LOAD & SAVE DATA

		private void LoadGridsSaveFile()
		{
			gridsData = Load.LoadGridsDataFile();
		}

		private void SaveGridSavesFiles()
		{
			Save.SaveGridDataFile(gridsData);
		}

		#endregion

		public void Dispose()
		{
			SaveGridSavesFiles();
		}
	}
}
