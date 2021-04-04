using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;

namespace Watcher
{
	public partial class Watcher : ServiceBase
	{
		private int eventId = 1;
		private MemoryMappedFile mmf;
		private readonly string WatcherDataPath = @"C:\Users\agaba\Desktop\ParsedNewsCounts.txt";

		public Watcher()
		{
			InitializeComponent();
			eventLog = new EventLog();
			eventLog.Source = "WatcherService";
			eventLog.Log = "WatcherLog";
			if (!EventLog.SourceExists(eventLog.Source))
				EventLog.CreateEventSource(eventLog.Source, eventLog.Log);
		}

		protected override void OnStart(string[] args)
		{
			eventLog.WriteEntry("In OnStart.");

			var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
			var ace = new AccessRule<MemoryMappedFileRights>(
				sid,
				MemoryMappedFileRights.FullControl,
				AccessControlType.Allow);
			var acl = new MemoryMappedFileSecurity();
			acl.AddAccessRule(ace);
			mmf = MemoryMappedFile.CreateNew(
				"sharedPaths",
				0x100000L,
				MemoryMappedFileAccess.ReadWrite,
				MemoryMappedFileOptions.None,
				acl,
				HandleInheritability.None);

			// Set up a timer that triggers every timer.Interval.
			var timer = new System.Timers.Timer();
			timer.Interval = 5000;
			timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
			timer.Start();
		}

		protected override void OnStop()
		{
			eventLog.WriteEntry("In OnStop.");
			mmf.Dispose();
		}

		public void OnTimer(object sender, ElapsedEventArgs args)
		{
			// TODO: Insert monitoring activities here.
			eventLog.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);

			List<string> paths = new List<string>();
			List<Mutex> mutices = new List<Mutex>();
			object locker = new object();

			if (Process.GetProcessesByName("vk_feed_parser").Count() != 0)
			{
				try
				{
					eventLog.WriteEntry("Enter to try block", EventLogEntryType.Information, eventId);
					using (MemoryMappedFile mmf = MemoryMappedFile.OpenExisting(@"Global\sharedPaths"))
					{
						using (var stream = mmf.CreateViewStream())
						{
							StreamReader reader = new StreamReader(stream);
							stream.Seek(0, SeekOrigin.Begin);
							string ret = reader.ReadToEnd();
							ret = ret.Trim();
							ret = ret.Remove(0, ret.IndexOf('['));
							paths = JsonConvert.DeserializeObject<string[]>(ret).ToList();
							eventLog.WriteEntry("DeserializeObject succeeded", EventLogEntryType.Information, eventId);
						}
					}
				}
				catch (Exception ex)
				{
					eventLog.WriteEntry("Getting data from memory mapped file and deserializing block: \n" +
						"Exception: " + ex.Message, EventLogEntryType.Error, eventId);
				}

				try
				{
					for (int i = 0; i < paths.Count; i++)
					{
						mutices.Add(Mutex.OpenExisting($@"Global\mutex{i}"));
					}
					eventLog.WriteEntry("Getting mutices succeeded", EventLogEntryType.Information, eventId);
				}
				catch (Exception ex)
				{
					eventLog.WriteEntry("Getting mutices block: \n" +
						"Exception: " + ex.Message, EventLogEntryType.Error, eventId);
				}

				if (File.Exists(WatcherDataPath))
					File.Delete(WatcherDataPath);
				for (int i = 0; i < paths.Count; i++)
				{
					GetCountingThread(mutices[i], paths[i], locker).Start();
				}
			}
		}

		/// <summary>
		/// Thread reading the file with posts and count them and write that number at file
		/// </summary>
		/// <param name="mutex">out Mutex, which locking file with posts</param>
		/// <param name="path">Path to parsed data</param>
		/// <param name="locker">loccal locker for locking file with counts of posts</param>
		private Thread GetCountingThread(Mutex mutex, string path, object locker) =>
			new Thread(()=> {
				try
				{
					mutex.WaitOne();
					eventLog.WriteEntry($"Entering to: {path}", EventLogEntryType.Information, eventId);
					string readedData = File.ReadAllText(path);
					List<object> deszedPosts = JsonConvert.DeserializeObject<List<object>>(readedData);
					int count = deszedPosts.Count;
					lock (locker)
					{
						if (File.Exists(path))
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
	}
}
