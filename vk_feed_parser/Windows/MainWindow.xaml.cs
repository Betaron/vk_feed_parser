using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace vk_feed_parser.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Parser parser = new Parser();
		private Config config = new Config();
		private Thread parsingThread;
		private Thread checkAuthThread;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			checkAuthThread = CheckAuthorise();
			Config.CheckConfigFileValid();
			UIWorker.SetWriterParams(Dispatcher, logStack);
			UIWorker.AddRecord("Welcome to VkFeedParcer 3000");
		}

		private void loginBtn_Click(object sender, RoutedEventArgs e)
		{
			config.ReadConfig();
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
					config = new Config() { appId = config.appId };
				}
				Config.WriteConfig(config);
			}
			if (!checkAuthThread.IsAlive)
			{
				checkAuthThread.Start();
			}
		}

		private Thread CheckAuthorise()
		{
			return new Thread(() =>
			{
				while (!parser.api.IsAuthorized && !parser.IsShutdown)
				{
				}
				if (!parser.IsShutdown)
				{
					this.Dispatcher.Invoke(() =>
					{
						UIWorker.AddRecord("Authorize - sucseed!");
						loginBtn.IsEnabled = false;
						logoutBtn.IsEnabled = true;
						setPreferencesBtn.IsEnabled = true;
						runParseBtn.IsEnabled = true;
					});
				}
			});
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
				parsingThread = parser.GetParseThread();
				parsingThread.Start();
				this.Dispatcher.Invoke(() =>
				{
					setPreferencesBtn.IsEnabled = false;
					runParseBtn.IsEnabled = false;
				});
				new Thread(() =>
				{
					parsingThread.Join();
					if (ThreadWorker.savingThread != null)
						ThreadWorker.savingThread.Join();
					if (!this.Dispatcher.HasShutdownStarted)
						this.Dispatcher.Invoke(() =>
						{
							setPreferencesBtn.IsEnabled = true;
							runParseBtn.IsEnabled = true;
						});
				}).Start();
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			parser.ThreadsShutdown();
			e.Cancel = false;
		}
	}
}
