using System;

using ABB.Cell.Data.Storage;
using ABB.Gameplay.Data.Types;

using UnityEngine;

namespace ABB.Cell.Data
{
	public class CellData
	{
		public string ID = string.Empty;

		public TerrainType TerrainType;
		public Vector3Int Position = Vector3Int.zero;

		public bool Owned => Owner != null;
		public string Owner = string.Empty;

		public bool HasBuildings => Buildings.Length != 0;
		public Building[] Buildings;

		public bool HasTroops => Troops.Length != 0;
		public Troop[] Troops;


		public CellData()
		{
			ID = Guid.NewGuid().ToString();
		}
	}
}


