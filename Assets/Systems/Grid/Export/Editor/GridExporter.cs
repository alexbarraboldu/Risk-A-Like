using ABB.Grid.Data;
using ABB.Grid.Editor.Utils;

using UnityEditor;

using UnityEngine;

public class GridExporter : MonoScript, IExportGrid
{
	public void ExportGrid(string path, int[][] grid)
	{
		GridTerrainDataSO gridData = ScriptableObject.CreateInstance<GridTerrainDataSO>();
		gridData.terrainData = grid;

		AssetDatabase.CreateAsset(gridData, path);
	}
}
