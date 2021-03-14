using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VkNet;

namespace vk_feed_parser.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Parser parser = new Parser();
		private Config config = new Config();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Config.CheckConfigFileValid();
			FileWorker.CreateEmptyFilesForSavingData();
			UIWorker.SetWriterParams(Dispatcher, logStack);
			UIWorker.AddRecord("Welcome to VkFeedParcer 3000");
		}

		private void loginBtn_Click(object sender, RoutedEventArgs e)
		{
			config = GetConfig();
			if (config.IsStayOnline)
			{
				try { parser.TokenAuthorize(config.token); }
				catch { Config.WriteConfig(new Config() { appId = config.appId }); }
			}
			else
			{
				try
				{
					parser.LoginAuth(config.appId);
					config = new Config()
					{
						appId = config.appId,
						IsStayOnline = true,
						token = parser.api.Token
					};
				}
				catch
				{
					config = new Config() {appId = config.appId };
				}
				Config.WriteConfig(config);
			}
			CheckAuthorise();
		}

		private void CheckAuthorise()
		{
			Thread checkAuthThread = new Thread(() =>
			{
				while (!parser.api.IsAuthorized)
				{
				}
				this.Dispatcher.Invoke(() => {
					UIWorker.AddRecord("Authorize - sucseed!");
					loginBtn.IsEnabled = false;
					logoutBtn.IsEnabled = true;
					setPreferencesBtn.IsEnabled = true;
					runParseBtn.IsEnabled = true;
				});
			});
			checkAuthThread.Start();
		}

		private Config GetConfig()
		{
			config.ReadConfig();
			return config;
		}

		private void logoutBtn_Click(object sender, RoutedEventArgs e)
		{
			Config.WriteConfig(new Config() { appId = config.appId });
			Application.Current.Shutdown();
			string path = Application.ResourceAssembly.Location;
			path = path.Remove(path.Length - 3, 3) + "exe";
			System.Diagnostics.Process.Start(path);
		}

		private void runParseBtn_Click(object sender, RoutedEventArgs e)
		{
			if (parser.api.IsAuthorized)
			{
				Thread parsingThread = parser.GetParseThread();
				parsingThread.Start();
			}
		}
	}
}
