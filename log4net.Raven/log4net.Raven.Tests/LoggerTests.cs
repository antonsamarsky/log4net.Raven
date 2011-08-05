using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Linq;
using log4net.Raven.Entities;

namespace log4net.Raven.Tests
{
	[TestFixture]
	public class LoggerTests
	{
		private ILog log;

		private RavenAppender appender;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			this.log = LogManager.GetLogger(this.GetType());

			Config.XmlConfigurator.Configure();

			var appenders = log.Logger.Repository.GetAppenders();
			Assert.IsTrue(appenders.Length > 0, "Seems that Raven Appender is not configured");

			this.appender = appenders[0] as RavenAppender;
			Assert.IsNotNull(appender, "Raven Appender is expected to be the only one configured for tests");
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			// ClearCollection();
			LogManager.Shutdown();
		}

		[Test]
		public void SmokeLogTest()
		{
			var message = "Log Info";

			log.Info(message);
		}

		[Test]
		public void WarnAndDeleteTest()
		{
			var message = "Log Warning";

			log.Warn(message);

			var entries = this.appender.DocumentSession.Query<LogEntry>().Where(le => (string)le.Message == message);

			Assert.That(entries.Any());
			Assert.That(entries.Count() == 1);

			var entry = entries.First();
			this.Delete(entry);
		}

		private void Delete(object entity)
		{
			this.appender.DocumentSession.Delete(entity);
		}
	}
}
