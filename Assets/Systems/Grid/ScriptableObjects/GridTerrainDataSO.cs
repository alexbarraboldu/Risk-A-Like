using UnityEngine;

namespace ABB.Grid.Data
{
	public class GridTerrainDataSO : ScriptableObject
	{
		[SerializeField] private int columns;
		[SerializeField] private int rows;
		[SerializeField] private int[] inlineGrid;

		[System.NonSerialized] private int[][] lookup;

		public int Width => columns;
		public int Height => rows;
		public int[][] terrainData
		{
			get
			{
				if (lookup == null)
				{
					SetLookUp();
				}

				return lookup;
			}

			set
			{
				columns = value.Length;
				rows = value[0].Length;
				int length = columns * rows;

				inlineGrid = new int[length];

				int c = 0, r = 0, i = 0;
				for (i = 0; i < length;)
				{
					inlineGrid[i] = value[c][r];

					i++;
					r++;

					if (1 == i / (rows * (c + 1)))
					{
						r = 0;
						c++;
					}
				}
			}
		}


		public GridTerrainDataSO() { }

		public GridTerrainDataSO(int[][] grid)
		{
			this.terrainData = grid;
		}

		private void SetLookUp()
		{
			int[][] result = new int[columns][];

			int i = 0;
			for (int c = 0; c < columns; c++)
			{
				result[c] = new int[rows];
				for (int r = 0; r < rows; r++)
				{
					result[c][r] = inlineGrid[i];
					i++;
				}
			}

			lookup = result;
		}
	}
}
