using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace vk_feed_parser
{
	public static class UIWorker
	{
		public static Dispatcher mainDispatcher;
		public static StackPanel mainPanel;

		public static void SetWriterParams(Dispatcher disp, StackPanel panel)
		{
			mainDispatcher = disp;
			mainPanel = panel;
		}

		public static void AddRecord(String text)
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

		public static void SetLineState(Rectangle line, bool state)
		{
			mainDispatcher.Invoke(()=> line.Fill = state?Brushes.LightGreen:Brushes.Green);
		}
	}
}
