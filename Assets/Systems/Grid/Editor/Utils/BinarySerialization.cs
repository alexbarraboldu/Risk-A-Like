using System.IO;

namespace ABB.Grid.Editor.Utils
{
	///	Consider using a temporary file to prevent data loss.
	///	FileStream.Flush(true), File.Repalce
	public class BinarySerialization
	{
		public static void WriteToBinary<T>(string filePath, T objectToWrite)
		{
			using (Stream stream = File.Open(filePath, FileMode.Create, FileAccess.Write))
			{
				var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				binaryFormatter.Serialize(stream, objectToWrite);
			}
		}

		public static T ReadFromBinaryFile<T>(string filePath)
		{
			using (Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
			{
				var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				return (T)binaryFormatter.Deserialize(stream);
			}
		}
	}
}
