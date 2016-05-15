#if NET_4_5
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NHibernate.Engine;
using NHibernate.Type;
using NUnit.Framework;
using NHibernate.Collection.Generic;
using NHibernate.Persister.Collection;
using System.Threading.Tasks;

namespace NHibernate.Test.GenericTest.MapGeneric
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class MapGenericFixture : TestCase
	{
		[Test]
		public async Task SimpleAsync()
		{
			A a = new A();
			a.Name = "first generic type";
			a.Items = new Dictionary<string, B>();
			B firstB = new B();
			firstB.Name = "first b";
			B secondB = new B();
			secondB.Name = "second b";
			a.Items.Add("first", firstB);
			a.Items.Add("second", secondB);
			using (ISession s = OpenSession())
			{
				await (s.SaveOrUpdateAsync(a));
				// this flush should test how NH wraps a generic collection with its
				// own persistent collection
				await (s.FlushAsync());
			}

			Assert.IsNotNull(a.Id);
			// should have cascaded down to B
			Assert.IsNotNull(firstB.Id);
			Assert.IsNotNull(secondB.Id);
			using (ISession s = OpenSession())
			{
				a = await (s.LoadAsync<A>(a.Id));
				B thirdB = new B();
				thirdB.Name = "third B";
				// ensuring the correct generic type was constructed
				a.Items.Add("third", thirdB);
				Assert.AreEqual(3, a.Items.Count, "3 items in the map now");
				await (s.FlushAsync());
			}

			// NH-839
			using (ISession s = OpenSession())
			{
				a = await (s.LoadAsync<A>(a.Id));
				a.Items["second"] = a.Items["third"];
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task SimpleTypesAsync()
		{
			A a = new A();
			a.Name = "first generic type";
			a.Items = new Dictionary<string, B>();
			B firstB = new B();
			firstB.Name = "first b";
			B secondB = new B();
			secondB.Name = "second b";
			B thirdB = new B();
			thirdB.Name = "third b";
			a.Items.Add("first", firstB);
			a.Items.Add("second", secondB);
			a.Items.Add("third", thirdB);
			using (ISession s = OpenSession())
			{
				await (s.SaveOrUpdateAsync(a));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				a = await (s.LoadAsync<A>(a.Id));
				IDictionary<string, B> genericDict = a.Items;
				IEnumerable<KeyValuePair<string, B>> genericEnum = a.Items;
				IEnumerable nonGenericEnum = a.Items;
				foreach (var enumerable in genericDict)
				{
					Assert.That(enumerable, Is.InstanceOf<KeyValuePair<string, B>>());
				}

				foreach (var enumerable in genericEnum)
				{
					Assert.That(enumerable, Is.InstanceOf<KeyValuePair<string, B>>());
				}

				foreach (var enumerable in nonGenericEnum)
				{
					Assert.That(enumerable, Is.InstanceOf<KeyValuePair<string, B>>());
				}
			}
		}

		// NH-669
		[Test]
		public async Task UpdatesToSimpleMapAsync()
		{
			A a = new A();
			a.Name = "A";
			using (ISession s = OpenSession())
			{
				await (s.SaveAsync(a));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				a = await (s.LoadAsync<A>(a.Id));
				a.SortedList.Add("abc", 10);
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				await (s.DeleteAsync("from A"));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task CopyAsync()
		{
			A a = new A();
			a.Name = "original A";
			a.Items = new Dictionary<string, B>();
			B b1 = new B();
			b1.Name = "b1";
			a.Items["b1"] = b1;
			B b2 = new B();
			b2.Name = "b2";
			a.Items["b2"] = b2;
			A copiedA;
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					copiedA = await (s.MergeAsync(a));
					await (t.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					A loadedA = await (s.GetAsync<A>(copiedA.Id));
					Assert.IsNotNull(loadedA);
					await (s.DeleteAsync(loadedA));
					await (t.CommitAsync());
				}
		}

		[Test]
		public async Task SortedCollectionsAsync()
		{
			A a = new A();
			a.SortedDictionary = new SortedDictionary<string, int>();
			a.SortedList = new SortedList<string, int>();
			a.SortedDictionary["10"] = 5;
			a.SortedList["20"] = 10;
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					await (s.SaveAsync(a));
					await (t.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					a = await (s.LoadAsync<A>(a.Id));
					ISessionFactoryImplementor si = (ISessionFactoryImplementor)sessions;
					ICollectionPersister cpSortedList = si.GetCollectionPersister(typeof (A).FullName + ".SortedList");
					ICollectionPersister cpSortedDictionary = si.GetCollectionPersister(typeof (A).FullName + ".SortedDictionary");
					PersistentGenericMap<string, int> sd = a.SortedDictionary as PersistentGenericMap<string, int>;
					Assert.IsNotNull(sd);
					Assert.IsTrue(cpSortedList.CollectionType is GenericSortedListType<string, int>);
					Assert.IsTrue(cpSortedDictionary.CollectionType is GenericSortedDictionaryType<string, int>);
					// This is a hack to check that the internal collection is a SortedDictionary<,>.
					// The hack works because PersistentGenericMap.Entries() returns the internal collection
					// casted to IEnumerable
					Assert.IsTrue(sd.Entries(cpSortedDictionary) is SortedDictionary<string, int>);
					PersistentGenericMap<string, int> sl = a.SortedList as PersistentGenericMap<string, int>;
					Assert.IsNotNull(sl);
					// This is a hack, see above
					Assert.IsTrue(sl.Entries(cpSortedList) is SortedList<string, int>);
					await (t.CommitAsync());
				}
		}
	}
}
#endif
