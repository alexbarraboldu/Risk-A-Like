using System.Collections.Generic;
using System.Threading.Tasks;

using ABB.Cell;

using UnityEditor;

using UnityEngine;

namespace ABB.Grid
{
	public class GridGenerator : MonoBehaviour
	{
		[SerializeField] HexagonCell _cellPrefab;

		[SerializeField] private int _totalCells = 0;

		[SerializeField] private int _gridHeight = 0;
		[SerializeField] private int _gridWidth = 0;

		private HexagonCell[][] _grid = null;
		public HexagonCell[][] Grid => _grid;


		[Header("Demonstration")]
		[SerializeField, Tooltip("in milliseconds")]
		private int _delayTime = 10;


		[ContextMenu("Generate Grid")]
		public async void GenerateGrid()
		{
			ResetGrid();

			_totalCells = _gridHeight * _gridWidth;

			InstantiateParameters instantiateParams = new();
			instantiateParams.parent = transform;
			instantiateParams.worldSpace = true;

			Vector3 lastPosition = Vector3.zero;

			///	Generate cell mesh
			Mesh cellMesh = ABB.Polygons.C_Hexagon.GeneratePrimitive();

			///	Generate Grid Cells
			_grid = new HexagonCell[_gridHeight][];
			for (int i = 0; i < _gridHeight; i++)
			{
				_grid[i] = new HexagonCell[_gridWidth];
				for (int j = 0; j < _gridWidth; j++)
				{
					HexagonCell gridCell = Instantiate(_cellPrefab, instantiateParams);
					gridCell.AssignMesh(cellMesh);
					gridCell.AssignName(i, j);
					gridCell.AssignPosition(i, j);

					float y = (j % 2 == 0) ? 0.4330127f : -0.4330127f;
					gridCell.transform.position = lastPosition + new Vector3(0.75f, y, 0f);

					lastPosition = gridCell.transform.position;

					_grid[i][j] = gridCell;

					await Task.Delay(_delayTime);
				}

				lastPosition.y += 0.4330127f * 2f;
				lastPosition.x = 0f;
			}
		}

		public async void GenerateGridFromIntersectionsCoordinates(Dictionary<Vector3, Vector3> intersectionsCoordinates)
		{
			ResetGrid();

			InstantiateParameters instantiateParams = new();
			instantiateParams.parent = transform;
			instantiateParams.worldSpace = true;

			///	Generate cell mesh
			Mesh cellMesh = ABB.Polygons.C_Hexagon.GeneratePrimitive();

			const float xSpacing = 0.75f;
			const float yOffset = 0.4330127f;
			const float ySpacing = yOffset * 2f;

			///	Generate Grid Cells
			var intersectionEnumerator = intersectionsCoordinates.GetEnumerator();

			_gridHeight = intersectionsCoordinates.Count;
			_grid = new HexagonCell[_gridHeight][];
			for (int i = 0; i < _gridHeight; i++)
			{
				intersectionEnumerator.MoveNext();
				var currentPair = intersectionEnumerator.Current;

				int startColumn = Mathf.CeilToInt(currentPair.Key.x / xSpacing);
				int endColumn = Mathf.FloorToInt(currentPair.Value.x / xSpacing);

				int gridWidth = endColumn - startColumn + 1;

				_totalCells += _gridWidth;

				int row = Mathf.RoundToInt(currentPair.Key.y / ySpacing);
				float rowY = row * ySpacing;

				_grid[i] = new HexagonCell[gridWidth];
				for (int j = 0; j < gridWidth; j++)
				{
					HexagonCell gridCell = Instantiate(_cellPrefab, instantiateParams);
					gridCell.AssignMesh(cellMesh);

					int column = startColumn + j;

					float x = column * xSpacing;
					float y = rowY + ((column & 1) != 0 ? yOffset : 0f);

					gridCell.transform.position = new Vector3(x, y, 0f);
					gridCell.AssignName(row, column);
					gridCell.AssignPosition(row, column);

					_grid[i][j] = gridCell;

					await Task.Delay(_delayTime);
				}
			}
		}


		[ContextMenu("Generate Grid", true)]
		private bool ValidateGenerateGrid()
		{
			return _gridHeight != 0 && _gridWidth != 0;
		}

		private void ResetGrid()
		{
			Transform[] cells = transform.GetComponentsInChildren<Transform>(true);
			ArrayUtility.Remove(ref cells, transform);

			for (int i = 0; i < cells.Length; i++)
			{
				if (cells[i] != null)
					DestroyImmediate(cells[i].gameObject);
			}
		}


		public Vector2 GetCellSize()
		{
			if (_grid != null)
				return _grid[0][0]?.GetSize() ?? Vector2.negativeInfinity;
			else
				return _cellPrefab?.GetSize() ?? Vector2.negativeInfinity;
		}


		private void Reset()
		{
			ResetGrid();

#if UNITY_EDITOR
			GetComponent<Default_GridGenerator>()?.OnReset();

			_totalCells = _gridHeight * _gridWidth;
#endif
		}

#if UNITY_EDITOR
		public int GridHeight
		{
			get => _gridHeight;
			set => _gridHeight = value;
		}

		public int GridWidth
		{
			get => _gridWidth;
			set => _gridWidth = value;
		}

		public int DelayTime
		{
			get => _delayTime;
			set => _delayTime = value;
		}

		public HexagonCell CellPrefab
		{
			get => _cellPrefab;
			set => _cellPrefab = value;
		}
#endif
	}
}
