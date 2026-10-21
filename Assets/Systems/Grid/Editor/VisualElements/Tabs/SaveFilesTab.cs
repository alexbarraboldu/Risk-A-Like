using System;

using ABB.Grid.Editor.Data;

using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class SaveFilesTab : Tab, IDisposable
	{
		private SaveFilesView _saveFilesView = null;

		public event Action<GridData> OnGridDataSelected
		{
			add => _saveFilesView.OnSelected += value;
			remove => _saveFilesView.OnSelected += value;
		}


		public SaveFilesTab(string label)
		{
			base.label = label;
			closeable = false;

			_saveFilesView = new();

			Add(_saveFilesView);
		}

		internal void SaveGridData(GridData data)
		{
			_saveFilesView.SaveGridData(data, -1);
		}

		public void Dispose()
		{
			_saveFilesView.Dispose();
		}
	}
}
