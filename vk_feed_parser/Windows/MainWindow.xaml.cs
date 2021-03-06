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
		private UIWorker UI;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Config.CheckConfigFileValid();
			UIWorker.AddRecord(logStack, "Welcome to VkFeedParcer 3000", Brushes.Green);
			UI = new UIWorker(mainGrid.Dispatcher, parser);
		}

		private void loginBtn_Click(object sender, RoutedEventArgs e)
		{
			config = GetConfig();
			if (config.IsStayOnline)
			{
				parser.TokenAuthorize(config.token);
			}
			else
			{
				parser.LoginAuth(config.appId);
				config = new Config()
				{
					appId = config.appId,
					IsStayOnline = true,
					token = parser.api.Token
				};
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
				this.Dispatcher.Invoke(() =>
				UIWorker.AddRecord(logStack, "Authorize - sucseed!", Brushes.Green)
				);
			});
			checkAuthThread.Start();
		}

		private Config GetConfig()
		{
			config.ReadConfig();
			return config;
		} 
	}
}
