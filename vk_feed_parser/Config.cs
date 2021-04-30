using System.IO;

namespace vk_feed_parser
{
	public class Config
	{
		public ulong appId;
		public string token;
		public bool IsStayOnline;

		public void SetConfig(Config externalConfig)
		{
			appId = externalConfig.appId;
			IsStayOnline = externalConfig.IsStayOnline;
			token = externalConfig.token;
		}

		public static void WriteConfig(Config externalConfig) =>
			FileWorker.SaveToJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.json"), externalConfig);

		public static void CheckConfigFileValid()
		{
			string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
			if (!File.Exists(path))
				WriteConfig(new Config());
			else if (FileWorker.LoadFromJsonFile<Config>(path).Equals(null))
				WriteConfig(new Config());
		}

		public void ReadConfig()
		{
			string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");

			if (File.Exists(path))
			{
				Config externalConfig = FileWorker.LoadFromJsonFile<Config>(path);

				if (!externalConfig.Equals(null))
					SetConfig(externalConfig);
				else
					WriteConfig(new Config());
			}
		}
	}
}
