using System;

namespace ABB.Grid.Editor.Data
{
	[Serializable]
	public class ExportSettingsData
	{
		public string assemblyQualifiedName;
		public string folderPath;
		public string fileName;


		public ExportSettingsData() { }

		public ExportSettingsData(ExportSettingsData exportSettingsData)
		{
			assemblyQualifiedName = exportSettingsData.assemblyQualifiedName;
			folderPath = exportSettingsData.folderPath;
			fileName = exportSettingsData.fileName;
		}

		public ExportSettingsData(string gameObjectGUID, string folderPath, string fileName)
		{
			this.assemblyQualifiedName = gameObjectGUID;
			this.folderPath = folderPath;
			this.fileName = fileName;
		}
	}
}
