using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using VkNet;
using VkNet.Abstractions.Authorization;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace vk_feed_parser
{
	public class Parser
	{
		public VkApi api;

		public void ApiAuth()
		{
			api = new VkApi(InitDi());

			if (api.IsAuthorized)
			{
				return;
			}

			api.Authorize(new ApiAuthParams
			{
				ApplicationId = 7773939,
				Settings = Settings.All | Settings.Offline
			});
		}

		private static ServiceCollection InitDi()
		{
			var di = new ServiceCollection();

			di.AddSingleton<IAuthorizationFlow, WpfAuthorize>();

			return di;
		}
	}
}
