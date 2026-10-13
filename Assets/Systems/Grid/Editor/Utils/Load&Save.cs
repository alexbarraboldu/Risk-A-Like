using System;
using System.IO;

using ABB.Grid.Editor.Data;

namespace ABB.Grid.Editor.Utils
{
	public static class Constants
	{
		private static readonly string COMPANY_NAME = "ABB";
		private static readonly string APPLICATION_NAME = "GridEditor";

		public static readonly string PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), COMPANY_NAME, APPLICATION_NAME);

		public static readonly string TERRAIN_COLORS_FILE_NAME = "gridEditorTerrainColors.bin";
	}


	public static class Load
	{
		public static TerrainColorData LoadTerrainColorsFile()
		{
			return BinarySerialization.ReadFromBinaryFile<TerrainColorData>($"{Constants.PATH}\\{Constants.TERRAIN_COLORS_FILE_NAME}");
		}
	}

	public static class Save
	{
		public static void SaveTerrainColorsFile(in TerrainColorData terrainColorsFile)
		{
			if (!Directory.Exists(Constants.PATH))
			{
				Directory.CreateDirectory(Constants.PATH);
			}

			try
			{
				if (terrainColorsFile == null) throw new NullReferenceException();

				BinarySerialization.WriteToBinary<TerrainColorData>($"{Constants.PATH}\\{Constants.TERRAIN_COLORS_FILE_NAME}", terrainColorsFile);
			}
			catch (Exception)
			{
				//Debug.Log("Could not serialize session file data");
				//Debug.LogException(exception);
			}
		}
	}
}
