using System;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;
using log4net.Core;
using log4net.Util;

namespace log4net.Raven.Tests
{
	[TestFixture]
	public class RavenAppenderTests
	{
		private IDocumentStore documentStore;
		private IDocumentSession documentSession;
		private RavenAppender appender;

		[SetUp]
		public void SetUp()
		{
			this.documentStore = new EmbeddableDocumentStore
				{
					Configuration =
						{
							RunInUnreliableYetFastModeThatIsNotSuitableForProduction = true,
							RunInMemory = true,
						}
				};
			this.documentStore.Initialize();

			this.documentSession = this.documentStore.OpenSession();

			this.appender = new RavenAppender(this.documentStore);
		}

		[TearDown]
		public void TearDown()
		{
			this.appender.Close();
		}

		[Test]
		public void DoAppendTest()
		{
			var logEvent = new LoggingEvent(new LoggingEventData { Level = Level.Error, Message = "Hello world" });

			this.appender.DoAppend(logEvent);

			var entry = this.documentSession.Load<Log>(1);

			Assert.That(entry.Message == logEvent.RenderedMessage);
			Assert.That(entry.Level == logEvent.Level.ToString());
		}

		[Test]
		public void DoAppendWithPropertiesTest()
		{
			var properties = new PropertiesDictionary();
			properties["key"] = "value";

			var logEvent = new LoggingEvent(new LoggingEventData { Properties = properties});

			this.appender.DoAppend(logEvent);

			var entry = this.documentSession.Load<Log>(1);

			Assert.That(entry.Properties["key"] == "value");
		}

		[Test]
		public void SmokeIntegrationalTest()
		{
			Config.XmlConfigurator.Configure();

			var logger = LogManager.GetLogger(this.GetType());

			var appenders = logger.Logger.Repository.GetAppenders();
			Assert.IsTrue(appenders.Length > 0, "Seems that Raven Appender is not configured");

			this.appender = appenders[0] as RavenAppender;
			Assert.IsNotNull(appender, "Raven Appender is expected to be the only one configured for tests");

			// Assert
			logger.Info("Log Info");
			logger.Warn("Log Warning");
			logger.Error("Log Warning", new Exception("Something wrong happened", new UserException()));

			LogManager.Shutdown();
		}
	}
}
