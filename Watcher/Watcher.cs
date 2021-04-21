using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Timers;
using Newtonsoft.Json;

namespace Watcher
{
	public partial class Watcher : ServiceBase
	{
		private int eventId = 1;
		//private MemoryMappedFile mmf;
		public static string[] filesForSaving =
		{
			@"D:\VkFeedParser\DataStorages\TextData.json",
			@"D:\VkFeedParser\DataStorages\LinksData.json",
			@"D:\VkFeedParser\DataStorages\ImagesData.json"
		};
		private readonly string WatcherDataPath = @"D:\VkFeedParser\ParsedNewsCounts.txt";

		static Mutex[] mutices = new Mutex[]
		{
			new Mutex(false, @"Global\mutex0"),
			new Mutex(false, @"Global\mutex1"),
			new Mutex(false, @"Global\mutex2"),
		};

		public readonly Action<Mutex> WaitOneAction = (mutex) =>
		{
			try { mutex.WaitOne(); }
			catch (AbandonedMutexException)
			{ mutex.ReleaseMutex(); mutex.WaitOne(); }
		};

		/// <summary>
		/// in default constructor creates logfile
		/// </summary>
		public Watcher()
		{
			InitializeComponent();
			eventLog = new EventLog();
			eventLog.Source = "WatcherService";
			eventLog.Log = "WatcherLog";
			if (!EventLog.SourceExists(eventLog.Source))
				EventLog.CreateEventSource(eventLog.Source, eventLog.Log);
		}

		/// <summary>
		/// at the start of service creates a Global memory mapped file and timer with 5 sec interval
		/// </summary>
		protected override void OnStart(string[] args)
		{
			eventLog.WriteEntry("In OnStart.");

			// Set up a timer that triggers every timer.Interval.
			var timer = new System.Timers.Timer();
			timer.Interval = 5000;
			timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
			timer.Start();
		}

		/// <summary>
		/// when service is stopping, memory mepped file disposing
		/// </summary>
		protected override void OnStop()
		{
			eventLog.WriteEntry("In OnStop.");
		}

		/// <summary>
		/// in the timer event handler, 
		/// counting the number of elements in each of the storages
		/// and writing these quantities to the file takes place 
		/// </summary>
		public void OnTimer(object sender, ElapsedEventArgs args)
		{
			eventLog.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);

			object locker = new object();

			if (File.Exists(WatcherDataPath)) File.Delete(WatcherDataPath);

			for (int i = 0; i < filesForSaving.Count(); i++)
			{
				GetCountingThread(mutices[i], filesForSaving[i], locker).Start();
			}
		}

		/// <summary>
		/// Thread reading the file with posts and count them and write that number at file
		/// </summary>
		/// <param name="mutex">out Mutex, which locking file with posts</param>
		/// <param name="path">Path to parsed data</param>
		/// <param name="locker">loccal locker for locking file with counts of posts</param>
		private Thread GetCountingThread(Mutex mutex, string path, object locker) =>
			new Thread(() =>
			{
				try
				{
					WaitOneAction(mutex);
					eventLog.WriteEntry($"Entering to: {path}", EventLogEntryType.Information, eventId);
					string readedData = File.ReadAllText(path);
					List<object> deszedPosts = JsonConvert.DeserializeObject<List<object>>(readedData);
					int count = deszedPosts.Count;
					if (!File.Exists(path))
					{
						createFullPath(Directory.GetParent(WatcherDataPath).ToString());
						File.Create(WatcherDataPath).Close();
					}
					lock (locker)
					{
						File.AppendAllText(WatcherDataPath, $"{path}: {count}\n");
					}
				}
				catch (Exception ex)
				{
					eventLog.WriteEntry("Countng and writing block: \n" +
						"Exception: " + ex.Message, EventLogEntryType.Error, eventId);
				}
				finally
				{
					mutex.ReleaseMutex();
				}
			});

		public static void createFullPath(string path)
		{
			if (!Directory.Exists(path))
			{
				var parentDir = Directory.GetParent(path);
				if (parentDir != null)
					createFullPath(parentDir.ToString());
				Directory.CreateDirectory(path);
			}
		}
	}
}
