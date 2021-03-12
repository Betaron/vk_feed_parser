using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VkNet.Model;
using System.Linq;
using System.Collections;

namespace vk_feed_parser
{
	public class ThreadWorker
	{
		Queue<NewsItem> posts;
		static object getQueueLocker = new object();

		// fields to pass to the method
		static ArrayList dataStorages = new ArrayList()
		{
			new List<PostData.TextData>(),
			new List<PostData.LinksData>(),
			new List<PostData.ImagesData>()
		};
		object[] threadsLokers = new object[dataStorages.Count];
		string[] paths = new string[dataStorages.Count];

		public ThreadWorker(IEnumerable<NewsItem> posts)
		{
			this.posts = new Queue<NewsItem>(posts);
		}

		private Thread GetPackingThread<TData>(
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

		private Thread GetSavingThread()
		{
			return new Thread(() =>
			{
				while (true)
				{
					int targetIndex = GetIndexOfMostFilledStorage();
					lock (threadsLokers[targetIndex])
					{
						switch (targetIndex)
						{
							case 0:
								UniteAndSaveData<PostData.TextData>(targetIndex);
								break;
							case 1:
								UniteAndSaveData<PostData.LinksData>(targetIndex);
								break;
							case 2:
								UniteAndSaveData<PostData.ImagesData>(targetIndex);
								break;
							default: break;
						}
					}
				}
			});
		}

		private int GetIndexOfMostFilledStorage()
		{
			int maxIndex = 0;
			for (int i = 1; i < dataStorages.Count; i++)
			{
				if ((dataStorages[i] as IList).Count > (dataStorages[i-1] as IList).Count)
					maxIndex = i;
			}
			return maxIndex;
		}

		private void UniteAndSaveData<TData>(int index)
		{
			var bufList = FileWorker.LoadFromJsonFile<List<TData>>(paths[index]);
			bufList.AddRange(dataStorages[index] as List<TData>);
			FileWorker.SaveToJsonFile(paths[index], bufList);
		}
	}
}
