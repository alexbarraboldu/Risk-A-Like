using System.Threading.Tasks;

using ABB.Cell;

using UnityEngine;

namespace ABB.Grid.Squared
{
	public class SquaredGridGenerator : GridGenerator
	{
		protected override bool GenerateGrid()
		{
			ResetGrid();

			try
			{
				GenerateSquaredGrid();

				return true;
			}
			catch (System.Exception exc)
			{
				Debug.LogException(exc, this);

				return false;
			}
		}

		public async void GenerateSquaredGrid()
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

					if (j % _cellsPerFrame == 0)
						await Task.Yield();
				}

				lastPosition.y += 0.4330127f * 2f;
				lastPosition.x = 0f;
			}
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
#endif
	}
}
