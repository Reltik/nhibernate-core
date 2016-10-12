﻿#if NET_4_5
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using Exception = System.Exception;
using NHibernate.Util;

// ReSharper disable InconsistentNaming
namespace NHibernate.Test.NHSpecificTest.NH2705
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class TestAsync : BugTestCaseAsync
	{
		private static async Task<IEnumerable<T>> GetAndFetchAsync<T>(string name, ISession session)where T : ItemBase
		{
			// this is a valid abstraction, the calling code should be able to ask that a property is eagerly loaded/available
			// without having to know how it is mapped
			return await (session.Query<T>().Fetch(p => p.SubItem).ThenFetch(p => p.Details) // should be able to fetch .Details when used with components (NH2615)
			.Where(p => p.SubItem.Name == name).ToListAsync());
		}

		[Test]
		public async Task Fetch_OnComponent_ShouldNotThrowAsync()
		{
			using (ISession s = OpenSession())
			{
				Assert.That(async () => await (GetAndFetchAsync<ItemWithComponentSubItem>("hello", s)), Throws.Nothing);
			}
		}

		[Test]
		public async Task HqlQueryWithFetch_WhenDerivedClassesUseComponentAndManyToOne_DoesNotGenerateInvalidSqlAsync()
		{
			using (ISession s = OpenSession())
			{
				using (var log = new SqlLogSpy())
				{
					Assert.That(async () => await (s.CreateQuery("from ItemWithComponentSubItem i left join fetch i.SubItem").ListAsync()), Throws.Nothing);
				}
			}
		}

		[Test]
		public async Task HqlQueryWithFetch_WhenDerivedClassesUseComponentAndEagerFetchManyToOne_DoesNotGenerateInvalidSqlAsync()
		{
			using (ISession s = OpenSession())
			{
				using (var log = new SqlLogSpy())
				{
					Assert.That(async () => await (s.CreateQuery("from ItemWithComponentSubItem i left join fetch i.SubItem.Details").ListAsync()), Throws.Nothing);
				}
			}
		}
	}
}
#endif
