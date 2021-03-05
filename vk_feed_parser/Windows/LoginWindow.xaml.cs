using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace vk_feed_parser
{
	/// <summary>
	/// Interaction logic for LoginWindow.xaml
	/// </summary>
	public partial class LoginWindow : Window
	{
		private Parser parser;
		private Config config = new Config();

		public LoginWindow(Parser parser)
		{
			InitializeComponent();
			AnalyzeConfig();
			this.parser = parser;
		}

		private void AnalyzeConfig()
		{
			config.ReadConfig();
			if (config.IsStayOnline)
				TokenAutorize();
		}

		private async void Authorize(Action<ApiAuthParams> authMethod , ApiAuthParams @params)
		{
			try
			{
			if (config.IsStayOnline)
				checkBox.IsChecked = true;

			sendBtn.IsEnabled = false;
			connctionScreen.Visibility = Visibility.Visible;

			await Task.Run(() => authMethod(@params));
			}
			catch
			{
				
			}
			finally
			{
			sendBtn.IsEnabled = true;
			connctionScreen.Visibility = Visibility.Hidden;

			if (parser.api.IsAuthorized)
				this.Close();
			}
		}

		private void TokenAutorize()
		{
			try {
			Authorize(parser.api.Authorize, new ApiAuthParams() {
				AccessToken = config.token
			});
			}
			catch
			{

			}
		}

		private void LoginAutorize()
		{
			try
			{
				Authorize(parser.api.Authorize, new ApiAuthParams()
				{
					ApplicationId = config.appId,
					Login = loginField.Text,
					Password = passwordField.Password,
					Settings = Settings.All | Settings.Offline,
					TwoFactorAuthorization = TwoFactorAuth
				});
			}
			catch
			{
			}
		}

		private string TwoFactorAuth()
		{
			string res = string.Empty;
			Action getTwofactor = () => {
				var twoFactorAuthWindow = new TwoFactorAuthWindow();
				res = twoFactorAuthWindow.ShowDialog() == true ?
					twoFactorAuthWindow.passBox.Password : string.Empty;
				
			};
			Dispatcher.Invoke(getTwofactor);
			
			return res;
		}

		private void sendBtn_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(loginField.Text) || string.IsNullOrEmpty(passwordField.Password))
			{
				alertLabel.Visibility = Visibility.Visible;
				return;
			}

			LoginAutorize();

			Thread loginThread = Thread.CurrentThread;

			Thread writeConfigThread = new Thread(() =>
			{
				loginThread.Join();
			if (config.IsStayOnline && parser.api.IsAuthorized)
			{
				config.token = parser.api.Token;
				config.IsStayOnline = true;
				config.WriteConfig(config);
			}
			else
			{
				config.WriteConfig(default);
			}
			});
			writeConfigThread.Start();
			this.Close();
		}
	}
}
