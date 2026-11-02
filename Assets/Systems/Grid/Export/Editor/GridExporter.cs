using System.IO;

using ABB.Grid.Data;
using ABB.Grid.Editor.Utils;

using UnityEditor;

using UnityEngine;

public class GridExporter : IExportGrid
{
	public bool ExportGrid(string fileName, string path, int[][] grid)
	{
		string relativePath = FileUtil.GetProjectRelativePath(path);
		if (!AssetDatabase.AssetPathExists(relativePath))
		{
			Debug.LogError($"Path must be inside Assets directory.");

			return false;
		}

		GridTerrainDataSO gridData = null;
		try
		{
			gridData = ScriptableObject.CreateInstance<GridTerrainDataSO>();
			gridData.terrainData = ParseGrid(grid);

			string finalPath = Path.Combine(relativePath, fileName + ".asset");

			AssetDatabase.CreateAsset(gridData, finalPath);

			return true;
		}
		catch
		{
			Object.DestroyImmediate(gridData);
			Debug.LogError($"Failed on AssetDatabase.CreateAsset. Could not create {fileName}.asset");

			return false;
		}
		finally
		{
			if (gridData != null && !EditorUtility.IsPersistent(gridData))
				UnityEngine.Object.DestroyImmediate(gridData);
		}
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
