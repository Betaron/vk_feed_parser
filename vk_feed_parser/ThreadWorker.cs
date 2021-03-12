using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VkNet.Model;
using System.Linq;

namespace vk_feed_parser
{
	public class ThreadWorker
	{
		Queue<NewsItem> posts;
		static object getQueueLocker = new object();

		// fields to pass to the method
		const uint numOfThreads = 3;
		object[] threadsLokers = new object[numOfThreads];
		string[] paths = new string[numOfThreads];
		List<PostData.TextData> textDataList;
		List<PostData.LinksData> linksDataList;
		List<PostData.ImagesData> imagesDataList;


		public ThreadWorker(IEnumerable<NewsItem> posts)
		{
			this.posts = new Queue<NewsItem>(posts);
		}

		public Thread GetPackingThread<TData>(
			object locker, 
			string path, 
			Func<NewsItem, TData> SeparateData, 
			List<TData> dataList
			)
		{
			return new Thread(() =>
			{
				const uint range = 10;
				var bufData = new List<TData>();
				Queue<NewsItem> localPosts;
				lock (getQueueLocker)
				{
					localPosts = posts;
				}
				while (localPosts.Count != 0)
				{
					lock (locker)
					{
						for (int i = 0; i < range; i++)
							bufData.Add(SeparateData(localPosts.Dequeue()));
						var externalData = FileWorker.LoadFromJsonFile<List<TData>>(path);
						dataList.AddRange(bufData.Except(externalData));
					}
				}
			});
		}

		public Thread GetSavingThread()
		{
			return new Thread(() =>
			{
			});
		}
	}
}
