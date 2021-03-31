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
		public bool IsStop = true;
		Queue<NewsItem> posts;
		static object queueLocker = new object();
		List<Thread> packingThreads = new List<Thread>();
		Thread savingThread;

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

		Func<NewsItem, PostData>[] separatingFuncs = new Func<NewsItem, PostData>[]
		{
			PostData.SeparateText,
			PostData.SeparateLinks,
			PostData.SeparateImages
		};

		// fields to pass to the method
		static List<PostData>[] dataStorages = new List<PostData>[]
		{
			new List<PostData>(),
			new List<PostData>(),
			new List<PostData>()
		};

		private Thread GetPackingThread(
			object locker,
			string path,
			Func<NewsItem, PostData> SeparateData,
			List<PostData> dataList
			)
		{
			return new Thread(() =>
			{
				const uint range = 10;
				var bufData = new List<PostData>();
				Queue<NewsItem> localPosts;
				lock (queueLocker)
				{
					localPosts = new Queue<NewsItem>(posts);
				}
				while (localPosts.Count != 0 && !IsStop)
				{
					lock (locker)
					{
						for (int i = 0; i < range; i++)
							if (localPosts.Count != 0)
								bufData.Add(SeparateData(localPosts.Dequeue()));
						var externalData = FileWorker.LoadFromJsonFile<List<PostData>>(path);
						dataList.AddRange(bufData.Except(
							externalData ?? new List<PostData>(),
							new PostDataEqualityComparer()
							));
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
				while (!IsStop)
				{
					for (int targetIndex = 0; targetIndex < threadsLokers.Length; targetIndex++)
					{
						lock (threadsLokers[targetIndex])
						{
							UniteAndSaveData(dataStorages[targetIndex], paths[targetIndex]);
							dataStorages[targetIndex].Clear();
						}
						if (CheckStopCondition()) StopNewsSaving();
					}
				}
			})
			{ Name = "saver.exe" };
		}

		private void UniteAndSaveData(List<PostData> storage, string path)
		{
			var bufList = FileWorker.LoadFromJsonFile<List<PostData>>(path) ?? new List<PostData>();
			bufList.AddRange(storage);
			FileWorker.SaveToJsonFile(path, bufList);
		}

		private bool CheckStopCondition()
		{
			if (packingThreads.Count == 0)
				return true;
			for (int i = 0; i < threadsLokers.Length; i++)
			{
				if (packingThreads[i].IsAlive || (dataStorages[i]).Count != 0)
					return false;
			}
			return true;
		}

		public void StartNewsSaving(IEnumerable<NewsItem> posts)
		{
			IsStop = false;
			this.posts = new Queue<NewsItem>(posts);

			for (int i = 0; i < threadsLokers.Length; i++)
			{
				packingThreads.Add(GetPackingThread(
					threadsLokers[i],
					paths[i],
					separatingFuncs[i],
					dataStorages[i]
					));
			}
			savingThread = GetSavingThread();

			new Thread((object p)=> 
			{
				foreach (var item in p as IEnumerable<NewsItem>)
				{
					UIWorker.AddRecord($"{item.SourceId}_{item.PostId}");
				}
			}).Start(posts);

			foreach (var item in packingThreads) item.Start();
			savingThread.Start();
		}

		private void StopNewsSaving()
		{
			posts.Clear();
			packingThreads.Clear();
			IsStop = true;
		}
	}
}
