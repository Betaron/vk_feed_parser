using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace vk_feed_parser
{
	class UIWorker
	{
		Parser mainParser;

		public static Dispatcher mainDispatcher;
		public static StackPanel mainPanel;

		public UIWorker(Parser parser)
		{
			mainParser = parser;
		}

		public static void SetWriterParams(Dispatcher disp, StackPanel panel)
		{
			mainDispatcher = disp;
			mainPanel = panel;
		}

		static public void AddRecord(String text)
		{
			mainDispatcher.Invoke(() => {
				var tBlock = new TextBox
				{
					IsReadOnly = true,
					Foreground = Brushes.Green,
					Background = Brushes.Transparent,
					BorderBrush = Brushes.Transparent,
					FontFamily = new FontFamily("Consolas"),
					Text = "> "
				};
			try
			{
				tBlock.Text += text;
				mainPanel.Children.Add(tBlock);
			}
			catch { }
		});
		}

		internal void ShowLoginWindow()
		{
			mainDispatcher.Invoke(() =>
			{
				//LoginWindow loginWindow = new LoginWindow(mainParser);
				var loginWindow = new Windows.BrowserLoginWindow();
				loginWindow.Show();
			});
		}
	}
}
