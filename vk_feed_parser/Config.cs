using System.IO;

namespace vk_feed_parser
{
	public class Config
	{
		public ulong appId { get; set; }
		public string token { get; set; }
		public bool IsStayOnline { get; set; }

		public void SetDefaultConfig()
		{
			appId = 0;
			IsStayOnline = false;
			token = string.Empty;
		}

		public void SetConfig(Config externalConfig)
		{
			appId = externalConfig.appId;
			IsStayOnline = externalConfig.IsStayOnline;
			token = externalConfig.token;
		}

		public void WriteConfig(Config externalConfig) =>
			FileWorker.SaveToJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.json"), externalConfig);

		public void ReadConfig()
		{
			string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
			try
			{
				Config externalConfig = FileWorker.LoadFromJsonFile<Config>(path);
				SetConfig(externalConfig);
			}
			catch
			{
				SetDefaultConfig();
				WriteConfig(this);
			}
		}
	}
}
