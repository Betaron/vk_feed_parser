﻿using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace vk_feed_parser
{
	public static class UIWorker
	{
		public static Dispatcher mainDispatcher;
		public static Dispatcher logDispatcher;
		public static StackPanel logPanel;

		private const int maxEntryCount = 200;

		/// <param name="mDisp">dispatcher of main window</param>
		/// <param name="panel">panel, in which will be writting messages</param>
		public static void SetWriterParams(Dispatcher mDisp, StackPanel panel)
		{
			mainDispatcher = mDisp;
			logPanel = panel;
			logDispatcher = panel.Dispatcher;
		}

		/// <summary>
		/// add record to logPanel
		/// </summary>
		/// <param name="text">message test</param>
		public static void AddRecord(String text)
		{
			if (mainDispatcher.HasShutdownStarted) return;
			logDispatcher.Invoke(() => {
				bool onBottom = (logPanel.Parent as ScrollViewer).VerticalOffset == 
				(logPanel.Parent as ScrollViewer).ScrollableHeight;
				var tBlock = new TextBox
				{
					IsReadOnly = true,
					Foreground = Brushes.Green,
					Background = Brushes.Transparent,
					BorderBrush = Brushes.Transparent,
					FontFamily = new FontFamily("Consolas"),
					Text = "> "
				};
				tBlock.Text += text;
				logPanel.Children.Add(tBlock);

				if (logPanel.Children.Count > maxEntryCount)
					logPanel.Children.Remove(logPanel.Children[0]);

				if (onBottom) (logPanel.Parent as ScrollViewer).ScrollToBottom();
			});
		}

		public static void SetLineState(Rectangle line, bool state)
		{
			mainDispatcher.Invoke(()=> line.Fill = state?Brushes.LightGreen:Brushes.Green);
		}
	}
}
