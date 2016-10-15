﻿#if NET_4_5
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH2500
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FixtureAsync : TestCaseMappingByCodeAsync
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ConventionModelMapper();
			mapper.BeforeMapClass += (mi, t, x) => x.Id(map => map.Generator(Generators.Guid));
			return mapper.CompileMappingFor(new[]{typeof (Foo)});
		}

		protected override async Task OnSetUpAsync()
		{
			await (base.OnSetUpAsync());
			using (ISession session = Sfi.OpenSession())
				using (ITransaction transaction = session.BeginTransaction())
				{
					await (session.PersistAsync(new Foo{Name = "Banana"}));
					await (session.PersistAsync(new Foo{Name = "Egg"}));
					await (transaction.CommitAsync());
				}
		}

		protected override async Task OnTearDownAsync()
		{
			using (ISession session = Sfi.OpenSession())
				using (ITransaction transaction = session.BeginTransaction())
				{
					await (session.CreateQuery("delete from Foo").ExecuteUpdateAsync());
					await (transaction.CommitAsync());
				}

			await (base.OnTearDownAsync());
		}

		private int count;
		[Test]
		public async Task TestLinqProjectionExpressionDoesntCacheParametersAsync()
		{
			using (ISession session = Sfi.OpenSession())
				using (ITransaction transaction = session.BeginTransaction())
				{
					this.count = 1;
					var foos1 = await (session.Query<Foo>().Where(x => x.Name == "Banana").Select(x => new
					{
					x.Name, count, User = "abc"
					}

					).FirstAsync());
					this.count = 2;
					var foos2 = await (session.Query<Foo>().Where(x => x.Name == "Egg").Select(x => new
					{
					x.Name, count, User = "def"
					}

					).FirstAsync());
					Assert.AreEqual(1, foos1.count);
					Assert.AreEqual(2, foos2.count);
					Assert.AreEqual("abc", foos1.User);
					Assert.AreEqual("def", foos2.User);
					await (transaction.CommitAsync());
				}
		}
	}
}
#endif