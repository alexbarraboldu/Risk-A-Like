using System;

using ABB.Gameplay.Data.Types;

using UnityEngine;

namespace ABB.Grid.Editor.Data
{
	[Serializable]
	public class TerrainColorData
	{
		public TerrainColor[] terrainColors = Array.Empty<TerrainColor>();


		public TerrainColorData()
		{
			int size = Enum.GetValues(typeof(TerrainType)).Length;
			terrainColors = new TerrainColor[size];

			for (int i = 0; i < size; i++)
			{
				terrainColors[i] = new TerrainColor((TerrainType)i, Color.black);
			}
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

		public Color color => new Color(r, g, b, a);

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
