using System;
using System.Threading.Tasks;

using ABB.Cell;
using ABB.Gameplay.Data.Types;
using ABB.Grid.Biome;
using ABB.Grid.Data;

using UnityEngine;

namespace ABB.Grid.Squared
{
	public class SquaredGridGenerator : GridGenerator
	{
		[Space(10)]
		[SerializeField] private GridTerrainDataSO gridTerrainData;
		[SerializeField] private BiomesLibrary terrainLibrary;


		[ContextMenu("Generate Grid")]
		protected override async Task<bool> GenerateGrid()
		{
			ResetGrid();

			try
			{
				CheckGridTerrainData();

				await GenerateSquaredGrid();

				return true;
			}
			catch (System.Exception exc)
			{
				Debug.LogException(exc, this);

				return false;
			}
		}

		private void CheckGridTerrainData()
		{
			if (gridTerrainData == null)
			{
				throw new NullReferenceException("gridTerrainData is null");
			}

			if (gridTerrainData.terrainData == null)
			{
				throw new NullReferenceException("gridTerrainData.terrainData is null");
			}

			if (gridTerrainData.Height <= 0 || gridTerrainData.Width <= 0)
			{
				throw new Exception("gridTerrainData's height or width are 0");
			}
		}

		private async Task<bool> GenerateSquaredGrid()
		{
			_gridHeight = gridTerrainData.Height;
			_gridWidth = gridTerrainData.Width;

			_totalCells = _gridHeight * _gridWidth;

			InstantiateParameters instantiateParams = new();
			instantiateParams.parent = _gridParent.transform;
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

					TerrainType terrain = (TerrainType)gridTerrainData.terrainData[j][i];

					Biome.Biome biome = terrainLibrary.BiomesByTerrain.TryGetValue(terrain, out Biome.Biome value) ? value : null;
					if (biome != null)
						gridCell.SetBiome(biome);
					else Debug.LogWarning($"Biome {terrain} not added to the library");

					float y = (j % 2 == 0) ? 0.4330127f : -0.4330127f;
					gridCell.transform.position = lastPosition + new Vector3(0.75f, y, 0f);

					lastPosition = gridCell.transform.position;

					_grid[i][j] = gridCell;

					if (_cellsPerFrame > 0 && j % _cellsPerFrame == 0)
						await Task.Yield();

					if (!Application.isPlaying)
					{
						ResetGrid();
						return false;
					}
				}

				lastPosition.y += 0.4330127f * 2f;
				lastPosition.x = 0f;
			}

			return true;
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
