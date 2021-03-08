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

		public IEnumerable<NewsItem> GetPostsArr(ushort newsCount)
		{
			NewsFeed newsFeed = api.NewsFeed.Get(new NewsFeedGetParams() 
			{ 
				Filters = NewsTypes.Post,
				Count = newsCount,
				StartFrom = nextFrom
			});

			nextFrom = newsFeed.NextFrom;

			return newsFeed.Items;
		}

		//	https://github.com/vudeam - is owner of this method idea.
		private List<TAttType> GetAttachments<TAttType>(NewsItem post) where TAttType : class
		{
			if (post != null)
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


		public PostData SeparatePost(NewsItem post)
		{
			string id = $"{post.SourceId}_{post.PostId}";
			List<string> Images =
				(from item in GetAttachments<Photo>(post)
				 select item.Sizes.Last().Url.ToString()).ToList();

			List<string> Links =
				(from item in GetAttachments<Link>(post)
				 select item.Uri.ToString()).ToList();

			PostData postData = new PostData()
			{
				textData = {
					postID = id,
					postText = post.Text
				},
				imagesData =
				{
					postID = id,
					postImages = Images
				},
				linksData =
				{
					postID = id,
					postLinks = Links					
				}
			};

			return postData;
		}

		public Thread GetParseThread()
		{
			var thread = new Thread(() =>
			{
				var postsDataList = (from item in GetPostsArr(5)
				 select SeparatePost(item)).ToList();
				foreach (var i in postsDataList)
				{
					UIWorker.AddRecord(i.ToString());
				}
			});

			return thread;
		}
	}
}
