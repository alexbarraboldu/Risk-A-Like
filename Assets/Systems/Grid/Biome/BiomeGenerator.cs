using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ABB.Cell;
using ABB.Gameplay.Data.Types;

using UnityEditor;

using UnityEngine;

using Random = System.Random;


namespace ABB.Grid.Biome
{
	[RequireComponent(typeof(GridGenerator))]
	public class BiomeGenerator : MonoBehaviour
	{
		HexagonCell[][] _grid = null;


		[SerializeField] private BiomesLibrary terrainLibrary;


		[Header("Demonstration")]
		[SerializeField, Tooltip("in milliseconds")]
		private int _cellsPerFrame = 10;


		public void Awake()
		{
			GridGenerator gridGenerator = GetComponent<GridGenerator>();
			gridGenerator.OnGridGenerated += SetGridAndRegenerateBiomes;

			lakeRandom = new Random();
		}


		[ContextMenu("Regenerate Biomes")]
		private void RegenerateBiomes()
		{
			ResetBiomes();
			GenerateBiomes();
		}

		private void SetGridAndRegenerateBiomes(HexagonCell[][] grid)
		{
			if (grid != null) _grid = grid;

			ResetBiomes();
			GenerateBiomes();
		}

		private void ResetBiomes()
		{
			foreach (var rows in _grid)
			{
				foreach (var cell in rows)
				{
					cell.SetBiome(terrainLibrary.BiomesByTerrain[TerrainType.PLAIN]);
				}
			}
		}

		public async void GenerateBiomes()
		{
			TerrainType[] terrains = terrainLibrary.BiomesByTerrain.Keys.ToArray();
			foreach (TerrainType terrain in terrains)
			{
				switch (terrain)
				{
					case TerrainType.PLAIN:
						GeneratePlain();
						break;
					case TerrainType.PLATEAU:
						break;
					case TerrainType.MOUNTAIN:
						await GenerateMountain();
						break;
					case TerrainType.VALLEY:
						break;
					case TerrainType.CANYON:
						break;
					case TerrainType.PLAYA:
						break;
					case TerrainType.DESSERT:
						break;
					case TerrainType.WETLAND:
						break;
					case TerrainType.LAKE:
						await GenerateLake();
						break;
					case TerrainType.SEA:
						break;
					default:
						break;
				}
			}
		}

		private void GeneratePlain()
		{
			for (int i = 0; i < _grid.Length; i++)
			{
				for (int j = 0; j < _grid[i].Length; j++)
				{
					_grid[i][j].SetBiome(terrainLibrary.BiomesByTerrain[TerrainType.PLAIN]);
				}
			}
		}

		private async Task GenerateMountain()
		{
			var random = new Random();

			Vector2Int[] mountainCounts = { new(1, 6), new(6, 12), new(12, 24) };
			int mountainCountsIndex = 2;

			int mountainCount = random.Next(mountainCounts[mountainCountsIndex].x, mountainCounts[mountainCountsIndex].y);

			int rowCount = _grid.Length;
			int columnCount = _grid.Max(x => x.Length);

			for (int m = 0; m < mountainCount; m++)
			{
				Vector2Int startPos = new(random.Next(0, columnCount), random.Next(0, rowCount));

				Vector2Int[] mountainLengthes = { new(3, 7), new(7, 14), new(14, 28) };
				int mountainLengthesIndex = 2;

				int mountainLength = random.Next(mountainLengthes[mountainLengthesIndex].x, mountainLengthes[mountainLengthesIndex].y);

				//int xDirection = random.Next(0, 2) == 0 ? -1 : 1;
				//int yDirection = random.Next(0, 2) == 0 ? -1 : 1;

				///	Generate parabola last point & distance
				Vector2Int currentPos = startPos;
				Vector2Int[] parabolaPoints = GenerateParabolaPoints(rowCount, columnCount, startPos, mountainLength);
				///----------------------------

				for (int i = 0; i < mountainLength - 1; i++)
				{
					Vector2 direction = ((Vector2)(parabolaPoints[i + 1] - parabolaPoints[i])).normalized;
					Vector2Int nextCellPos = GetNeighbourFromDirection(startPos, direction);
					currentPos += nextCellPos;

					//float localX = i * xDirection;

					//float x = startPos.x + localX;
					//float y = startPos.y + yDirection * (localX * localX) / (4f * p);

					//int column = Mathf.RoundToInt(x);
					//int row = Mathf.RoundToInt(y);

					int column = currentPos.x;
					int row = currentPos.y;

					if ((row < 0 || row >= rowCount) ||
						(column < 0 || column >= _grid[row].Length))
						break;

					_grid[row][column].SetBiome(terrainLibrary.BiomesByTerrain[TerrainType.MOUNTAIN]);
				}

				if (m % _cellsPerFrame == 0)
					await Task.Yield();
			}
		}

		private Vector2Int GetNeighbourFromDirection(Vector2Int cellPosition, Vector2 direction)
		{
			int x = Mathf.RoundToInt(direction.x);
			int y = Mathf.RoundToInt(direction.y);

			return new Vector2Int(x, y);
		}

		private Vector2Int[] GenerateParabolaPoints(int rowCount, int columnCount, Vector2Int startPos, int mountainLength)
		{
			var random = new Random();

			Vector2Int[] result = new Vector2Int[mountainLength];

			int xDirection = random.Next(0, 2) == 0 ? -1 : 1;
			int yDirection = random.Next(0, 2) == 0 ? -1 : 1; ;

			float p = random.Next(1, 5);

			for (int i = 0; i < mountainLength; i++)
			{
				float localX = i * xDirection;

				float x = startPos.x + localX;
				float y = startPos.y + yDirection * (localX * localX) / (4f * p);

				int column = Mathf.RoundToInt(x);
				int row = Mathf.RoundToInt(y);

				if ((row < 0 || row >= rowCount) ||
					(column < 0 || column >= columnCount))
					break;

				result[i] = new Vector2Int(row, column);
			}

			return result;
		}


		Random lakeRandom = null;
		private async Task GenerateLake()
		{
			var random = lakeRandom;

			HashSet<Vector2Int> visitedCells = new();

			int lakeCount = random.Next(2, 8);
			for (int l = 0; l < lakeCount; l++)
			{
				int x = random.Next(0, _grid.Length);
				Vector2Int startPos = new(x, random.Next(0, _grid[x].Length));

				TerrainType terrain = _grid[startPos.x][startPos.y].Data.TerrainType;
				if (terrain == TerrainType.PLAIN && !visitedCells.Contains(startPos))
				{
					visitedCells.Add(startPos);
					_grid[startPos.x][startPos.y].SetBiome(terrainLibrary.BiomesByTerrain[TerrainType.LAKE]);

					int lakeMaxSize = 21;
					int lakeSize = random.Next(0, lakeMaxSize);
					List<Vector2Int> neighbours = new(lakeMaxSize);

					int maxNeighbours = Mathf.CeilToInt((float)lakeSize / 6f);
					for (int i = 0; i < maxNeighbours; i++)
					{
						neighbours.AddRange(GetNeighbours(startPos));
						startPos = neighbours[random.Next(0, neighbours.Count)];
					}
					neighbours.RemoveRange(lakeSize, neighbours.Count - lakeSize);
					neighbours.TrimExcess();

					for (int i = 0; i < lakeSize; i++)
					{
						Vector2Int currentCell = neighbours[i];

						if (currentCell == new Vector2Int(-1, -1)) continue;
						if (visitedCells.Contains(currentCell)) continue;

						_grid[currentCell.x][currentCell.y].SetBiome(terrainLibrary.BiomesByTerrain[TerrainType.LAKE]);
						visitedCells.Add(currentCell);
					}

					if (l % _cellsPerFrame == 0)
						await Task.Yield();
				}
				else
				{
					l--;
					continue;
				}
			}

			Material green = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Green.mat");
			Biome grassLand = new(TerrainType.WETLAND, green);

			foreach (Vector2Int cell in visitedCells)
			{
				Vector2Int[] neighbours = GetNeighbours(cell);

				int n = 0;
				foreach (Vector2Int neighbour in neighbours)
				{
					if (neighbour.x == -1) continue;

					if (_grid[neighbour.x][neighbour.y].Data.TerrainType != TerrainType.PLAIN) continue;

					_grid[neighbour.x][neighbour.y].SetBiome(grassLand);

					if (n % _cellsPerFrame == 0)
						await Task.Yield();

					n++;
				}
			}
		}

		private Vector2Int[] GetNeighbours(Vector2Int cell)
		{
			Vector2Int[] EvenColumnNeighbourOffsets = {
				new(0, 1),		///	BOTTOM-RIGHT
				new(-1, 0),		///	BOTTOM
				new(0, -1),		///	BOTTOM-LEFT
				new(1, -1),		///	TOP-LEFT
				new(1, 0),		///	TOP
				new(1, 1),		///	TOP-RIGHT
			};
			Vector2Int[] OddColumnNeighbourOffsets = {
				new(-1, 1),		///	BOTTOM-RIGHT
				new(-1, 0),		///	BOTTOM
				new(-1, -1),	///	BOTTOM-LEFT
				new(0, -1),		///	TOP-LEFT
				new(1, 0),		///	TOP
				new(0, 1),		///	TOP-RIGHT
			};

			Vector2Int[] offsets = (cell.y & 1) == 0 ? EvenColumnNeighbourOffsets : OddColumnNeighbourOffsets;

			Vector2Int[] result = new Vector2Int[6] { new(-1, -1), new(-1, -1), new(-1, -1), new(-1, -1), new(-1, -1), new(-1, -1) };

			for (int i = 0; i < offsets.Length; i++)
			{
				Vector2Int neighbour = cell + offsets[i];

				int row = neighbour.x;
				int column = neighbour.y;

				if (row < 0 || row >= _grid.Length) continue;

				if (column < 0 || column >= _grid[row].Length) continue;

				result[i] = neighbour;
			}

			return result;
		}
	}
}
