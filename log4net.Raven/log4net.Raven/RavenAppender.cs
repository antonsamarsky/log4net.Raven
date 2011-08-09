﻿using System;
using System.Linq;
using Raven.Client;
using Raven.Client.Document;
using log4net.Appender;
using log4net.Core;

namespace log4net.Raven
{
	public class RavenAppender : BufferingAppenderSkeleton
	{
		private readonly object lockObject = new object();

		private string databaseName = "Logs"; // Default Database Name

		private DocumentStore documentStore;

		#region Appender configuration properties

		public string ConnectionString { get; set; }

		public string DatabaseName
		{
			get { return this.databaseName; }
			set { this.databaseName = value; }
		}

		// By default the number of remote calls to the server per session is limited to 30.
		public int MaxNumberOfRequestsPerSession { get; set; }

		// public string CollectionName { get; set; }

		#endregion

		public IDocumentSession DocumentSession { get; protected set; }

		protected override void SendBuffer(LoggingEvent[] events)
		{
			this.InitOrCheckSession();

			foreach (var entry in events.Select(loggingEvent => new Log(loggingEvent)))
			{
				this.DocumentSession.Store(entry);
			}

			this.Commit();
		}

		public override void ActivateOptions()
		{
			try
			{
				this.InitServer();
			}
			catch (Exception e)
			{
				ErrorHandler.Error("Exception while initializing Raven Appender", e, ErrorCode.GenericFailure);
			}
		}

		protected override void OnClose()
		{
			this.Commit();

			try
			{
				if (this.DocumentSession != null)
				{
					this.DocumentSession.Dispose();
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
			if (this.DocumentSession == null)
			{
				return;
			}

			try
			{
				this.DocumentSession.SaveChanges();
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
		private void InitOrCheckSession()
		{
			if (this.DocumentSession != null)
			{
				return;
			}

			lock (this.lockObject)
			{
				if (this.DocumentSession != null)
				{
					if (this.DocumentSession.Advanced.NumberOfRequests >= this.DocumentSession.Advanced.MaxNumberOfRequestsPerSession)
					{
						this.DocumentSession.SaveChanges();
						this.DocumentSession.Dispose();
					}
					else
					{
						return;
					}
				}

				this.DocumentSession = this.documentStore.OpenSession();
				this.DocumentSession.Advanced.UseOptimisticConcurrency = true;

				if (this.MaxNumberOfRequestsPerSession > 0)
				{
					this.DocumentSession.Advanced.MaxNumberOfRequestsPerSession = this.MaxNumberOfRequestsPerSession;
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

			lock (this.lockObject)
			{
				if (this.DocumentSession != null)
				{
					return;
				}

				this.documentStore = new DocumentStore
				{
					Identifier = this.DatabaseName,
					//DefaultDatabase = this.DatabaseName,
					ConnectionStringName = this.ConnectionString
				};

				this.documentStore.Initialize();
			}
		}
	}
}
