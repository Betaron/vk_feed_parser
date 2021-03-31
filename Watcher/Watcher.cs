using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Watcher
{
	public partial class Watcher : ServiceBase
	{
		private int eventId = 1;

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
		}

		protected override void OnStart(string[] args)
		{
			eventLog1.WriteEntry("In OnStart.");
			// Set up a timer that triggers every minute.
			Timer timer = new Timer();
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
		}
	}
}
