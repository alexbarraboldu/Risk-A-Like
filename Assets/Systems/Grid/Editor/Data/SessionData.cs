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

		public uint leftColor;

		public uint rightColor;


		public SessionData() { }

		public SessionData(SessionData sessionFile)
		{
			columns = sessionFile.columns;
			rows = sessionFile.rows;
			grid = sessionFile.grid;
			leftColor = sessionFile.leftColor;
			rightColor = sessionFile.rightColor;
		}

		public SessionData(Vector2Int gridSize, int[][] grid, float cellSize, string cellGameObjectId, uint leftColor, uint rightColor)
		{
			columns = (uint)gridSize.x;
			rows = (uint)gridSize.y;
			this.grid = grid;
			this.cellSize = cellSize;
			this.leftColor = leftColor;
			this.rightColor = rightColor;
		}
	}
}
