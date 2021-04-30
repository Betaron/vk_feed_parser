using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using VkNet;
using VkNet.Abstractions.Authorization;
using VkNet.Model.RequestParams;
using VkNet.Enums.Filters;
using VkNet.Model;
using System.Threading;

namespace vk_feed_parser
{
	public class Parser
	{
		public VkApi api;
		public bool IsShutdown = false;
		private string nextFrom = string.Empty;

		/// <summary>
		/// login with browser
		/// </summary>
		/// <param name="appId">registred application ID</param>
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

		/// <summary>
		/// login with token
		/// </summary>
		/// <param name="token">private VK account token</param>
		public void TokenAuthorize(string token)
		{
			api = new VkApi();

			api.Authorize(new ApiAuthParams
			{
				AccessToken = token
			});
		}

		/// <summary>
		/// support method for login with browser methof
		/// </summary>
		private static ServiceCollection InitDi()
		{
			var di = new ServiceCollection();

			di.AddSingleton<IAuthorizationFlow, WpfAuthorize>();

			return di;
		}

		/// <summary>
		/// gets specified number of posts
		/// </summary>
		/// <param name="newsCount">the number of posts requested</param>
		/// <returns>returns the received posts</returns>
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
		/// gets a NewsFeed with the specified number of posts
		/// </summary>
		/// <param name="count">the number of posts requested. Less than or equal to 100</param>
		/// <returns>the received NewsFeed</returns>
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

		//	https://github.com/vudeam - is owner of this method idea
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

		/// <summary>
		/// thread, which starts parsing process
		/// </summary>
		/// <returns>new parsing thread</returns>
		public Thread GetParseThread()
		{
			return new Thread(() =>
			{
				ThreadWorker.parseStateOn.Set();
				for (int i = 0; i < 10; i++)
				{
					if (IsShutdown)
					{
						ThreadWorker.IsStop = true;
						break;
					}
					ThreadWorker.StartNewsSaving(GetPostsList(10));
					Thread.Sleep(200);
				}
				new Thread(() => { 
					if (ThreadWorker.savingThread != null) 
						ThreadWorker.savingThread.Join();
					ThreadWorker.parseStateOff.Set();
					ThreadWorker.process_service.Set();
					UIWorker.AddRecord("Parsing ended!!!");
				}).Start();
			});
		}

		public void ThreadsShutdown()
		{
			IsShutdown = true;
			ThreadWorker.IsStop = true;
			ThreadWorker.process_program.Set();
			ThreadWorker.process_service.Set();
			ThreadWorker.parseStateOff.Set();
			foreach (var item in ThreadWorker.packingThreads) 
				if (item != null) item.Join();
			if (ThreadWorker.savingThread != null) ThreadWorker.savingThread.Join();
		}
	}
}
