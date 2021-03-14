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

		/// <summary>
		/// Creating new empty JSON files for saving data about text, links, images in 'DataStorages' folder, if they does not exists.
		/// </summary>
		public static void CreateEmptyFilesForSavingData()
		{
			string rootFolder = Directory.GetCurrentDirectory();
			string dataFilesFolder = "DataStorages";
			string textDataPath = Path.Combine(rootFolder, dataFilesFolder, "TextData.json");
			string linksDataPath = Path.Combine(rootFolder, dataFilesFolder, "LinksData.json");
			string imagesDataPath = Path.Combine(rootFolder, dataFilesFolder, "ImagesData.json");
			Directory.CreateDirectory(Path.Combine(rootFolder, dataFilesFolder));
			if (!File.Exists(textDataPath))
				File.Create(textDataPath);
			if (!File.Exists(linksDataPath))
				File.Create(linksDataPath);
			if (!File.Exists(imagesDataPath))
				File.Create(imagesDataPath);
		}
	}
}
