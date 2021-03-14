using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VkNet.Model;
using System.Linq;
using System.Collections;
using System.IO;
using System.Diagnostics.CodeAnalysis;

namespace vk_feed_parser
{
	public class ThreadWorker
	{
		bool STOP = false;
		Queue<NewsItem> posts;
		static object getQueueLocker = new object();
		Thread[] packingThreads;

		// fields to pass to the method
		static ArrayList dataStorages = new ArrayList()
		{
			new List<PostData.TextData>(),
			new List<PostData.LinksData>(),
			new List<PostData.ImagesData>()
		};
		static object[] threadsLokers = new object[]
		{
			new object(),
			new object(),
			new object()
		};
		string[] paths = new string[]
		{
			Path.Combine(Directory.GetCurrentDirectory(), "DataStorages", "TextData.json"),
			Path.Combine(Directory.GetCurrentDirectory(), "DataStorages", "LinksData.json"),
			Path.Combine(Directory.GetCurrentDirectory(), "DataStorages", "ImagesData.json")
		};

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
					localPosts = new Queue<NewsItem>(posts);
				}
				while (localPosts.Count != 0 && !STOP)
				{
					lock (locker)
					{
						for (int i = 0; i < range; i++)
							if (localPosts.Count != 0)
								bufData.Add(SeparateData(localPosts.Dequeue()));
						var externalData = FileWorker.LoadFromJsonFile<List<TData>>(path);
						dataList.AddRange(bufData.Except(externalData ?? new List<TData>()));
						bufData.Clear();
					}
					Thread.Sleep(5);
				}
			})
			{ Name = "packer.exe" };
		}

		private Thread GetSavingThread()
		{
			return new Thread(() =>
			{
				Thread.Sleep(50);
				while (!STOP)
				{
					for (int targetIndex = 0; targetIndex < dataStorages.Count; targetIndex++)
					{
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
							(dataStorages[targetIndex] as IList).Clear();
						}
						if (CheckStopCondition()) StopNewsSaving();
					}
				}
			})
			{ Name = "saver.exe" };
		}

		private void UniteAndSaveData<TData>(int index)
		{
			var bufList = FileWorker.LoadFromJsonFile<List<TData>>(paths[index]) ?? new List<TData>();
			bufList.AddRange(dataStorages[index] as List<TData>);
			FileWorker.SaveToJsonFile(paths[index], bufList);
		}

		private bool CheckStopCondition()
		{
			for (int i = 0; i < dataStorages.Count; i++)
			{
				if (packingThreads[i].IsAlive || (dataStorages[i] as IList).Count != 0)
					return false;
			}
			return true;
		}

		public void StartNewsSaving()
		{
			packingThreads = new Thread[]
			{
				GetPackingThread(
					threadsLokers[0],
					paths[0],
					PostData.SeparateText,
					dataStorages[0] as List<PostData.TextData>),
				GetPackingThread(
					threadsLokers[1],
					paths[1],
					PostData.SeparateLinks,
					dataStorages[1] as List<PostData.LinksData>),
				GetPackingThread(
					threadsLokers[2],
					paths[2],
					PostData.SeparateImages,
					dataStorages[2] as List<PostData.ImagesData>)
			};
			var savingThread = GetSavingThread();

			foreach (var item in packingThreads)
			{
				item.Start();
			}

			savingThread.Start();
		}

		public void StopNewsSaving() => STOP = true;
	}
}
