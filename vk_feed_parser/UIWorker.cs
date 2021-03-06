using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace vk_feed_parser
{
	class UIWorker
	{
		Dispatcher mainDispatcher;
		Parser mainParser;

		public UIWorker(Dispatcher dispatcher, Parser parser)
		{
			mainDispatcher = dispatcher;
			mainParser = parser;
		}

		static internal bool AddRecord(StackPanel panel, String text, Brush textColor)
		{
			var tBlock = new TextBlock();
			tBlock.Foreground = textColor;
			tBlock.FontFamily = new FontFamily("Consolas");
			tBlock.Text = "> ";
			try
			{
				tBlock.Text += text;
				panel.Children.Add(tBlock);
				return true;
			}
			catch
			{
				return false;
			}
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
