#if NET_4_5
using System;
using System.Collections;
using System.IO;
using System.Xml;
using NHibernate.Cfg;
using NHibernate.DomainModel;
using NHibernate.Engine;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Util;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;
using System.Threading.Tasks;

namespace NHibernate.Test.CfgTest
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class ConfigurationFixture
	{
		/// <summary>
		/// Received sample code that Configuration could not be configured manually.  It can be configured
		/// manually just need to set all of the properties before adding classes
		/// </summary>
		[Test, Explicit]
		public async Task ManualConfigurationAsync()
		{
			//log4net.Config.DOMConfigurator.ConfigureAndWatch( new FileInfo("log4net.cfg.xml") ); //use xml file instead of config
			Configuration cfg = new Configuration();
			IDictionary props = new Hashtable();
			props[Environment.ConnectionProvider] = "NHibernate.Connection.DriverConnectionProvider";
			props[Environment.Dialect] = "NHibernate.Dialect.MsSql2000Dialect";
			props[Environment.ConnectionDriver] = "NHibernate.Driver.SqlClientDriver";
			props[Environment.ConnectionString] = "Server=localhost;initial catalog=nhibernate;Integrated Security=SSPI";
			foreach (DictionaryEntry de in props)
			{
				cfg.SetProperty(de.Key.ToString(), de.Value.ToString());
			}

			cfg.AddClass(typeof (Simple));
			await (new SchemaExport(cfg).CreateAsync(true, true));
			ISessionFactory factory = cfg.BuildSessionFactory();
		}

		/// <summary>
		/// Verify that setting the default assembly and namespace through
		/// <see cref = "Configuration"/> works as intended.
		/// </summary>
		[Test]
		public async Task SetDefaultAssemblyAndNamespaceAsync()
		{
			string hbmFromDomainModel = @"<?xml version='1.0' ?>
<hibernate-mapping xmlns='urn:nhibernate-mapping-2.2'>
	<class name='A'>
		<id name='Id' column='somecolumn'>
			<generator class='native' />
		</id>
	</class>
</hibernate-mapping>";
			string hbmFromTest = @"<?xml version='1.0' ?>
<hibernate-mapping xmlns='urn:nhibernate-mapping-2.2'>
	<class name='LocatedInTestAssembly' lazy='false'>
		<id name='Id' column='somecolumn'>
			<generator class='native' />
		</id>
	</class>
</hibernate-mapping>";
			Configuration cfg = new Configuration();
			cfg.SetDefaultAssembly("NHibernate.DomainModel").SetDefaultNamespace("NHibernate.DomainModel").AddXmlString(hbmFromDomainModel);
			cfg.SetDefaultAssembly("NHibernate.Test").SetDefaultNamespace(typeof (LocatedInTestAssembly).Namespace).AddXmlString(hbmFromTest);
			await (cfg.BuildSessionFactory().CloseAsync());
		}
	}
}
#endif
