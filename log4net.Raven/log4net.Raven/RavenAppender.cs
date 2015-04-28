using System;
using System.Linq;
using Raven.Client;
using Raven.Client.Document;
using log4net.Appender;
using log4net.Core;
using System.Threading.Tasks;
using System.Threading;

namespace log4net.Raven
{
	// ToDO: write own implementation of buffered appender using unit of work: document session.
    public class RavenAppender : BufferingAppenderSkeleton
    {

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

        /// <summary>
        /// Initializes a new instance of the <see cref="RavenAppender"/> class.
        /// </summary>
        public RavenAppender() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RavenAppender"/> class.
        /// </summary>
        /// <param name="documentStore">The document store.</param>
        public RavenAppender(IDocumentStore documentStore)
        {
            if (documentStore == null)
            {
                throw new ArgumentNullException("documentStore");
            }

            this.documentStore = documentStore;
        }

        protected override async void SendBuffer(LoggingEvent[] events)
        {
            if (events == null || !events.Any())
            {
                return;
            }

            await Task.Run(() =>
            {
                try
                {

                    using (documentSession = this.documentStore.OpenSession())
                    {
                        foreach (var entry in events.Where(e => e != null).Select(e => new Log(e)))
                        {
                            documentSession.Store(entry);
                        }
                        documentSession.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ErrorHandler.Error("Exception while commiting to the Raven DB", e, ErrorCode.GenericFailure);
                }
            });

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
                // throw;
            }
        }

        protected override void OnClose()
        {
            this.Flush();

            try
            {
                
                if (documentSession != null && documentSession.Advanced.HasChanges)
                {
                    while (documentSession.Advanced.HasChanges)
                    {
                        Thread.Sleep(500);
                    }
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

            this.documentStore = new Lazy<IDocumentStore>(CreateStore).Value;
        }

        private IDocumentStore CreateStore()
        {
            IDocumentStore store = new DocumentStore()
            {
                ConnectionStringName = this.ConnectionString
            }.Initialize();

            return store;
        }
    }
}
