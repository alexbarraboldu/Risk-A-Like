using System;
using System.IO;
using System.Text;

namespace ABB.Grid.Editor.Utils
{
	public class BinarySerialization
	{
		public static void WriteToBinary<T>(string filePath, T value, Action<BinaryWriter, T> write)
		{
			string tempFilePath = $"{filePath}.{Guid.NewGuid():N}.tmp";

			try
			{
				using (var stream = File.Open(tempFilePath, FileMode.Create, FileAccess.Write))
				using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
				{
					write(writer, value);
				}

				if (File.Exists(filePath))
					File.Replace(tempFilePath, filePath, null);
				else
					File.Move(tempFilePath, filePath);
			}
			finally
			{
				if (File.Exists(tempFilePath))
					File.Delete(tempFilePath);
			}
		}

		public static T ReadFromBinaryFile<T>(string filePath, Func<BinaryReader, T> read)
		{
			using Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
			using var reader = new BinaryReader(stream);

			return read(reader);
		}
	}
}
