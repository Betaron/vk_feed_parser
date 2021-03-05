using System.IO;
using Newtonsoft.Json;

namespace vk_feed_parser
{
	static class FileWorker
	{
		public static void SaveToJsonFile(string path, object item, Formatting options = Formatting.Indented)
		{
			File.WriteAllText(path, JsonConvert.SerializeObject(item, options));
		}

		public static TItem LoadFromJsonFile<TItem>(string path, JsonSerializerSettings settings = null)
		{
			return JsonConvert.DeserializeObject<TItem>(File.ReadAllText(path), settings);
		}
	}
}
