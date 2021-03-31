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
using VkNet.Model.Attachments;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media;

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
				Settings = Settings.Wall | Settings.Groups | Settings.Friends | Settings.Offline
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

		/// <summary>
		/// Gets specified number of posts.
		/// </summary>
		/// <param name="newsCount">The number of posts requested.</param>
		/// <returns>Returns the received posts.</returns>
		public List<NewsItem> GetPostsList(uint newsCount)
		{
			if (newsCount == 0) return new List<NewsItem>();

			var newsItems = new List<NewsItem>();
			uint requestNumber;
			ushort residualAmount;
			if (newsCount > 100)
			{
				requestNumber = newsCount / 100;
				residualAmount = (ushort)(newsCount - requestNumber * 100);
			}
			else
			{
				requestNumber = 0;
				residualAmount = (ushort)newsCount;
			}
			for (uint i = 0; i < requestNumber; i++)
				newsItems.AddRange(GetNewsFeed(100).Items.ToList());
			if (residualAmount > 0)
				newsItems.AddRange(GetNewsFeed(residualAmount).Items.ToList());
			return newsItems;
		}

		/// <summary>
		/// Gets a NewsFeed with the specified number of posts.
		/// </summary>
		/// <param name="count">The number of posts requested. Less than or equal to 100.</param>
		/// <returns>Returns the received NewsFeed.</returns>
		private NewsFeed GetNewsFeed(ushort count)
		{
			NewsFeed newsFeed = api.NewsFeed.Get(new NewsFeedGetParams()
			{
				Filters = NewsTypes.Post,
				Count = count,
				StartFrom = nextFrom
			});
			nextFrom = newsFeed.NextFrom;

			return newsFeed;
		}

		//	https://github.com/vudeam - is owner of this method idea.
		public static List<TAttType> GetAttachments<TAttType>(NewsItem post) where TAttType : class
		{
			if (post.Attachments != null)
			{
				return (from item in post.Attachments
						where item.Type == typeof(TAttType)
						select item.Instance as TAttType).ToList();
			}
			else
			{
				return new List<TAttType>();
			}
		}

		public Thread GetParseThread()
		{
			var thread = new Thread(() =>
			{
				ThreadWorker worker = new ThreadWorker();
				for (int i = 0; i < 1; i++)
				{
					worker.StartNewsSaving(GetPostsList(100));
					while (!worker.IsStop) { }
				}
				UIWorker.AddRecord("Parsing ended!!!");
			});
			return thread;
		}
	}
}
