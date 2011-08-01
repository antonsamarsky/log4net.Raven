using Raven.Client;
using Raven.Client.Document;
using log4net.Appender;
using log4net.Core;

namespace log4net.Raven
{
	/// <summary>
	/// log4net Appender itself
	/// </summary>
	public class RavenAppender : AppenderSkeleton
	{
		public RavenAppender()
		{
			this.Url = @"http://localhost";
		}

		public RavenAppender(string connectionStringName)
		{
			this.ConnectionStringName = connectionStringName;
		}

		public RavenAppender(string connectionStringName, string databaseName)
		{
			this.ConnectionStringName = connectionStringName;
			this.DatabaseName = databaseName;
		}

		public string Url { get; set; }

		public string ConnectionStringName { get; set; }

		public string DatabaseName { get; set; }

		public string CollectionName { get; set; }

		public IDocumentSession DocumentSession { get; private set; }

		public DocumentStore DocumentStore { get; private set; }

		protected override void Append(LoggingEvent loggingEvent)
		{
			var entry = new LogEntry(loggingEvent);
		}
	}
}
