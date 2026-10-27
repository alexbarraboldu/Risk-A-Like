using System;

using ABB.Grid.Editor.Data;
using ABB.Grid.Editor.Windows;

using UnityEditor;

using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class EditorTab : Tab
	{
		private SessionData session = null;
		private GridData gridData = null;

		private ToolsView _toolsView = null;
		private CanvasView _canvasView = null;

		private event Action<GridData, int> SaveGridData;


		public EditorTab(string label, ref SessionData session, ref TerrainColorData terrainColors, GridData gridData)
		{
			base.label = label;
			closeable = false;

			///	STYLE
			style.flexGrow = 1;
			contentContainer.style.flexGrow = 1;

			this.session = session;
			this.gridData = gridData;

			///	CREATE UI
			_toolsView = new(ref session, ref terrainColors);
			_canvasView = new(ref session, ref terrainColors);

			///	ADD UI
			contentContainer.Add(_toolsView);
			contentContainer.Add(_canvasView);

			///	ADD LOGIC
			_toolsView.OnLeftTerrainChanged += OnLeftTerrainChanged;
			_toolsView.OnRightTerrainChanged += OnRightTerrainChanged;
			_toolsView.OnLeftColorChanged += (value, color) => _canvasView.MarkDirtyRepaint();
			_toolsView.OnRightColorChanged += (value, color) => _canvasView.MarkDirtyRepaint();
			_toolsView.OnGridSizeChanged += _canvasView.SetGridSize;
			_toolsView.OnDeleteCell += _canvasView.DeleteCell;
			_toolsView.OnResetGrid += _canvasView.ResetGrid;
			_toolsView.OnFillGrid += _canvasView.FillGrid;
			_toolsView.OnExportGrid += OnExportGrid;
			_toolsView.OnSaveGrid += OnSaveGrid;

			///	ADD DATA
			_canvasView.SetGridData(gridData);
			_toolsView.SetGridSize(_canvasView.GridSize);
		}


		public void RegisterCallbackSaveGridData(Action<GridData, int> saveGridDataEvent)
		{
			if (saveGridDataEvent == null) return;

			SaveGridData += (x, y) =>
			{
				saveGridDataEvent.Invoke(x, session.saveFileIndex);
			};
		}

		#region ACTIONS

		private void OnExportGrid()
		{
			GridEditorWindow gridEditorWindow = EditorWindow.GetWindow<GridEditorWindow>();
			gridEditorWindow.tabView.SetEnabled(false);

			ExportWindow.OpenWindow(gridData, OnDestroyExportWindow);

			void OnDestroyExportWindow()
			{
				GridEditorWindow gridEditorWindow = EditorWindow.GetWindow<GridEditorWindow>();
				gridEditorWindow.tabView.SetEnabled(true);
			}
		}

		private void OnSaveGrid()
		{
			///	FIRST OPTION
			/// Swap to SaveFiles tab and add an entrance with the name change enabled.
			///	SECOND OPTION
			///	Open SaveWindow and write there the name then save the file.

			///	By now this:
			gridData.fileName = "grid";

			SaveGridData?.Invoke(gridData, session.saveFileIndex);
		}

		private void OnLeftTerrainChanged(int terrain) => session.leftColor = (uint)terrain;

		private void OnRightTerrainChanged(int terrain) => session.rightColor = (uint)terrain;

		#endregion

		public void LoadGridClicked(GridData gridData, int index)
		{
			this.gridData = gridData;
			session.saveFileIndex = index;

			_canvasView.SetGridData(gridData);
			_toolsView.SetGridSize(_canvasView.GridSize);
		}
	}
}
