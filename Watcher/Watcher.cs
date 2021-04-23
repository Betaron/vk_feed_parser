﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using System.Timers;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace Watcher
{
	public partial class Watcher : ServiceBase
	{
		private int eventId = 1;

		private Thread taskWorkerThread;
		private bool exitFlag = false;
		private EventWaitHandleSecurity eventWaitHandlerSecurity = new EventWaitHandleSecurity();


		private static EventWaitHandle process_program;
		private static EventWaitHandle process_service;

		private static Timer timer = new Timer() { Interval = 5000 };


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

			eventWaitHandlerSecurity.AddAccessRule(new EventWaitHandleAccessRule
				(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
				EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify, AccessControlType.Allow));

			process_program = new EventWaitHandle(false, EventResetMode.ManualReset, @"Global\Program", 
				out _, eventWaitHandlerSecurity);

			process_service = new EventWaitHandle(false, EventResetMode.ManualReset, @"Global\Service",
				out _, eventWaitHandlerSecurity);

			timer.Elapsed += OnTimer;

			taskWorkerThread = new Thread(() =>
			{
				while (!exitFlag)
				{
					process_service.Reset();
					timer.Start();

					process_program.Set();
					process_service.WaitOne();
					if (timer.Enabled)
						timer.Stop();

					CheckFiles();
				}
			});
			taskWorkerThread.Start();
		}

		/// <summary>
		/// when service is stopping, memory mepped file disposing
		/// </summary>
		protected override void OnStop()
		{
			eventLog.WriteEntry("In OnStop.");

			exitFlag = true;

			process_service.Set();
			process_program.Set();

			taskWorkerThread.Join();
			
			eventLog.WriteEntry("Service is closed.");
		}

		/// <summary>
		/// in the timer event handler, 
		/// counting the number of elements in each of the storages
		/// and writing these quantities to the file takes place 
		/// </summary>
		public void CheckFiles()
		{
			eventLog.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);

			object locker = new object();

			if (File.Exists(WatcherDataPath)) File.Delete(WatcherDataPath);

			List<Thread> countingThreads = new List<Thread>();

			for (int i = 0; i < filesForSaving.Count(); i++)
			{
				countingThreads.Add(GetCountingThread(mutices[i], filesForSaving[i], locker));
				countingThreads[i].Start();
			}

			foreach (var item in countingThreads) { item.Join(); }
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

		private static void OnTimer(object sender, ElapsedEventArgs e) 
		{
			
			if (Process.GetProcessesByName("vk_feed_parser").Count() == 0)
				process_service.Set();
			//timer.Stop();
		}

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
