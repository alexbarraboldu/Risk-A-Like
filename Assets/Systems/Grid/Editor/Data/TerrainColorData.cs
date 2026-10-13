using System;

using ABB.Gameplay.Data.Types;

using UnityEngine;

namespace ABB.Grid.Editor.Data
{
	[Serializable]
	public class TerrainColorData
	{
		public TerrainColor[] terrainColors;

		public TerrainColorData() { }

		public TerrainColorData(TerrainColor[] terrainColors)
		{
			this.terrainColors = terrainColors;
		}
	}

	[Serializable]
	public struct TerrainColor
	{
		public int terrain;
		public float r;
		public float g;
		public float b;
		public float a;

		public TerrainColor(TerrainType terrain, Color color)
		{
			this.terrain = (int)terrain;
			this.r = color.r;
			this.g = color.g;
			this.b = color.b;
			this.a = color.a;
		}
	}
}
