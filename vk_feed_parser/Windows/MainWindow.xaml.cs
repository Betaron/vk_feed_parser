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

namespace vk_feed_parser
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private Parser parser;
		private UIWorker UI;


		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			UIWorker.AddRecord(logStack, "Welcome to VkFeedParcer 3000", Brushes.Green);
			parser = new Parser();
			UI = new UIWorker(mainGrid.Dispatcher, parser);
			IsAuthorisedTrue();
		}

		private void loginBtn_Click(object sender, RoutedEventArgs e)
		{
			UI.ShowLoginWindow();
		}

		private void IsAuthorisedTrue()
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
	}
}
