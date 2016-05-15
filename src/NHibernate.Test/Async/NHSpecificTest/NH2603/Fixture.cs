#if NET_4_5
using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH2603
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class Fixture : BugTestCase
	{
		[Test]
		public async Task ListAsync()
		{
			using (new ListScenario(Sfi))
			{
				// by design NH will clean null elements at the end of the List since 'null' and 'no element' mean the same.
				// the effective ammount store will be 1(one) because ther is only one valid element but whem we initialize the collection
				// it will have 2 elements (the first with null)
				using (ISession s = OpenSession())
					using (ITransaction t = s.BeginTransaction())
					{
						var entity = await (s.CreateQuery("from Parent").UniqueResultAsync<Parent>());
						IList<object[]> members = await (s.GetNamedQuery("ListMemberSpy").SetParameter("parentid", entity.Id).ListAsync<object[]>());
						int lazyCount = entity.ListChildren.Count;
						Assert.That(NHibernateUtil.IsInitialized(entity.ListChildren), Is.False);
						await (NHibernateUtil.InitializeAsync(entity.ListChildren));
						int initCount = entity.ListChildren.Count;
						Assert.That(initCount, Is.EqualTo(lazyCount));
						Assert.That(members, Has.Count.EqualTo(1), "because only the valid element should be persisted.");
					}
			}
		}

		[Test]
		public async Task MapAsync()
		{
			using (new MapScenario(Sfi))
				using (ISession s = OpenSession())
				{
					// for the case of <map> what really matter is the key, then NH should count the KEY and not the elements.
					using (ITransaction t = s.BeginTransaction())
					{
						var entity = await (s.CreateQuery("from Parent").UniqueResultAsync<Parent>());
						IList<object[]> members = await (s.GetNamedQuery("MapMemberSpy").SetParameter("parentid", entity.Id).ListAsync<object[]>());
						int lazyCount = entity.MapChildren.Count;
						Assert.That(NHibernateUtil.IsInitialized(entity.MapChildren), Is.False);
						await (NHibernateUtil.InitializeAsync(entity.MapChildren));
						int initCount = entity.MapChildren.Count;
						Assert.That(initCount, Is.EqualTo(lazyCount));
						Assert.That(members, Has.Count.EqualTo(3), "because all elements with a valid key should be persisted.");
					}
				}
		}
	}
}
#endif
