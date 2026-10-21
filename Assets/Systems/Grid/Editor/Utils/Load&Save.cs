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
		public static readonly string GRIDS_FILE_NAME = "gridEditorSessions.bin";
		public static readonly string SESSION_FILE_NAME = "gridEditorSession.bin";
	}


	public static class Load
	{
		public static SessionData LoadSessionFile()
		{
			return BinarySerialization.ReadFromBinaryFile<SessionData>($"{Constants.PATH}\\{Constants.SESSION_FILE_NAME}");
		}

		public static TerrainColorData LoadTerrainColorsFile()
		{
			return BinarySerialization.ReadFromBinaryFile<TerrainColorData>($"{Constants.PATH}\\{Constants.TERRAIN_COLORS_FILE_NAME}");
		}

		public static GridData[] LoadGridsDataFile()
		{
			return BinarySerialization.ReadFromBinaryFile<GridData[]>($"{Constants.PATH}\\{Constants.GRIDS_FILE_NAME}");
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
			catch //(Exception exception)
			{
				//Debug.Log("Could not serialize session file data");
				//Debug.LogException(exception);
			}
		}

		public static void SaveSessionFile(in SessionData sessionData)
		{
			if (!Directory.Exists(Constants.PATH))
			{
				Directory.CreateDirectory(Constants.PATH);
			}

			try
			{
				if (sessionData == null) throw new NullReferenceException();

				BinarySerialization.WriteToBinary<SessionData>($"{Constants.PATH}\\{Constants.SESSION_FILE_NAME}", sessionData);
			}
			catch //(Exception exception)
			{
				//Debug.Log("Could not serialize session file data");
				//Debug.LogException(exception);
			}
		}

		public static void SaveGridDataFile(in GridData[] gridsData)
		{
			if (!Directory.Exists(Constants.PATH))
			{
				Directory.CreateDirectory(Constants.PATH);
			}

			try
			{
				if (gridsData == null) throw new NullReferenceException();

				BinarySerialization.WriteToBinary<GridData[]>($"{Constants.PATH}\\{Constants.GRIDS_FILE_NAME}", gridsData);
			}
			catch //(Exception exception)
			{
				//Debug.Log("Could not serialize session file data");
				//Debug.LogException(exception);
			}
		}
	}
}
