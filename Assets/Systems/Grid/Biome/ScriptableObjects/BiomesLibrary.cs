using System;
using System.Collections.Generic;

using ABB.Gameplay.Data.Types;

using UnityEngine;

namespace ABB.Grid.Biome
{
	[CreateAssetMenu(fileName = "BiomesLibrary", menuName = "Grid/Biomes Library")]
	public class BiomesLibrary : ScriptableObject
	{
		[SerializeField] private Biome[] biomes = Array.Empty<Biome>();

		private Dictionary<TerrainType, Biome> lookup;

		public IReadOnlyDictionary<TerrainType, Biome> BiomesByTerrain
		{
			get
			{
				if (lookup == null)
				{
					var result = new Dictionary<TerrainType, Biome>();

					foreach (Biome biome in biomes)
					{
						result.Add(biome.terrain, biome);
					}

					lookup = result;
				}

				return lookup;
			}
		}

		public IEnumerable<Biome> Biomes => BiomesByTerrain.Values;

		private void OnEnable() => lookup = null;

		private void OnValidate() => lookup = null;
	}
}
