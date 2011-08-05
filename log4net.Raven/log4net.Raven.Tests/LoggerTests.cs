using NUnit.Framework;
using Raven.Client;

namespace log4net.Raven.Tests
{
	[TestFixture]
	public class LoggerTests
	{
		private ILog log;

		private RavenAppender appender;

		private IDocumentSession documentSession;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			this.log = LogManager.GetLogger(this.GetType());

			Config.XmlConfigurator.Configure();

			var appenders = log.Logger.Repository.GetAppenders();
			Assert.IsTrue(appenders.Length > 0, "Seems that Raven Appender is not configured");

			appender = appenders[0] as RavenAppender;
			Assert.IsNotNull(appender, "Raven Appender is expected to be the only one configured for tests");

			documentSession = appender.DocumentSession;
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
			log.Info("Log Info");
		}

		protected void ClearCollection()
		{
			//documentSession.
		}
	}
}
