using System;

using UnityEngine;

namespace ABB.Grid.Editor.Data
{
	[Serializable]
	public class TerrainColorData
	{
		public TerrainColor[] terrainColors = Array.Empty<TerrainColor>();
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

		public Color color
		{
			get => new Color(r, g, b, a);
			set
			{
				r = value.r;
				g = value.g;
				b = value.b;
				a = value.a;
			}
		}

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
