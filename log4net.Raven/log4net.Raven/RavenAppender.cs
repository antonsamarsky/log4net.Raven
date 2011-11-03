using System;
using System.Linq;
using Raven.Client;
using Raven.Client.Document;
using log4net.Appender;
using log4net.Core;

namespace log4net.Raven
{
	// ToDO: write own implementation of buffered appender using unit of work: document session.
	public class RavenAppender : BufferingAppenderSkeleton
	{
		private readonly object lockObject = new object();

		private IDocumentStore documentStore;

		private IDocumentSession documentSession;

		#region Appender configuration properties

		/// <summary>
		/// Gets or sets the connection string.
		/// </summary>
		/// <value>
		/// The connection string.
		/// </value>
		public string ConnectionString { get; set; }

		/// <summary>
		/// Gets or sets the max number of requests per session.
		/// By default the number of remote calls to the server per session is limited to 30.
		/// </summary>
		/// <value>
		/// The max number of requests per session.
		/// </value>
		public int MaxNumberOfRequestsPerSession { get; set; }

		#endregion

		protected override void SendBuffer(LoggingEvent[] events)
		{
			if (events == null || !events.Any())
			{
				return;
			}

			this.CheckSession();

			foreach (var entry in events.Where(e => e != null).Select(e => new Log(e)))
			{
				this.documentSession.Store(entry);
			}

			this.Commit();
		}

		public override void ActivateOptions()
		{
			try
			{
				this.InitServer();
			}
			catch (Exception exception)
			{
				ErrorHandler.Error("Exception while initializing Raven Appender", exception, ErrorCode.GenericFailure);
				throw;
			}
		}

		protected override void OnClose()
		{
			this.Flush();
			this.Commit();

			try
			{
				if (this.documentSession != null)
				{
					this.documentSession.Dispose();
				}

				if (this.documentStore != null && !this.documentStore.WasDisposed)
				{
					this.documentStore.Dispose();
				}
			}
			catch (Exception e)
			{
				ErrorHandler.Error("Exception while closing Raven Appender", e, ErrorCode.GenericFailure);
			}

			base.OnClose();
		}

		protected virtual void Commit()
		{
			if (this.documentSession == null)
			{
				return;
			}

			try
			{
				this.documentSession.SaveChanges();
			}
			catch (Exception e)
			{
				ErrorHandler.Error("Exception while commiting to the Raven DB", e, ErrorCode.GenericFailure);
			}
		}

		/// <summary>
		/// IDocumentSession - Instances of this interface are created by the DocumentStore, 
		/// they are cheap to create and not thread safe. 
		/// If an exception is thrown by an IDocumentSession method, 
		/// the behavior of all of the methods (except Dispose) is undefined.
		/// The document session is used to interact with the Raven database, 
		/// load data from the database, query the database, save and delete. 
		/// Instances of this interface implement the Unit of Work pattern and change tracking.
		/// </summary>
		private void CheckSession()
		{
			if (this.documentSession != null)
			{
				return;
			}

			lock (this.lockObject)
			{
				if (this.documentSession != null)
				{
					if (this.documentSession.Advanced.NumberOfRequests >= this.documentSession.Advanced.MaxNumberOfRequestsPerSession)
					{
						this.Commit();
						this.documentSession.Dispose();
					}
					else
					{
						return;
					}
				}

				this.documentSession = this.documentStore.OpenSession();
				this.documentSession.Advanced.UseOptimisticConcurrency = true;

				if (this.MaxNumberOfRequestsPerSession > 0)
				{
					this.documentSession.Advanced.MaxNumberOfRequestsPerSession = this.MaxNumberOfRequestsPerSession;
				}
			}
		}

		/// <summary>
		/// IDocumentStore - This is expensive to create, 
		/// thread safe and should only be created once per application. 
		/// The Document Store is used to create DocumentSessions, 
		/// to hold the conventions related to saving/loading data and 
		/// any other global configuration. 
		/// </summary>
		private void InitServer()
		{
			if (this.documentStore != null)
			{
				return;
			}

			if (string.IsNullOrEmpty(this.ConnectionString))
			{
				var exception = new InvalidOperationException("Connection string is not specified.");
				ErrorHandler.Error("Connection string is not specified.", exception, ErrorCode.GenericFailure);

				return;
			}

			this.documentStore = new DocumentStore
			{
				ConnectionStringName = this.ConnectionString
			};

			this.documentStore.Initialize();
		}
	}
}
