using System;

using ABB.Grid.Editor.Data;

using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class SaveFilesTab : Tab, IDisposable
	{
		private SaveFilesView _saveFilesView = null;


		public event Action<GridData, int> OnGridDataSelected
		{
			add => _saveFilesView.OnGridDataSelected += value;
			remove => _saveFilesView.OnGridDataSelected += value;
		}

		public GridData[] GridsData => _saveFilesView.gridsData;


		public SaveFilesTab(string label)
		{
			base.label = label;
			closeable = false;

			_saveFilesView = new();

			Add(_saveFilesView);
		}

		internal void AddEntry(GridData data, int index)
		{
			_saveFilesView.AddEntry(data, index);
		}

		public void Dispose()
		{
			_saveFilesView.Dispose();
		}
	}
}
