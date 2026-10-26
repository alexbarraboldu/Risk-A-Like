using System;

namespace ABB.Grid.Editor.Data
{
	[Serializable]
	public class GridData
	{
		public int columns = 0;
		public int rows = 0;

		public int[][] grid = Array.Empty<int[]>();

		public string fileName = string.Empty;
	}
}
