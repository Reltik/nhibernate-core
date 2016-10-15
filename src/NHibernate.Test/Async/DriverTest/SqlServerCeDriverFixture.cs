﻿#if NET_4_5
using System;
using System.Collections;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NUnit.Framework;
using NHibernate.Criterion;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NHibernate.Test.DriverTest
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class SqlServerCeDriverFixtureAsync : TestCaseAsync
	{
		protected override string MappingsAssembly
		{
			get
			{
				return "NHibernate.Test";
			}
		}

		protected override IList Mappings
		{
			get
			{
				return new[]{"DriverTest.SqlServerCeEntity.hbm.xml"};
			}
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return dialect is MsSqlCeDialect;
		}

		protected override async Task OnTearDownAsync()
		{
			await (base.OnTearDownAsync());
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					await (s.DeleteAsync("from SqlServerCeEntity"));
					await (tx.CommitAsync());
				}
		}

		[Test]
		public async Task SaveLoadAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					SqlServerCeEntity entity = new SqlServerCeEntity();
					entity.StringProp = "a small string";
					entity.BinaryProp = new byte[100];
					entity.StringClob = new String('a', 8193);
					entity.BinaryBlob = new byte[8193];
					await (s.SaveAsync(entity));
					await (tx.CommitAsync());
				}

			using (ISession s = OpenSession())
			{
				SqlServerCeEntity entity = await (s.CreateCriteria(typeof (SqlServerCeEntity)).UniqueResultAsync<SqlServerCeEntity>());
				Assert.That(entity.StringProp, Is.EqualTo("a small string"));
				Assert.That(entity.BinaryProp.Length, Is.EqualTo(100));
				Assert.That(entity.StringClob, Is.EqualTo(new String('a', 8193)));
				Assert.That(entity.BinaryBlob.Length, Is.EqualTo(8193));
			}
		}

		[Test]
		public async Task QueryAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					SqlServerCeEntity entity = new SqlServerCeEntity();
					entity.StringProp = "a small string";
					entity.BinaryProp = System.Text.ASCIIEncoding.ASCII.GetBytes("binary string");
					entity.StringClob = new String('a', 8193);
					entity.BinaryBlob = new byte[8193];
					await (s.SaveAsync(entity));
					await (tx.CommitAsync());
				}

			using (ISession s = OpenSession())
			{
				IList<SqlServerCeEntity> entities = await (s.CreateCriteria(typeof (SqlServerCeEntity)).Add(Restrictions.Eq("StringProp", "a small string")).Add(Restrictions.Eq("BinaryProp", System.Text.ASCIIEncoding.ASCII.GetBytes("binary string"))).ListAsync<SqlServerCeEntity>());
				Assert.That(entities.Count, Is.EqualTo(1));
			}
		}
	}
}
#endif