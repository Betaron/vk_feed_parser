using System.IO;

namespace vk_feed_parser
{
	public class Config
	{
		public ulong appId { get; set; }
		public string token { get; set; }
		public bool IsStayOnline { get; set; }

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
			try { Config externalConfig = FileWorker.LoadFromJsonFile<Config>(path); }
			catch {	WriteConfig(new Config()); }
		}

		public void ReadConfig()
		{
			string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");

			try
			{
				Config externalConfig = FileWorker.LoadFromJsonFile<Config>(path);
				SetConfig(externalConfig);
			}
			catch { WriteConfig(new Config()); }
		}
	}
}
