using ABB.Gameplay.Data.Types;

using UnityEngine;

namespace ABB.Cell.Data.Storage
{
	public class CellDataSO : ScriptableObject
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


		public static implicit operator CellDataSO(CellData cellData)
		{
			CellDataSO cellDataSO = null;

			cellDataSO.ID = cellData.ID;

			cellDataSO.TerrainType = cellData.TerrainType;
			cellDataSO.Position = cellData.Position;

			cellDataSO.Owner = cellData.Owner;

			cellDataSO.Buildings = cellData.Buildings;

			cellDataSO.Troops = cellData.Troops;

			return cellDataSO;
		}
	}
}
