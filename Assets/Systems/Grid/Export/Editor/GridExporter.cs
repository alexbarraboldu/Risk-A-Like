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
		gridData.terrainData = ParseGrid(grid);

		string relativePath = FileUtil.GetProjectRelativePath(path);
		string finalPath = Path.Combine(relativePath, fileName + ".asset");

		if (!AssetDatabase.AssetPathExists(relativePath))
		{
			Debug.LogError($"Asset path does not exists. {relativePath}");
			return;
		}

		AssetDatabase.CreateAsset(gridData, finalPath);
	}

	private int[][] ParseGrid(int[][] grid)
	{
		int columns = grid.Length;
		int rows = grid != null && grid.Length > 0 ? grid[0]?.Length ?? 0 : 0;
		int[][] result = new int[columns][];

		for (int c = 0, C = columns - 1; c < columns; c++, --C)
		{
			result[c] = new int[rows];
			for (int r = 0, R = rows - 1; r < rows; r++, --R)
			{
				result[c][r] = grid[c][R];
			}
		}

		return result;
	}
}
