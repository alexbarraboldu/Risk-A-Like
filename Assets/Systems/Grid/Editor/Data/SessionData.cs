using System;

using UnityEngine;

namespace ABB.Grid.Editor.Data
{
	[Serializable]
	public class SessionData
	{
		public uint columns;
		public uint rows;

		public int[][] grid;

		public float cellSize;


		public SessionData() { }

		public SessionData(SessionData sessionFile)
		{
			columns = sessionFile.columns;
			rows = sessionFile.rows;
			grid = sessionFile.grid;
		}

		public SessionData(Vector2Int gridSize, int[][] grid, float cellSize, string cellGameObjectId)
		{
			columns = (uint)gridSize.x;
			rows = (uint)gridSize.y;
			this.grid = grid;
			this.cellSize = cellSize;
		}
	}
}
