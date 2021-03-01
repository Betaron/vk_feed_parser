using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace vk_feed_parser
{
	class UIWorker
	{
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
	}
}
