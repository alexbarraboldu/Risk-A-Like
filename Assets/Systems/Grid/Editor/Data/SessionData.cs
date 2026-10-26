using System;

namespace ABB.Grid.Editor.Data
{
	[Serializable]
	public class SessionData
	{
		public int saveFileIndex = -1;

		public float cellSize = 10;

		public uint leftColor = 0;
		public uint rightColor = 0;
	}
}
