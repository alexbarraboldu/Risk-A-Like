using System;

using ABB.Grid.Editor.Data;

using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class SaveFilesTab : Tab
	{
		private SaveFilesView _saveFilesView = null;


		public event Action<GridData, int> OnGridDataSelected
		{
			add => _saveFilesView.OnGridDataSelected += value;
			remove => _saveFilesView.OnGridDataSelected -= value;
		}


		public SaveFilesTab(string label, ref GridData[] gridSaves)
		{
			base.label = label;
			closeable = false;

			_saveFilesView = new(ref gridSaves);

			Add(_saveFilesView);
		}

		internal void SetEntries(GridData[] grids)
		{
			_saveFilesView.SetEntries(grids);
		}
	}
}
