#if NET_4_5
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NHibernate.DomainModel;
using NHibernate.Criterion;
using NHibernate.Type;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Legacy
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FumTest : TestCase
	{
		[Test]
		public async Task CriteriaCollectionAsync()
		{
			//if( dialect is Dialect.HSQLDialect ) return;
			using (ISession s = OpenSession())
			{
				Fum fum = new Fum(FumKey("fum"));
				fum.FumString = "a value";
				fum.MapComponent.Fummap["self"] = fum;
				fum.MapComponent.Stringmap["string"] = "a staring";
				fum.MapComponent.Stringmap["string2"] = "a notha staring";
				fum.MapComponent.Count = 1;
				await (s.SaveAsync(fum));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				Fum b = (Fum)await (s.CreateCriteria(typeof (Fum)).Add(Expression.In("FumString", new string[]{"a value", "no value"})).UniqueResultAsync());
				//Assert.IsTrue( NHibernateUtil.IsInitialized( b.MapComponent.Fummap ) );
				Assert.IsTrue(NHibernateUtil.IsInitialized(b.MapComponent.Stringmap));
				Assert.IsTrue(b.MapComponent.Fummap.Count == 1);
				Assert.IsTrue(b.MapComponent.Stringmap.Count == 2);
				int none = (await (s.CreateCriteria(typeof (Fum)).Add(Expression.In("FumString", new string[0])).ListAsync())).Count;
				Assert.AreEqual(0, none);
				await (s.DeleteAsync(b));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task CriteriaAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction txn = s.BeginTransaction())
				{
					Fum fum = new Fum(FumKey("fum"));
					fum.Fo = new Fum(FumKey("fo"));
					fum.FumString = "fo fee fi";
					fum.Fo.FumString = "stuff";
					Fum fr = new Fum(FumKey("fr"));
					fr.FumString = "goo";
					Fum fr2 = new Fum(FumKey("fr2"));
					fr2.FumString = "soo";
					fum.Friends = new HashSet<Fum>{fr, fr2};
					await (s.SaveAsync(fr));
					await (s.SaveAsync(fr2));
					await (s.SaveAsync(fum.Fo));
					await (s.SaveAsync(fum));
					ICriteria baseCriteria = s.CreateCriteria(typeof (Fum)).Add(Expression.Like("FumString", "f", MatchMode.Start));
					baseCriteria.CreateCriteria("Fo").Add(Expression.IsNotNull("FumString"));
					baseCriteria.CreateCriteria("Friends").Add(Expression.Like("FumString", "g%"));
					IList list = await (baseCriteria.ListAsync());
					Assert.AreEqual(1, list.Count);
					Assert.AreSame(fum, list[0]);
					baseCriteria = s.CreateCriteria(typeof (Fum)).Add(Expression.Like("FumString", "f%")).SetResultTransformer(CriteriaSpecification.AliasToEntityMap);
					baseCriteria.CreateCriteria("Fo", "fo").Add(Expression.IsNotNull("FumString"));
					baseCriteria.CreateCriteria("Friends", "fum").Add(Expression.Like("FumString", "g", MatchMode.Start));
					IDictionary map = (IDictionary)await (baseCriteria.UniqueResultAsync());
					Assert.AreSame(fum, map["this"]);
					Assert.AreSame(fum.Fo, map["fo"]);
					Assert.IsTrue(fum.Friends.Contains((Fum)map["fum"]));
					Assert.AreEqual(3, map.Count);
					baseCriteria = s.CreateCriteria(typeof (Fum)).Add(Expression.Like("FumString", "f%")).SetResultTransformer(CriteriaSpecification.AliasToEntityMap).SetFetchMode("Friends", FetchMode.Eager);
					baseCriteria.CreateCriteria("Fo", "fo").Add(Expression.Eq("FumString", fum.Fo.FumString));
					map = (IDictionary)(await (baseCriteria.ListAsync()))[0];
					Assert.AreSame(fum, map["this"]);
					Assert.AreSame(fum.Fo, map["fo"]);
					Assert.AreEqual(2, map.Count);
					list = await (s.CreateCriteria(typeof (Fum)).CreateAlias("Friends", "fr").CreateAlias("Fo", "fo").Add(Expression.Like("FumString", "f%")).Add(Expression.IsNotNull("Fo")).Add(Expression.IsNotNull("fo.FumString")).Add(Expression.Like("fr.FumString", "g%")).Add(Expression.EqProperty("fr.id.Short", "id.Short")).ListAsync());
					Assert.AreEqual(1, list.Count);
					Assert.AreSame(fum, list[0]);
					await (txn.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction txn = s.BeginTransaction())
				{
					ICriteria baseCriteria = s.CreateCriteria(typeof (Fum)).Add(Expression.Like("FumString", "f%"));
					baseCriteria.CreateCriteria("Fo").Add(Expression.IsNotNull("FumString"));
					baseCriteria.CreateCriteria("Friends").Add(Expression.Like("FumString", "g%"));
					Fum fum = (Fum)(await (baseCriteria.ListAsync()))[0];
					Assert.AreEqual(2, fum.Friends.Count);
					await (s.DeleteAsync(fum));
					await (s.DeleteAsync(fum.Fo));
					foreach (object friend in fum.Friends)
					{
						await (s.DeleteAsync(friend));
					}

					await (txn.CommitAsync());
				}
		}

		[Test]
		public async Task ListIdentifiersAsync()
		{
			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();
			Fum fum = new Fum(FumKey("fum"));
			fum.FumString = "fo fee fi";
			await (s.SaveAsync(fum));
			fum = new Fum(FumKey("fi"));
			fum.FumString = "fee fi fo";
			await (s.SaveAsync(fum));
			// not doing a flush because the Find will do an auto flush unless we tell the session a 
			// different FlushMode
			IList list = await (s.CreateQuery("select fum.Id from fum in class NHibernate.DomainModel.Fum where not fum.FumString = 'FRIEND'").ListAsync());
			Assert.AreEqual(2, list.Count, "List Identifiers");
			IEnumerator enumerator = (await (s.CreateQuery("select fum.Id from fum in class NHibernate.DomainModel.Fum where not fum.FumString='FRIEND'").EnumerableAsync())).GetEnumerator();
			int i = 0;
			while (enumerator.MoveNext())
			{
				Assert.IsTrue(enumerator.Current is FumCompositeID, "Iterating Identifiers");
				i++;
			}

			Assert.AreEqual(2, i, "Number of Ids found.");
			// clean up by deleting the 2 Fum objects that were added.
			await (s.DeleteAsync(await (s.LoadAsync(typeof (Fum), list[0]))));
			await (s.DeleteAsync(await (s.LoadAsync(typeof (Fum), list[1]))));
			await (txn.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CompositeIDAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Fum fum = new Fum(FumKey("fum"));
			fum.FumString = "fee fi fo";
			await (s.SaveAsync(fum));
			Assert.AreSame(fum, await (s.LoadAsync(typeof (Fum), FumKey("fum"), LockMode.Upgrade)));
			//s.Flush();
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			fum = (Fum)await (s.LoadAsync(typeof (Fum), FumKey("fum"), LockMode.Upgrade));
			Assert.IsNotNull(fum, "Load by composite key");
			Fum fum2 = new Fum(FumKey("fi"));
			fum2.FumString = "fee fo fi";
			fum.Fo = fum2;
			await (s.SaveAsync(fum2));
			IList list = await (s.CreateQuery("from fum in class NHibernate.DomainModel.Fum where not fum.FumString='FRIEND'").ListAsync());
			Assert.AreEqual(2, list.Count, "Find a List of Composite Keyed objects");
			IList list2 = await (s.CreateQuery("select fum from fum in class NHibernate.DomainModel.Fum where fum.FumString='fee fi fo'").ListAsync());
			Assert.AreEqual(fum, (Fum)list2[0], "Find one Composite Keyed object");
			fum.Fo = null;
			//s.Flush();
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			IEnumerator enumerator = (await (s.CreateQuery("from fum in class NHibernate.DomainModel.Fum where not fum.FumString='FRIEND'").EnumerableAsync())).GetEnumerator();
			int i = 0;
			while (enumerator.MoveNext())
			{
				fum = (Fum)enumerator.Current;
				await (s.DeleteAsync(fum));
				i++;
			}

			Assert.AreEqual(2, i, "Iterate on Composite Key");
			//s.Flush();
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CompositeIDOneToOneAsync()
		{
			ISession s = OpenSession();
			Fum fum = new Fum(FumKey("fum"));
			fum.FumString = "fee fi fo";
			//s.Save(fum); commented out in h2.0.3
			Fumm fumm = new Fumm();
			fumm.Fum = fum;
			await (s.SaveAsync(fumm));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			fumm = (Fumm)await (s.LoadAsync(typeof (Fumm), FumKey("fum")));
			//s.delete(fumm.Fum); commented out in h2.0.3
			await (s.DeleteAsync(fumm));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task CompositeIDQueryAsync()
		{
			ISession s = OpenSession();
			Fum fee = new Fum(FumKey("fee", true));
			fee.FumString = "fee";
			await (s.SaveAsync(fee));
			Fum fi = new Fum(FumKey("fi", true));
			fi.FumString = "fi";
			short fiShort = fi.Id.Short;
			await (s.SaveAsync(fi));
			Fum fo = new Fum(FumKey("fo", true));
			fo.FumString = "fo";
			await (s.SaveAsync(fo));
			Fum fum = new Fum(FumKey("fum", true));
			fum.FumString = "fum";
			await (s.SaveAsync(fum));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			// Try to find the Fum object "fo" that we inserted searching by the string in the id
			IList vList = await (s.CreateQuery("from fum in class NHibernate.DomainModel.Fum where fum.Id.String='fo'").ListAsync());
			Assert.AreEqual(1, vList.Count, "find by composite key query (find fo object)");
			fum = (Fum)vList[0];
			Assert.AreEqual("fo", fum.Id.String, "find by composite key query (check fo object)");
			// Try to fnd the Fum object "fi" that we inserted by searching the date in the id
			vList = await (s.CreateQuery("from fum in class NHibernate.DomainModel.Fum where fum.Id.Short = ?").SetInt16(0, fiShort).ListAsync());
			Assert.AreEqual(1, vList.Count, "find by composite key query (find fi object)");
			fi = (Fum)vList[0];
			Assert.AreEqual("fi", fi.Id.String, "find by composite key query (check fi object)");
			// make sure we can return all of the objects by searching by the date id
			vList = await (s.CreateQuery("from fum in class NHibernate.DomainModel.Fum where fum.Id.Date <= ? and not fum.FumString='FRIEND'").SetDateTime(0, DateTime.Now).ListAsync());
			Assert.AreEqual(4, vList.Count, "find by composite key query with arguments");
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			Assert.IsTrue((await (s.CreateQuery("select fum.Id.Short, fum.Id.Date, fum.Id.String from fum in class NHibernate.DomainModel.Fum").EnumerableAsync())).GetEnumerator().MoveNext());
			Assert.IsTrue((await (s.CreateQuery("select fum.Id from fum in class NHibernate.DomainModel.Fum").EnumerableAsync())).GetEnumerator().MoveNext());
			IQuery qu = s.CreateQuery("select fum.FumString, fum, fum.FumString, fum.Id.Date from fum in class NHibernate.DomainModel.Fum");
			IType[] types = qu.ReturnTypes;
			Assert.AreEqual(4, types.Length);
			for (int k = 0; k < types.Length; k++)
			{
				Assert.IsNotNull(types[k]);
			}

			Assert.IsTrue(types[0] is StringType);
			Assert.IsTrue(types[1] is EntityType);
			Assert.IsTrue(types[2] is StringType);
			Assert.IsTrue(types[3] is DateTimeType);
			IEnumerator enumer = (await (qu.EnumerableAsync())).GetEnumerator();
			int j = 0;
			while (enumer.MoveNext())
			{
				j++;
				Assert.IsTrue(((object[])enumer.Current)[1] is Fum);
			}

			Assert.AreEqual(8, j, "iterate on composite key");
			fum = (Fum)await (s.LoadAsync(typeof (Fum), fum.Id));
			await ((await (s.CreateFilterAsync(fum.QuxArray, "where this.Foo is null"))).ListAsync());
			(await (s.CreateFilterAsync(fum.QuxArray, "where this.Foo.id = ?"))).SetString(0, "fooid");
			IQuery f = await (s.CreateFilterAsync(fum.QuxArray, "where this.Foo.id = :fooId"));
			f.SetString("fooId", "abc");
			Assert.IsFalse((await (f.EnumerableAsync())).GetEnumerator().MoveNext());
			enumer = (await (s.CreateQuery("from fum in class NHibernate.DomainModel.Fum where not fum.FumString='FRIEND'").EnumerableAsync())).GetEnumerator();
			int i = 0;
			while (enumer.MoveNext())
			{
				fum = (Fum)enumer.Current;
				await (s.DeleteAsync(fum));
				i++;
			}

			Assert.AreEqual(4, i, "iterate on composite key");
			await (s.FlushAsync());
			await (s.CreateQuery("from fu in class Fum, fo in class Fum where fu.Fo.Id.String = fo.Id.String and fo.FumString is not null").EnumerableAsync());
			await (s.CreateQuery("from Fumm f1 inner join f1.Fum f2").ListAsync());
			s.Close();
		}

		[Test]
		public async Task CompositeIDCollectionsAsync()
		{
			ISession s = OpenSession();
			Fum fum1 = new Fum(FumKey("fum1"));
			Fum fum2 = new Fum(FumKey("fum2"));
			fum1.FumString = "fee fo fi";
			fum2.FumString = "fee fo fi";
			await (s.SaveAsync(fum1));
			await (s.SaveAsync(fum2));
			Qux q = new Qux();
			await (s.SaveAsync(q));
			q.Fums = new HashSet<Fum>{fum1, fum2};
			q.MoreFums = new List<Fum>{fum1};
			fum1.QuxArray = new Qux[]{q};
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			q = (Qux)await (s.LoadAsync(typeof (Qux), q.Key));
			Assert.AreEqual(2, q.Fums.Count, "collection of fums");
			Assert.AreEqual(1, q.MoreFums.Count, "collection of fums");
			Assert.AreSame(q, ((Fum)q.MoreFums[0]).QuxArray[0], "unkeyed composite id collection");
			IEnumerator enumer = q.Fums.GetEnumerator();
			enumer.MoveNext();
			await (s.DeleteAsync((Fum)enumer.Current));
			enumer.MoveNext();
			await (s.DeleteAsync((Fum)enumer.Current));
			await (s.DeleteAsync(q));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task DeleteOwnerAsync()
		{
			ISession s = OpenSession();
			Qux q = new Qux();
			await (s.SaveAsync(q));
			Fum f1 = new Fum(FumKey("f1"));
			Fum f2 = new Fum(FumKey("f2"));
			f1.FumString = "f1";
			f2.FumString = "f2";
			q.Fums = new HashSet<Fum>{f1, f2};
			q.MoreFums = new List<Fum>{f1, f2};
			await (s.SaveAsync(f1));
			await (s.SaveAsync(f2));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			ITransaction t = s.BeginTransaction();
			q = (Qux)await (s.LoadAsync(typeof (Qux), q.Key, LockMode.Upgrade));
			await (s.LockAsync(q, LockMode.Upgrade));
			await (s.DeleteAsync(q));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			var list = await (s.CreateQuery("from fum in class NHibernate.DomainModel.Fum where not fum.FumString='FRIEND'").ListAsync());
			Assert.AreEqual(2, list.Count, "deleted owner");
			await (s.LockAsync(list[0], LockMode.Upgrade));
			await (s.LockAsync(list[1], LockMode.Upgrade));
			foreach (object obj in list)
			{
				await (s.DeleteAsync(obj));
			}

			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CompositeIDsAsync()
		{
			ISession s = OpenSession();
			Fo fo = Fo.NewFo();
			await (s.SaveAsync(fo, FumKey("an instance of fo")));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			fo = (Fo)await (s.LoadAsync(typeof (Fo), FumKey("an instance of fo")));
			fo.X = 5;
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			fo = (Fo)await (s.LoadAsync(typeof (Fo), FumKey("an instance of fo")));
			Assert.AreEqual(5, fo.X);
			IEnumerator enumer = (await (s.CreateQuery("from fo in class NHibernate.DomainModel.Fo where fo.id.String like 'an instance of fo'").EnumerableAsync())).GetEnumerator();
			Assert.IsTrue(enumer.MoveNext());
			Assert.AreSame(fo, enumer.Current);
			await (s.DeleteAsync(fo));
			await (s.FlushAsync());
			try
			{
				await (s.SaveAsync(Fo.NewFo()));
				Assert.Fail("should not get here");
			}
			catch (Exception e)
			{
				Assert.IsNotNull(e);
			}

			s.Close();
		}

		[Test]
		public async Task KeyManyToOneAsync()
		{
			ISession s = OpenSession();
			Inner sup = new Inner();
			InnerKey sid = new InnerKey();
			sup.Dudu = "dudu";
			sid.AKey = "a";
			sid.BKey = "b";
			sup.Id = sid;
			Middle m = new Middle();
			MiddleKey mid = new MiddleKey();
			mid.One = "one";
			mid.Two = "two";
			mid.Sup = sup;
			m.Id = mid;
			m.Bla = "bla";
			Outer d = new Outer();
			OuterKey did = new OuterKey();
			did.Master = m;
			did.DetailId = "detail";
			d.Id = did;
			d.Bubu = "bubu";
			await (s.SaveAsync(sup));
			await (s.SaveAsync(m));
			await (s.SaveAsync(d));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			d = (Outer)await (s.LoadAsync(typeof (Outer), did));
			Assert.AreEqual("dudu", d.Id.Master.Id.Sup.Dudu);
			await (s.DeleteAsync(d));
			await (s.DeleteAsync(d.Id.Master));
			await (s.SaveAsync(d.Id.Master));
			await (s.SaveAsync(d));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			d = (Outer)(await (s.CreateQuery("from Outer o where o.id.DetailId=?").SetString(0, d.Id.DetailId).ListAsync()))[0];
			await (s.CreateQuery("from Outer o where o.Id.Master.Id.Sup.Dudu is not null").ListAsync());
			await (s.CreateQuery("from Outer o where o.Id.Master.Bla = ''").ListAsync());
			await (s.CreateQuery("from Outer o where o.Id.Master.Id.One = ''").ListAsync());
			await (s.DeleteAsync(d));
			await (s.DeleteAsync(d.Id.Master));
			await (s.DeleteAsync(d.Id.Master.Id.Sup));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task CompositeKeyPathExpressionsAsync()
		{
			using (ISession s = OpenSession())
			{
				await (s.CreateQuery("select fum1.Fo from fum1 in class Fum where fum1.Fo.FumString is not null").ListAsync());
				await (s.CreateQuery("from fum1 in class Fum where fum1.Fo.FumString is not null order by fum1.Fo.FumString").ListAsync());
				if (Dialect.SupportsSubSelects)
				{
					await (s.CreateQuery("from fum1 in class Fum where exists elements(fum1.Friends)").ListAsync());
					await (s.CreateQuery("from fum1 in class Fum where size(fum1.Friends) = 0").ListAsync());
				}

				await (s.CreateQuery("select elements(fum1.Friends) from fum1 in class Fum").ListAsync());
				await (s.CreateQuery("from fum1 in class Fum, fr in elements( fum1.Friends )").ListAsync());
			}
		}

		[Test]
		public async Task UnflushedSessionSerializationAsync()
		{
			///////////////////////////////////////////////////////////////////////////
			// Test insertions across serializations
			ISession s2;
			// NOTE: H2.1 has getSessions().openSession() here (and below),
			// instead of just the usual openSession()
			using (ISession s = sessions.OpenSession())
			{
				s.FlushMode = FlushMode.Never;
				Simple simple = new Simple();
				simple.Address = "123 Main St. Anytown USA";
				simple.Count = 1;
				simple.Date = new DateTime(2005, 1, 1);
				simple.Name = "My UnflushedSessionSerialization Simple";
				simple.Pay = 5000.0f;
				await (s.SaveAsync(simple, 10L));
				// Now, try to serialize session without flushing...
				s.Disconnect();
				s2 = SpoofSerialization(s);
			}

			Simple check, other;
			using (ISession s = s2)
			{
				s.Reconnect();
				Simple simple = (Simple)await (s.LoadAsync(typeof (Simple), 10L));
				other = new Simple();
				other.Init();
				await (s.SaveAsync(other, 11L));
				simple.Other = other;
				await (s.FlushAsync());
				check = simple;
			}

			///////////////////////////////////////////////////////////////////////////
			// Test updates across serializations
			using (ISession s = sessions.OpenSession())
			{
				s.FlushMode = FlushMode.Never;
				Simple simple = (Simple)await (s.GetAsync(typeof (Simple), 10L));
				Assert.AreEqual(check.Name, simple.Name, "Not same parent instances");
				Assert.AreEqual(check.Other.Name, other.Name, "Not same child instances");
				simple.Name = "My updated name";
				s.Disconnect();
				s2 = SpoofSerialization(s);
				check = simple;
			}

			using (ISession s = s2)
			{
				s.Reconnect();
				await (s.FlushAsync());
			}

			///////////////////////////////////////////////////////////////////////////
			// Test deletions across serializations
			using (ISession s = sessions.OpenSession())
			{
				s.FlushMode = FlushMode.Never;
				Simple simple = (Simple)await (s.GetAsync(typeof (Simple), 10L));
				Assert.AreEqual(check.Name, simple.Name, "Not same parent instances");
				Assert.AreEqual(check.Other.Name, other.Name, "Not same child instances");
				// Now, lets delete across serialization...
				await (s.DeleteAsync(simple));
				s.Disconnect();
				s2 = SpoofSerialization(s);
			}

			using (ISession s = s2)
			{
				s.Reconnect();
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				await (s.DeleteAsync("from Simple"));
				await (s.FlushAsync());
			}
		}
	}
}
#endif
