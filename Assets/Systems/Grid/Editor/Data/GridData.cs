using System;

namespace ABB.Grid.Editor.Data
{
	[Serializable]
	public class GridData
	{
		public int columns;
		public int rows;

		public int[][] grid;

		public string fileName;
	}
}
