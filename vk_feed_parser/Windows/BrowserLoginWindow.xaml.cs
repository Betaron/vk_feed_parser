using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace vk_feed_parser.Windows
{
	/// <summary>
	/// Interaction logic for BrowserLoginWindow.xaml
	/// </summary>
	public partial class BrowserLoginWindow : Window
	{
		public VkNet.Model.AuthorizationResult Auth { get; set; }

		public BrowserLoginWindow()
		{
			InitializeComponent();
		}
	}
}
