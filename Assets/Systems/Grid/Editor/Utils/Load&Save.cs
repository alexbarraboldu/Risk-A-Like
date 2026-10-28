using System;
using System.IO;

using ABB.Grid.Editor.Data;

using UnityEngine;

namespace ABB.Grid.Editor.Utils
{
	public static class Constants
	{
		private static readonly string COMPANY_NAME = "ABB";
		private static readonly string APPLICATION_NAME = "GridEditor";
		private static readonly string PRODUCT_NAME = Application.productName;

		public static readonly string PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), COMPANY_NAME, APPLICATION_NAME, PRODUCT_NAME);

		public static readonly string TERRAIN_COLORS_FILE_NAME = "terrainColors.bin";
		public static readonly string GRIDS_FILE_NAME = "grids.bin";
		public static readonly string SESSION_FILE_NAME = "session.bin";
		public static readonly string EXPORT_SETTINGS_FILE_NAME = "exportSettings.bin";
	}


	public static class Load
	{
		public static SessionData LoadSessionFile()
		{
			try
			{
				return BinarySerialization.ReadFromBinaryFile<SessionData>(Path.Combine(Constants.PATH, Constants.SESSION_FILE_NAME));
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return null;
			}
		}

		public static TerrainColorData LoadTerrainColorsFile()
		{
			try
			{
				return BinarySerialization.ReadFromBinaryFile<TerrainColorData>(Path.Combine(Constants.PATH, Constants.TERRAIN_COLORS_FILE_NAME));
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return null;
			}
		}

		public static GridData[] LoadGridsDataFile()
		{
			try
			{
				return BinarySerialization.ReadFromBinaryFile<GridData[]>(Path.Combine(Constants.PATH, Constants.GRIDS_FILE_NAME));
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return null;
			}
		}

		public static ExportSettingsData LoadExportSettingsFile()
		{
			try
			{
				return BinarySerialization.ReadFromBinaryFile<ExportSettingsData>(Path.Combine(Constants.PATH, Constants.EXPORT_SETTINGS_FILE_NAME));
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return null;
			}
		}
	}

	public static class Save
	{
		public static void SaveSessionFile(SessionData sessionData)
		{
			try
			{
				if (sessionData == null) sessionData = new();

				BinarySerialization.WriteToBinary<SessionData>(Path.Combine(Constants.PATH, Constants.SESSION_FILE_NAME), sessionData);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		public static void SaveTerrainColorsFile(TerrainColorData terrainColorsData)
		{
			try
			{
				if (terrainColorsData == null) terrainColorsData = new();

				BinarySerialization.WriteToBinary<TerrainColorData>(Path.Combine(Constants.PATH, Constants.TERRAIN_COLORS_FILE_NAME), terrainColorsData);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		public static void SaveGridDataFile(GridData[] gridsData)
		{
			try
			{
				if (gridsData == null) gridsData = Array.Empty<GridData>();

				BinarySerialization.WriteToBinary<GridData[]>(Path.Combine(Constants.PATH, Constants.GRIDS_FILE_NAME), gridsData);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		public static void SaveExportSettingsFile(ExportSettingsData exportSettingsData)
		{
			try
			{
				if (exportSettingsData == null) exportSettingsData = new();

				BinarySerialization.WriteToBinary<ExportSettingsData>(Path.Combine(Constants.PATH, Constants.EXPORT_SETTINGS_FILE_NAME), exportSettingsData);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}
}
