using System.IO;

using ABB.Grid.Data;
using ABB.Grid.Editor.Utils;

using UnityEditor;

using UnityEngine;

public class GridExporter : MonoScript, IExportGrid
{
	public void ExportGrid(string fileName, string path, int[][] grid)
	{
		GridTerrainDataSO gridData = ScriptableObject.CreateInstance<GridTerrainDataSO>();
		gridData.terrainData = grid;

		string relativePath = FileUtil.GetProjectRelativePath(path);
		string finalPath = Path.Combine(relativePath, fileName + ".asset");

		if (!AssetDatabase.AssetPathExists(relativePath))
		{
			Debug.LogError($"Asset path does not exists. {relativePath}");
			return;
		}

		AssetDatabase.CreateAsset(gridData, finalPath);
	}
}
