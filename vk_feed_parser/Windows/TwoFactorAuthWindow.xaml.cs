﻿using System;
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

namespace vk_feed_parser
{
	/// <summary>
	/// Interaction logic for TwoFactorAuthWindow.xaml
	/// </summary>
	public partial class TwoFactorAuthWindow : Window
	{
		public TwoFactorAuthWindow()
		{
			InitializeComponent();
		}

		private void apply_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}