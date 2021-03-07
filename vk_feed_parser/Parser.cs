using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using VkNet;
using VkNet.Abstractions.Authorization;
using VkNet.Model.RequestParams;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace vk_feed_parser
{
	public class Parser
	{
		public VkApi api;
		private string nextFrom = string.Empty;

		public void LoginAuth(ulong appId)
		{
			api = new VkApi(InitDi());

			if (api.IsAuthorized)
			{
				return;
			}

			api.Authorize(new ApiAuthParams
			{
				ApplicationId = appId,
				Settings = Settings.Groups | Settings.Friends | Settings.Offline
			});
		}

		public void TokenAuthorize(string token)
		{
			api = new VkApi();

			api.Authorize(new ApiAuthParams
			{
				AccessToken = token
			});
		}

		private static ServiceCollection InitDi()
		{
			var di = new ServiceCollection();

			di.AddSingleton<IAuthorizationFlow, WpfAuthorize>();

			return di;
		}

		public IEnumerable<NewsItem> GetPostsArr()
		{
			NewsFeed newsFeed = api.NewsFeed.Get(new NewsFeedGetParams() 
			{ 
				Filters = NewsTypes.Post,
				Count = 10,
				StartFrom = nextFrom
			});

			nextFrom = newsFeed.NextFrom;

			return newsFeed.Items;
		}
	}
}
