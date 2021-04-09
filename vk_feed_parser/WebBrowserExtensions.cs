using System.Reflection;
using CefSharp.Wpf;

namespace vk_feed_parser
{
	public static class WebBrowserExtensions
	{
		public static void SetSilent(this ChromiumWebBrowser wb)
		{
			var fiComWebBrowser = typeof(ChromiumWebBrowser)
				.GetField("_axIWebBrowser2",
					BindingFlags.Instance | BindingFlags.NonPublic);

			if (fiComWebBrowser == null)
			{
				return;
			}

			var objComWebBrowser = fiComWebBrowser.GetValue(wb);

			objComWebBrowser?.GetType()
				.InvokeMember("Silent",
					BindingFlags.SetProperty,
					null,
					objComWebBrowser,
					new object[]
					{
						true
					});
		}
	}
}