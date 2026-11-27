using System;

using UnityEngine;

namespace ABB.Grid.Editor.Data
{
	[Serializable]
	public class SessionData
	{
		public int currentTab = 0;
		public int saveFileIndex = -1;

		public float cellSize = 10f;

		public Vector2 gridPosition = Vector2.zero;
		public bool hasGridPosition = false;

		public bool canPaint => leftColor >= 0 && rightColor >= 0;
		public int leftColor = 0;
		public int rightColor = 0;

		public bool xLocked = false;
		public bool yLocked = false;
	}
}
