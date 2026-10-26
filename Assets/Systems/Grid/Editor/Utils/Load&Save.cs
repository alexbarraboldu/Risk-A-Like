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

		public static readonly string PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), COMPANY_NAME, APPLICATION_NAME);

		public static readonly string TERRAIN_COLORS_FILE_NAME = "gridEditorTerrainColors.bin";
		public static readonly string GRIDS_FILE_NAME = "gridEditorSessions.bin";
		public static readonly string SESSION_FILE_NAME = "gridEditorSession.bin";
		public static readonly string EXPORT_SETTINGS_FILE_NAME = "exportSettings.bin";
	}


	public static class Load
	{
		public static SessionData LoadSessionFile()
		{
			return BinarySerialization.ReadFromBinaryFile<SessionData>(Path.Combine(Constants.PATH, Constants.SESSION_FILE_NAME));
		}

		public static TerrainColorData LoadTerrainColorsFile()
		{
			return BinarySerialization.ReadFromBinaryFile<TerrainColorData>(Path.Combine(Constants.PATH, Constants.TERRAIN_COLORS_FILE_NAME));
		}

		public static GridData[] LoadGridsDataFile()
		{
			return BinarySerialization.ReadFromBinaryFile<GridData[]>(Path.Combine(Constants.PATH, Constants.GRIDS_FILE_NAME));
		}

		public static ExportSettingsData LoadExportSettingsFile()
		{
			return BinarySerialization.ReadFromBinaryFile<ExportSettingsData>(Path.Combine(Constants.PATH, Constants.EXPORT_SETTINGS_FILE_NAME));
		}
	}

	public static class Save
	{
		public static void SaveSessionFile(in SessionData sessionData)
		{
			try
			{
				if (sessionData == null) throw new NullReferenceException();

				BinarySerialization.WriteToBinary<SessionData>(Path.Combine(Constants.PATH, Constants.SESSION_FILE_NAME), sessionData);
			}
			catch
			{
				Debug.LogWarning($"Could not serialize {typeof(SessionData)}");
			}
		}

		public static void SaveTerrainColorsFile(in TerrainColorData terrainColorsData)
		{
			try
			{
				if (terrainColorsData == null) throw new NullReferenceException();

				BinarySerialization.WriteToBinary<TerrainColorData>(Path.Combine(Constants.PATH, Constants.TERRAIN_COLORS_FILE_NAME), terrainColorsData);
			}
			catch
			{
				Debug.LogWarning($"Could not serialize {typeof(TerrainColorData)}");
			}
		}

		public static void SaveGridDataFile(in GridData[] gridsData)
		{
			try
			{
				if (gridsData == null) throw new NullReferenceException();

				BinarySerialization.WriteToBinary<GridData[]>(Path.Combine(Constants.PATH, Constants.GRIDS_FILE_NAME), gridsData);
			}
			catch
			{
				Debug.LogWarning($"Could not serialize {typeof(GridData)}");
			}
		}

		public static void SaveExportSettingsFile(in ExportSettingsData exportSettingsData)
		{
			try
			{
				if (exportSettingsData == null) throw new NullReferenceException();

				BinarySerialization.WriteToBinary<ExportSettingsData>(Path.Combine(Constants.PATH, Constants.EXPORT_SETTINGS_FILE_NAME), exportSettingsData);
			}
			catch (Exception exception)
			{
				Debug.LogError("Could not serialize export settings file data");
				Debug.LogException(exception);
			}
		}
	}
}
