using System;
using System.Threading.Tasks;

using ABB.Cell;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;

namespace ABB.Grid
{
	public abstract class GridGenerator : MonoBehaviour
	{
		[SerializeField] protected HexagonCell _cellPrefab;

		[SerializeField] protected int _totalCells = 0;

		[SerializeField] protected int _gridHeight = 0;
		[SerializeField] protected int _gridWidth = 0;

		protected HexagonCell[][] _grid = null;
		public HexagonCell[][] Grid => _grid;

		[Header("Demonstration")]
		[SerializeField, Tooltip("Cells spawned per frame")]
		protected int _cellsPerFrame = 10;

		[SerializeField, HideInInspector] protected GameObject _gridParent;

		public event Action<HexagonCell[][]> OnGridGenerated;


		protected virtual void InvokeOnGridGenerated()
		{
			OnGridGenerated?.Invoke(_grid);
		}

		protected virtual async Task<bool> GenerateGrid() => false;

		public Vector2 GetCellSize()
		{
			if (_grid != null)
				return _grid[0][0]?.GetSize() ?? Vector2.negativeInfinity;
			else
				return _cellPrefab?.GetSize() ?? Vector2.negativeInfinity;
		}

		protected void ResetGrid()
		{
			Transform[] cells = _gridParent.transform.GetComponentsInChildren<Transform>(true);
			ArrayUtility.Remove(ref cells, _gridParent.transform);

			for (int i = 0; i < cells.Length; i++)
			{
				if (cells[i] != null)
					DestroyImmediate(cells[i].gameObject);
			}
		}


#if UNITY_EDITOR
		private void Reset()
		{
			EditorApplication.delayCall += CreateHelpers;
			EditorApplication.delayCall += ResetGrid;
		}

		private void CreateHelpers()
		{
			if (this == null) return;

			bool createdSomething = false;

			if (_gridParent == null)
			{
				GameObject gridParent = new GameObject("GridParent");
				gridParent.transform.SetParent(transform);
				_gridParent = gridParent;

				createdSomething = true;
			}

			if (createdSomething)
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(gameObject.scene);
			}
		}


		public int CellsPerFrame
		{
			get => _cellsPerFrame;
			set => _cellsPerFrame = value;
		}

		public HexagonCell CellPrefab
		{
			get => _cellPrefab;
			set => _cellPrefab = value;
		}
#endif
	}
}
