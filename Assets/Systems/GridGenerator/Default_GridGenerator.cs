#if UNITY_EDITOR

using ABB.Cell;

using UnityEditor;

using UnityEngine;

namespace ABB.Grid
{
	[RequireComponent(typeof(GridGenerator))]
	public class Default_GridGenerator : MonoBehaviour
	{
		[SerializeField] public int gridHeight = 20;
		[SerializeField] public int gridWidth = 40;
		[SerializeField] public int delayTime = 10;
		[SerializeField] public HexagonCell cellPrefab = null;

		private GridGenerator _gridGenerator;

		public void OnReset()
		{
			_gridGenerator = GetComponent<GridGenerator>();

			_gridGenerator.GridHeight = gridHeight;
			_gridGenerator.GridWidth = gridWidth;
			_gridGenerator.DelayTime = delayTime;
			_gridGenerator.CellPrefab = cellPrefab;
		}

		private void Reset()
		{
			gridHeight = 20;
			gridWidth = 40;
			delayTime = 10;
			cellPrefab = AssetDatabase.LoadAssetAtPath<HexagonCell>("Assets/Systems/GridGenerator/GridCell.prefab");
		}
	}
}
#endif
