using System;

using ABB.Grid.Editor.Data;

using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class SaveFilesTab : Tab
	{
		private SaveFilesView _saveFilesView = null;


		public event Action<GridData, int> OnGridLoaded
		{
			add => _saveFilesView.OnGridLoaded += value;
			remove => _saveFilesView.OnGridLoaded -= value;
		}

		public event Action<int> OnGridDeleted
		{
			add => _saveFilesView.OnGridDeleted += value;
			remove => _saveFilesView.OnGridDeleted -= value;
		}


		public SaveFilesTab(string label, GridData[] gridSaves, SessionData sessionData)
		{
			base.label = label;
			closeable = false;

			_saveFilesView = new(gridSaves, sessionData);

			Add(_saveFilesView);
		}

		internal void SetEntries(GridData[] grids)
		{
			_saveFilesView.SetEntries(grids);
		}
	}
}
