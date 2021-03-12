using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VkNet.Model;

namespace vk_feed_parser
{
	public class ThreadWorker
	{
		Queue<NewsItem> posts;
		static object queueGetLocker = new object();

		public ThreadWorker(IEnumerable<NewsItem> posts)
		{
			this.posts = new Queue<NewsItem>(posts);
		}

		public Thread GetPackingThread()
		{
			return new Thread(() =>
			{
				Queue<NewsItem> localPosts;
				lock (queueGetLocker)
				{
					localPosts = posts;
				}
			});
		}
	}
}
