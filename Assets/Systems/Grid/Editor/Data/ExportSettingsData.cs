using System;

namespace ABB.Grid.Editor.Data
{
	[Serializable]
	public class ExportSettingsData
	{
		public string cellGUID;
		public string folderPath;
		public string fileName;


		public ExportSettingsData() { }

		public ExportSettingsData(ExportSettingsData exportSettingsData)
		{
			cellGUID = exportSettingsData.cellGUID;
			folderPath = exportSettingsData.folderPath;
			fileName = exportSettingsData.fileName;
		}

		public ExportSettingsData(string gameObjectGUID, string folderPath, string fileName)
		{
			this.cellGUID = gameObjectGUID;
			this.folderPath = folderPath;
			this.fileName = fileName;
		}
	}
}
