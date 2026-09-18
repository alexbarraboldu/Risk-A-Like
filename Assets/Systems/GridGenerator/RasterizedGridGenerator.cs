using System.Collections.Generic;
using System.Threading.Tasks;

using ABB.Cell;

using UnityEngine;

namespace ABB.Grid.Rasterizer
{
	[RequireComponent(typeof(GridImageRasterizer))]
	public class RasterizedGridGenerator : GridGenerator
	{
		private GridImageRasterizer _gridImageRasterizer = null;


		private void Awake()
		{
			_gridImageRasterizer = GetComponent<GridImageRasterizer>();
		}

		private void Start()
		{
			GenerateGrid();
		}


		[ContextMenu("Generate Grid")]
		protected override async Task<bool> GenerateGrid()
		{
			ResetGrid();

			try
			{
				_gridImageRasterizer.cellHeight = GetCellSize().y;
				Dictionary<Vector3, Vector3> intersectionsCoordinates = _gridImageRasterizer.Rasterize();

				await GenerateGridFromIntersectionsCoordinates(intersectionsCoordinates);

				InvokeOnGridGenerated();

				return true;
			}
			catch (System.Exception exc)
			{
				Debug.LogException(exc, this);

				return false;
			}
		}

		private async Task<bool> GenerateGridFromIntersectionsCoordinates(Dictionary<Vector3, Vector3> intersectionsCoordinates)
		{
			ResetGrid();

			InstantiateParameters instantiateParams = new();
			instantiateParams.parent = _gridParent.transform;
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

					if (j % _cellsPerFrame == 0)
						await Task.Yield();
				}
			}

			return true;
		}
	}
}
