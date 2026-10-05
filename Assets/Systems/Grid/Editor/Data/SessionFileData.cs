using System;

using UnityEngine;

namespace ABB.Grid.Editor.Data
{
	[Serializable]
	public class SessionFileData
	{
		public uint columns;
		public uint rows;

		public int[][] grid;

		public float cellSize;

		public string cellGameObjectId;


		public SessionFileData() { }

		public SessionFileData(SessionFileData sessionFile)
		{
			this.columns = sessionFile.columns;
			this.rows = sessionFile.rows;
			this.grid = sessionFile.grid;
			this.cellGameObjectId = sessionFile.cellGameObjectId;
		}

		public SessionFileData(Vector2Int gridSize, int[][] grid, float cellSize, string cellGameObjectId)
		{
			this.columns = (uint)gridSize.x;
			this.rows = (uint)gridSize.y;
			this.grid = grid;
			this.cellSize = cellSize;
			this.cellGameObjectId = cellGameObjectId;
		}
	}
}
