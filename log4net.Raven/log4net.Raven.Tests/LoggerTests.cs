using System;
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
			this.ClearDatabase();
		}

		[Test]
		public void SmokeLogTest()
		{
			log.Info("Log Info");
		}

		[Test]
		public void WarnAndDeleteTest()
		{
			var message = "Log Warning";

			log.Warn(message);

			// Get log
			var entry = this.appender.DocumentSession.Load<Log>(1);

			Assert.AreEqual(entry.Message, message);
			Assert.AreEqual(entry.LoggerName, this.GetType().FullName);
		}

		[Test]
		public void TestException()
		{
			var exception = new Exception("Something wrong happened", new Exception("I'm the inner"));

			log.Error("I'm sorry", exception);

			// Get log
			var entry = this.appender.DocumentSession.Load<Log>(1);

			// verify values
			Assert.AreEqual(entry.Level, "ERROR", "Exception not logged with ERROR level");

			var exceptionEntry = entry.Exception;
			Assert.AreEqual(exception, exceptionEntry);
		}

		private void ClearDatabase()
		{
			//this.appender.DocumentSession.Advanced.DocumentStore.
		}
	}
}
