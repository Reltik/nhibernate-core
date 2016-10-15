﻿#if NET_4_5
using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg;
using NHibernate.Engine;
using NHibernate.Mapping;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using System.Threading.Tasks;
using Exception = System.Exception;
using NHibernate.Util;

namespace NHibernate.Test.Tools.hbm2ddl.SchemaMetadataUpdaterTest
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class SchemaMetadataUpdaterFixtureAsync
	{
		[Test]
		public async Task CanRetrieveReservedWordsAsync()
		{
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			var dialect = Dialect.Dialect.GetDialect(configuration.Properties);
			var connectionHelper = new ManagedProviderConnectionHelper(configuration.Properties);
			await (connectionHelper.PrepareAsync());
			try
			{
				var metaData = dialect.GetDataBaseSchema(connectionHelper.Connection);
				var reserved = metaData.GetReservedWords();
				Assert.That(reserved, Is.Not.Empty);
				Assert.That(reserved, Has.Member("SELECT"));
				Assert.That(reserved, Has.Member("FROM"));
			}
			finally
			{
				connectionHelper.Release();
			}
		}

		[Test]
		public async Task UpdateReservedWordsInDialectAsync()
		{
			var reservedDb = new HashSet<string>();
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			var dialect = Dialect.Dialect.GetDialect(configuration.Properties);
			var connectionHelper = new ManagedProviderConnectionHelper(configuration.Properties);
			await (connectionHelper.PrepareAsync());
			try
			{
				var metaData = dialect.GetDataBaseSchema(connectionHelper.Connection);
				foreach (var rw in metaData.GetReservedWords())
				{
					reservedDb.Add(rw.ToLowerInvariant());
				}
			}
			finally
			{
				connectionHelper.Release();
			}

			var sf = (ISessionFactoryImplementor)configuration.BuildSessionFactory();
			await (SchemaMetadataUpdater.UpdateAsync(sf));
			var match = reservedDb.Intersect(sf.Dialect.Keywords);
			Assert.That(match, Is.EquivalentTo(reservedDb));
		}

		[Test]
		public async Task ExplicitAutoQuoteAsync()
		{
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			configuration.AddResource("NHibernate.Test.Tools.hbm2ddl.SchemaMetadataUpdaterTest.HeavyEntity.hbm.xml", GetType().Assembly);
			await (SchemaMetadataUpdater.QuoteTableAndColumnsAsync(configuration));
			var cm = configuration.GetClassMapping(typeof (Order));
			Assert.That(cm.Table.IsQuoted);
			var culs = new List<Column>(cm.Table.ColumnIterator);
			Assert.That(GetColumnByName(culs, "From").IsQuoted);
			Assert.That(GetColumnByName(culs, "And").IsQuoted);
			Assert.That(GetColumnByName(culs, "Select").IsQuoted);
			Assert.That(!GetColumnByName(culs, "Name").IsQuoted);
		}

		[Test]
		public async Task AutoQuoteTableAndColumnsAtStratupIncludeKeyWordsImportAsync()
		{
			var reservedDb = new HashSet<string>();
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			var dialect = Dialect.Dialect.GetDialect(configuration.Properties);
			var connectionHelper = new ManagedProviderConnectionHelper(configuration.Properties);
			await (connectionHelper.PrepareAsync());
			try
			{
				var metaData = dialect.GetDataBaseSchema(connectionHelper.Connection);
				foreach (var rw in metaData.GetReservedWords())
				{
					reservedDb.Add(rw.ToLowerInvariant());
				}
			}
			finally
			{
				connectionHelper.Release();
			}

			configuration.SetProperty(Environment.Hbm2ddlKeyWords, "auto-quote");
			configuration.AddResource("NHibernate.Test.Tools.hbm2ddl.SchemaMetadataUpdaterTest.HeavyEntity.hbm.xml", GetType().Assembly);
			var sf = (ISessionFactoryImplementor)configuration.BuildSessionFactory();
			var match = reservedDb.Intersect(sf.Dialect.Keywords);
			Assert.That(match, Is.EquivalentTo(reservedDb));
		}

		private static Column GetColumnByName(IEnumerable<Column> columns, string colName)
		{
			return columns.FirstOrDefault(column => column.Name.Equals(colName));
		}

		[Test]
		public async Task CanWorkWithAutoQuoteTableAndColumnsAtStratupAsync()
		{
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			configuration.SetProperty(Environment.Hbm2ddlKeyWords, "auto-quote");
			configuration.SetProperty(Environment.Hbm2ddlAuto, "create-drop");
			configuration.AddResource("NHibernate.Test.Tools.hbm2ddl.SchemaMetadataUpdaterTest.HeavyEntity.hbm.xml", GetType().Assembly);
			var sf = configuration.BuildSessionFactory();
			using (ISession s = sf.OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					await (s.SaveAsync(new Order{From = "from", Column = "column", And = "order"}));
					await (t.CommitAsync());
				}

			using (ISession s = sf.OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					await (s.DeleteAsync("from Order"));
					await (t.CommitAsync());
				}

			await (new SchemaExport(configuration).DropAsync(false, false));
		}

		[Test]
		public async Task WhenConfiguredOnlyExplicitAutoQuoteAsync()
		{
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			var configuredDialect = Dialect.Dialect.GetDialect();
			if (!configuredDialect.DefaultProperties.ContainsKey(Environment.ConnectionDriver))
			{
				Assert.Ignore(GetType() + " does not apply to " + configuredDialect);
			}

			configuration.Properties.Remove(Environment.ConnectionDriver);
			configuration.AddResource("NHibernate.Test.Tools.hbm2ddl.SchemaMetadataUpdaterTest.HeavyEntity.hbm.xml", GetType().Assembly);
			await (SchemaMetadataUpdater.QuoteTableAndColumnsAsync(configuration));
			var cm = configuration.GetClassMapping(typeof (Order));
			Assert.That(cm.Table.IsQuoted);
			var culs = new List<Column>(cm.Table.ColumnIterator);
			Assert.That(GetColumnByName(culs, "From").IsQuoted);
			Assert.That(GetColumnByName(culs, "And").IsQuoted);
			Assert.That(GetColumnByName(culs, "Select").IsQuoted);
			Assert.That(!GetColumnByName(culs, "Name").IsQuoted);
		}
	}
}
#endif