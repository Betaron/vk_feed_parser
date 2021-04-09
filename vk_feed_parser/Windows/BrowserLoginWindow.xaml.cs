using System.Windows;

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
