using System;
using System.Collections.Generic;

using ABB.Gameplay.Data.Types;

using UnityEngine;

namespace ABB.Grid.Biome
{
	[CreateAssetMenu(fileName = "TerrainTypeLibrary", menuName = "Grid/Terrain Library")]
	public class TerrainTypeLibrary : ScriptableObject
	{
		[Serializable]
		private struct Entry
		{
			public TerrainType terrain;
			public Material material;
		}

		[SerializeField] private Entry[] entries = Array.Empty<Entry>();

		private Dictionary<TerrainType, Biome> lookup;

		public IReadOnlyDictionary<TerrainType, Biome> BiomesByTerrain
		{
			get
			{
				if (lookup == null)
				{
					var result = new Dictionary<TerrainType, Biome>();

					foreach (Entry entry in entries)
					{
						result.Add(entry.terrain, new Biome(entry.terrain, entry.material));
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
