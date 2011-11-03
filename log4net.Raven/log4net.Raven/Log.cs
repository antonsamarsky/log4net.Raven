using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Core;

namespace log4net.Raven
{
	/// <summary>
	/// The log entry document entity that will be stored to the database.
	/// </summary>
	public class Log : INamedDocument
	{
		public Log()
		{
		}

		public Log(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException("id");
			}

			this.Id = id;
		}

		public Log(LoggingEvent logEvent)
		{
			if (logEvent == null)
			{
				throw new ArgumentNullException("logEvent");
			}

			this.LoggerName = logEvent.LoggerName;
			this.Domain = logEvent.Domain;
			this.Identity = logEvent.Identity;
			this.Level = logEvent.Level.ToString();
			this.ClassName = logEvent.LocationInformation.ClassName;
			this.FileName = logEvent.LocationInformation.FileName;
			this.LineNumber = logEvent.LocationInformation.LineNumber;
			this.FullInfo = logEvent.LocationInformation.FullInfo;
			this.MethodName = logEvent.LocationInformation.MethodName;
			this.ThreadName = logEvent.ThreadName;
			this.UserName = logEvent.UserName;
			this.Message = logEvent.MessageObject;
			this.TimeStamp = logEvent.TimeStamp;
			this.Exception = logEvent.ExceptionObject;
			this.Fix = logEvent.Fix.ToString();
			// Raven doesn't serialize unknown types like log4net's PropertiesDictionary
			this.Properties = logEvent.Properties.GetKeys().ToDictionary(key => key, key => logEvent.Properties[key].ToString());
		}

		public Log(string id, LoggingEvent logEvent) : this(logEvent)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException("id");
			}

			this.Id = id;
		}

		public DateTime TimeStamp { get; set; }

		public object Message { get; set; }

		public object Exception { get; set; }

		public string LoggerName { get; set; }

		public string Domain { get; set; }

		public string Identity { get; set; }

		public string Level { get; set; }

		public string ClassName { get; set; }

		public string FileName { get; set; }

		public string LineNumber { get; set; }

		public string FullInfo { get; set; }

		public string MethodName { get; set; }

		public string Fix { get; set; }

		public IDictionary<string, string> Properties { get; set; }

		public string UserName { get; set; }

		public string ThreadName { get; set; }

		#region Implementation of IDocument

		public string Id { get; set; }

		#endregion
	}
}