using System.IO;
using Newtonsoft.Json;

namespace vk_feed_parser
{
	static class FileWorker
	{
		/// <summary>
		/// A method that stores an object as a JSON.
		/// </summary>
		/// <param name="path">Final file location and name</param>
		/// <param name="item">Serializing object</param>
		public static void SaveToJsonFile(string path, object item, Formatting options = Formatting.Indented)
		{
			createFullPath(Directory.GetParent(path).ToString());
			File.WriteAllText(path, JsonConvert.SerializeObject(item, options));
		}

		/// <summary>
		/// Method unloads data from the specified JSON.
		/// </summary>
		/// <typeparam name="TItem">Type of uploaded data</typeparam>
		/// <param name="path">A path to the file</param>
		/// <returns>Deserialized object</returns>
		public static TItem LoadFromJsonFile<TItem>(string path, JsonSerializerSettings settings = null)
		{
			return JsonConvert.DeserializeObject<TItem>(File.ReadAllText(path), settings);
		}

		public static void createFullPath(string path)
		{
			if (!Directory.Exists(path))
			{
				var parentDir = Directory.GetParent(path);
				if (parentDir != null)
					createFullPath(parentDir.ToString());
				Directory.CreateDirectory(path);
			}
		}
	}
}
