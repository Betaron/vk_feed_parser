using System;
using System.Collections.Generic;
using System.Threading;
using VkNet.Model;
using System.Linq;
using System.IO;
using System.IO.MemoryMappedFiles;
using Newtonsoft.Json;

namespace vk_feed_parser
{
	public class ThreadWorker
	{
		public static bool IsStop = true;
		Queue<NewsItem> posts;
		static object queueLocker = new object();
		List<Thread> packingThreads = new List<Thread>();
		Thread savingThread;

		public readonly Action<Mutex> WaitOneAction = (mutex) =>
		{
			try { mutex.WaitOne(); }
			catch (AbandonedMutexException)
			{ mutex.ReleaseMutex(); mutex.WaitOne(); }
		};

		// mutises for sinchronize saving and writing threads
		static Mutex[] mutices = new Mutex[]
		{
			new Mutex(false, @"Global\mutex0"),
			new Mutex(false, @"Global\mutex1"),
			new Mutex(false, @"Global\mutex2"),
		};

		// paths to saving separated data
		static string[] paths = new string[]
		{
			@"D:\VkFeedParser\DataStorages\TextData.json",
			@"D:\VkFeedParser\DataStorages\LinksData.json",
			@"D:\VkFeedParser\DataStorages\ImagesData.json"
		};

		// methods for separating raw data
		static Func<NewsItem, PostData>[] separatingFuncs = new Func<NewsItem, PostData>[]
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

		/// <summary>
		/// method creating a thread, which packing raw data to ready to save json format
		/// </summary>
		/// <param name="mutex">mutex for sinchronize saving and this writing threads</param>
		/// <param name="path">path to file, in which containing the parsed data</param>
		/// <param name="SeparateData">func for separating data</param>
		/// <param name="dataList">buffer storage for collecting data. This storage imagine the queue for saving into file</param>
		/// <returns>packing thread</returns>
		private Thread GetPackingThread(
			Mutex mutex,
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
					WaitOneAction(mutex);
					{
						for (int i = 0; i < range; i++)
							if (localPosts.Count != 0)
								bufData.Add(SeparateData(localPosts.Dequeue()));
						var externalData = new List<PostData>();
						if (File.Exists(path))
							externalData = FileWorker.LoadFromJsonFile<List<PostData>>(path);
						dataList.AddRange(bufData.Except(
							externalData ?? new List<PostData>(),
							new PostDataEqualityComparer()
							));
						bufData.Clear();
					}
					mutex.ReleaseMutex();
					Thread.Sleep(5);
				}
			})
			{ Name = "packer.exe" };
		}

		/// <summary>
		/// create thread, which saving previously prepeared data to corresponding file
		/// </summary>
		/// <returns>saving thread</returns>
		private Thread GetSavingThread()
		{
			return new Thread(() =>
			{
				Thread.Sleep(50);
				while (!IsStop)
				{
					for (int targetIndex = 0; targetIndex < mutices.Length; targetIndex++)
					{
						WaitOneAction(mutices[targetIndex]);
						{
							UniteAndSaveData(dataStorages[targetIndex], paths[targetIndex]);
							dataStorages[targetIndex].Clear();
						}
						mutices[targetIndex].ReleaseMutex();
						if (CheckStopCondition()) StopNewsSaving();
					}
				}
			})
			{ Name = "saver.exe" };
		}

		/// <summary>
		/// writing post data to JSON file
		/// </summary>
		/// <param name="storage">buffer storage for collecting data. This storage imagine the queue for saving into file</param>
		/// <param name="path">path to file, in which containing the parsed data</param>
		private void UniteAndSaveData(List<PostData> storage, string path)
		{
			var bufList = new List<PostData>();
			if (File.Exists(path))
				bufList = FileWorker.LoadFromJsonFile<List<PostData>>(path);
			bufList.AddRange(storage);
			FileWorker.SaveToJsonFile(path, bufList);
		}

		/// <summary>
		/// check run condition of parsing process
		/// </summary>
		/// <returns>if programm is stop - true, else - false</returns>
		private bool CheckStopCondition()
		{
			if (packingThreads.Count == 0)
				return true;
			for (int i = 0; i < mutices.Length; i++)
			{
				if (packingThreads[i].IsAlive || (dataStorages[i]).Count != 0)
					return false;
			}
			return true;
		}

		/// <summary>
		/// starting the all packing and one saving thread
		/// </summary>
		/// <param name="posts">raw posts, right from vk</param>
		public void StartNewsSaving(IEnumerable<NewsItem> posts)
		{
			IsStop = false;
			this.posts = new Queue<NewsItem>(posts);

			for (int i = 0; i < mutices.Length; i++)
			{
				packingThreads.Add(GetPackingThread(
					mutices[i],
					paths[i],
					separatingFuncs[i],
					dataStorages[i]
					));
			}
			savingThread = GetSavingThread();

			new Thread((object p) =>
			{
				foreach (var item in p as IEnumerable<NewsItem>)
				{
					if (!IsStop)
						UIWorker.AddRecord($"{item.SourceId}_{item.PostId}");
				}
			}).Start(posts);

			foreach (var item in packingThreads) item.Start();
			savingThread.Start();
		}

		/// <summary>
		/// Clears raw posts and array of packing threads and change run state
		/// </summary>
		private void StopNewsSaving()
		{
			posts.Clear();
			packingThreads.Clear();
			IsStop = true;
		}
	}
}
