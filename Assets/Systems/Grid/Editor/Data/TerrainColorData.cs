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
			var enumVaules = (TerrainType[])Enum.GetValues(typeof(TerrainType));
			terrainColors = new TerrainColor[enumVaules.Length];

			for (int i = 0; i < enumVaules.Length; i++)
			{
				TerrainColor terrainColor = new TerrainColor(enumVaules[i].ToString(), i, Color.black);
				terrainColors[i] = terrainColor;
			}
		}
	}

	[Serializable]
	public struct TerrainColor
	{
		public string name;
		public int terrain;
		public float r;
		public float g;
		public float b;
		public float a;

		public Color color => new Color(r, g, b, a);

		public TerrainColor(string name, int terrain, Color color)
		{
			this.name = name;
			this.terrain = terrain;
			this.r = color.r;
			this.g = color.g;
			this.b = color.b;
			this.a = color.a;
		}
	}
}
