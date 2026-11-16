using System.IO;

using ABB.Grid.Data;
using ABB.Grid.Editor.Utils;

using UnityEditor;

using UnityEngine;

public class GridExporter : IExportGrid
{
	public bool ExportGrid(string fileName, string path, int[] grid, int columns, int rows)
	{
		string relativePath = FileUtil.GetProjectRelativePath(path);
		if (!AssetDatabase.IsValidFolder(relativePath))
		{
			Debug.LogError($"Path must be inside Assets directory.");
			return false;
		}

		if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains('/') || fileName.Contains('\\') || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
		{
			Debug.LogError("Enter a valid file name.");
			return false;
		}

		GridTerrainDataSO gridData = null;
		try
		{
			gridData = ScriptableObject.CreateInstance<GridTerrainDataSO>();
			gridData.terrainData = ParseGrid(grid, columns, rows);

			string finalPath = Path.Combine(relativePath, fileName + ".asset");

			AssetDatabase.CreateAsset(gridData, finalPath);

			return gridData != null && AssetDatabase.LoadMainAssetAtPath(finalPath) == gridData;
		}
		catch (System.Exception exception)
		{
			Debug.LogException(exception);
			return false;
		}
		finally
		{
			if (gridData != null && !EditorUtility.IsPersistent(gridData))
				UnityEngine.Object.DestroyImmediate(gridData);
		}
	}

	private int[][] ParseGrid(int[] grid, int columns, int rows)
	{
		int[][] result = new int[columns][];

		for (int column = 0, C = columns - 1; column < columns; column++, --C)
		{
			result[column] = new int[rows];
			for (int row = 0, R = rows - 1; row < rows; row++, --R)
			{
				result[column][row] = grid[column * rows + R];
			}
		}

		return result;
	}
}
