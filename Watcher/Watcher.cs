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

		public Watcher()
		{
			InitializeComponent();
			eventLog1 = new System.Diagnostics.EventLog();
			if (!System.Diagnostics.EventLog.SourceExists("MySource"))
			{
				System.Diagnostics.EventLog.CreateEventSource("MySource", "MyNewLog");
			}
			eventLog1.Source = "MySource";
			eventLog1.Log = "MyNewLog";


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
		}

		~Watcher()
		{
			mmf.Dispose();
		}

		protected override void OnStart(string[] args)
		{
			eventLog1.WriteEntry("In OnStart.");
			// Set up a timer that triggers every minute.
			var timer = new System.Timers.Timer();
			timer.Interval = 5000;
			timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
			timer.Start();
		}

		protected override void OnStop()
		{
			eventLog1.WriteEntry("In OnStop.");
		}

		public void OnTimer(object sender, ElapsedEventArgs args)
		{
			// TODO: Insert monitoring activities here.
			eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);

			List<Mutex> mutices = new List<Mutex>();
			List<string> paths = new List<string>();

			if (Process.GetProcessesByName("vk_feed_parser").Count() != 0)
			{
				try
				{
					eventLog1.WriteEntry("Enter to try block", EventLogEntryType.Information, eventId);
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
							eventLog1.WriteEntry("DeserializeObject sucseed", EventLogEntryType.Information, eventId);
						}

						File.WriteAllText(@"C:\Users\agaba\Desktop\paths.txt", $"{paths[0]}");
						
					}
				}
				catch (Exception ex)
				{
					eventLog1.WriteEntry("Exception: " + ex.Message, EventLogEntryType.Error, eventId);
				}
			}
		}
	}
}
