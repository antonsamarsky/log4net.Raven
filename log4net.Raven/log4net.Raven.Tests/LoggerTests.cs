using NUnit.Framework;

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
			LogManager.Shutdown();
		}

		[TearDown]
		public void TearDown()
		{
			//this.ClearAllLogs();
		}

		[Test]
		public void SmokeTest()
		{
			log.Info("Log Info");
		}

		/*
		[Test]
		public void LogAndLoadTest()
		{
			const string message = "Log Warning";

			log.Warn(message);

			// Get log
			var entry = this.appender.DocumentSession.Query<Log>().First();

			Assert.AreEqual(entry.Message, message);
			Assert.AreEqual(entry.LoggerName, this.GetType().FullName);
		}

		[Test]
		public void TestException()
		{
			var exception = new Exception("Something wrong happened", new Exception("I'm the inner"));

			const string message = "Log Warning";
			log.Error(message, exception);

			// Get log
			var entry = this.appender.DocumentSession.Query<Log>().First(l => (string)l.Message == message);

			// verify values
			Assert.AreEqual(entry.Level, "ERROR", "Exception not logged with ERROR level");

			var exceptionEntry = entry.Exception;
			Assert.AreEqual(exception, exceptionEntry);
		}

		private void ClearAllLogs()
		{
			var logs = this.appender.DocumentSession.Query<Log>();
			logs.ToList().ForEach(l => this.appender.DocumentSession.Delete(l));
		}*/
	}
}
