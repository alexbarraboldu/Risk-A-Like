using System;

namespace ABB.Grid.Editor.Data
{
	[Serializable]
	public class SessionData
	{
		public int saveFileIndex;

		public float cellSize;

		public uint leftColor;
		public uint rightColor;


		public SessionData() { }

		public SessionData(SessionData sessionFile)
		{
			saveFileIndex = sessionFile.saveFileIndex;
			cellSize = sessionFile.cellSize;
			leftColor = sessionFile.leftColor;
			rightColor = sessionFile.rightColor;
		}

		public SessionData(int saveFileIndex, float cellSize, uint leftColor, uint rightColor)
		{
			this.saveFileIndex = saveFileIndex;
			this.cellSize = cellSize;
			this.leftColor = leftColor;
			this.rightColor = rightColor;
		}
	}
}
