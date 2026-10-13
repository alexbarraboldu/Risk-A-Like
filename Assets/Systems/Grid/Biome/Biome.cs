using System;

using ABB.Gameplay.Data.Types;

using UnityEngine;

namespace ABB.Grid.Biome
{
	[Serializable]
	public class Biome
	{
		public TerrainType terrain;
		public Material material;

		public Biome() { }
		public Biome(TerrainType terrain, Material material)
		{
			this.terrain = terrain;
			this.material = material;
		}
	}
}
