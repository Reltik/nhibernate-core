#if NET_4_5
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.DialectTest
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class DialectFixtureAsync
	{
		protected Dialect.Dialect d = null;
		private const int BeforeQuoteIndex = 0;
		private const int AfterQuoteIndex = 1;
		private const int AfterUnquoteIndex = 2;
		protected string[] tableWithNothingToBeQuoted;
		// simulating a string already enclosed in the Dialects quotes of Quote"d[Na$` 
		// being passed in that should be returned as Quote""d[Na$` - notice the "" before d
		protected string[] tableAlreadyQuoted;
		// simulating a string that has NOT been enclosed in the Dialects quotes and needs to 
		// be.
		protected string[] tableThatNeedsToBeQuoted;
		[SetUp]
		public virtual void SetUp()
		{
			// Generic Dialect inherits all of the Quoting functions from
			// Dialect (which is abstract)
			d = new GenericDialect();
			tableWithNothingToBeQuoted = new string[]{"plainname", "\"plainname\""};
			tableAlreadyQuoted = new string[]{"\"Quote\"\"d[Na$`\"", "\"Quote\"\"d[Na$`\"", "Quote\"d[Na$`"};
			tableThatNeedsToBeQuoted = new string[]{"Quote\"d[Na$`", "\"Quote\"\"d[Na$`\"", "Quote\"d[Na$`"};
		}

		[Test]
		public async Task CurrentTimestampSelectionAsync()
		{
			var conf = TestConfigurationHelper.GetDefaultConfiguration();
			Dialect.Dialect dialect = Dialect.Dialect.GetDialect(conf.Properties);
			if (!dialect.SupportsCurrentTimestampSelection)
			{
				Assert.Ignore("This test does not apply to " + dialect.GetType().FullName);
			}

			var sessions = (ISessionFactoryImplementor)conf.BuildSessionFactory();
			sessions.ConnectionProvider.Configure(conf.Properties);
			IDriver driver = sessions.ConnectionProvider.Driver;
			using (DbConnection connection = await (sessions.ConnectionProvider.GetConnectionAsync()))
			{
				DbCommand statement = driver.GenerateCommand(CommandType.Text, new SqlString(dialect.CurrentTimestampSelectString), new SqlType[0]);
				statement.Connection = connection;
				using (DbDataReader reader = await (statement.ExecuteReaderAsync()))
				{
					Assert.That(await (reader.ReadAsync()), "should return one record");
					Assert.That(reader[0], Is.InstanceOf<DateTime>());
				}
			}
		}
	}
}
#endif
