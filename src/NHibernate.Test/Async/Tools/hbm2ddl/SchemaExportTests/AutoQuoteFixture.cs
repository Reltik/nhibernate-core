#if NET_4_5
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping;
using NHibernate.Test.Tools.hbm2ddl.SchemaMetadataUpdaterTest;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Tools.hbm2ddl.SchemaExportTests
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class AutoQuoteFixture
	{
		[Test]
		public async Task WhenCalledExplicitlyThenTakeInAccountHbm2DdlKeyWordsSettingAsync()
		{
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			var dialect = NHibernate.Dialect.Dialect.GetDialect(configuration.Properties);
			if (!(dialect is MsSql2000Dialect))
			{
				Assert.Ignore(GetType() + " does not apply to " + dialect);
			}

			configuration.SetProperty(Environment.Hbm2ddlKeyWords, "auto-quote");
			configuration.AddResource("NHibernate.Test.Tools.hbm2ddl.SchemaMetadataUpdaterTest.HeavyEntity.hbm.xml", GetType().Assembly);
			var script = new StringBuilder();
			await (new SchemaExport(configuration).ExecuteAsync(s => script.AppendLine(s), false, false));
			Assert.That(script.ToString(), Is.StringContaining("[Order]").And.Contains("[Select]").And.Contains("[From]").And.Contains("[And]"));
		}

		[Test]
		public async Task WhenUpdateCalledExplicitlyThenTakeInAccountHbm2DdlKeyWordsSettingAsync()
		{
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			var dialect = NHibernate.Dialect.Dialect.GetDialect(configuration.Properties);
			if (!(dialect is MsSql2000Dialect))
			{
				Assert.Ignore(GetType() + " does not apply to " + dialect);
			}

			configuration.SetProperty(Environment.Hbm2ddlKeyWords, "auto-quote");
			configuration.AddResource("NHibernate.Test.Tools.hbm2ddl.SchemaMetadataUpdaterTest.HeavyEntity.hbm.xml", GetType().Assembly);
			var script = new StringBuilder();
			await (new Tool.hbm2ddl.SchemaUpdate(configuration).ExecuteAsync(s => script.AppendLine(s), false));
			// With SchemaUpdate the auto-quote method should be called and the conf. should hold quoted stuff
			var cm = configuration.GetClassMapping(typeof (Order));
			var culs = cm.Table.ColumnIterator.ToList();
			Assert.That(cm.Table.IsQuoted, Is.True);
			Assert.That(culs.First(c => "From" == c.Name).IsQuoted, Is.True);
			Assert.That(culs.First(c => "And" == c.Name).IsQuoted, Is.True);
			Assert.That(culs.First(c => "Select" == c.Name).IsQuoted, Is.True);
			Assert.That(culs.First(c => "Column" == c.Name).IsQuoted, Is.True);
		}
	}
}
#endif
