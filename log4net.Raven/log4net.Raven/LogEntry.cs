using System;
using log4net.Core;
using log4net.Util;

namespace log4net.Raven
{
	/// <summary>
	/// The log entry document entity that will be stored to the database.
	/// </summary>
	public class LogEntry : INamedDocument
	{
		public LogEntry()
		{
		}

		public LogEntry(string id)
		{
			this.Id = id;
		}

		public LogEntry(LoggingEvent logEvent)
		{
			this.LoggerName = logEvent.LoggerName;
			this.Domain = logEvent.Domain;
			this.Identity = logEvent.Identity;
			this.Level = logEvent.Level;
			this.LocationInformation = logEvent.LocationInformation;
			this.Fix = logEvent.Fix;
			this.Properties = logEvent.Properties;
			this.ThreadName = logEvent.ThreadName;
			this.UserName = logEvent.UserName;
			this.Message = logEvent.MessageObject;
			this.TimeStamp = logEvent.TimeStamp;
			this.Exception = logEvent.ExceptionObject;
		}

		public LogEntry(string id, LoggingEvent logEvent) : this(logEvent)
		{
		}

		public DateTime TimeStamp { get; set; }

		public object Message { get; set; }

		public object Exception { get; set; }

		public string LoggerName { get; set; }

		public string Domain { get; set; }

		public string Identity { get; set; }

		public Level Level { get; set; }

		public LocationInfo LocationInformation { get; set; }

		public FixFlags Fix { get; set; }

		public PropertiesDictionary Properties { get; set; }

		public string UserName { get; set; }

		public string ThreadName { get; set; }

		#region Implementation of IDocument

		public string Id { get; set; }

		#endregion
	}
}