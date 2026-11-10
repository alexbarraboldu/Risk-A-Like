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

		public static readonly int MAX_GRID_DIMENSION = 10000;
		public static readonly int MAX_SAVED_GRIDS = 100;
	}


	public static class Load
	{
		public static SessionData LoadSessionFile()
		{
			try
			{
				return BinarySerialization.ReadFromBinaryFile<SessionData>(Path.Combine(Constants.PATH, Constants.SESSION_FILE_NAME), reader =>
				{
					SessionData sessionData = new();
					sessionData.saveFileIndex = reader.ReadInt32();
					sessionData.cellSize = reader.ReadSingle();
					sessionData.leftColor = reader.ReadUInt32();
					sessionData.rightColor = reader.ReadUInt32();

					return sessionData;
				});
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
				return BinarySerialization.ReadFromBinaryFile(Path.Combine(Constants.PATH, Constants.TERRAIN_COLORS_FILE_NAME), reader =>
				{
					int count = reader.ReadInt32();
					const int minEntryBytes = 1 + sizeof(int) + 4 * sizeof(float);
					long remaining = reader.BaseStream.Length - reader.BaseStream.Position;

					if (count < 0 || count > remaining / minEntryBytes)
						throw new InvalidDataException("Invalid terrain color count.");

					var colors = new TerrainColor[count];

					for (int i = 0; i < count; i++)
					{
						colors[i] = new TerrainColor
						{
							name = reader.ReadString(),
							terrain = reader.ReadInt32(),
							r = reader.ReadSingle(),
							g = reader.ReadSingle(),
							b = reader.ReadSingle(),
							a = reader.ReadSingle()
						};
					}

					return new TerrainColorData { terrainColors = colors };
				});
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
				return BinarySerialization.ReadFromBinaryFile<GridData[]>(Path.Combine(Constants.PATH, Constants.GRIDS_FILE_NAME), reader =>
				{
					const int minEntryBytes = 2 * sizeof(int) + 1;

					int grids = reader.ReadInt32();
					long remaining = reader.BaseStream.Length - reader.BaseStream.Position;

					if (grids < 0 || grids > Constants.MAX_SAVED_GRIDS || grids > remaining / minEntryBytes)
						throw new InvalidDataException("Invalid grid count.");

					GridData[] gridsData = new GridData[grids];

					for (int i = 0; i < grids; i++)
					{
						GridData grid = new()
						{
							columns = reader.ReadInt32(),
							rows = reader.ReadInt32()
						};

						if (grid.columns < 0 || grid.columns > Constants.MAX_GRID_DIMENSION ||
							grid.rows < 0 || grid.rows > Constants.MAX_GRID_DIMENSION)
							throw new InvalidDataException("Invalid grid dimensions.");

						remaining = reader.BaseStream.Length - reader.BaseStream.Position;
						long cellCount = (long)grid.columns * grid.rows;

						if (remaining < 1 || cellCount > (remaining - 1) / sizeof(int))
							throw new InvalidDataException("Incomplete grid data.");

						grid.grid = new int[grid.columns][];
						for (int column = 0; column < grid.columns; column++)
						{
							grid.grid[column] = new int[grid.rows];
							for (int row = 0; row < grid.rows; row++)
							{
								grid.grid[column][row] = reader.ReadInt32();
							}
						}

						grid.fileName = reader.ReadString();
						gridsData[i] = grid;
					}

					return gridsData;
				});
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
				return BinarySerialization.ReadFromBinaryFile<ExportSettingsData>(Path.Combine(Constants.PATH, Constants.EXPORT_SETTINGS_FILE_NAME), reader =>
				{
					ExportSettingsData exportSettingsData = new();

					exportSettingsData.assemblyQualifiedName = reader.ReadString();
					exportSettingsData.folderPath = reader.ReadString();

					return exportSettingsData;
				});
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
		public static bool SaveSessionFile(SessionData sessionData)
		{
			try
			{
				if (sessionData == null) sessionData = new();

				BinarySerialization.WriteToBinary<SessionData>(Path.Combine(Constants.PATH, Constants.SESSION_FILE_NAME), sessionData, (writer, sessionData) =>
				{
					writer.Write(sessionData.saveFileIndex);
					writer.Write(sessionData.cellSize);
					writer.Write(sessionData.leftColor);
					writer.Write(sessionData.rightColor);
				});

				return true;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);

				return false;
			}
		}

		public static bool SaveTerrainColorsFile(TerrainColorData terrainColorsData)
		{
			try
			{
				if (terrainColorsData == null) terrainColorsData = new();

				BinarySerialization.WriteToBinary<TerrainColorData>(Path.Combine(Constants.PATH, Constants.TERRAIN_COLORS_FILE_NAME), terrainColorsData, (writer, terrainColorsData) =>
				{
					writer.Write(terrainColorsData.terrainColors.Length);
					for (int i = 0; i < terrainColorsData.terrainColors.Length; i++)
					{
						writer.Write(terrainColorsData.terrainColors[i].name);
						writer.Write(terrainColorsData.terrainColors[i].terrain);
						writer.Write(terrainColorsData.terrainColors[i].r);
						writer.Write(terrainColorsData.terrainColors[i].g);
						writer.Write(terrainColorsData.terrainColors[i].b);
						writer.Write(terrainColorsData.terrainColors[i].a);
					}
				});

				return true;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);

				return false;
			}
		}

		public static bool SaveGridDataFile(GridData[] gridsData)
		{
			try
			{
				if (gridsData == null) gridsData = Array.Empty<GridData>();

				BinarySerialization.WriteToBinary<GridData[]>(Path.Combine(Constants.PATH, Constants.GRIDS_FILE_NAME), gridsData, (writer, gridsData) =>
				{
					int grids = gridsData.Length;

					writer.Write(grids);

					for (int i = 0; i < grids; i++)
					{
						int[][] grid = gridsData[i].grid;
						int columns = gridsData[i].columns, rows = gridsData[i].rows;

						writer.Write(columns);
						writer.Write(rows);

						for (int column = 0; column < columns; column++)
						{
							for (int row = 0; row < rows; row++)
							{
								writer.Write(grid[column][row]);
							}
						}

						writer.Write(gridsData[i].fileName);
					}
				});

				return true;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);

				return false;
			}
		}

		public static bool SaveExportSettingsFile(ExportSettingsData exportSettingsData)
		{
			try
			{
				if (exportSettingsData == null) exportSettingsData = new();

				BinarySerialization.WriteToBinary<ExportSettingsData>(Path.Combine(Constants.PATH, Constants.EXPORT_SETTINGS_FILE_NAME), exportSettingsData, (writer, exportSettingsData) =>
				{
					writer.Write(exportSettingsData.assemblyQualifiedName);
					writer.Write(exportSettingsData.folderPath);
				});

				return true;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);

				return false;
			}
		}
	}
}
