﻿#if NET_4_5
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Iesi.Collections.Generic;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.DomainModel;
using NHibernate.Criterion;
using NHibernate.Proxy;
using NHibernate.Type;
using NHibernate.Util;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Legacy
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FooBarTestAsync : TestCaseAsync
	{
		// Equivalent of Java String.getBytes()
		private static byte[] GetBytes(string str)
		{
			return Encoding.Unicode.GetBytes(str);
		}

		protected override IList Mappings
		{
			get
			{
				return new string[]{"FooBar.hbm.xml", "Baz.hbm.xml", "Qux.hbm.xml", "Glarch.hbm.xml", "Fum.hbm.xml", "Fumm.hbm.xml", "Fo.hbm.xml", "One.hbm.xml", "Many.hbm.xml", "Immutable.hbm.xml", "Fee.hbm.xml", "Vetoer.hbm.xml", "Holder.hbm.xml", "Location.hbm.xml", "Stuff.hbm.xml", "Container.hbm.xml", "Simple.hbm.xml", "XY.hbm.xml"};
			}
		}

		[Test]
		public async Task CollectionVersioningAsync()
		{
			using (ISession s = OpenSession())
			{
				One one = new One();
				one.Manies = new HashSet<Many>();
				await (s.SaveAsync(one));
				await (s.FlushAsync());
				Many many = new Many();
				many.One = one;
				one.Manies.Add(many);
				await (s.SaveAsync(many));
				await (s.FlushAsync());
				// Versions are incremented compared to Hibernate because they start from 1
				// in NH.
				Assert.AreEqual(1, many.V);
				Assert.AreEqual(2, one.V);
				await (s.DeleteAsync(many));
				await (s.DeleteAsync(one));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task ForCertainAsync()
		{
			Glarch g = new Glarch();
			Glarch g2 = new Glarch();
			IList<string> strings = new List<string>();
			strings.Add("foo");
			g2.Strings = strings;
			object gid, g2id;
			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					gid = await (s.SaveAsync(g));
					g2id = await (s.SaveAsync(g2));
					await (t.CommitAsync());
					// Versions are initialized to 1 in NH.
					Assert.AreEqual(1, g.Version);
					Assert.AreEqual(1, g2.Version);
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					g = (Glarch)await (s.GetAsync(typeof (Glarch), gid));
					g2 = (Glarch)await (s.GetAsync(typeof (Glarch), g2id));
					Assert.AreEqual(1, g2.Strings.Count);
					await (s.DeleteAsync(g));
					await (s.DeleteAsync(g2));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task BagMultipleElementsAsync()
		{
			string bazCode;
			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					Baz baz = new Baz();
					baz.Bag = new List<string>();
					baz.ByteBag = new List<byte[]>();
					await (s.SaveAsync(baz));
					baz.Bag.Add("foo");
					baz.Bag.Add("bar");
					baz.ByteBag.Add(GetBytes("foo"));
					baz.ByteBag.Add(GetBytes("bar"));
					await (t.CommitAsync());
					bazCode = baz.Code;
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					//put in cache
					Baz baz = (Baz)await (s.GetAsync(typeof (Baz), bazCode));
					Assert.AreEqual(2, baz.Bag.Count);
					Assert.AreEqual(2, baz.ByteBag.Count);
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					Baz baz = (Baz)await (s.GetAsync(typeof (Baz), bazCode));
					Assert.AreEqual(2, baz.Bag.Count);
					Assert.AreEqual(2, baz.ByteBag.Count);
					baz.Bag.Remove("bar");
					baz.Bag.Add("foo");
					baz.ByteBag.Add(GetBytes("bar"));
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					Baz baz = (Baz)await (s.GetAsync(typeof (Baz), bazCode));
					Assert.AreEqual(2, baz.Bag.Count);
					Assert.AreEqual(3, baz.ByteBag.Count);
					await (s.DeleteAsync(baz));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task WierdSessionAsync()
		{
			object id;
			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					id = await (s.SaveAsync(new Foo()));
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				s.FlushMode = FlushMode.Never;
				using (ITransaction t = s.BeginTransaction())
				{
					Foo foo = (Foo)await (s.GetAsync(typeof (Foo), id));
					await (t.CommitAsync());
				}

				s.Disconnect();
				s.Reconnect();
				using (ITransaction t = s.BeginTransaction())
				{
					await (s.FlushAsync());
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					Foo foo = (Foo)await (s.GetAsync(typeof (Foo), id));
					await (s.DeleteAsync(foo));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task DereferenceLazyCollectionAsync()
		{
			string fooKey;
			string bazCode;
			using (ISession s = OpenSession())
			{
				Baz baz = new Baz();
				baz.FooSet = new HashSet<FooProxy>();
				Foo foo = new Foo();
				baz.FooSet.Add(foo);
				await (s.SaveAsync(foo));
				await (s.SaveAsync(baz));
				foo.Bytes = GetBytes("foobar");
				await (s.FlushAsync());
				fooKey = foo.Key;
				bazCode = baz.Code;
			}

			using (ISession s = OpenSession())
			{
				Foo foo = (Foo)await (s.GetAsync(typeof (Foo), fooKey));
				Assert.IsTrue(NHibernateUtil.IsInitialized(foo.Bytes));
				// H2.1 has 6 here, but we are using Unicode
				Assert.AreEqual(12, foo.Bytes.Length);
				Baz baz = (Baz)await (s.GetAsync(typeof (Baz), bazCode));
				Assert.AreEqual(1, baz.FooSet.Count);
				await (s.FlushAsync());
			}

			sessions.EvictCollection("NHibernate.DomainModel.Baz.FooSet");
			using (ISession s = OpenSession())
			{
				Baz baz = (Baz)await (s.GetAsync(typeof (Baz), bazCode));
				Assert.IsFalse(NHibernateUtil.IsInitialized(baz.FooSet));
				baz.FooSet = null;
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				Foo foo = (Foo)await (s.GetAsync(typeof (Foo), fooKey));
				Assert.AreEqual(12, foo.Bytes.Length);
				Baz baz = (Baz)await (s.GetAsync(typeof (Baz), bazCode));
				Assert.IsFalse(NHibernateUtil.IsInitialized(baz.FooSet));
				Assert.AreEqual(0, baz.FooSet.Count);
				await (s.DeleteAsync(baz));
				await (s.DeleteAsync(foo));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task MoveLazyCollectionAsync()
		{
			string fooKey, bazCode, baz2Code;
			using (ISession s = OpenSession())
			{
				Baz baz = new Baz();
				Baz baz2 = new Baz();
				baz.FooSet = new HashSet<FooProxy>();
				Foo foo = new Foo();
				baz.FooSet.Add(foo);
				await (s.SaveAsync(foo));
				await (s.SaveAsync(baz));
				await (s.SaveAsync(baz2));
				foo.Bytes = GetBytes("foobar");
				await (s.FlushAsync());
				fooKey = foo.Key;
				bazCode = baz.Code;
				baz2Code = baz2.Code;
			}

			using (ISession s = OpenSession())
			{
				Foo foo = (Foo)await (s.GetAsync(typeof (Foo), fooKey));
				Assert.IsTrue(NHibernateUtil.IsInitialized(foo.Bytes));
				Assert.AreEqual(12, foo.Bytes.Length);
				Baz baz = (Baz)await (s.GetAsync(typeof (Baz), bazCode));
				Assert.AreEqual(1, baz.FooSet.Count);
				await (s.FlushAsync());
			}

			sessions.EvictCollection("NHibernate.DomainModel.Baz.FooSet");
			using (ISession s = OpenSession())
			{
				Baz baz = (Baz)await (s.GetAsync(typeof (Baz), bazCode));
				Assert.IsFalse(NHibernateUtil.IsInitialized(baz.FooSet));
				Baz baz2 = (Baz)await (s.GetAsync(typeof (Baz), baz2Code));
				baz2.FooSet = baz.FooSet;
				baz.FooSet = null;
				Assert.IsFalse(NHibernateUtil.IsInitialized(baz2.FooSet));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				Foo foo = (Foo)await (s.GetAsync(typeof (Foo), fooKey));
				Assert.AreEqual(12, foo.Bytes.Length);
				Baz baz = (Baz)await (s.GetAsync(typeof (Baz), bazCode));
				Baz baz2 = (Baz)await (s.GetAsync(typeof (Baz), baz2Code));
				Assert.IsFalse(NHibernateUtil.IsInitialized(baz.FooSet));
				Assert.AreEqual(0, baz.FooSet.Count);
				Assert.IsTrue(NHibernateUtil.IsInitialized(baz2.FooSet)); //FooSet has batching enabled
				Assert.AreEqual(1, baz2.FooSet.Count);
				await (s.DeleteAsync(baz));
				await (s.DeleteAsync(baz2));
				await (s.DeleteAsync(foo));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task CriteriaCollectionAsync()
		{
			ISession s = OpenSession();
			Baz bb = (Baz)await (s.CreateCriteria(typeof (Baz)).UniqueResultAsync());
			Baz baz = new Baz();
			await (s.SaveAsync(baz));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			Baz b = (Baz)await (s.CreateCriteria(typeof (Baz)).UniqueResultAsync());
			Assert.IsTrue(NHibernateUtil.IsInitialized(b.TopGlarchez));
			Assert.AreEqual(0, b.TopGlarchez.Count);
			await (s.DeleteAsync(b));
			await (s.FlushAsync());
			await (s.CreateCriteria(typeof (Baz)).CreateCriteria("TopFoos").Add(Expression.IsNotNull("id")).ListAsync());
			await (s.CreateCriteria(typeof (Baz)).CreateCriteria("Foo").CreateCriteria("Component.Glarch").CreateCriteria("ProxySet").Add(Expression.IsNotNull("id")).ListAsync());
			s.Close();
		}

		private static bool IsEmpty(IEnumerable enumerable)
		{
			return !enumerable.GetEnumerator().MoveNext();
		}

		private static bool ContainsSingleObject(IEnumerable enumerable, object obj)
		{
			IEnumerator enumerator = enumerable.GetEnumerator();
			// Fail if no items
			if (!enumerator.MoveNext())
			{
				return false;
			}

			// Fail if item not equal
			if (!Equals(obj, enumerator.Current))
			{
				return false;
			}

			// Fail if more items
			if (enumerator.MoveNext())
			{
				return false;
			}

			// Succeed
			return true;
		}

		[Test]
		public async Task QueryAsync()
		{
			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();
			Foo foo = new Foo();
			await (s.SaveAsync(foo));
			Foo foo2 = new Foo();
			await (s.SaveAsync(foo2));
			foo.TheFoo = foo2;
			IList list = await (s.CreateQuery("from Foo foo inner join fetch foo.TheFoo").ListAsync());
			Foo foof = (Foo)list[0];
			Assert.IsTrue(NHibernateUtil.IsInitialized(foof.TheFoo));
			list = await (s.CreateQuery("from Baz baz left outer join fetch baz.FooToGlarch").ListAsync());
			list = await ((await (s.CreateQuery("select foo, bar from Foo foo left outer join foo.TheFoo bar where foo = ?").SetEntityAsync(0, foo))).ListAsync());
			object[] row1 = (object[])list[0];
			Assert.IsTrue(row1[0] == foo && row1[1] == foo2);
			await (s.CreateQuery("select foo.TheFoo.TheFoo.String from foo in class Foo where foo.TheFoo = 'bar'").ListAsync());
			await (s.CreateQuery("select foo.TheFoo.TheFoo.TheFoo.String from foo in class Foo where foo.TheFoo.TheFoo = 'bar'").ListAsync());
			await (s.CreateQuery("select foo.TheFoo.TheFoo.String from foo in class Foo where foo.TheFoo.TheFoo.TheFoo.String = 'bar'").ListAsync());
			//			if( !( dialect is Dialect.HSQLDialect ) ) 
			//			{
			await (s.CreateQuery("select foo.String from foo in class Foo where foo.TheFoo.TheFoo.TheFoo = foo.TheFoo.TheFoo").ListAsync());
			//			}
			await (s.CreateQuery("select foo.String from foo in class Foo where foo.TheFoo.TheFoo = 'bar' and foo.TheFoo.TheFoo.TheFoo = 'baz'").ListAsync());
			await (s.CreateQuery("select foo.String from foo in class Foo where foo.TheFoo.TheFoo.TheFoo.String = 'a' and foo.TheFoo.String = 'b'").ListAsync());
			await (s.CreateQuery("from bar in class Bar, foo in elements(bar.Baz.FooArray)").ListAsync());
			if (Dialect is DB2Dialect)
			{
				await (s.CreateQuery("from foo in class Foo where lower( foo.TheFoo.String ) = 'foo'").ListAsync());
				await (s.CreateQuery("from foo in class Foo where lower( (foo.TheFoo.String || 'foo') || 'bar' ) = 'foo'").ListAsync());
				await (s.CreateQuery("from foo in class Foo where repeat( (foo.TheFoo.STring || 'foo') || 'bar', 2 ) = 'foo'").ListAsync());
				await (s.CreateQuery("From foo in class Bar where foo.TheFoo.Integer is not null and repeat( (foo.TheFoo.String || 'foo') || 'bar', (5+5)/2 ) = 'foo'").ListAsync());
				await (s.CreateQuery("From foo in class Bar where foo.TheFoo.Integer is not null or repeat( (foo.TheFoo.String || 'foo') || 'bar', (5+5)/2 ) = 'foo'").ListAsync());
			}

			if (Dialect is MsSql2000Dialect)
			{
				await (s.CreateQuery("select baz from Baz as baz join baz.FooArray foo group by baz order by sum(foo.Float)").EnumerableAsync());
			}

			await (s.CreateQuery("from Foo as foo where foo.Component.Glarch.Name is not null").ListAsync());
			await (s.CreateQuery("from Foo as foo left outer join foo.Component.Glarch as glarch where glarch.Name = 'foo'").ListAsync());
			list = await (s.CreateQuery("from Foo").ListAsync());
			Assert.AreEqual(2, list.Count);
			Assert.IsTrue(list[0] is FooProxy);
			list = await (s.CreateQuery("from Foo foo left outer join foo.TheFoo").ListAsync());
			Assert.AreEqual(2, list.Count);
			Assert.IsTrue(((object[])list[0])[0] is FooProxy);
			await (s.CreateQuery("From Foo, Bar").ListAsync());
			await (s.CreateQuery("from Baz baz left join baz.FooToGlarch, Bar bar join bar.TheFoo").ListAsync());
			await (s.CreateQuery("from Baz baz left join baz.FooToGlarch join baz.FooSet").ListAsync());
			await (s.CreateQuery("from Baz baz left join baz.FooToGlarch join fetch baz.FooSet foo left join fetch foo.TheFoo").ListAsync());
			list = await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo where foo.String='osama bin laden' and foo.Boolean = true order by foo.String asc, foo.Component.Count desc").ListAsync());
			Assert.AreEqual(0, list.Count, "empty query");
			IEnumerable enumerable = await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo where foo.String='osama bin laden' order by foo.String asc, foo.Component.Count desc").EnumerableAsync());
			Assert.IsTrue(IsEmpty(enumerable), "empty enumerator");
			list = await (s.CreateQuery("select foo.TheFoo from foo in class NHibernate.DomainModel.Foo").ListAsync());
			Assert.AreEqual(1, list.Count, "query");
			Assert.AreEqual(foo.TheFoo, list[0], "returned object");
			foo.TheFoo.TheFoo = foo;
			foo.String = "fizard";
			if (Dialect.SupportsSubSelects && TestDialect.SupportsOperatorSome)
			{
				if (!(Dialect is FirebirdDialect))
				{
					list = await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo where ? = some elements(foo.Component.ImportantDates)").SetDateTime(0, DateTime.Today).ListAsync());
					Assert.AreEqual(2, list.Count, "component query");
				}

				list = await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo where size(foo.Component.ImportantDates) = 3").ListAsync());
				Assert.AreEqual(2, list.Count, "component query");
				list = await (s.CreateQuery("from foo in class Foo where 0 = size(foo.Component.ImportantDates)").ListAsync());
				Assert.AreEqual(0, list.Count, "component query");
				list = await (s.CreateQuery("from foo in class Foo where exists elements(foo.Component.ImportantDates)").ListAsync());
				Assert.AreEqual(2, list.Count, "component query");
				await (s.CreateQuery("from foo in class Foo where not exists (from bar in class Bar where bar.id = foo.id)").ListAsync());
				await (s.CreateQuery("select foo.TheFoo from foo in class Foo where foo = some(select x from x in class Foo where x.Long > foo.TheFoo.Long)").ListAsync());
				await (s.CreateQuery("from foo in class Foo where foo = some(select x from x in class Foo where x.Long > foo.TheFoo.Long) and foo.TheFoo.String='baz'").ListAsync());
				await (s.CreateQuery("from foo in class Foo where foo.TheFoo.String='baz' and foo = some(select x from x in class Foo where x.Long>foo.TheFoo.Long)").ListAsync());
				await (s.CreateQuery("from foo in class Foo where foo = some(select x from x in class Foo where x.Long > foo.TheFoo.Long)").ListAsync());
				await (s.CreateQuery("select foo.String, foo.Date, foo.TheFoo.String, foo.id from foo in class Foo, baz in class Baz where foo in elements(baz.FooArray) and foo.String like 'foo'").EnumerableAsync());
			}

			list = await (s.CreateQuery("from foo in class Foo where foo.Component.Count is null order by foo.Component.Count").ListAsync());
			Assert.AreEqual(0, list.Count, "component query");
			list = await (s.CreateQuery("from foo in class Foo where foo.Component.Name='foo'").ListAsync());
			Assert.AreEqual(2, list.Count, "component query");
			list = await (s.CreateQuery("select distinct foo.Component.Name, foo.Component.Name from foo in class Foo where foo.Component.Name='foo'").ListAsync());
			Assert.AreEqual(1, list.Count, "component query");
			list = await (s.CreateQuery("select distinct foo.Component.Name, foo.id from foo in class Foo where foo.Component.Name='foo'").ListAsync());
			Assert.AreEqual(2, list.Count, "component query");
			list = await (s.CreateQuery("select foo.TheFoo from foo in class Foo").ListAsync());
			Assert.AreEqual(2, list.Count, "query");
			list = await (s.CreateQuery("from foo in class Foo where foo.id=?").SetString(0, foo.Key).ListAsync());
			Assert.AreEqual(1, list.Count, "id query");
			list = await (s.CreateQuery("from foo in class Foo where foo.Key=?").SetString(0, foo.Key).ListAsync());
			Assert.AreEqual(1, list.Count, "named id query");
			Assert.AreSame(foo, list[0], "id query");
			list = await (s.CreateQuery("select foo.TheFoo from foo in class Foo where foo.String='fizard'").ListAsync());
			Assert.AreEqual(1, list.Count, "query");
			Assert.AreSame(foo.TheFoo, list[0], "returned object");
			list = await (s.CreateQuery("from foo in class Foo where foo.Component.Subcomponent.Name='bar'").ListAsync());
			Assert.AreEqual(2, list.Count, "components of components");
			list = await (s.CreateQuery("select foo.TheFoo from foo in class Foo where foo.TheFoo.id=?").SetString(0, foo.TheFoo.Key).ListAsync());
			Assert.AreEqual(1, list.Count, "by id query");
			Assert.AreSame(foo.TheFoo, list[0], "by id returned object");
			await ((await (s.CreateQuery("from foo in class Foo where foo.TheFoo = ?").SetEntityAsync(0, foo.TheFoo))).ListAsync());
			Assert.IsTrue(IsEmpty(await (s.CreateQuery("from bar in class Bar where bar.String='a string' or bar.String='a string'").EnumerableAsync())));
			enumerable = await (s.CreateQuery("select foo.Component.Name, elements(foo.Component.ImportantDates) from foo in class Foo where foo.TheFoo.id=?").SetString(0, foo.TheFoo.Key).EnumerableAsync());
			int i = 0;
			foreach (object[] row in enumerable)
			{
				i++;
				Assert.IsTrue(row[0] is String);
				Assert.IsTrue(row[1] == null || row[1] is DateTime);
			}

			Assert.AreEqual(3, i); //WAS: 4
			enumerable = await (s.CreateQuery("select max(elements(foo.Component.ImportantDates)) from foo in class Foo group by foo.id").EnumerableAsync());
			IEnumerator enumerator = enumerable.GetEnumerator();
			Assert.IsTrue(enumerator.MoveNext());
			Assert.IsTrue(enumerator.Current is DateTime);
			list = await (s.CreateQuery("select foo.TheFoo.TheFoo.TheFoo from foo in class Foo, foo2 in class Foo where" + " foo = foo2.TheFoo and not not ( not foo.String='fizard' )" + " and foo2.String between 'a' and (foo.TheFoo.String)" + (Dialect is SQLiteDialect ? " and ( foo2.String in ( 'fiz', 'blah') or 1=1 )" : " and ( foo2.String in ( 'fiz', 'blah', foo.TheFoo.String, foo.String, foo2.String ) )")).ListAsync());
			Assert.AreEqual(1, list.Count, "complex query");
			Assert.AreSame(foo, list[0], "returned object");
			foo.String = "from BoogieDown  -tinsel town  =!@#$^&*())";
			list = await (s.CreateQuery("from foo in class Foo where foo.String='from BoogieDown  -tinsel town  =!@#$^&*())'").ListAsync());
			Assert.AreEqual(1, list.Count, "single quotes");
			list = await (s.CreateQuery("from foo in class Foo where not foo.String='foo''bar'").ListAsync());
			Assert.AreEqual(2, list.Count, "single quotes");
			list = await (s.CreateQuery("from foo in class Foo where foo.Component.Glarch.Next is null").ListAsync());
			Assert.AreEqual(2, list.Count, "query association in component");
			Bar bar = new Bar();
			Baz baz = new Baz();
			baz.SetDefaults();
			bar.Baz = baz;
			baz.ManyToAny = new List<object>();
			baz.ManyToAny.Add(bar);
			baz.ManyToAny.Add(foo);
			await (s.SaveAsync(bar));
			await (s.SaveAsync(baz));
			list = await (s.CreateQuery(" from bar in class Bar where bar.Baz.Count=667 and bar.Baz.Count!=123 and not bar.Baz.Name='1-E-1'").ListAsync());
			Assert.AreEqual(1, list.Count, "query many-to-one");
			list = await (s.CreateQuery(" from i in class Bar where i.Baz.Name='Bazza'").ListAsync());
			Assert.AreEqual(1, list.Count, "query many-to-one");
			if (DialectSupportsCountDistinct)
			{
				enumerable = await (s.CreateQuery("select count(distinct foo.TheFoo) from foo in class Foo").EnumerableAsync());
				Assert.IsTrue(ContainsSingleObject(enumerable, (long)2), "count"); // changed to Int64 (HQLFunction H3.2)
			}

			enumerable = await (s.CreateQuery("select count(foo.TheFoo.Boolean) from foo in class Foo").EnumerableAsync());
			Assert.IsTrue(ContainsSingleObject(enumerable, (long)2), "count"); // changed to Int64 (HQLFunction H3.2)
			enumerable = await (s.CreateQuery("select count(*), foo.Int from foo in class Foo group by foo.Int").EnumerableAsync());
			enumerator = enumerable.GetEnumerator();
			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual(3L, (long)((object[])enumerator.Current)[0]);
			Assert.IsFalse(enumerator.MoveNext());
			enumerable = await (s.CreateQuery("select sum(foo.TheFoo.Int) from foo in class Foo").EnumerableAsync());
			Assert.IsTrue(ContainsSingleObject(enumerable, (long)4), "sum"); // changed to Int64 (HQLFunction H3.2)
			enumerable = await (s.CreateQuery("select count(foo) from foo in class Foo where foo.id=?").SetString(0, foo.Key).EnumerableAsync());
			Assert.IsTrue(ContainsSingleObject(enumerable, (long)1), "id query count");
			list = await (s.CreateQuery("from foo in class Foo where foo.Boolean = ?").SetBoolean(0, true).ListAsync());
			list = await (s.CreateQuery("select new Foo(fo.X) from Fo fo").ListAsync());
			list = await (s.CreateQuery("select new Foo(fo.Integer) from Foo fo").ListAsync());
			list = await (s.CreateQuery("select new Foo(fo.X) from Foo fo").SetCacheable(true).ListAsync());
			Assert.IsTrue(list.Count == 3);
			list = await (s.CreateQuery("select new Foo(fo.X) from Foo fo").SetCacheable(true).ListAsync());
			Assert.IsTrue(list.Count == 3);
			enumerable = await (s.CreateQuery("select new Foo(fo.X) from Foo fo").EnumerableAsync());
			enumerator = enumerable.GetEnumerator();
			Assert.IsTrue(enumerator.MoveNext(), "projection iterate (results)");
			Assert.IsTrue(typeof (Foo).IsAssignableFrom(enumerator.Current.GetType()), "projection iterate (return check)");
			// TODO: ScrollableResults not implemented
			//ScrollableResults sr = s.CreateQuery("select new Foo(fo.x) from Foo fo").Scroll();
			//Assert.IsTrue( "projection scroll (results)", sr.next() );
			//Assert.IsTrue( "projection scroll (return check)", typeof(Foo).isAssignableFrom( sr.get(0).getClass() ) );
			list = await (s.CreateQuery("select foo.Long, foo.Component.Name, foo, foo.TheFoo from foo in class Foo").ListAsync());
			Assert.IsTrue(list.Count > 0);
			foreach (object[] row in list)
			{
				Assert.IsTrue(row[0] is long);
				Assert.IsTrue(row[1] is string);
				Assert.IsTrue(row[2] is Foo);
				Assert.IsTrue(row[3] is Foo);
			}

			if (DialectSupportsCountDistinct)
			{
				list = await (s.CreateQuery("select avg(foo.Float), max(foo.Component.Name), count(distinct foo.id) from foo in class Foo").ListAsync());
				Assert.IsTrue(list.Count > 0);
				foreach (object[] row in list)
				{
					Assert.IsTrue(row[0] is double); // changed from float to double (HQLFunction H3.2) 
					Assert.IsTrue(row[1] is string);
					Assert.IsTrue(row[2] is long); // changed from int to long (HQLFunction H3.2)
				}
			}

			list = await (s.CreateQuery("select foo.Long, foo.Component, foo, foo.TheFoo from foo in class Foo").ListAsync());
			Assert.IsTrue(list.Count > 0);
			foreach (object[] row in list)
			{
				Assert.IsTrue(row[0] is long);
				Assert.IsTrue(row[1] is FooComponent);
				Assert.IsTrue(row[2] is Foo);
				Assert.IsTrue(row[3] is Foo);
			}

			await (s.SaveAsync(new Holder("ice T")));
			await (s.SaveAsync(new Holder("ice cube")));
			Assert.AreEqual(15, (await (s.CreateQuery("from o in class System.Object").ListAsync())).Count);
			Assert.AreEqual(7, (await (s.CreateQuery("from n in class INamed").ListAsync())).Count);
			Assert.IsTrue((await (s.CreateQuery("from n in class INamed where n.Name is not null").ListAsync())).Count == 4);
			foreach (INamed named in await (s.CreateQuery("from n in class INamed").EnumerableAsync()))
			{
				Assert.IsNotNull(named);
			}

			await (s.SaveAsync(new Holder("bar")));
			enumerable = await (s.CreateQuery("from n0 in class INamed, n1 in class INamed where n0.Name = n1.Name").EnumerableAsync());
			int cnt = 0;
			foreach (object[] row in enumerable)
			{
				if (row[0] != row[1])
				{
					cnt++;
				}
			}

			//if ( !(dialect is Dialect.HSQLDialect) )
			//{
			Assert.IsTrue(cnt == 2);
			Assert.IsTrue((await (s.CreateQuery("from n0 in class INamed, n1 in class INamed where n0.Name = n1.Name").ListAsync())).Count == 7);
			//}
			IQuery qu = s.CreateQuery("from n in class INamed where n.Name = :name");
			object temp = qu.ReturnTypes;
			temp = qu.NamedParameters;
			int c = 0;
			foreach (object obj in await (s.CreateQuery("from o in class System.Object").EnumerableAsync()))
			{
				c++;
			}

			Assert.IsTrue(c == 16);
			await (s.CreateQuery("select baz.Code, min(baz.Count) from baz in class Baz group by baz.Code").EnumerableAsync());
			Assert.IsTrue(IsEmpty(await (s.CreateQuery("selecT baz from baz in class Baz where baz.StringDateMap['foo'] is not null or baz.StringDateMap['bar'] = ?").SetDateTime(0, DateTime.Today).EnumerableAsync())));
			list = await (s.CreateQuery("select baz from baz in class Baz where baz.StringDateMap['now'] is not null").ListAsync());
			Assert.AreEqual(1, list.Count);
			list = await (s.CreateQuery("select baz from baz in class Baz where baz.StringDateMap[:now] is not null").SetString("now", "now").ListAsync());
			Assert.AreEqual(1, list.Count);
			list = await (s.CreateQuery("select baz from baz in class Baz where baz.StringDateMap['now'] is not null and baz.StringDateMap['big bang'] < baz.StringDateMap['now']").ListAsync());
			Assert.AreEqual(1, list.Count);
			list = await (s.CreateQuery("select index(date) from Baz baz join baz.StringDateMap date").ListAsync());
			Console.WriteLine(list);
			Assert.AreEqual(3, list.Count);
			await (s.CreateQuery("from foo in class Foo where foo.Integer not between 1 and 5 and foo.String not in ('cde', 'abc') and foo.String is not null and foo.Integer<=3").ListAsync());
			await (s.CreateQuery("from Baz baz inner join baz.CollectionComponent.Nested.Foos foo where foo.String is null").ListAsync());
			if (Dialect.SupportsSubSelects)
			{
				await (s.CreateQuery("from Baz baz inner join baz.FooSet where '1' in (from baz.FooSet foo where foo.String is not null)").ListAsync());
				await (s.CreateQuery("from Baz baz where 'a' in elements(baz.CollectionComponent.Nested.Foos) and 1.0 in elements(baz.CollectionComponent.Nested.Floats)").ListAsync());
				await (s.CreateQuery("from Baz baz where 'b' in elements(baz.CollectionComponent.Nested.Foos) and 1.0 in elements(baz.CollectionComponent.Nested.Floats)").ListAsync());
			}

			await (s.CreateQuery("from Foo foo join foo.TheFoo where foo.TheFoo in ('1','2','3')").ListAsync());
			//if ( !(dialect is Dialect.HSQLDialect) )
			await (s.CreateQuery("from Foo foo left join foo.TheFoo where foo.TheFoo in ('1','2','3')").ListAsync());
			await (s.CreateQuery("select foo.TheFoo from Foo foo where foo.TheFoo in ('1','2','3')").ListAsync());
			await (s.CreateQuery("select foo.TheFoo.String from Foo foo where foo.TheFoo in ('1','2','3')").ListAsync());
			await (s.CreateQuery("select foo.TheFoo.String from Foo foo where foo.TheFoo.String in ('1','2','3')").ListAsync());
			await (s.CreateQuery("select foo.TheFoo.Long from Foo foo where foo.TheFoo.String in ('1','2','3')").ListAsync());
			await (s.CreateQuery("select count(*) from Foo foo where foo.TheFoo.String in ('1','2','3') or foo.TheFoo.Long in (1,2,3)").ListAsync());
			await (s.CreateQuery("select count(*) from Foo foo where foo.TheFoo.String in ('1','2','3') group by foo.TheFoo.Long").ListAsync());
			await (s.CreateQuery("from Foo foo1 left join foo1.TheFoo foo2 left join foo2.TheFoo where foo1.String is not null").ListAsync());
			await (s.CreateQuery("from Foo foo1 left join foo1.TheFoo.TheFoo where foo1.String is not null").ListAsync());
			await (s.CreateQuery("from Foo foo1 left join foo1.TheFoo foo2 left join foo1.TheFoo.TheFoo foo3 where foo1.String is not null").ListAsync());
			await (s.CreateQuery("select foo.Formula from Foo foo where foo.Formula > 0").ListAsync());
			int len = (await (s.CreateQuery("from Foo as foo join foo.TheFoo as foo2 where foo2.id >'a' or foo2.id <'a'").ListAsync())).Count;
			Assert.IsTrue(len == 2);
			await (s.DeleteAsync("from Holder"));
			await (txn.CommitAsync());
			s.Close();
			s = OpenSession();
			txn = s.BeginTransaction();
			baz = (Baz)await (s.CreateQuery("from Baz baz left outer join fetch baz.ManyToAny").UniqueResultAsync());
			Assert.IsTrue(NHibernateUtil.IsInitialized(baz.ManyToAny));
			Assert.IsTrue(baz.ManyToAny.Count == 2);
			BarProxy barp = (BarProxy)baz.ManyToAny[0];
			await (s.CreateQuery("from Baz baz join baz.ManyToAny").ListAsync());
			Assert.IsTrue((await (s.CreateQuery("select baz from Baz baz join baz.ManyToAny a where index(a) = 0").ListAsync())).Count == 1);
			FooProxy foop = (FooProxy)await (s.GetAsync(typeof (Foo), foo.Key));
			Assert.IsTrue(foop == baz.ManyToAny[1]);
			barp.Baz = baz;
			Assert.IsTrue((await (s.CreateQuery("select bar from Bar bar where bar.Baz.StringDateMap['now'] is not null").ListAsync())).Count == 1);
			Assert.IsTrue((await (s.CreateQuery("select bar from Bar bar join bar.Baz b where b.StringDateMap['big bang'] < b.StringDateMap['now'] and b.StringDateMap['now'] is not null").ListAsync())).Count == 1);
			Assert.IsTrue((await (s.CreateQuery("select bar from Bar bar where bar.Baz.StringDateMap['big bang'] < bar.Baz.StringDateMap['now'] and bar.Baz.StringDateMap['now'] is not null").ListAsync())).Count == 1);
			list = await (s.CreateQuery("select foo.String, foo.Component, foo.id from Bar foo").ListAsync());
			Assert.IsTrue(((FooComponent)((object[])list[0])[1]).Name == "foo");
			list = await (s.CreateQuery("select elements(baz.Components) from Baz baz").ListAsync());
			Assert.IsTrue(list.Count == 2);
			list = await (s.CreateQuery("select bc.Name from Baz baz join baz.Components bc").ListAsync());
			Assert.IsTrue(list.Count == 2);
			//list = s.CreateQuery("select bc from Baz baz join baz.components bc").List();
			await (s.CreateQuery("from Foo foo where foo.Integer < 10 order by foo.String").SetMaxResults(12).ListAsync());
			await (s.DeleteAsync(barp));
			await (s.DeleteAsync(baz));
			await (s.DeleteAsync(foop.TheFoo));
			await (s.DeleteAsync(foop));
			await (txn.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CascadeDeleteDetachedAsync()
		{
			Baz baz;
			using (ISession s = OpenSession())
			{
				baz = new Baz();
				IList<Fee> list = new List<Fee>();
				list.Add(new Fee());
				baz.Fees = list;
				await (s.SaveAsync(baz));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				baz = (Baz)await (s.GetAsync(typeof (Baz), baz.Code));
			}

			Assert.IsFalse(NHibernateUtil.IsInitialized(baz.Fees));
			using (ISession s = OpenSession())
			{
				await (s.DeleteAsync(baz));
				await (s.FlushAsync());
				Assert.IsFalse((await (s.CreateQuery("from Fee").EnumerableAsync())).GetEnumerator().MoveNext());
			}

			using (ISession s = OpenSession())
			{
				baz = new Baz();
				IList<Fee> list = new List<Fee>();
				list.Add(new Fee());
				list.Add(new Fee());
				baz.Fees = list;
				await (s.SaveAsync(baz));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				baz = (Baz)await (s.GetAsync(typeof (Baz), baz.Code));
				await (NHibernateUtil.InitializeAsync(baz.Fees));
			}

			Assert.AreEqual(2, baz.Fees.Count);
			using (ISession s = OpenSession())
			{
				await (s.DeleteAsync(baz));
				await (s.FlushAsync());
				Assert.IsTrue(IsEmpty(await (s.CreateQuery("from Fee").EnumerableAsync())));
			}
		}

		[Test]
		public async Task ForeignKeysAsync()
		{
			Baz baz;
			using (ISession s = OpenSession())
			{
				baz = new Baz();
				Foo foo = new Foo();
				IList<Foo> bag = new List<Foo>();
				bag.Add(foo);
				baz.IdFooBag = bag;
				baz.Foo = foo;
				await (s.SaveAsync(baz));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
				await (s.DeleteAsync(baz));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task NonlazyCollectionsAsync()
		{
			object glarchId;
			using (ISession s = OpenSession())
			{
				Glarch glarch1 = new Glarch();
				glarch1.ProxySet = new LinkedHashSet<GlarchProxy>();
				Glarch glarch2 = new Glarch();
				glarch1.ProxySet.Add(glarch1);
				await (s.SaveAsync(glarch2));
				glarchId = await (s.SaveAsync(glarch1));
				await (s.FlushAsync());
			}

			Glarch loadedGlarch;
			using (ISession s = OpenSession())
			{
				loadedGlarch = (Glarch)await (s.GetAsync(typeof (Glarch), glarchId));
				Assert.IsTrue(NHibernateUtil.IsInitialized(loadedGlarch.ProxySet));
			}

			// ProxySet is a non-lazy collection, so this should work outside
			// a session.
			Assert.AreEqual(1, loadedGlarch.ProxySet.Count);
			using (ISession s = OpenSession())
			{
				await (s.DeleteAsync("from Glarch"));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task ReuseDeletedCollectionAsync()
		{
			Baz baz, baz2;
			using (ISession s = OpenSession())
			{
				baz = new Baz();
				baz.SetDefaults();
				await (s.SaveAsync(baz));
				await (s.FlushAsync());
				await (s.DeleteAsync(baz));
				baz2 = new Baz();
				baz2.StringArray = new string[]{"x-y-z"};
				await (s.SaveAsync(baz2));
				await (s.FlushAsync());
			}

			baz2.StringSet = baz.StringSet;
			baz2.StringArray = baz.StringArray;
			baz2.FooArray = baz.FooArray;
			using (ISession s = OpenSession())
			{
				await (s.UpdateAsync(baz2));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				baz2 = (Baz)await (s.LoadAsync(typeof (Baz), baz2.Code));
				Assert.AreEqual(3, baz2.StringArray.Length);
				Assert.AreEqual(3, baz2.StringSet.Count);
				await (s.DeleteAsync(baz2));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task PropertyRefAsync()
		{
			object qid;
			object hid;
			using (ISession s = OpenSession())
			{
				Holder h = new Holder();
				h.Name = "foo";
				Holder h2 = new Holder();
				h2.Name = "bar";
				h.OtherHolder = h2;
				hid = await (s.SaveAsync(h));
				Qux q = new Qux();
				q.Holder = h2;
				qid = await (s.SaveAsync(q));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				Holder h = (Holder)await (s.LoadAsync(typeof (Holder), hid));
				Assert.AreEqual(h.Name, "foo");
				Assert.AreEqual(h.OtherHolder.Name, "bar");
				object[] res = (object[])(await (s.CreateQuery("from Holder h join h.OtherHolder oh where h.OtherHolder.Name = 'bar'").ListAsync()))[0];
				Assert.AreSame(h, res[0]);
				Qux q = (Qux)await (s.GetAsync(typeof (Qux), qid));
				Assert.AreSame(q.Holder, h.OtherHolder);
				await (s.DeleteAsync(h));
				await (s.DeleteAsync(q));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task QueryCollectionOfValuesAsync()
		{
			object gid;
			using (ISession s = OpenSession())
			{
				Baz baz = new Baz();
				baz.SetDefaults();
				await (s.SaveAsync(baz));
				Glarch g = new Glarch();
				gid = await (s.SaveAsync(g));
				if (Dialect.SupportsSubSelects)
				{
					await ((await (s.CreateFilterAsync(baz.FooArray, "where size(this.Bytes) > 0"))).ListAsync());
					await ((await (s.CreateFilterAsync(baz.FooArray, "where 0 in elements(this.Bytes)"))).ListAsync());
				}

				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				//s.CreateQuery("from Baz baz where baz.FooSet.String = 'foo'").List();
				//s.CreateQuery("from Baz baz where baz.FooArray.String = 'foo'").List();
				//s.CreateQuery("from Baz baz where baz.FooSet.foo.String = 'foo'").List();
				//s.CreateQuery("from Baz baz join baz.FooSet.Foo foo where foo.String = 'foo'").List();
				await (s.CreateQuery("from Baz baz join baz.FooSet foo join foo.TheFoo.TheFoo foo2 where foo2.String = 'foo'").ListAsync());
				await (s.CreateQuery("from Baz baz join baz.FooArray foo join foo.TheFoo.TheFoo foo2 where foo2.String = 'foo'").ListAsync());
				await (s.CreateQuery("from Baz baz join baz.StringDateMap date where index(date) = 'foo'").ListAsync());
				await (s.CreateQuery("from Baz baz join baz.TopGlarchez g where index(g) = 'A'").ListAsync());
				await (s.CreateQuery("select index(g) from Baz baz join baz.TopGlarchez g").ListAsync());
				Assert.AreEqual(3, (await (s.CreateQuery("from Baz baz left join baz.StringSet").ListAsync())).Count);
				Baz baz = (Baz)(await (s.CreateQuery("from Baz baz join baz.StringSet str where str='foo'").ListAsync()))[0];
				Assert.IsFalse(NHibernateUtil.IsInitialized(baz.StringSet));
				baz = (Baz)(await (s.CreateQuery("from Baz baz left join fetch baz.StringSet").ListAsync()))[0];
				Assert.IsTrue(NHibernateUtil.IsInitialized(baz.StringSet));
				Assert.AreEqual(1, (await (s.CreateQuery("from Baz baz join baz.StringSet string where string='foo'").ListAsync())).Count);
				Assert.AreEqual(1, (await (s.CreateQuery("from Baz baz inner join baz.Components comp where comp.Name='foo'").ListAsync())).Count);
				//IList bss = s.CreateQuery("select baz, ss from Baz baz inner join baz.StringSet ss").List();
				await (s.CreateQuery("from Glarch g inner join g.FooComponents comp where comp.Fee is not null").ListAsync());
				await (s.CreateQuery("from Glarch g inner join g.FooComponents comp join comp.Fee fee where fee.Count > 0").ListAsync());
				await (s.CreateQuery("from Glarch g inner join g.FooComponents comp where comp.Fee.Count is not null").ListAsync());
				await (s.DeleteAsync(baz));
				//s.delete("from Glarch g");
				await (s.DeleteAsync(await (s.GetAsync(typeof (Glarch), gid))));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task BatchLoadAsync()
		{
			Baz baz, baz2, baz3;
			using (ISession s = OpenSession())
			{
				baz = new Baz();
				var stringSet = new SortedSet<string>{"foo", "bar"};
				var fooSet = new HashSet<FooProxy>();
				for (int i = 0; i < 3; i++)
				{
					Foo foo = new Foo();
					await (s.SaveAsync(foo));
					fooSet.Add(foo);
				}

				baz.FooSet = fooSet;
				baz.StringSet = stringSet;
				await (s.SaveAsync(baz));
				baz2 = new Baz();
				fooSet = new HashSet<FooProxy>();
				for (int i = 0; i < 2; i++)
				{
					Foo foo = new Foo();
					await (s.SaveAsync(foo));
					fooSet.Add(foo);
				}

				baz2.FooSet = fooSet;
				await (s.SaveAsync(baz2));
				baz3 = new Baz();
				stringSet = new SortedSet<string>();
				stringSet.Add("foo");
				stringSet.Add("baz");
				baz3.StringSet = stringSet;
				await (s.SaveAsync(baz3));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
				baz2 = (Baz)await (s.LoadAsync(typeof (Baz), baz2.Code));
				baz3 = (Baz)await (s.LoadAsync(typeof (Baz), baz3.Code));
				Assert.IsFalse(NHibernateUtil.IsInitialized(baz.FooSet));
				Assert.IsFalse(NHibernateUtil.IsInitialized(baz2.FooSet));
				Assert.IsFalse(NHibernateUtil.IsInitialized(baz3.FooSet));
				Assert.IsFalse(NHibernateUtil.IsInitialized(baz.StringSet));
				Assert.IsFalse(NHibernateUtil.IsInitialized(baz2.StringSet));
				Assert.IsFalse(NHibernateUtil.IsInitialized(baz3.StringSet));
				Assert.AreEqual(3, baz.FooSet.Count);
				Assert.IsTrue(NHibernateUtil.IsInitialized(baz.FooSet));
				Assert.IsTrue(NHibernateUtil.IsInitialized(baz2.FooSet));
				Assert.IsTrue(NHibernateUtil.IsInitialized(baz3.FooSet));
				Assert.AreEqual(2, baz2.FooSet.Count);
				Assert.IsTrue(baz3.StringSet.Contains("baz"));
				Assert.IsTrue(NHibernateUtil.IsInitialized(baz.StringSet));
				Assert.IsTrue(NHibernateUtil.IsInitialized(baz2.StringSet));
				Assert.IsTrue(NHibernateUtil.IsInitialized(baz3.StringSet));
				Assert.AreEqual(2, baz.StringSet.Count);
				Assert.AreEqual(0, baz2.StringSet.Count);
				await (s.DeleteAsync(baz));
				await (s.DeleteAsync(baz2));
				await (s.DeleteAsync(baz3));
				IEnumerable en = new JoinedEnumerable(new IEnumerable[]{baz.FooSet, baz2.FooSet});
				foreach (object obj in en)
				{
					await (s.DeleteAsync(obj));
				}

				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task FetchInitializedCollectionAsync()
		{
			ISession s = OpenSession();
			Baz baz = new Baz();
			IList<Foo> fooBag = new List<Foo>();
			fooBag.Add(new Foo());
			fooBag.Add(new Foo());
			baz.FooBag = fooBag;
			await (s.SaveAsync(baz));
			await (s.FlushAsync());
			fooBag = baz.FooBag;
			await (s.CreateQuery("from Baz baz left join fetch baz.FooBag").ListAsync());
			Assert.IsTrue(NHibernateUtil.IsInitialized(fooBag));
			s.Close();
			s = OpenSession();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			Object bag = baz.FooBag;
			Assert.IsFalse(NHibernateUtil.IsInitialized(bag));
			await (s.CreateQuery("from Baz baz left join fetch baz.FooBag").ListAsync());
			Assert.IsTrue(bag == baz.FooBag);
			Assert.IsTrue(baz.FooBag.Count == 2);
			await (s.DeleteAsync(baz));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task LateCollectionAddAsync()
		{
			object id;
			using (ISession s = OpenSession())
			{
				Baz baz = new Baz();
				IList<string> l = new List<string>();
				baz.StringList = l;
				l.Add("foo");
				id = await (s.SaveAsync(baz));
				l.Add("bar");
				await (s.FlushAsync());
				l.Add("baz");
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				Baz baz = (Baz)await (s.LoadAsync(typeof (Baz), id));
				Assert.AreEqual(3, baz.StringList.Count);
				Assert.IsTrue(baz.StringList.Contains("foo"));
				Assert.IsTrue(baz.StringList.Contains("bar"));
				Assert.IsTrue(baz.StringList.Contains("baz"));
				await (s.DeleteAsync(baz));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task UpdateAsync()
		{
			ISession s = OpenSession();
			Foo foo = new Foo();
			await (s.SaveAsync(foo));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			FooProxy foo2 = (FooProxy)await (s.LoadAsync(typeof (Foo), foo.Key));
			foo2.String = "dirty";
			foo2.Boolean = false;
			foo2.Bytes = new byte[]{1, 2, 3};
			foo2.Date = DateTime.Today;
			foo2.Short = 69;
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			Foo foo3 = new Foo();
			await (s.LoadAsync(foo3, foo.Key));
			Assert.IsTrue(foo2.EqualsFoo(foo3), "update");
			await (s.DeleteAsync(foo3));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task ListRemoveAsync()
		{
			using (ISession s = OpenSession())
			{
				Baz b = new Baz();
				IList<string> stringList = new List<string>();
				IList<Fee> feeList = new List<Fee>();
				b.Fees = feeList;
				b.StringList = stringList;
				feeList.Add(new Fee());
				feeList.Add(new Fee());
				feeList.Add(new Fee());
				feeList.Add(new Fee());
				stringList.Add("foo");
				stringList.Add("bar");
				stringList.Add("baz");
				stringList.Add("glarch");
				await (s.SaveAsync(b));
				await (s.FlushAsync());
				stringList.RemoveAt(1);
				feeList.RemoveAt(1);
				await (s.FlushAsync());
				await (s.EvictAsync(b));
				await (s.RefreshAsync(b));
				Assert.AreEqual(3, b.Fees.Count);
				stringList = b.StringList;
				Assert.AreEqual(3, stringList.Count);
				Assert.AreEqual("baz", stringList[1]);
				Assert.AreEqual("foo", stringList[0]);
				await (s.DeleteAsync(b));
				await (s.DeleteAsync("from Fee"));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task FetchInitializedCollectionDupeAsync()
		{
			string bazCode;
			using (ISession s = OpenSession())
			{
				Baz baz = new Baz();
				IList<Foo> fooBag = new List<Foo>();
				fooBag.Add(new Foo());
				fooBag.Add(new Foo());
				baz.FooBag = fooBag;
				await (s.SaveAsync(baz));
				await (s.FlushAsync());
				fooBag = baz.FooBag;
				await (s.CreateQuery("from Baz baz left join fetch baz.FooBag").ListAsync());
				Assert.IsTrue(NHibernateUtil.IsInitialized(fooBag));
				Assert.AreSame(fooBag, baz.FooBag);
				Assert.AreEqual(2, baz.FooBag.Count);
				bazCode = baz.Code;
			}

			using (ISession s = OpenSession())
			{
				Baz baz = (Baz)await (s.LoadAsync(typeof (Baz), bazCode));
				object bag = baz.FooBag;
				Assert.IsFalse(NHibernateUtil.IsInitialized(bag));
				await (s.CreateQuery("from Baz baz left join fetch baz.FooBag").ListAsync());
				Assert.IsTrue(NHibernateUtil.IsInitialized(bag));
				Assert.AreSame(bag, baz.FooBag);
				Assert.AreEqual(2, baz.FooBag.Count);
				await (s.DeleteAsync(baz));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task SortablesAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Baz b = new Baz();
			var ss = new HashSet<Sortable>{new Sortable("foo"), new Sortable("bar"), new Sortable("baz")};
			b.Sortablez = ss;
			await (s.SaveAsync(b));
			await (s.FlushAsync());
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			IList result = await (s.CreateCriteria(typeof (Baz)).AddOrder(Order.Asc("Name")).ListAsync());
			b = (Baz)result[0];
			Assert.IsTrue(b.Sortablez.Count == 3);
			// compare the first item in the "Set" sortablez - can't reference
			// the first item using b.sortablez[0] because it thinks 0 is the
			// DictionaryEntry key - not the index.
			foreach (Sortable sortable in b.Sortablez)
			{
				Assert.AreEqual(sortable.name, "bar");
				break;
			}

			await (s.FlushAsync());
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			result = await (s.CreateQuery("from Baz baz left join fetch baz.Sortablez order by baz.Name asc").ListAsync());
			b = (Baz)result[0];
			Assert.IsTrue(b.Sortablez.Count == 3);
			foreach (Sortable sortable in b.Sortablez)
			{
				Assert.AreEqual(sortable.name, "bar");
				break;
			}

			await (s.FlushAsync());
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			result = await (s.CreateQuery("from Baz baz order by baz.Name asc").ListAsync());
			b = (Baz)result[0];
			Assert.IsTrue(b.Sortablez.Count == 3);
			foreach (Sortable sortable in b.Sortablez)
			{
				Assert.AreEqual(sortable.name, "bar");
				break;
			}

			await (s.DeleteAsync(b));
			await (s.FlushAsync());
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task FetchListAsync()
		{
			ISession s = OpenSession();
			Baz baz = new Baz();
			await (s.SaveAsync(baz));
			Foo foo = new Foo();
			await (s.SaveAsync(foo));
			Foo foo2 = new Foo();
			await (s.SaveAsync(foo2));
			await (s.FlushAsync());
			IList<Fee> list = new List<Fee>();
			for (int i = 0; i < 5; i++)
			{
				Fee fee = new Fee();
				list.Add(fee);
			}

			baz.Fees = list;
			var result = await (s.CreateQuery("from Foo foo, Baz baz left join fetch baz.Fees").ListAsync());
			Assert.IsTrue(NHibernateUtil.IsInitialized(((Baz)((object[])result[0])[1]).Fees));
			await (s.DeleteAsync(foo));
			await (s.DeleteAsync(foo2));
			await (s.DeleteAsync(baz));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task BagOneToManyAsync()
		{
			ISession s = OpenSession();
			Baz baz = new Baz();
			IList<Baz> list = new List<Baz>();
			baz.Bazez = list;
			list.Add(new Baz());
			await (s.SaveAsync(baz));
			await (s.FlushAsync());
			list.Add(new Baz());
			await (s.FlushAsync());
			list.Insert(0, new Baz());
			await (s.FlushAsync());
			object toDelete = list[1];
			list.RemoveAt(1);
			await (s.DeleteAsync(toDelete));
			await (s.FlushAsync());
			await (s.DeleteAsync(baz));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task QueryLockModeAsync()
		{
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			Bar bar = new Bar();
			Assert.IsNull(bar.Bytes);
			await (s.SaveAsync(bar));
			Assert.IsNotNull(bar.Bytes);
			await (s.FlushAsync());
			Assert.IsNotNull(bar.Bytes);
			bar.String = "changed";
			Baz baz = new Baz();
			baz.Foo = bar;
			await (s.SaveAsync(baz));
			Assert.IsNotNull(bar.Bytes);
			IQuery q = s.CreateQuery("from Foo foo, Bar bar");
			q.SetLockMode("bar", LockMode.Upgrade);
			object[] result = (object[])(await (q.ListAsync()))[0];
			Assert.IsNotNull(bar.Bytes);
			object b = result[0];
			Assert.IsTrue(s.GetCurrentLockMode(b) == LockMode.Write && s.GetCurrentLockMode(result[1]) == LockMode.Write);
			await (tx.CommitAsync());
			Assert.IsNotNull(bar.Bytes);
			s.Disconnect();
			s.Reconnect();
			tx = s.BeginTransaction();
			Assert.IsNotNull(bar.Bytes);
			Assert.AreEqual(LockMode.None, s.GetCurrentLockMode(b));
			Assert.IsNotNull(bar.Bytes);
			await (s.CreateQuery("from Foo foo").ListAsync());
			Assert.IsNotNull(bar.Bytes);
			Assert.AreEqual(LockMode.None, s.GetCurrentLockMode(b));
			q = s.CreateQuery("from Foo foo");
			q.SetLockMode("foo", LockMode.Read);
			await (q.ListAsync());
			Assert.AreEqual(LockMode.Read, s.GetCurrentLockMode(b));
			await (s.EvictAsync(baz));
			await (tx.CommitAsync());
			s.Disconnect();
			s.Reconnect();
			tx = s.BeginTransaction();
			Assert.AreEqual(LockMode.None, s.GetCurrentLockMode(b));
			await (s.DeleteAsync(await (s.LoadAsync(typeof (Baz), baz.Code))));
			Assert.AreEqual(LockMode.None, s.GetCurrentLockMode(b));
			await (tx.CommitAsync());
			s.Close();
			s = OpenSession();
			tx = s.BeginTransaction();
			q = s.CreateQuery("from Foo foo, Bar bar, Bar bar2");
			q.SetLockMode("bar", LockMode.Upgrade);
			q.SetLockMode("bar2", LockMode.Read);
			result = (object[])(await (q.ListAsync()))[0];
			Assert.IsTrue(s.GetCurrentLockMode(result[0]) == LockMode.Upgrade && s.GetCurrentLockMode(result[1]) == LockMode.Upgrade);
			await (s.DeleteAsync(result[0]));
			await (tx.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task ManyToManyBagAsync()
		{
			ISession s = OpenSession();
			Baz baz = new Baz();
			object id = await (s.SaveAsync(baz));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), id));
			baz.FooBag.Add(new Foo());
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), id));
			Assert.IsFalse(NHibernateUtil.IsInitialized(baz.FooBag));
			Assert.AreEqual(1, baz.FooBag.Count);
			Assert.IsTrue(NHibernateUtil.IsInitialized(baz.FooBag[0]));
			await (s.DeleteAsync(baz));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task IdBagAsync()
		{
			ISession s = OpenSession();
			Baz baz = new Baz();
			await (s.SaveAsync(baz));
			IList<Foo> l = new List<Foo>();
			IList<byte[]> l2 = new List<byte[]>();
			baz.IdFooBag = l;
			baz.ByteBag = l2;
			l.Add(new Foo());
			l.Add(new Bar());
			byte[] bytes = GetBytes("ffo");
			l2.Add(bytes);
			l2.Add(GetBytes("foo"));
			await (s.FlushAsync());
			l.Add(new Foo());
			l.Add(new Bar());
			l2.Add(GetBytes("bar"));
			await (s.FlushAsync());
			object removedObject = l[3];
			l.RemoveAt(3);
			await (s.DeleteAsync(removedObject));
			bytes[1] = Convert.ToByte('o');
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			Assert.AreEqual(3, baz.IdFooBag.Count);
			Assert.AreEqual(3, baz.ByteBag.Count);
			bytes = GetBytes("foobar");
			foreach (object obj in baz.IdFooBag)
			{
				await (s.DeleteAsync(obj));
			}

			baz.IdFooBag = null;
			baz.ByteBag.Add(bytes);
			baz.ByteBag.Add(bytes);
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			Assert.AreEqual(0, baz.IdFooBag.Count);
			Assert.AreEqual(5, baz.ByteBag.Count);
			await (s.DeleteAsync(baz));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task ForceOuterJoinAsync()
		{
			if (sessions.Settings.IsOuterJoinFetchEnabled == false)
			{
				// don't bother to run the test if we can't test it
				return;
			}

			ISession s = OpenSession();
			Glarch g = new Glarch();
			FooComponent fc = new FooComponent();
			fc.Glarch = g;
			FooProxy f = new Foo();
			FooProxy f2 = new Foo();
			f.Component = fc;
			f.TheFoo = f2;
			await (s.SaveAsync(f2));
			object id = await (s.SaveAsync(f));
			object gid = s.GetIdentifier(f.Component.Glarch);
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			f = (FooProxy)await (s.LoadAsync(typeof (Foo), id));
			Assert.IsFalse(NHibernateUtil.IsInitialized(f));
			Assert.IsTrue(NHibernateUtil.IsInitialized(f.Component.Glarch)); //outer-join="true"
			Assert.IsFalse(NHibernateUtil.IsInitialized(f.TheFoo)); //outer-join="auto"
			Assert.AreEqual(gid, s.GetIdentifier(f.Component.Glarch));
			await (s.DeleteAsync(f));
			await (s.DeleteAsync(f.TheFoo));
			await (s.DeleteAsync(f.Component.Glarch));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task EmptyCollectionAsync()
		{
			ISession s = OpenSession();
			object id = await (s.SaveAsync(new Baz()));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			Baz baz = (Baz)await (s.LoadAsync(typeof (Baz), id));
			Assert.IsTrue(baz.FooSet.Count == 0);
			Foo foo = new Foo();
			baz.FooSet.Add(foo);
			await (s.SaveAsync(foo));
			await (s.FlushAsync());
			await (s.DeleteAsync(foo));
			await (s.DeleteAsync(baz));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task OneToOneGeneratorAsync()
		{
			ISession s = OpenSession();
			X x = new X();
			Y y = new Y();
			x.Y = y;
			y.TheX = x;
			object yId = await (s.SaveAsync(y));
			object xId = await (s.SaveAsync(x));
			Assert.AreEqual(yId, xId);
			await (s.FlushAsync());
			Assert.IsTrue(await (s.ContainsAsync(y)) && await (s.ContainsAsync(x)));
			s.Close();
			Assert.AreEqual(x.Id, y.Id);
			s = OpenSession();
			x = new X();
			y = new Y();
			x.Y = y;
			y.TheX = x;
			await (s.SaveAsync(y));
			await (s.FlushAsync());
			Assert.IsTrue(await (s.ContainsAsync(y)) && await (s.ContainsAsync(x)));
			s.Close();
			Assert.AreEqual(x.Id, y.Id);
			s = OpenSession();
			x = new X();
			y = new Y();
			x.Y = y;
			y.TheX = x;
			xId = await (s.SaveAsync(x));
			Assert.AreEqual(xId, y.Id);
			Assert.AreEqual(xId, x.Id);
			await (s.FlushAsync());
			Assert.IsTrue(await (s.ContainsAsync(y)) && await (s.ContainsAsync(x)));
			await (s.DeleteAsync("from X x"));
			await (s.FlushAsync());
			s.Close();
			// check to see if Y can exist without a X
			y = new Y();
			s = OpenSession();
			await (s.SaveAsync(y));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			y = (Y)await (s.LoadAsync(typeof (Y), y.Id));
			Assert.IsNull(y.X, "y does not need an X");
			await (s.DeleteAsync(y));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task LimitAsync()
		{
			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();
			for (int i = 0; i < 10; i++)
			{
				await (s.SaveAsync(new Foo()));
			}

			IEnumerable enumerable = await (s.CreateQuery("from Foo foo").SetMaxResults(4).SetFirstResult(2).EnumerableAsync());
			int count = 0;
			IEnumerator e = enumerable.GetEnumerator();
			while (e.MoveNext())
			{
				object temp = e.Current;
				count++;
			}

			Assert.AreEqual(4, count);
			Assert.AreEqual(10, await (s.DeleteAsync("from Foo foo")));
			await (txn.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CustomAsync()
		{
			GlarchProxy g = new Glarch();
			Multiplicity m = new Multiplicity();
			m.count = 12;
			m.glarch = (Glarch)g;
			g.Multiple = m;
			ISession s = OpenSession();
			object gid = await (s.SaveAsync(g));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			g = (Glarch)(await (s.CreateQuery("from Glarch g where g.Multiple.glarch=g and g.Multiple.count=12").ListAsync()))[0];
			Assert.IsNotNull(g.Multiple);
			Assert.AreEqual(12, g.Multiple.count);
			Assert.AreSame(g, g.Multiple.glarch);
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			g = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), gid));
			Assert.IsNotNull(g.Multiple);
			Assert.AreEqual(12, g.Multiple.count);
			Assert.AreSame(g, g.Multiple.glarch);
			await (s.DeleteAsync(g));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task SaveAddDeleteAsync()
		{
			ISession s = OpenSession();
			Baz baz = new Baz();
			baz.CascadingBars = new HashSet<BarProxy>();
			await (s.SaveAsync(baz));
			await (s.FlushAsync());
			baz.CascadingBars.Add(new Bar());
			await (s.DeleteAsync(baz));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task NamedParamsAsync()
		{
			Bar bar = new Bar();
			Bar bar2 = new Bar();
			bar.Name = "Bar";
			bar2.Name = "Bar Two";
			Baz baz = new Baz();
			baz.CascadingBars = new HashSet<BarProxy>{bar};
			bar.Baz = baz;
			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();
			await (s.SaveAsync(baz));
			await (s.SaveAsync(bar2));
			IList list = await (s.CreateQuery("from Bar bar left join bar.Baz baz left join baz.CascadingBars b where bar.Name like 'Bar %'").ListAsync());
			object row = list[0];
			Assert.IsTrue(row is object[] && ((object[])row).Length == 3);
			IQuery q = s.CreateQuery("select bar, b " + "from Bar bar " + "left join bar.Baz baz left join baz.CascadingBars b " + "where (bar.Name in (:nameList) or bar.Name in (:nameList)) and bar.String = :stringVal");
			var nameList = new List<string>{"bar", "Bar", "Bar Two"};
			q.SetParameterList("nameList", nameList);
			q.SetParameter("stringVal", "a string");
			list = await (q.ListAsync());
			// a check for SAPDialect here
			Assert.AreEqual(2, list.Count);
			q = s.CreateQuery("select bar, b " + "from Bar bar " + "inner join bar.Baz baz inner join baz.CascadingBars b " + "where bar.Name like 'Bar%'");
			list = await (q.ListAsync());
			Assert.AreEqual(1, list.Count);
			q = s.CreateQuery("select bar, b " + "from Bar bar " + "left join bar.Baz baz left join baz.CascadingBars b " + "where bar.Name like :name and b.Name like :name");
			// add a check for HSQLDialect
			q.SetString("name", "Bar%");
			list = await (q.ListAsync());
			Assert.AreEqual(1, list.Count);
			await (s.DeleteAsync(baz));
			await (s.DeleteAsync(bar2));
			await (txn.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task VerifyParameterNamedMissingAsync()
		{
			using (ISession s = OpenSession())
			{
				IQuery q = s.CreateQuery("select bar from Bar as bar where bar.X > :myX");
				Assert.ThrowsAsync<QueryException>(async () => await (q.ListAsync()));
			}
		}

		[Test]
		public async Task VerifyParameterPositionalMissingAsync()
		{
			using (ISession s = OpenSession())
			{
				IQuery q = s.CreateQuery("select bar from Bar as bar where bar.X > ?");
				Assert.ThrowsAsync<QueryException>(async () => await (q.ListAsync()));
			}
		}

		[Test]
		public async Task VerifyParameterPositionalInQuotesAsync()
		{
			using (ISession s = OpenSession())
			{
				IQuery q = s.CreateQuery("select bar from Bar as bar where bar.X > ? or bar.Short = 1 or bar.String = 'ff ? bb'");
				q.SetInt32(0, 1);
				await (q.ListAsync());
			}
		}

		[Test]
		public async Task VerifyParameterPositionalInQuotes2Async()
		{
			using (ISession s = OpenSession())
			{
				IQuery q = s.CreateQuery("select bar from Bar as bar where bar.String = ' ? ' or bar.String = '?'");
				await (q.ListAsync());
			}
		}

		[Test]
		public async Task VerifyParameterPositionalMissing2Async()
		{
			using (ISession s = OpenSession())
			{
				IQuery q = s.CreateQuery("select bar from Bar as bar where bar.String = ? or bar.String = ? or bar.String = ?");
				q.SetParameter(0, "bull");
				q.SetParameter(2, "shit");
				Assert.ThrowsAsync<QueryException>(async () => await (q.ListAsync()));
			}
		}

		[Test]
		public async Task DynaAsync()
		{
			ISession s = OpenSession();
			GlarchProxy g = new Glarch();
			g.Name = "G";
			object id = await (s.SaveAsync(g));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			g = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), id));
			Assert.AreEqual("G", g.Name);
			Assert.AreEqual("foo", g.DynaBean["foo"]);
			Assert.AreEqual(66, g.DynaBean["bar"]);
			Assert.IsFalse(g is Glarch);
			g.DynaBean["foo"] = "bar";
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			g = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), id));
			Assert.AreEqual("bar", g.DynaBean["foo"]);
			Assert.AreEqual(66, g.DynaBean["bar"]);
			g.DynaBean = null;
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			g = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), id));
			Assert.IsNull(g.DynaBean);
			await (s.DeleteAsync(g));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task FindByCriteriaAsync()
		{
			ISession s = OpenSession();
			Foo f = new Foo();
			await (s.SaveAsync(f));
			await (s.FlushAsync());
			IList list = await (s.CreateCriteria(typeof (Foo)).Add(Expression.Eq("Integer", f.Integer)).Add(Expression.EqProperty("Integer", "Integer")).Add(Expression.Like("String", f.String)).Add(Expression.In("Boolean", new bool[]{f.Boolean, f.Boolean})).SetFetchMode("TheFoo", FetchMode.Eager).SetFetchMode("Baz", FetchMode.Lazy).ListAsync());
			Assert.IsTrue(list.Count == 1 && list[0] == f);
			list = await (s.CreateCriteria(typeof (Foo)).Add(Expression.Disjunction().Add(Expression.Eq("Integer", f.Integer)).Add(Expression.Like("String", f.String)).Add(Expression.Eq("Boolean", f.Boolean))).Add(Expression.IsNotNull("Boolean")).ListAsync());
			Assert.IsTrue(list.Count == 1 && list[0] == f);
			ICriterion andExpression;
			ICriterion orExpression;
			andExpression = Expression.And(Expression.Eq("Integer", f.Integer), Expression.Like("String", f.String));
			orExpression = Expression.Or(andExpression, Expression.Eq("Boolean", f.Boolean));
			list = await (s.CreateCriteria(typeof (Foo)).Add(orExpression).ListAsync());
			Assert.IsTrue(list.Count == 1 && list[0] == f);
			list = await (s.CreateCriteria(typeof (Foo)).SetMaxResults(5).AddOrder(Order.Asc("Date")).ListAsync());
			Assert.IsTrue(list.Count == 1 && list[0] == f);
			list = await (s.CreateCriteria(typeof (Foo)).SetMaxResults(0).ListAsync());
			Assert.AreEqual(0, list.Count);
			list = await (s.CreateCriteria(typeof (Foo)).SetFirstResult(1).AddOrder(Order.Asc("Date")).AddOrder(Order.Desc("String")).ListAsync());
			Assert.AreEqual(0, list.Count);
			f.TheFoo = new Foo();
			await (s.SaveAsync(f.TheFoo));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			list = await (s.CreateCriteria(typeof (Foo)).Add(Expression.Eq("Integer", f.Integer)).Add(Expression.Like("String", f.String)).Add(Expression.In("Boolean", new bool[]{f.Boolean, f.Boolean})).Add(Expression.IsNotNull("TheFoo")).SetFetchMode("TheFoo", FetchMode.Eager).SetFetchMode("Baz", FetchMode.Lazy).SetFetchMode("Component.Glarch", FetchMode.Lazy).SetFetchMode("TheFoo.Baz", FetchMode.Lazy).SetFetchMode("TheFoo.Component.Glarch", FetchMode.Lazy).ListAsync());
			f = (Foo)list[0];
			Assert.IsTrue(NHibernateUtil.IsInitialized(f.TheFoo));
			Assert.IsFalse(NHibernateUtil.IsInitialized(f.Component.Glarch));
			await (s.DeleteAsync(f.TheFoo));
			await (s.DeleteAsync(f));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task AfterDeleteAsync()
		{
			ISession s = OpenSession();
			Foo foo = new Foo();
			await (s.SaveAsync(foo));
			await (s.FlushAsync());
			await (s.DeleteAsync(foo));
			await (s.SaveAsync(foo));
			await (s.DeleteAsync(foo));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task CollectionWhereAsync()
		{
			Foo foo1 = new Foo();
			Foo foo2 = new Foo();
			Baz baz = new Baz();
			Foo[] arr = new Foo[10];
			arr[0] = foo1;
			arr[9] = foo2;
			ISession s = OpenSession();
			await (s.SaveAsync(foo1));
			await (s.SaveAsync(foo2));
			baz.FooArray = arr;
			await (s.SaveAsync(baz));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			Assert.AreEqual(1, baz.FooArray.Length);
			Assert.AreEqual(1, (await (s.CreateQuery("from Baz baz join baz.FooArray foo").ListAsync())).Count);
			Assert.AreEqual(2, (await (s.CreateQuery("from Foo foo").ListAsync())).Count);
			Assert.AreEqual(1, (await ((await (s.CreateFilterAsync(baz.FooArray, ""))).ListAsync())).Count);
			await (s.DeleteAsync("from Foo foo"));
			await (s.DeleteAsync(baz));
			DbCommand deleteCmd = s.Connection.CreateCommand();
			deleteCmd.CommandText = "delete from FooArray where id_='" + baz.Code + "' and i>=8";
			deleteCmd.CommandType = CommandType.Text;
			int rows = await (deleteCmd.ExecuteNonQueryAsync());
			Assert.AreEqual(1, rows);
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task ComponentParentAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			BarProxy bar = new Bar();
			bar.Component = new FooComponent();
			Baz baz = new Baz();
			baz.Components = new FooComponent[]{new FooComponent(), new FooComponent()};
			await (s.SaveAsync(bar));
			await (s.SaveAsync(baz));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			bar = (BarProxy)await (s.LoadAsync(typeof (Bar), bar.Key));
			await (s.LoadAsync(baz, baz.Code));
			Assert.AreEqual(bar, bar.BarComponent.Parent);
			Assert.IsTrue(baz.Components[0].Baz == baz && baz.Components[1].Baz == baz);
			await (s.DeleteAsync(baz));
			await (s.DeleteAsync(bar));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CollectionCacheAsync()
		{
			ISession s = OpenSession();
			Baz baz = new Baz();
			baz.SetDefaults();
			await (s.SaveAsync(baz));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			await (s.LoadAsync(typeof (Baz), baz.Code));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			await (s.DeleteAsync(baz));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		//[Ignore("TimeZone Portions commented out - http://nhibernate.jira.com/browse/NH-88")]
		public async Task AssociationIdAsync()
		{
			string id;
			Bar bar;
			MoreStuff more;
			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					bar = new Bar();
					id = (string)await (s.SaveAsync(bar));
					more = new MoreStuff();
					more.Name = "More Stuff";
					more.IntId = 12;
					more.StringId = "id";
					Stuff stuf = new Stuff();
					stuf.MoreStuff = more;
					more.Stuffs = new List<Stuff>{stuf};
					stuf.Foo = bar;
					stuf.Id = 1234;
					await (s.SaveAsync(more));
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					//The special property (lowercase) id may be used to reference the unique identifier of an object. (You may also use its property name.) 
					string hqlString = "from s in class Stuff where s.Foo.id = ? and s.id.Id = ? and s.MoreStuff.id.IntId = ? and s.MoreStuff.id.StringId = ?";
					object[] values = new object[]{bar, (long)1234, 12, "id"};
					IType[] types = new IType[]{NHibernateUtil.Entity(typeof (Foo)), NHibernateUtil.Int64, NHibernateUtil.Int32, NHibernateUtil.String};
					//IList results = s.List( hqlString, values, types );
					IQuery q = s.CreateQuery(hqlString);
					for (int i = 0; i < values.Length; i++)
					{
						q.SetParameter(i, values[i], types[i]);
					}

					IList results = await (q.ListAsync());
					Assert.AreEqual(1, results.Count);
					hqlString = "from s in class Stuff where s.Foo.id = ? and s.id.Id = ? and s.MoreStuff.Name = ?";
					values = new object[]{bar, (long)1234, "More Stuff"};
					types = new IType[]{NHibernateUtil.Entity(typeof (Foo)), NHibernateUtil.Int64, NHibernateUtil.String};
					q = s.CreateQuery(hqlString);
					for (int i = 0; i < values.Length; i++)
					{
						q.SetParameter(i, values[i], types[i]);
					}

					results = await (q.ListAsync());
					Assert.AreEqual(1, results.Count);
					hqlString = "from s in class Stuff where s.Foo.String is not null";
					await (s.CreateQuery(hqlString).ListAsync());
					hqlString = "from s in class Stuff where s.Foo > '0' order by s.Foo";
					results = await (s.CreateQuery(hqlString).ListAsync());
					Assert.AreEqual(1, results.Count);
					await (t.CommitAsync());
				}
			}

			FooProxy foo;
			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					foo = (FooProxy)await (s.LoadAsync(typeof (Foo), id));
					await (s.LoadAsync(more, more));
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					Stuff stuff = new Stuff();
					stuff.Foo = foo;
					stuff.Id = 1234;
					stuff.MoreStuff = more;
					await (s.LoadAsync(stuff, stuff));
					Assert.AreEqual("More Stuff", stuff.MoreStuff.Name);
					await (s.DeleteAsync("from ms in class MoreStuff"));
					await (s.DeleteAsync("from foo in class Foo"));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task CascadeSaveAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Baz baz = new Baz();
			IList<Fee> list = new List<Fee>();
			list.Add(new Fee());
			list.Add(new Fee());
			baz.Fees = list;
			await (s.SaveAsync(baz));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			Assert.AreEqual(2, baz.Fees.Count);
			await (s.DeleteAsync(baz));
			Assert.IsTrue(IsEmpty(await (s.CreateQuery("from fee in class Fee").EnumerableAsync())));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CompositeKeyPathExpressionsAsync()
		{
			ISession s = OpenSession();
			string hql = "select fum1.Fo from fum1 in class Fum where fum1.Fo.FumString is not null";
			await (s.CreateQuery(hql).ListAsync());
			hql = "from fum1 in class Fum where fum1.Fo.FumString is not null order by fum1.Fo.FumString";
			await (s.CreateQuery(hql).ListAsync());
			if (Dialect.SupportsSubSelects)
			{
				hql = "from fum1 in class Fum where size(fum1.Friends) = 0";
				await (s.CreateQuery(hql).ListAsync());
				hql = "from fum1 in class Fum where exists elements (fum1.Friends)";
				await (s.CreateQuery(hql).ListAsync());
			}

			hql = "select elements(fum1.Friends) from fum1 in class Fum";
			await (s.CreateQuery(hql).ListAsync());
			hql = "from fum1 in class Fum, fr in elements( fum1.Friends )";
			await (s.CreateQuery(hql).ListAsync());
			s.Close();
		}

		[Test]
		public async Task CollectionsInSelectAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Foo[] foos = new Foo[]{null, new Foo()};
			await (s.SaveAsync(foos[1]));
			Baz baz = new Baz();
			baz.SetDefaults();
			baz.FooArray = foos;
			await (s.SaveAsync(baz));
			Baz baz2 = new Baz();
			baz2.SetDefaults();
			await (s.SaveAsync(baz2));
			Bar bar = new Bar();
			bar.Baz = baz;
			await (s.SaveAsync(bar));
			IList list = await (s.CreateQuery("select new Result(foo.String, foo.Long, foo.Integer) from foo in class Foo").ListAsync());
			Assert.AreEqual(2, list.Count);
			Assert.IsTrue(list[0] is Result);
			Assert.IsTrue(list[1] is Result);
			list = await (s.CreateQuery("select new Result( baz.Name, foo.Long, count(elements(baz.FooArray)) ) from Baz baz join baz.FooArray foo group by baz.Name, foo.Long").ListAsync());
			Assert.AreEqual(1, list.Count);
			Assert.IsTrue(list[0] is Result);
			Result r = (Result)list[0];
			Assert.AreEqual(baz.Name, r.Name);
			Assert.AreEqual(1, r.Count);
			Assert.AreEqual(foos[1].Long, r.Amount);
			list = await (s.CreateQuery("select new Result( baz.Name, max(foo.Long), count(foo) ) from Baz baz join baz.FooArray foo group by baz.Name").ListAsync());
			Assert.AreEqual(1, list.Count);
			Assert.IsTrue(list[0] is Result);
			r = (Result)list[0];
			Assert.AreEqual(baz.Name, r.Name);
			Assert.AreEqual(1, r.Count);
			await (s.CreateQuery("select max( elements(bar.Baz.FooArray) ) from Bar as bar").ListAsync());
			// the following test is disable for databases with no subselects... also for Interbase (not sure why) - comment from h2.0.3
			if (Dialect.SupportsSubSelects)
			{
				await (s.CreateQuery("select count(*) from Baz as baz where 1 in indices(baz.FooArray)").ListAsync());
				await (s.CreateQuery("select count(*) from Bar as bar where 'abc' in elements(bar.Baz.FooArray)").ListAsync());
				await (s.CreateQuery("select count(*) from Bar as bar where 1 in indices(bar.Baz.FooArray)").ListAsync());
				await (s.CreateQuery("select count(*) from Bar as bar where '1' in (from bar.Component.Glarch.ProxyArray g where g.Name='foo')").ListAsync());
				// The nex query is wrong and is not present in H3.2:
				// The SQL result, from Classic parser, is the same of the previous query.
				// The AST parser has some problem to parse 'from g in bar.Component.Glarch.ProxyArray'
				// which should be parsed as 'from bar.Component.Glarch.ProxyArray g'
				//s.CreateQuery(
				//  "select count(*) from Bar as bar where '1' in (from g in bar.Component.Glarch.ProxyArray.elements where g.Name='foo')")
				//  .List();
				// TODO: figure out why this is throwing an ORA-1722 error
				// probably the conversion ProxyArray.id (to_number ensuring a not null value)
				if (!(Dialect is Oracle8iDialect))
				{
					await (s.CreateQuery("select count(*) from Bar as bar join bar.Component.Glarch.ProxyArray as g where cast(g.id as Int32) in indices(bar.Baz.FooArray)").ListAsync());
					await (s.CreateQuery("select max( elements(bar.Baz.FooArray) ) from Bar as bar join bar.Component.Glarch.ProxyArray as g where cast(g.id as Int32) in indices(bar.Baz.FooArray)").ListAsync());
					await (s.CreateQuery("select count(*) from Bar as bar left outer join bar.Component.Glarch.ProxyArray as pg where '1' in (from g in bar.Component.Glarch.ProxyArray)").ListAsync());
				}
			}

			list = await (s.CreateQuery("from Baz baz left join baz.FooToGlarch join fetch baz.FooArray foo left join fetch foo.TheFoo").ListAsync());
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual(2, ((object[])list[0]).Length);
			list = await (s.CreateQuery("select baz.Name from Bar bar inner join bar.Baz baz inner join baz.FooSet foo where baz.Name = bar.String").ListAsync());
			await (s.CreateQuery("SELECT baz.Name FROM Bar AS bar INNER JOIN bar.Baz AS baz INNER JOIN baz.FooSet AS foo WHERE baz.Name = bar.String").ListAsync());
			await (s.CreateQuery("select baz.Name from Bar bar join bar.Baz baz left outer join baz.FooSet foo where baz.Name = bar.String").ListAsync());
			await (s.CreateQuery("select baz.Name from Bar bar join bar.Baz baz join baz.FooSet foo where baz.Name = bar.String").ListAsync());
			await (s.CreateQuery("SELECT baz.Name FROM Bar AS bar join bar.Baz AS baz join baz.FooSet AS foo WHERE baz.Name = bar.String").ListAsync());
			await (s.CreateQuery("select baz.Name from Bar bar left join bar.Baz baz left join baz.FooSet foo where baz.Name = bar.String").ListAsync());
			await (s.CreateQuery("select foo.String from Bar bar left join bar.Baz.FooSet foo where bar.String = foo.String").ListAsync());
			await (s.CreateQuery("select baz.Name from Bar bar left join bar.Baz baz left join baz.FooArray foo where baz.Name = bar.String").ListAsync());
			await (s.CreateQuery("select foo.String from Bar bar left join bar.Baz.FooArray foo where bar.String = foo.String").ListAsync());
			await (s.CreateQuery("select bar.String, foo.String from bar in class Bar inner join bar.Baz as baz inner join elements(baz.FooSet) as foo where baz.Name = 'name'").ListAsync());
			await (s.CreateQuery("select foo from bar in class Bar inner join bar.Baz as baz inner join baz.FooSet as foo").ListAsync());
			await (s.CreateQuery("select foo from bar in class Bar inner join bar.Baz.FooSet as foo").ListAsync());
			await (s.CreateQuery("select bar.String, foo.String from bar in class Bar join bar.Baz as baz, elements(baz.FooSet) as foo where baz.Name = 'name'").ListAsync());
			await (s.CreateQuery("select foo from bar in class Bar join bar.Baz as baz join baz.FooSet as foo").ListAsync());
			await (s.CreateQuery("select foo from bar in class Bar join bar.Baz.FooSet as foo").ListAsync());
			Assert.AreEqual(1, (await (s.CreateQuery("from Bar bar join bar.Baz.FooArray foo").ListAsync())).Count);
			Assert.AreEqual(0, (await (s.CreateQuery("from bar in class Bar, foo in elements(bar.Baz.FooSet)").ListAsync())).Count);
			Assert.AreEqual(1, (await (s.CreateQuery("from bar in class Bar, foo in elements( bar.Baz.FooArray )").ListAsync())).Count);
			await (s.DeleteAsync(bar));
			await (s.DeleteAsync(baz));
			await (s.DeleteAsync(baz2));
			await (s.DeleteAsync(foos[1]));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task NewFlushingAsync()
		{
			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();
			Baz baz = new Baz();
			baz.SetDefaults();
			await (s.SaveAsync(baz));
			await (s.FlushAsync());
			baz.StringArray[0] = "a new value";
			IEnumerator enumer = (await (s.CreateQuery("from baz in class Baz").EnumerableAsync())).GetEnumerator(); // no flush
			Assert.IsTrue(enumer.MoveNext());
			Assert.AreSame(baz, enumer.Current);
			enumer = (await (s.CreateQuery("select elements(baz.StringArray) from baz in class Baz").EnumerableAsync())).GetEnumerator();
			bool found = false;
			while (enumer.MoveNext())
			{
				if (enumer.Current.Equals("a new value"))
				{
					found = true;
				}
			}

			Assert.IsTrue(found);
			baz.StringArray = null;
			await (s.CreateQuery("from baz in class Baz").EnumerableAsync()); // no flush
			enumer = (await (s.CreateQuery("select elements(baz.StringArray) from baz in class Baz").EnumerableAsync())).GetEnumerator();
			Assert.IsFalse(enumer.MoveNext());
			baz.StringList.Add("1E1");
			enumer = (await (s.CreateQuery("from foo in class Foo").EnumerableAsync())).GetEnumerator(); // no flush
			Assert.IsFalse(enumer.MoveNext());
			enumer = (await (s.CreateQuery("select elements(baz.StringList) from baz in class Baz").EnumerableAsync())).GetEnumerator();
			found = false;
			while (enumer.MoveNext())
			{
				if (enumer.Current.Equals("1E1"))
				{
					found = true;
				}
			}

			Assert.IsTrue(found);
			baz.StringList.Remove("1E1");
			await (s.CreateQuery("select elements(baz.StringArray) from baz in class Baz").EnumerableAsync()); //no flush
			enumer = (await (s.CreateQuery("select elements(baz.StringList) from baz in class Baz").EnumerableAsync())).GetEnumerator();
			found = false;
			while (enumer.MoveNext())
			{
				if (enumer.Current.Equals("1E1"))
				{
					found = true;
				}
			}

			Assert.IsFalse(found);
			IList<string> newList = new List<string>();
			newList.Add("value");
			baz.StringList = newList;
			(await (s.CreateQuery("from foo in class Foo").EnumerableAsync())).GetEnumerator(); //no flush
			baz.StringList = null;
			enumer = (await (s.CreateQuery("select elements(baz.StringList) from baz in class Baz").EnumerableAsync())).GetEnumerator();
			Assert.IsFalse(enumer.MoveNext());
			await (s.DeleteAsync(baz));
			await (txn.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task PersistCollectionsAsync()
		{
			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();
			IEnumerator enumer = (await (s.CreateQuery("select count(*) from b in class Bar").EnumerableAsync())).GetEnumerator();
			enumer.MoveNext();
			Assert.AreEqual(0L, enumer.Current);
			Baz baz = new Baz();
			await (s.SaveAsync(baz));
			baz.SetDefaults();
			baz.StringArray = new string[]{"stuff"};
			baz.CascadingBars = new HashSet<BarProxy>{new Bar()};
			IDictionary<string, Glarch> sgm = new Dictionary<string, Glarch>();
			sgm["a"] = new Glarch();
			sgm["b"] = new Glarch();
			baz.StringGlarchMap = sgm;
			await (txn.CommitAsync());
			s.Close();
			s = OpenSession();
			txn = s.BeginTransaction();
			baz = (Baz)((object[])(await (s.CreateQuery("select baz, baz from baz in class NHibernate.DomainModel.Baz").ListAsync()))[0])[1];
			Assert.AreEqual(1, baz.CascadingBars.Count, "baz.CascadingBars.Count");
			Foo foo = new Foo();
			await (s.SaveAsync(foo));
			Foo foo2 = new Foo();
			await (s.SaveAsync(foo2));
			baz.FooArray = new Foo[]{foo, foo, null, foo2};
			baz.FooSet.Add(foo);
			baz.Customs.Add(new string[]{"new", "custom"});
			baz.StringArray = null;
			baz.StringList[0] = "new value";
			baz.StringSet = new HashSet<string>();
			// NOTE: We put two items in the map, but expect only one to come back, because
			// of where="..." specified in the mapping for StringGlarchMap
			Assert.AreEqual(1, baz.StringGlarchMap.Count, "baz.StringGlarchMap.Count");
			IList list;
			// disable this for dbs with no subselects
			if (Dialect.SupportsSubSelects && TestDialect.SupportsOperatorAll)
			{
				list = await (s.CreateQuery("select foo from foo in class NHibernate.DomainModel.Foo, baz in class NHibernate.DomainModel.Baz where foo in elements(baz.FooArray) and 3 = some elements(baz.IntArray) and 4 > all indices(baz.IntArray)").ListAsync());
				Assert.AreEqual(2, list.Count, "collection.elements find");
			}

			// sapdb doesn't like distinct with binary type
			//if( !(dialect is Dialect.SAPDBDialect) ) 
			//{
			list = await (s.CreateQuery("select distinct foo from baz in class NHibernate.DomainModel.Baz, foo in elements(baz.FooArray)").ListAsync());
			Assert.AreEqual(2, list.Count, "collection.elements find");
			//}
			list = await (s.CreateQuery("select foo from baz in class NHibernate.DomainModel.Baz, foo in elements(baz.FooSet)").ListAsync());
			Assert.AreEqual(1, list.Count, "association.elements find");
			await (txn.CommitAsync());
			s.Close();
			s = OpenSession();
			txn = s.BeginTransaction();
			baz = (Baz)(await (s.CreateQuery("select baz from baz in class NHibernate.DomainModel.Baz order by baz").ListAsync()))[0];
			Assert.AreEqual(4, baz.Customs.Count, "collection of custom types - added element");
			Assert.IsNotNull(baz.Customs[0], "collection of custom types - added element");
			Assert.IsNotNull(baz.Components[1].Subcomponent, "component of component in collection");
			Assert.AreSame(baz, baz.Components[1].Baz);
			IEnumerator fooSetEnumer = baz.FooSet.GetEnumerator();
			fooSetEnumer.MoveNext();
			Assert.IsTrue(((FooProxy)fooSetEnumer.Current).Key.Equals(foo.Key), "set of objects");
			Assert.AreEqual(0, baz.StringArray.Length, "collection removed");
			Assert.AreEqual("new value", baz.StringList[0], "changed element");
			Assert.AreEqual(0, baz.StringSet.Count, "replaced set");
			baz.StringSet.Add("two");
			baz.StringSet.Add("one");
			baz.Bag.Add("three");
			await (txn.CommitAsync());
			s.Close();
			s = OpenSession();
			txn = s.BeginTransaction();
			baz = (Baz)(await (s.CreateQuery("select baz from baz in class NHibernate.DomainModel.Baz order by baz").ListAsync()))[0];
			Assert.AreEqual(2, baz.StringSet.Count);
			int index = 0;
			foreach (string key in baz.StringSet)
			{
				// h2.0.3 doesn't have this because the Set has a first() and last() method
				index++;
				if (index == 1)
				{
					Assert.AreEqual("one", key);
				}

				if (index == 2)
				{
					Assert.AreEqual("two", key);
				}

				if (index > 2)
				{
					Assert.Fail("should not be more than 2 items in StringSet");
				}
			}

			Assert.AreEqual(5, baz.Bag.Count);
			baz.StringSet.Remove("two");
			baz.Bag.Remove("duplicate");
			await (txn.CommitAsync());
			s.Close();
			s = OpenSession();
			txn = s.BeginTransaction();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			Bar bar = new Bar();
			Bar bar2 = new Bar();
			await (s.SaveAsync(bar));
			await (s.SaveAsync(bar2));
			baz.TopFoos = new HashSet<Bar>{bar, bar2};
			baz.TopGlarchez = new Dictionary<char, GlarchProxy>();
			GlarchProxy g = new Glarch();
			await (s.SaveAsync(g));
			baz.TopGlarchez['G'] = g;
			var map = new Dictionary<Foo, GlarchProxy>();
			map[bar] = g;
			map[bar2] = g;
			baz.FooToGlarch = map;
			var map2 = new Dictionary<FooComponent, Foo>();
			map2[new FooComponent("name", 123, null, null)] = bar;
			map2[new FooComponent("nameName", 12, null, null)] = bar;
			baz.FooComponentToFoo = map2;
			var map3 = new Dictionary<Foo, GlarchProxy>();
			map3[bar] = g;
			baz.GlarchToFoo = map3;
			await (txn.CommitAsync());
			s.Close();
			using (s = OpenSession())
				using (txn = s.BeginTransaction())
				{
					baz = (Baz)(await (s.CreateQuery("select baz from baz in class NHibernate.DomainModel.Baz order by baz").ListAsync()))[0];
					ISession s2 = OpenSession();
					ITransaction txn2 = s2.BeginTransaction();
					baz = (Baz)(await (s.CreateQuery("select baz from baz in class NHibernate.DomainModel.Baz order by baz").ListAsync()))[0];
					object o = baz.FooComponentToFoo[new FooComponent("name", 123, null, null)];
					Assert.IsNotNull(o);
					Assert.AreEqual(o, baz.FooComponentToFoo[new FooComponent("nameName", 12, null, null)]);
					await (txn2.CommitAsync());
					s2.Close();
					Assert.AreEqual(2, baz.TopFoos.Count);
					Assert.AreEqual(1, baz.TopGlarchez.Count);
					enumer = baz.TopFoos.GetEnumerator();
					Assert.IsTrue(enumer.MoveNext());
					Assert.IsNotNull(enumer.Current);
					Assert.AreEqual(1, baz.StringSet.Count);
					Assert.AreEqual(4, baz.Bag.Count);
					Assert.AreEqual(2, baz.FooToGlarch.Count);
					Assert.AreEqual(2, baz.FooComponentToFoo.Count);
					Assert.AreEqual(1, baz.GlarchToFoo.Count);
					enumer = baz.FooToGlarch.Keys.GetEnumerator();
					for (int i = 0; i < 2; i++)
					{
						enumer.MoveNext();
						Assert.IsTrue(enumer.Current is BarProxy);
					}

					enumer = baz.FooComponentToFoo.Keys.GetEnumerator();
					enumer.MoveNext();
					FooComponent fooComp = (FooComponent)enumer.Current;
					Assert.IsTrue((fooComp.Count == 123 && fooComp.Name.Equals("name")) || (fooComp.Count == 12 && fooComp.Name.Equals("nameName")));
					Assert.IsTrue(baz.FooComponentToFoo[fooComp] is BarProxy);
					Glarch g2 = new Glarch();
					await (s.SaveAsync(g2));
					g = (GlarchProxy)baz.TopGlarchez['G'];
					baz.TopGlarchez['H'] = g;
					baz.TopGlarchez['G'] = g2;
					await (txn.CommitAsync());
					s.Close();
				}

			s = OpenSession();
			txn = s.BeginTransaction();
			baz = (Baz)(await (s.CreateQuery("select baz from baz in class NHibernate.DomainModel.Baz order by baz").ListAsync()))[0];
			Assert.AreEqual(2, baz.TopGlarchez.Count);
			await (txn.CommitAsync());
			s.Disconnect();
			// serialize and then deserialize the session.
			Stream stream = new MemoryStream();
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, s);
			s.Close();
			stream.Position = 0;
			s = (ISession)formatter.Deserialize(stream);
			stream.Close();
			s.Reconnect();
			txn = s.BeginTransaction();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			await (s.DeleteAsync(baz));
			await (s.DeleteAsync(baz.TopGlarchez['G']));
			await (s.DeleteAsync(baz.TopGlarchez['H']));
			DbCommand cmd = s.Connection.CreateCommand();
			s.Transaction.Enlist(cmd);
			cmd.CommandText = "update " + Dialect.QuoteForTableName("glarchez") + " set baz_map_id=null where baz_map_index='a'";
			int rows = await (cmd.ExecuteNonQueryAsync());
			Assert.AreEqual(1, rows);
			Assert.AreEqual(2, await (s.DeleteAsync("from bar in class NHibernate.DomainModel.Bar")));
			FooProxy[] arr = baz.FooArray;
			Assert.AreEqual(4, arr.Length);
			Assert.AreEqual(foo.Key, arr[1].Key);
			for (int i = 0; i < arr.Length; i++)
			{
				if (arr[i] != null)
				{
					await (s.DeleteAsync(arr[i]));
				}
			}

			try
			{
				await (s.LoadAsync(typeof (Qux), (long)666)); //nonexistent
			}
			catch (ObjectNotFoundException onfe)
			{
				Assert.IsNotNull(onfe, "should not find a Qux with id of 666 when Proxies are not implemented.");
			}

			Assert.AreEqual(1, await (s.DeleteAsync("from g in class Glarch")), "Delete('from g in class Glarch')");
			await (txn.CommitAsync());
			s.Disconnect();
			// serialize and then deserialize the session.
			stream = new MemoryStream();
			formatter.Serialize(stream, s);
			s.Close();
			stream.Position = 0;
			s = (ISession)formatter.Deserialize(stream);
			stream.Close();
			Qux nonexistentQux = (Qux)await (s.LoadAsync(typeof (Qux), (long)666)); //nonexistent
			Assert.IsNotNull(nonexistentQux, "even though it doesn't exists should still get a proxy - no db hit.");
			s.Close();
		}

		[Test]
		public async Task SaveFlushAsync()
		{
			ISession s = OpenSession();
			Fee fee = new Fee();
			await (s.SaveAsync(fee, "key"));
			fee.Fi = "blah";
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			fee = (Fee)await (s.LoadAsync(typeof (Fee), fee.Key));
			Assert.AreEqual("blah", fee.Fi);
			Assert.AreEqual("key", fee.Key);
			await (s.DeleteAsync(fee));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task CreateUpdateAsync()
		{
			ISession s = OpenSession();
			Foo foo = new Foo();
			await (s.SaveAsync(foo));
			foo.String = "dirty";
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			Foo foo2 = new Foo();
			await (s.LoadAsync(foo2, foo.Key));
			Assert.IsTrue(foo.EqualsFoo(foo2), "create-update");
			await (s.DeleteAsync(foo2));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			foo = new Foo();
			await (s.SaveAsync(foo, "assignedid"));
			foo.String = "dirty";
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			await (s.LoadAsync(foo2, "assignedid"));
			Assert.IsTrue(foo.EqualsFoo(foo2), "create-update");
			await (s.DeleteAsync(foo2));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task UpdateCollectionsAsync()
		{
			ISession s = OpenSession();
			Holder baz = new Holder();
			baz.Name = "123";
			Foo f1 = new Foo();
			Foo f2 = new Foo();
			Foo f3 = new Foo();
			One o = new One();
			baz.Ones = new List<One>();
			baz.Ones.Add(o);
			Foo[] foos = new Foo[]{f1, null, f2};
			baz.FooArray = foos;
			// in h2.0.3 this is a Set
			baz.Foos = new HashSet<Foo>{f1};
			await (s.SaveAsync(f1));
			await (s.SaveAsync(f2));
			await (s.SaveAsync(f3));
			await (s.SaveAsync(o));
			await (s.SaveAsync(baz));
			await (s.FlushAsync());
			s.Close();
			baz.Ones[0] = null;
			baz.Ones.Add(o);
			baz.Foos.Add(f2);
			foos[0] = f3;
			foos[1] = f1;
			s = OpenSession();
			await (s.SaveOrUpdateAsync(baz));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			Holder h = (Holder)await (s.LoadAsync(typeof (Holder), baz.Id));
			Assert.IsNull(h.Ones[0]);
			Assert.IsNotNull(h.Ones[1]);
			Assert.IsNotNull(h.FooArray[0]);
			Assert.IsNotNull(h.FooArray[1]);
			Assert.IsNotNull(h.FooArray[2]);
			Assert.AreEqual(2, h.Foos.Count);
			// new to nh to make sure right items in right index
			Assert.AreEqual(f3.Key, h.FooArray[0].Key);
			Assert.AreEqual(o.Key, ((One)h.Ones[1]).Key);
			Assert.AreEqual(f1.Key, h.FooArray[1].Key);
			Assert.AreEqual(f2.Key, h.FooArray[2].Key);
			s.Close();
			baz.Foos.Remove(f1);
			baz.Foos.Remove(f2);
			baz.FooArray[0] = null;
			baz.FooArray[1] = null;
			baz.FooArray[2] = null;
			s = OpenSession();
			await (s.SaveOrUpdateAsync(baz));
			await (s.DeleteAsync("from f in class NHibernate.DomainModel.Foo"));
			baz.Ones.Remove(o);
			await (s.DeleteAsync("from o in class NHibernate.DomainModel.One"));
			await (s.DeleteAsync(baz));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task LoadAsync()
		{
			ISession s = OpenSession();
			Qux q = new Qux();
			await (s.SaveAsync(q));
			BarProxy b = new Bar();
			await (s.SaveAsync(b));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			q = (Qux)await (s.LoadAsync(typeof (Qux), q.Key));
			b = (BarProxy)await (s.LoadAsync(typeof (Foo), b.Key));
			string tempKey = b.Key;
			Assert.IsFalse(NHibernateUtil.IsInitialized(b), "b should have been an unitialized Proxy");
			string tempString = b.BarString;
			Assert.IsTrue(NHibernateUtil.IsInitialized(b), "b should have been an initialized Proxy");
			BarProxy b2 = (BarProxy)await (s.LoadAsync(typeof (Bar), tempKey));
			Qux q2 = (Qux)await (s.LoadAsync(typeof (Qux), q.Key));
			Assert.AreSame(q, q2, "loaded same Qux");
			Assert.AreSame(b, b2, "loaded same BarProxy");
			await (s.DeleteAsync(q2));
			await (s.DeleteAsync(b2));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task CreateAsync()
		{
			ISession s = OpenSession();
			Foo foo = new Foo();
			await (s.SaveAsync(foo));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			Foo foo2 = new Foo();
			await (s.LoadAsync(foo2, foo.Key));
			Assert.IsTrue(foo.EqualsFoo(foo2), "create");
			await (s.DeleteAsync(foo2));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task CallbackAsync()
		{
			ISession s = OpenSession();
			Qux q = new Qux("0");
			await (s.SaveAsync(q));
			q.Child = (new Qux("1"));
			await (s.SaveAsync(q.Child));
			Qux q2 = new Qux("2");
			q2.Child = q.Child;
			Qux q3 = new Qux("3");
			q.Child.Child = q3;
			await (s.SaveAsync(q3));
			Qux q4 = new Qux("4");
			q4.Child = q3;
			await (s.SaveAsync(q4));
			await (s.SaveAsync(q2));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			IList l = await (s.CreateQuery("from q in class NHibernate.DomainModel.Qux").ListAsync());
			Assert.AreEqual(5, l.Count);
			await (s.DeleteAsync(l[0]));
			await (s.DeleteAsync(l[1]));
			await (s.DeleteAsync(l[2]));
			await (s.DeleteAsync(l[3]));
			await (s.DeleteAsync(l[4]));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task PolymorphismAsync()
		{
			ISession s = OpenSession();
			Bar bar = new Bar();
			await (s.SaveAsync(bar));
			bar.BarString = "bar bar";
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			FooProxy foo = (FooProxy)await (s.LoadAsync(typeof (Foo), bar.Key));
			Assert.IsTrue(foo is BarProxy, "polymorphic");
			Assert.IsTrue(((BarProxy)foo).BarString.Equals(bar.BarString), "subclass property");
			await (s.DeleteAsync(foo));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task RemoveContainsAsync()
		{
			ISession s = OpenSession();
			Baz baz = new Baz();
			baz.SetDefaults();
			await (s.SaveAsync(baz));
			await (s.FlushAsync());
			Assert.IsTrue(await (s.ContainsAsync(baz)));
			await (s.EvictAsync(baz));
			Assert.IsFalse(await (s.ContainsAsync(baz)), "baz should have been evicted");
			Baz baz2 = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			Assert.IsFalse(baz == baz2, "should be different objects because Baz not contained in Session");
			await (s.DeleteAsync(baz2));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task CollectionOfSelfAsync()
		{
			ISession s = OpenSession();
			Bar bar = new Bar();
			await (s.SaveAsync(bar));
			// h2.0.3 was a set
			bar.Abstracts = new HashSet<object>();
			bar.Abstracts.Add(bar);
			Bar bar2 = new Bar();
			bar.Abstracts.Add(bar2);
			bar.TheFoo = bar;
			await (s.SaveAsync(bar2));
			await (s.FlushAsync());
			s.Close();
			bar.Abstracts = null;
			s = OpenSession();
			await (s.LoadAsync(bar, bar.Key));
			Assert.AreEqual(2, bar.Abstracts.Count);
			Assert.IsTrue(bar.Abstracts.Contains(bar), "collection contains self");
			Assert.AreSame(bar, bar.TheFoo, "association to self");
			if (Dialect is MySQLDialect)
			{
				// Break the self-reference cycle to avoid error when deleting the row
				bar.TheFoo = null;
				await (s.FlushAsync());
			}

			foreach (object obj in bar.Abstracts)
			{
				await (s.DeleteAsync(obj));
			}

			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task FindAsync()
		{
			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();
			// some code commented out in h2.0.3
			Bar bar = new Bar();
			await (s.SaveAsync(bar));
			bar.BarString = "bar bar";
			bar.String = "xxx";
			Foo foo = new Foo();
			await (s.SaveAsync(foo));
			foo.String = "foo bar";
			await (s.SaveAsync(new Foo()));
			await (s.SaveAsync(new Bar()));
			IList list1 = await (s.CreateQuery("select foo from foo in class NHibernate.DomainModel.Foo where foo.String='foo bar'").ListAsync());
			Assert.AreEqual(1, list1.Count, "find size");
			Assert.AreSame(foo, list1[0], "find ==");
			IList list2 = await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo order by foo.String, foo.Date").ListAsync());
			Assert.AreEqual(4, list2.Count, "find size");
			list1 = await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo where foo.class='B'").ListAsync());
			Assert.AreEqual(2, list1.Count, "class special property");
			list1 = await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo where foo.class=NHibernate.DomainModel.Bar").ListAsync());
			Assert.AreEqual(2, list1.Count, "class special property");
			list1 = await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo where foo.class=Bar").ListAsync());
			list2 = await (s.CreateQuery("select bar from bar in class NHibernate.DomainModel.Bar, foo in class NHibernate.DomainModel.Foo where bar.String = foo.String and not bar=foo").ListAsync());
			Assert.AreEqual(2, list1.Count, "class special property");
			Assert.AreEqual(1, list2.Count, "select from a subclass");
			Trivial t = new Trivial();
			await (s.SaveAsync(t));
			await (txn.CommitAsync());
			s.Close();
			s = OpenSession();
			txn = s.BeginTransaction();
			list1 = await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo where foo.String='foo bar'").ListAsync());
			Assert.AreEqual(1, list1.Count, "find count");
			// There is an interbase bug that causes null integers to return as 0, also numeric precision is <=15 -h2.0.3 comment
			Assert.IsTrue(((Foo)list1[0]).EqualsFoo(foo), "find equals");
			list2 = await (s.CreateQuery("select foo from foo in class NHibernate.DomainModel.Foo").ListAsync());
			Assert.AreEqual(5, list2.Count, "find count");
			IList list3 = await (s.CreateQuery("from bar in class NHibernate.DomainModel.Bar where bar.BarString='bar bar'").ListAsync());
			Assert.AreEqual(1, list3.Count, "find count");
			Assert.IsTrue(list2.Contains(list1[0]) && list2.Contains(list2[0]), "find same instance");
			Assert.AreEqual(1, (await (s.CreateQuery("from t in class NHibernate.DomainModel.Trivial").ListAsync())).Count);
			await (s.DeleteAsync("from t in class NHibernate.DomainModel.Trivial"));
			list2 = await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo where foo.Date = ?").SetDateTime(0, new DateTime(1970, 01, 01)).ListAsync());
			Assert.AreEqual(4, list2.Count, "find by date");
			IEnumerator enumer = list2.GetEnumerator();
			while (enumer.MoveNext())
			{
				await (s.DeleteAsync(enumer.Current));
			}

			list2 = await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo").ListAsync());
			Assert.AreEqual(0, list2.Count, "find deleted");
			await (txn.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task DeleteRecursiveAsync()
		{
			ISession s = OpenSession();
			Foo x = new Foo();
			Foo y = new Foo();
			x.TheFoo = y;
			y.TheFoo = x;
			await (s.SaveAsync(x));
			await (s.SaveAsync(y));
			await (s.FlushAsync());
			await (s.DeleteAsync(y));
			await (s.DeleteAsync(x));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task ReachabilityAsync()
		{
			// first for unkeyed collections
			ISession s = OpenSession();
			Baz baz1 = new Baz();
			await (s.SaveAsync(baz1));
			Baz baz2 = new Baz();
			await (s.SaveAsync(baz2));
			baz1.IntArray = new int[]{1, 2, 3, 4};
			baz1.FooSet = new HashSet<FooProxy>();
			Foo foo = new Foo();
			await (s.SaveAsync(foo));
			baz1.FooSet.Add(foo);
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz2 = (Baz)await (s.LoadAsync(typeof (Baz), baz2.Code));
			baz1 = (Baz)await (s.LoadAsync(typeof (Baz), baz1.Code));
			baz2.FooSet = baz1.FooSet;
			baz1.FooSet = null;
			baz2.IntArray = baz1.IntArray;
			baz1.IntArray = null;
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz2 = (Baz)await (s.LoadAsync(typeof (Baz), baz2.Code));
			baz1 = (Baz)await (s.LoadAsync(typeof (Baz), baz1.Code));
			Assert.AreEqual(4, baz2.IntArray.Length, "unkeyed reachability - baz2.IntArray");
			Assert.AreEqual(1, baz2.FooSet.Count, "unkeyed reachability - baz2.FooSet");
			Assert.AreEqual(0, baz1.IntArray.Length, "unkeyed reachability - baz1.IntArray");
			Assert.AreEqual(0, baz1.FooSet.Count, "unkeyed reachability - baz1.FooSet");
			foreach (object obj in baz2.FooSet)
			{
				await (s.DeleteAsync((FooProxy)obj));
			}

			await (s.DeleteAsync(baz1));
			await (s.DeleteAsync(baz2));
			await (s.FlushAsync());
			s.Close();
			// now for collections of collections
			s = OpenSession();
			baz1 = new Baz();
			await (s.SaveAsync(baz1));
			baz2 = new Baz();
			await (s.SaveAsync(baz2));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz2 = (Baz)await (s.LoadAsync(typeof (Baz), baz2.Code));
			baz1 = (Baz)await (s.LoadAsync(typeof (Baz), baz1.Code));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz2 = (Baz)await (s.LoadAsync(typeof (Baz), baz2.Code));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz2 = (Baz)await (s.LoadAsync(typeof (Baz), baz2.Code));
			baz1 = (Baz)await (s.LoadAsync(typeof (Baz), baz1.Code));
			await (s.DeleteAsync(baz1));
			await (s.DeleteAsync(baz2));
			await (s.FlushAsync());
			s.Close();
			// now for keyed collections
			s = OpenSession();
			baz1 = new Baz();
			await (s.SaveAsync(baz1));
			baz2 = new Baz();
			await (s.SaveAsync(baz2));
			Foo foo1 = new Foo();
			Foo foo2 = new Foo();
			await (s.SaveAsync(foo1));
			await (s.SaveAsync(foo2));
			baz1.FooArray = new Foo[]{foo1, null, foo2};
			baz1.StringDateMap = new Dictionary<string, DateTime? >();
			baz1.StringDateMap["today"] = DateTime.Today;
			baz1.StringDateMap["foo"] = null;
			baz1.StringDateMap["tomm"] = new DateTime(DateTime.Today.Ticks + (new TimeSpan(1, 0, 0, 0, 0)).Ticks);
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz2 = (Baz)await (s.LoadAsync(typeof (Baz), baz2.Code));
			baz1 = (Baz)await (s.LoadAsync(typeof (Baz), baz1.Code));
			baz2.FooArray = baz1.FooArray;
			baz1.FooArray = null;
			baz2.StringDateMap = baz1.StringDateMap;
			baz1.StringDateMap = null;
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz2 = (Baz)await (s.LoadAsync(typeof (Baz), baz2.Code));
			baz1 = (Baz)await (s.LoadAsync(typeof (Baz), baz1.Code));
			Assert.AreEqual(3, baz2.StringDateMap.Count, "baz2.StringDateMap count - reachability");
			Assert.AreEqual(3, baz2.FooArray.Length, "baz2.FooArray length - reachability");
			Assert.AreEqual(0, baz1.StringDateMap.Count, "baz1.StringDateMap count - reachability");
			Assert.AreEqual(0, baz1.FooArray.Length, "baz1.FooArray length - reachability");
			Assert.IsNull(baz2.FooArray[1], "null element");
			Assert.IsNotNull(baz2.StringDateMap["today"], "today non-null element");
			Assert.IsNotNull(baz2.StringDateMap["tomm"], "tomm non-null element");
			Assert.IsNull(baz2.StringDateMap["foo"], "foo is null element");
			await (s.DeleteAsync(baz2.FooArray[0]));
			await (s.DeleteAsync(baz2.FooArray[2]));
			await (s.DeleteAsync(baz1));
			await (s.DeleteAsync(baz2));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task PersistentLifecycleAsync()
		{
			ISession s = OpenSession();
			Qux q = new Qux();
			await (s.SaveAsync(q));
			q.Stuff = "foo bar baz qux";
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			q = (Qux)await (s.LoadAsync(typeof (Qux), q.Key));
			Assert.IsTrue(q.Created, "lifecycle create");
			Assert.IsTrue(q.Loaded, "lifecycle load");
			Assert.IsNotNull(q.Foo, "lifecycle subobject");
			await (s.DeleteAsync(q));
			Assert.IsTrue(q.Deleted, "lifecyle delete");
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			Assert.AreEqual(0, (await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo").ListAsync())).Count, "subdeletion");
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task EnumerableAsync()
		{
			// this test used to be called Iterators()
			ISession s = OpenSession();
			for (int i = 0; i < 10; i++)
			{
				Qux q = new Qux();
				object qid = await (s.SaveAsync(q));
				Assert.IsNotNull(q, "q is not null");
				Assert.IsNotNull(qid, "qid is not null");
			}

			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			IEnumerator enumer = (await (s.CreateQuery("from q in class NHibernate.DomainModel.Qux where q.Stuff is null").EnumerableAsync())).GetEnumerator();
			int count = 0;
			while (enumer.MoveNext())
			{
				Qux q = (Qux)enumer.Current;
				q.Stuff = "foo";
				// can't remove item from IEnumerator in .net 
				if (count == 0 || count == 5)
				{
					await (s.DeleteAsync(q));
				}

				count++;
			}

			Assert.AreEqual(10, count, "found 10 items");
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			Assert.AreEqual(8, await (s.DeleteAsync("from q in class NHibernate.DomainModel.Qux where q.Stuff=?", "foo", NHibernateUtil.String)), "delete by query");
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			enumer = (await (s.CreateQuery("from q in class NHibernate.DomainModel.Qux").EnumerableAsync())).GetEnumerator();
			Assert.IsFalse(enumer.MoveNext(), "no items in enumerator");
			await (s.FlushAsync());
			s.Close();
		}

		/// <summary>
		/// Adding a test to verify that a database action can occur in the
		/// middle of an Enumeration.  Under certain conditions an open 
		/// DataReader can be kept open and cause any other action to fail. 
		/// </summary>
		[Test]
		public async Task EnumerableDisposableAsync()
		{
			// this test used to be called Iterators()
			ISession s = OpenSession();
			for (long i = 0L; i < 10L; i++)
			{
				Simple simple = new Simple();
				simple.Count = (int)i;
				await (s.SaveAsync(simple, i));
				Assert.IsNotNull(simple, "simple is not null");
			}

			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Simple simp = (Simple)await (s.LoadAsync(typeof (Simple), 8L));
			// the reader under the enum has to still be a SqlDataReader (subst db name here) and 
			// can't be a NDataReader - the best way to get this result is to query on just a property
			// of an object.  If the query is "from Simple as s" then it will be converted to a NDataReader
			// on the MoveNext so it can get the object from the id - thus needing another open DataReader so
			// it must convert to an NDataReader.
			IEnumerable enumer = await (s.CreateQuery("select s.Count from Simple as s").EnumerableAsync());
			//int count = 0;
			foreach (object obj in enumer)
			{
				if ((int)obj == 7)
				{
					break;
				}
			}

			// if Enumerable doesn't implement Dispose() then the test fails on this line
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			Assert.AreEqual(10, await (s.DeleteAsync("from Simple")), "delete by query");
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			enumer = await (s.CreateQuery("from Simple").EnumerableAsync());
			Assert.IsFalse(enumer.GetEnumerator().MoveNext(), "no items in enumerator");
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task VersioningAsync()
		{
			GlarchProxy g = new Glarch();
			GlarchProxy g2 = new Glarch();
			object gid, g2id;
			using (ISession s = OpenSession())
				using (ITransaction txn = s.BeginTransaction())
				{
					await (s.SaveAsync(g));
					await (s.SaveAsync(g2));
					gid = s.GetIdentifier(g);
					g2id = s.GetIdentifier(g2);
					g.Name = "glarch";
					await (txn.CommitAsync());
				}

			sessions.Evict(typeof (Glarch));
			using (ISession s = OpenSession())
				using (ITransaction txn = s.BeginTransaction())
				{
					g = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), gid));
					await (s.LockAsync(g, LockMode.Upgrade));
					g2 = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), g2id));
					// Versions are initialized to 1 in NH (not to 0 like in Hibernate)
					Assert.AreEqual(2, g.Version, "version");
					Assert.AreEqual(2, g.DerivedVersion, "version");
					Assert.AreEqual(1, g2.Version, "version");
					g.Name = "foo";
					Assert.IsTrue((await (s.CreateQuery("from g in class Glarch where g.Version=3").ListAsync())).Count == 1, "find by version");
					g.Name = "bar";
					await (txn.CommitAsync());
				}

			sessions.Evict(typeof (Glarch));
			using (ISession s = OpenSession())
				using (ITransaction txn = s.BeginTransaction())
				{
					g = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), gid));
					g2 = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), g2id));
					Assert.AreEqual(4, g.Version, "version");
					Assert.AreEqual(4, g.DerivedVersion, "version");
					Assert.AreEqual(1, g2.Version, "version");
					g.Next = null;
					g2.Next = g;
					await (s.DeleteAsync(g2));
					await (s.DeleteAsync(g));
					await (txn.CommitAsync());
				}
		}

		// The test below is commented out, it fails because Glarch is mapped with optimistic-lock="dirty"
		// which means that the version column is not used for optimistic locking.
		/*
		[Test]
		public void Versioning() 
		{
			object gid, g2id;

			using( ISession s = OpenSession() )
			{
				GlarchProxy g = new Glarch();
				s.Save(g);

				GlarchProxy g2 = new Glarch();
				s.Save(g2);

				gid = s.GetIdentifier(g);
				g2id = s.GetIdentifier(g2);
				g.Name = "glarch";
				s.Flush();
			}

			GlarchProxy gOld;

			// grab a version of g that is old and hold onto it until later
			// for a StaleObjectException check.
			using( ISession sOld = OpenSession() )
			{
				gOld = (GlarchProxy)sOld.Get( typeof(Glarch), gid );

				// want gOld to be initialized so later I can change a property
				Assert.IsTrue( NHibernateUtil.IsInitialized( gOld ), "should be initialized" );
			}

			using( ISession s = OpenSession() )
			{
				GlarchProxy g = (GlarchProxy)s.Load( typeof(Glarch), gid );
				s.Lock(g, LockMode.Upgrade);
				GlarchProxy g2 = (GlarchProxy)s.Load( typeof(Glarch), g2id );
				Assert.AreEqual(1, g.Version, "g's version");
				Assert.AreEqual(1, g.DerivedVersion, "g's derived version");
				Assert.AreEqual(0, g2.Version, "g2's version");
				g.Name = "foo";
				Assert.AreEqual(1, s.CreateQuery("from g in class NHibernate.DomainModel.Glarch where g.Version=2").List().Count, "find by version");
				g.Name = "bar";
				s.Flush();
			}

			// now that g has been changed verify that we can't go back and update 
			// it with an old version of g
			bool isStale = false;

			using( ISession sOld = OpenSession() )
			{
				gOld.Name = "should not update";
				try 
				{
					sOld.Update( gOld, gid );
					sOld.Flush();
					//sOld.Close();
					sOld.Dispose();
				}
				catch(Exception e) 
				{
					Exception exc = e;
					while( exc!=null ) 
					{
						if( exc is StaleObjectStateException ) 
						{
							isStale = true;
							break;
						}
						exc = exc.InnerException;
					}
				}
			}

			Assert.IsTrue( isStale, "Did not catch a stale object exception when updating an old GlarchProxy." );

			using( ISession s = OpenSession() )
			{
				GlarchProxy g = (GlarchProxy)s.Load( typeof(Glarch), gid );
				GlarchProxy g2 = (GlarchProxy)s.Load( typeof(Glarch), g2id );

				Assert.AreEqual(3, g.Version, "g's version");
				Assert.AreEqual(3, g.DerivedVersion, "g's derived version");
				Assert.AreEqual(0, g2.Version, "g2's version");

				g.Next = null;
				g2.Next = g;
				s.Delete(g2);
				s.Delete(g);
				s.Flush();
				//s.Close();
			}
		}
		*/
		[Test]
		public async Task VersionedCollectionsAsync()
		{
			ISession s = OpenSession();
			GlarchProxy g = new Glarch();
			await (s.SaveAsync(g));
			g.ProxyArray = new GlarchProxy[]{g};
			string gid = (string)s.GetIdentifier(g);
			IList<string> list = new List<string>();
			list.Add("foo");
			g.Strings = list;
			// <sets> in h2.0.3
			g.ProxySet = new HashSet<GlarchProxy>{g};
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			g = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), gid));
			Assert.AreEqual(1, g.Strings.Count);
			Assert.AreEqual(1, g.ProxyArray.Length);
			Assert.AreEqual(1, g.ProxySet.Count);
			Assert.AreEqual(2, g.Version, "version collection before");
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			g = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), gid));
			Assert.AreEqual("foo", g.Strings[0]);
			Assert.AreSame(g, g.ProxyArray[0]);
			IEnumerator enumer = g.ProxySet.GetEnumerator();
			enumer.MoveNext();
			Assert.AreSame(g, enumer.Current);
			Assert.AreEqual(2, g.Version, "versioned collection before");
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			g = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), gid));
			Assert.AreEqual(2, g.Version, "versioned collection before");
			g.Strings.Add("bar");
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			g = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), gid));
			Assert.AreEqual(3, g.Version, "versioned collection after");
			Assert.AreEqual(2, g.Strings.Count, "versioned collection after");
			g.ProxyArray = null;
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			g = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), gid));
			Assert.AreEqual(4, g.Version, "versioned collection after");
			Assert.AreEqual(0, g.ProxyArray.Length, "version collection after");
			g.FooComponents = new List<FooComponent>();
			g.ProxyArray = null;
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			g = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), gid));
			Assert.AreEqual(5, g.Version, "versioned collection after");
			await (s.DeleteAsync(g));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task RecursiveLoadAsync()
		{
			// Non polymorphisc class (there is an implementation optimization
			// being tested here) - from h2.0.3 - what does that mean?
			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();
			GlarchProxy last = new Glarch();
			await (s.SaveAsync(last));
			last.Order = 0;
			for (int i = 0; i < 5; i++)
			{
				GlarchProxy next = new Glarch();
				await (s.SaveAsync(next));
				last.Next = next;
				last = next;
				last.Order = (short)(i + 1);
			}

			IEnumerator enumer = (await (s.CreateQuery("from g in class NHibernate.DomainModel.Glarch").EnumerableAsync())).GetEnumerator();
			while (enumer.MoveNext())
			{
				object objTemp = enumer.Current;
			}

			IList list = await (s.CreateQuery("from g in class NHibernate.DomainModel.Glarch").ListAsync());
			Assert.AreEqual(6, list.Count, "recursive find");
			await (txn.CommitAsync());
			s.Close();
			s = OpenSession();
			txn = s.BeginTransaction();
			list = await (s.CreateQuery("from g in class NHibernate.DomainModel.Glarch").ListAsync());
			Assert.AreEqual(6, list.Count, "recursive iter");
			list = await (s.CreateQuery("from g in class NHibernate.DomainModel.Glarch where g.Next is not null").ListAsync());
			Assert.AreEqual(5, list.Count, "exclude the null next");
			await (txn.CommitAsync());
			s.Close();
			s = OpenSession();
			txn = s.BeginTransaction();
			enumer = (await (s.CreateQuery("from g in class NHibernate.DomainModel.Glarch order by g.Order asc").EnumerableAsync())).GetEnumerator();
			while (enumer.MoveNext())
			{
				GlarchProxy g = (GlarchProxy)enumer.Current;
				Assert.IsNotNull(g, "not null");
			// no equiv in .net - so ran a delete query
			// iter.remove();
			}

			await (s.DeleteAsync("from NHibernate.DomainModel.Glarch as g"));
			await (txn.CommitAsync());
			s.Close();
			// same thing bug using polymorphic class (no optimization possible)
			s = OpenSession();
			txn = s.BeginTransaction();
			FooProxy flast = new Bar();
			await (s.SaveAsync(flast));
			for (int i = 0; i < 5; i++)
			{
				FooProxy foo = new Bar();
				await (s.SaveAsync(foo));
				flast.TheFoo = foo;
				flast = flast.TheFoo;
				flast.String = "foo" + (i + 1);
			}

			enumer = (await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo").EnumerableAsync())).GetEnumerator();
			while (enumer.MoveNext())
			{
				object objTemp = enumer.Current;
			}

			list = await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo").ListAsync());
			Assert.AreEqual(6, list.Count, "recursive find");
			await (txn.CommitAsync());
			s.Close();
			s = OpenSession();
			txn = s.BeginTransaction();
			list = await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo").ListAsync());
			Assert.AreEqual(6, list.Count, "recursive iter");
			enumer = list.GetEnumerator();
			while (enumer.MoveNext())
			{
				Assert.IsTrue(enumer.Current is BarProxy, "polymorphic recursive load");
			}

			await (txn.CommitAsync());
			s.Close();
			s = OpenSession();
			txn = s.BeginTransaction();
			enumer = (await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo order by foo.String asc").EnumerableAsync())).GetEnumerator();
			string currentString = String.Empty;
			while (enumer.MoveNext())
			{
				BarProxy bar = (BarProxy)enumer.Current;
				string theString = bar.String;
				Assert.IsNotNull(bar, "not null");
				if (currentString != String.Empty)
				{
					Assert.IsTrue(theString.CompareTo(currentString) >= 0, "not in asc order");
				}

				currentString = theString;
			// no equiv in .net - so made a hql delete
			// iter.remove();
			}

			await (s.DeleteAsync("from NHibernate.DomainModel.Foo as foo"));
			await (txn.CommitAsync());
			s.Close();
		}

		// Not ported - testScrollableIterator - ScrollableResults are not supported by NH,
		// since they rely on the underlying ResultSet to support scrolling, and ADO.NET
		// DbDataReaders do not support it.
		private bool DialectSupportsCountDistinct
		{
			get
			{
				return !(Dialect is SQLiteDialect);
			}
		}

		[Test]
		public async Task MultiColumnQueriesAsync()
		{
			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();
			Foo foo = new Foo();
			await (s.SaveAsync(foo));
			Foo foo1 = new Foo();
			await (s.SaveAsync(foo1));
			foo.TheFoo = foo1;
			IList l = await (s.CreateQuery("select parent, child from parent in class NHibernate.DomainModel.Foo, child in class NHibernate.DomainModel.Foo where parent.TheFoo = child").ListAsync());
			Assert.AreEqual(1, l.Count, "multi-column find");
			IEnumerator rs;
			object[] row;
			if (DialectSupportsCountDistinct)
			{
				rs = (await (s.CreateQuery("select count(distinct child.id), count(distinct parent.id) from parent in class NHibernate.DomainModel.Foo, child in class NHibernate.DomainModel.Foo where parent.TheFoo = child").EnumerableAsync())).GetEnumerator();
				Assert.IsTrue(rs.MoveNext());
				row = (object[])rs.Current;
				Assert.AreEqual(1, row[0], "multi-column count");
				Assert.AreEqual(1, row[1], "multi-column count");
				Assert.IsFalse(rs.MoveNext());
			}

			rs = (await (s.CreateQuery("select child.id, parent.id, child.Long from parent in class NHibernate.DomainModel.Foo, child in class NHibernate.DomainModel.Foo where parent.TheFoo = child").EnumerableAsync())).GetEnumerator();
			Assert.IsTrue(rs.MoveNext());
			row = (object[])rs.Current;
			Assert.AreEqual(foo.TheFoo.Key, row[0], "multi-column id");
			Assert.AreEqual(foo.Key, row[1], "multi-column id");
			Assert.AreEqual(foo.TheFoo.Long, row[2], "multi-column property");
			Assert.IsFalse(rs.MoveNext());
			rs = (await (s.CreateQuery("select child.id, parent.id, child.Long, child, parent.TheFoo from parent in class NHibernate.DomainModel.Foo, child in class NHibernate.DomainModel.Foo where parent.TheFoo = child").EnumerableAsync())).GetEnumerator();
			Assert.IsTrue(rs.MoveNext());
			row = (object[])rs.Current;
			Assert.AreEqual(foo.TheFoo.Key, row[0], "multi-column id");
			Assert.AreEqual(foo.Key, row[1], "multi-column id");
			Assert.AreEqual(foo.TheFoo.Long, row[2], "multi-column property");
			Assert.AreSame(foo.TheFoo, row[3], "multi-column object");
			Assert.AreSame(row[3], row[4], "multi-column same object");
			Assert.IsFalse(rs.MoveNext());
			row = (object[])l[0];
			Assert.AreSame(foo, row[0], "multi-column find");
			Assert.AreSame(foo.TheFoo, row[1], "multi-column find");
			await (txn.CommitAsync());
			s.Close();
			s = OpenSession();
			txn = s.BeginTransaction();
			IEnumerator enumer = (await (s.CreateQuery("select parent, child from parent in class NHibernate.DomainModel.Foo, child in class NHibernate.DomainModel.Foo where parent.TheFoo = child and parent.String='a string'").EnumerableAsync())).GetEnumerator();
			int deletions = 0;
			while (enumer.MoveNext())
			{
				object[] pnc = (object[])enumer.Current;
				await (s.DeleteAsync(pnc[0]));
				await (s.DeleteAsync(pnc[1]));
				deletions++;
			}

			Assert.AreEqual(1, deletions, "multi-column enumerate");
			await (txn.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task DeleteTransientAsync()
		{
			Fee fee = new Fee();
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			await (s.SaveAsync(fee));
			await (s.FlushAsync());
			fee.Count = 123;
			await (tx.CommitAsync());
			s.Close();
			s = OpenSession();
			tx = s.BeginTransaction();
			await (s.DeleteAsync(fee));
			await (tx.CommitAsync());
			s.Close();
			s = OpenSession();
			tx = s.BeginTransaction();
			Assert.AreEqual(0, (await (s.CreateQuery("from fee in class Fee").ListAsync())).Count);
			await (tx.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task DeleteUpdatedTransientAsync()
		{
			Fee fee = new Fee();
			Fee fee2 = new Fee();
			fee2.AnotherFee = fee;
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					await (s.SaveAsync(fee));
					await (s.SaveAsync(fee2));
					await (s.FlushAsync());
					fee.Count = 123;
					await (tx.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					await (s.UpdateAsync(fee));
					//fee2.AnotherFee = null;
					await (s.UpdateAsync(fee2));
					await (s.DeleteAsync(fee));
					await (s.DeleteAsync(fee2));
					await (tx.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					Assert.AreEqual(0, (await (s.CreateQuery("from fee in class Fee").ListAsync())).Count);
					await (tx.CommitAsync());
				}
			}
		}

		[Test]
		public async Task UpdateOrderAsync()
		{
			Fee fee1, fee2, fee3;
			using (ISession s = OpenSession())
			{
				fee1 = new Fee();
				await (s.SaveAsync(fee1));
				fee2 = new Fee();
				fee1.TheFee = fee2;
				fee2.TheFee = fee1;
				fee2.Fees = new HashSet<string>();
				fee3 = new Fee();
				fee3.TheFee = fee1;
				fee3.AnotherFee = fee2;
				fee2.AnotherFee = fee3;
				await (s.SaveAsync(fee3));
				await (s.SaveAsync(fee2));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				fee1.Count = 10;
				fee2.Count = 20;
				fee3.Count = 30;
				await (s.UpdateAsync(fee1));
				await (s.UpdateAsync(fee2));
				await (s.UpdateAsync(fee3));
				await (s.FlushAsync());
				await (s.DeleteAsync(fee1));
				await (s.DeleteAsync(fee2));
				await (s.DeleteAsync(fee3));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					Assert.AreEqual(0, (await (s.CreateQuery("from fee in class Fee").ListAsync())).Count);
					await (tx.CommitAsync());
				}
			}
		}

		[Test]
		public async Task UpdateFromTransientAsync()
		{
			ISession s = OpenSession();
			Fee fee1 = new Fee();
			await (s.SaveAsync(fee1));
			Fee fee2 = new Fee();
			fee1.TheFee = fee2;
			fee2.TheFee = fee1;
			fee2.Fees = new HashSet<string>();
			Fee fee3 = new Fee();
			fee3.TheFee = fee1;
			fee3.AnotherFee = fee2;
			fee2.AnotherFee = fee3;
			await (s.SaveAsync(fee3));
			await (s.SaveAsync(fee2));
			await (s.FlushAsync());
			s.Close();
			fee1.Fi = "changed";
			s = OpenSession();
			await (s.SaveOrUpdateAsync(fee1));
			await (s.FlushAsync());
			s.Close();
			Qux q = new Qux("quxxy");
			fee1.Qux = q;
			s = OpenSession();
			await (s.SaveOrUpdateAsync(fee1));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			fee1 = (Fee)await (s.LoadAsync(typeof (Fee), fee1.Key));
			Assert.AreEqual("changed", fee1.Fi, "updated from transient");
			Assert.IsNotNull(fee1.Qux, "unsaved-value");
			await (s.DeleteAsync(fee1.Qux));
			fee1.Qux = null;
			await (s.FlushAsync());
			s.Close();
			fee2.Fi = "CHANGED";
			fee2.Fees.Add("an element");
			fee1.Fi = "changed again";
			s = OpenSession();
			await (s.SaveOrUpdateAsync(fee2));
			await (s.UpdateAsync(fee1, fee1.Key));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			Fee fee = new Fee();
			await (s.LoadAsync(fee, fee2.Key));
			fee1 = (Fee)await (s.LoadAsync(typeof (Fee), fee1.Key));
			Assert.AreEqual("changed again", fee1.Fi, "updated from transient");
			Assert.AreEqual("CHANGED", fee.Fi, "updated from transient");
			Assert.IsTrue(fee.Fees.Contains("an element"), "updated collection");
			await (s.FlushAsync());
			s.Close();
			fee.Fees.Clear();
			fee.Fees.Add("new element");
			fee1.TheFee = null;
			s = OpenSession();
			await (s.SaveOrUpdateAsync(fee));
			await (s.SaveOrUpdateAsync(fee1));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			await (s.LoadAsync(fee, fee.Key));
			Assert.IsNotNull(fee.AnotherFee, "update");
			Assert.IsNotNull(fee.TheFee, "update");
			Assert.AreSame(fee.AnotherFee.TheFee, fee.TheFee, "update");
			Assert.IsTrue(fee.Fees.Contains("new element"), "updated collection");
			Assert.IsFalse(fee.Fees.Contains("an element"), "updated collection");
			await (s.FlushAsync());
			s.Close();
			fee.Qux = new Qux("quxy");
			s = OpenSession();
			await (s.SaveOrUpdateAsync(fee));
			await (s.FlushAsync());
			s.Close();
			fee.Qux.Stuff = "xxx";
			s = OpenSession();
			await (s.SaveOrUpdateAsync(fee));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			await (s.LoadAsync(fee, fee.Key));
			Assert.IsNotNull(fee.Qux, "cascade update");
			Assert.AreEqual("xxx", fee.Qux.Stuff, "cascade update");
			Assert.IsNotNull(fee.AnotherFee, "update");
			Assert.IsNotNull(fee.TheFee, "update");
			Assert.AreSame(fee.AnotherFee.TheFee, fee.TheFee, "update");
			fee.AnotherFee.AnotherFee = null;
			await (s.DeleteAsync(fee));
			await (s.DeleteAsync("from fee in class NHibernate.DomainModel.Fee"));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task ArraysOfTimesAsync()
		{
			Baz baz;
			using (ISession s = OpenSession())
			{
				baz = new Baz();
				await (s.SaveAsync(baz));
				baz.SetDefaults();
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				baz.TimeArray[2] = new DateTime(123); // H2.1: new Date(123)
				baz.TimeArray[3] = new DateTime(1234); // H2.1: new java.sql.Time(1234)
				await (s.UpdateAsync(baz)); // missing in H2.1
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
				await (s.DeleteAsync(baz));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task ComponentsAsync()
		{
			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();
			Foo foo = new Foo();
			foo.Component = new FooComponent("foo", 69, null, new FooComponent("bar", 96, null, null));
			await (s.SaveAsync(foo));
			foo.Component.Name = "IFA";
			await (txn.CommitAsync());
			s.Close();
			foo.Component = null;
			s = OpenSession();
			txn = s.BeginTransaction();
			await (s.LoadAsync(foo, foo.Key));
			Assert.AreEqual("IFA", foo.Component.Name, "save components");
			Assert.AreEqual("bar", foo.Component.Subcomponent.Name, "save subcomponent");
			Assert.IsNotNull(foo.Component.Glarch, "cascades save via component");
			foo.Component.Subcomponent.Name = "baz";
			await (txn.CommitAsync());
			s.Close();
			foo.Component = null;
			s = OpenSession();
			txn = s.BeginTransaction();
			await (s.LoadAsync(foo, foo.Key));
			Assert.AreEqual("IFA", foo.Component.Name, "update components");
			Assert.AreEqual("baz", foo.Component.Subcomponent.Name, "update subcomponent");
			await (s.DeleteAsync(foo));
			await (txn.CommitAsync());
			s.Close();
			s = OpenSession();
			txn = s.BeginTransaction();
			foo = new Foo();
			await (s.SaveAsync(foo));
			foo.Custom = new string[]{"one", "two"};
			// Custom.s1 uses the first column under the <property name="Custom"...>
			// which is first_name
			Assert.AreSame(foo, (await (s.CreateQuery("from Foo foo where foo.Custom.s1 = 'one'").ListAsync()))[0]);
			await (s.DeleteAsync(foo));
			await (txn.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task EnumAsync()
		{
			ISession s = OpenSession();
			FooProxy foo = new Foo();
			object id = await (s.SaveAsync(foo));
			foo.Status = FooStatus.ON;
			await (s.FlushAsync());
			s.Close();
			// verify an enum can be in the ctor
			s = OpenSession();
			IList list = await (s.CreateQuery("select new Result(foo.String, foo.Long, foo.Integer, foo.Status) from foo in class Foo").ListAsync());
			Assert.AreEqual(1, list.Count, "Should have found foo");
			Assert.AreEqual(FooStatus.ON, ((Result)list[0]).Status, "verifying enum set in ctor - should have been ON");
			s.Close();
			s = OpenSession();
			foo = (FooProxy)await (s.LoadAsync(typeof (Foo), id));
			Assert.AreEqual(FooStatus.ON, foo.Status);
			foo.Status = FooStatus.OFF;
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			foo = (FooProxy)await (s.LoadAsync(typeof (Foo), id));
			Assert.AreEqual(FooStatus.OFF, foo.Status);
			s.Close();
			// verify that SetEnum with named params works correctly
			s = OpenSession();
			IQuery q = s.CreateQuery("from Foo as f where f.Status = :status");
			q.SetEnum("status", FooStatus.OFF);
			IList results = await (q.ListAsync());
			Assert.AreEqual(1, results.Count, "should have found 1");
			foo = (Foo)results[0];
			q = s.CreateQuery("from Foo as f where f.Status = :status");
			q.SetEnum("status", FooStatus.ON);
			results = await (q.ListAsync());
			Assert.AreEqual(0, results.Count, "no foo with status of ON");
			// try to have the Query guess the enum type
			q = s.CreateQuery("from Foo as f where f.Status = :status");
			q.SetParameter("status", FooStatus.OFF);
			results = await (q.ListAsync());
			Assert.AreEqual(1, results.Count, "found the 1 result");
			// have the query guess the enum type in a ParameterList.
			q = s.CreateQuery("from Foo as f where f.Status in (:status)");
			q.SetParameterList("status", new FooStatus[]{FooStatus.OFF, FooStatus.ON});
			results = await (q.ListAsync());
			Assert.AreEqual(1, results.Count, "should have found the 1 foo");
			q = s.CreateQuery("from Foo as f where f.Status = FooStatus.OFF");
			Assert.AreEqual(1, (await (q.ListAsync())).Count, "Enum in string - should have found OFF");
			q = s.CreateQuery("from Foo as f where f.Status = FooStatus.ON");
			Assert.AreEqual(0, (await (q.ListAsync())).Count, "Enum in string - should not have found ON");
			await (s.DeleteAsync(foo));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task NoForeignKeyViolationsAsync()
		{
			ISession s = OpenSession();
			Glarch g1 = new Glarch();
			Glarch g2 = new Glarch();
			g1.Next = g2;
			g2.Next = g1;
			await (s.SaveAsync(g1));
			await (s.SaveAsync(g2));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			IList l = await (s.CreateQuery("from g in class NHibernate.DomainModel.Glarch where g.Next is not null").ListAsync());
			await (s.DeleteAsync(l[0]));
			await (s.DeleteAsync(l[1]));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task LazyCollectionsAsync()
		{
			ISession s = OpenSession();
			Qux q = new Qux();
			await (s.SaveAsync(q));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			q = (Qux)await (s.LoadAsync(typeof (Qux), q.Key));
			await (s.FlushAsync());
			s.Close();
			// two exceptions are supposed to occur:")
			bool ok = false;
			try
			{
				int countMoreFums = q.MoreFums.Count;
			}
			catch (LazyInitializationException lie)
			{
				Debug.WriteLine("caught expected " + lie.ToString());
				ok = true;
			}

			Assert.IsTrue(ok, "lazy collection with one-to-many");
			ok = false;
			try
			{
				int countFums = q.Fums.Count;
			}
			catch (LazyInitializationException lie)
			{
				Debug.WriteLine("caught expected " + lie.ToString());
				ok = true;
			}

			Assert.IsTrue(ok, "lazy collection with many-to-many");
			s = OpenSession();
			q = (Qux)await (s.LoadAsync(typeof (Qux), q.Key));
			await (s.DeleteAsync(q));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task NewSessionLifecycleAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			object fid = null;
			try
			{
				Foo f = new Foo();
				await (s.SaveAsync(f));
				fid = s.GetIdentifier(f);
				//s.Flush();
				await (t.CommitAsync());
			}
			catch (Exception)
			{
				t.Rollback();
				throw;
			}
			finally
			{
				s.Close();
			}

			s = OpenSession();
			t = s.BeginTransaction();
			try
			{
				Foo f = new Foo();
				await (s.DeleteAsync(f));
				//s.Flush();
				await (t.CommitAsync());
			}
			catch (Exception)
			{
				t.Rollback();
			}
			finally
			{
				s.Close();
			}

			s = OpenSession();
			t = s.BeginTransaction();
			try
			{
				Foo f = (Foo)await (s.LoadAsync(typeof (Foo), fid, LockMode.Upgrade));
				await (s.DeleteAsync(f));
				// s.Flush();
				await (t.CommitAsync());
			}
			catch (Exception)
			{
				t.Rollback();
				throw;
			}
			finally
			{
				Assert.IsNull(s.Close());
			}
		}

		[Test]
		public async Task DisconnectAsync()
		{
			ISession s = OpenSession();
			Foo foo = new Foo();
			Foo foo2 = new Foo();
			await (s.SaveAsync(foo));
			await (s.SaveAsync(foo2));
			foo2.TheFoo = foo;
			await (s.FlushAsync());
			s.Disconnect();
			s.Reconnect();
			await (s.DeleteAsync(foo));
			foo2.TheFoo = null;
			await (s.FlushAsync());
			s.Disconnect();
			s.Reconnect();
			await (s.DeleteAsync(foo2));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task OrderByAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Foo foo = new Foo();
			await (s.SaveAsync(foo));
			IList list = await (s.CreateQuery("select foo from foo in class Foo, fee in class Fee where foo.Dependent = fee order by foo.String desc, foo.Component.Count asc, fee.id").ListAsync());
			Assert.AreEqual(1, list.Count, "order by");
			Foo foo2 = new Foo();
			await (s.SaveAsync(foo2));
			foo.TheFoo = foo2;
			list = await (s.CreateQuery("select foo.TheFoo, foo.Dependent from foo in class Foo order by foo.TheFoo.String desc, foo.Component.Count asc, foo.Dependent.id").ListAsync());
			Assert.AreEqual(1, list.Count, "order by");
			list = await (s.CreateQuery("select foo from foo in class NHibernate.DomainModel.Foo order by foo.Dependent.id, foo.Dependent.Fi").ListAsync());
			Assert.AreEqual(2, list.Count, "order by");
			await (s.DeleteAsync(foo));
			await (s.DeleteAsync(foo2));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			Many manyB = new Many();
			await (s.SaveAsync(manyB));
			One oneB = new One();
			await (s.SaveAsync(oneB));
			oneB.Value = "b";
			manyB.One = oneB;
			Many manyA = new Many();
			await (s.SaveAsync(manyA));
			One oneA = new One();
			await (s.SaveAsync(oneA));
			oneA.Value = "a";
			manyA.One = oneA;
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			IEnumerable enumerable = await (s.CreateQuery("SELECT one FROM one IN CLASS " + typeof (One).Name + " ORDER BY one.Value ASC").EnumerableAsync());
			int count = 0;
			foreach (One one in enumerable)
			{
				switch (count)
				{
					case 0:
						Assert.AreEqual("a", one.Value, "a - ordering failed");
						break;
					case 1:
						Assert.AreEqual("b", one.Value, "b - ordering failed");
						break;
					default:
						Assert.Fail("more than two elements");
						break;
				}

				count++;
			}

			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			enumerable = await (s.CreateQuery("SELECT many.One FROM many IN CLASS " + typeof (Many).Name + " ORDER BY many.One.Value ASC, many.One.id").EnumerableAsync());
			count = 0;
			foreach (One one in enumerable)
			{
				switch (count)
				{
					case 0:
						Assert.AreEqual("a", one.Value, "'a' should be first element");
						break;
					case 1:
						Assert.AreEqual("b", one.Value, "'b' should be second element");
						break;
					default:
						Assert.Fail("more than 2 elements");
						break;
				}

				count++;
			}

			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			oneA = (One)await (s.LoadAsync(typeof (One), oneA.Key));
			manyA = (Many)await (s.LoadAsync(typeof (Many), manyA.Key));
			oneB = (One)await (s.LoadAsync(typeof (One), oneB.Key));
			manyB = (Many)await (s.LoadAsync(typeof (Many), manyB.Key));
			await (s.DeleteAsync(manyA));
			await (s.DeleteAsync(oneA));
			await (s.DeleteAsync(manyB));
			await (s.DeleteAsync(oneB));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task ManyToOneAsync()
		{
			ISession s = OpenSession();
			One one = new One();
			await (s.SaveAsync(one));
			one.Value = "yada";
			Many many = new Many();
			many.One = one;
			await (s.SaveAsync(many));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			one = (One)await (s.LoadAsync(typeof (One), one.Key));
			int countManies = one.Manies.Count;
			s.Close();
			s = OpenSession();
			many = (Many)await (s.LoadAsync(typeof (Many), many.Key));
			Assert.IsNotNull(many.One, "many-to-one assoc");
			await (s.DeleteAsync(many.One));
			await (s.DeleteAsync(many));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task SaveDeleteAsync()
		{
			ISession s = OpenSession();
			Foo f = new Foo();
			await (s.SaveAsync(f));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			await (s.DeleteAsync(await (s.LoadAsync(typeof (Foo), f.Key))));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task ProxyArrayAsync()
		{
			ISession s = OpenSession();
			GlarchProxy g = new Glarch();
			Glarch g1 = new Glarch();
			Glarch g2 = new Glarch();
			g.ProxyArray = new GlarchProxy[]{g1, g2};
			Glarch g3 = new Glarch();
			await (s.SaveAsync(g3));
			g2.ProxyArray = new GlarchProxy[]{null, g3, g};
			g.ProxySet = new HashSet<GlarchProxy>{g1, g2};
			await (s.SaveAsync(g));
			await (s.SaveAsync(g1));
			await (s.SaveAsync(g2));
			object id = s.GetIdentifier(g);
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			g = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), id));
			Assert.AreEqual(2, g.ProxyArray.Length, "array of proxies");
			Assert.IsNotNull(g.ProxyArray[0], "array of proxies");
			Assert.IsNull(g.ProxyArray[1].ProxyArray[0], "deferred load test");
			Assert.AreEqual(g, g.ProxyArray[1].ProxyArray[2], "deferred load test");
			Assert.AreEqual(2, g.ProxySet.Count, "set of proxies");
			IEnumerator enumer = (await (s.CreateQuery("from g in class NHibernate.DomainModel.Glarch").EnumerableAsync())).GetEnumerator();
			while (enumer.MoveNext())
			{
				await (s.DeleteAsync(enumer.Current));
			}

			await (s.FlushAsync());
			s.Disconnect();
			// serialize the session.
			Stream stream = new MemoryStream();
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, s);
			// close the original session
			s.Close();
			// deserialize the session
			stream.Position = 0;
			s = (ISession)formatter.Deserialize(stream);
			stream.Close();
			s.Close();
		}

		[Test]
		public async Task CacheAsync()
		{
			NHibernate.DomainModel.Immutable im = new NHibernate.DomainModel.Immutable();
			using (ISession s = OpenSession())
			{
				await (s.SaveAsync(im));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				await (s.LoadAsync(im, im.Id));
			}

			using (ISession s = OpenSession())
			{
				await (s.LoadAsync(im, im.Id));
				NHibernate.DomainModel.Immutable imFromFind = (NHibernate.DomainModel.Immutable)(await ((await (s.CreateQuery("from im in class Immutable where im = ?").SetEntityAsync(0, im))).ListAsync()))[0];
				NHibernate.DomainModel.Immutable imFromLoad = (NHibernate.DomainModel.Immutable)await (s.LoadAsync(typeof (NHibernate.DomainModel.Immutable), im.Id));
				Assert.IsTrue(im == imFromFind, "cached object identity from Find ");
				Assert.IsTrue(im == imFromLoad, "cached object identity from Load ");
			}

			// Clean up the immutable. Need to do this using direct SQL, since ISession
			// refuses to delete immutable objects.
			using (ISession s = OpenSession())
			{
				DbConnection connection = s.Connection;
				using (DbCommand command = connection.CreateCommand())
				{
					command.CommandText = "delete from immut";
					await (command.ExecuteNonQueryAsync());
				}
			}
		}

		[Test]
		public async Task FindLoadAsync()
		{
			ISession s = OpenSession();
			FooProxy foo = new Foo();
			await (s.SaveAsync(foo));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			foo = (FooProxy)(await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo").ListAsync()))[0];
			FooProxy foo2 = (FooProxy)await (s.LoadAsync(typeof (Foo), foo.Key));
			Assert.AreSame(foo, foo2, "find returns same object as load");
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			foo2 = (FooProxy)await (s.LoadAsync(typeof (Foo), foo.Key));
			foo = (FooProxy)(await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo").ListAsync()))[0];
			Assert.AreSame(foo2, foo, "find returns same object as load");
			await (s.DeleteAsync("from foo in class NHibernate.DomainModel.Foo"));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task RefreshAsync()
		{
			ISession s = OpenSession();
			Foo foo = new Foo();
			await (s.SaveAsync(foo));
			await (s.FlushAsync());
			DbCommand cmd = s.Connection.CreateCommand();
			cmd.CommandText = "update " + Dialect.QuoteForTableName("foos") + " set long_ = -3";
			await (cmd.ExecuteNonQueryAsync());
			await (s.RefreshAsync(foo));
			Assert.AreEqual((long)-3, foo.Long);
			Assert.AreEqual(LockMode.Read, s.GetCurrentLockMode(foo));
			await (s.RefreshAsync(foo, LockMode.Upgrade));
			Assert.AreEqual(LockMode.Upgrade, s.GetCurrentLockMode(foo));
			await (s.DeleteAsync(foo));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task RefreshTransientAsync()
		{
			ISession s = OpenSession();
			Foo foo = new Foo();
			await (s.SaveAsync(foo));
			await (s.FlushAsync());
			/* 
			Commented to have same behavior of H3.2 (test named FooBarTest.testRefresh())
			s.Close(); 
			s = OpenSession();
			btw using close and open a new session more than Transient the entity will be detached.
			*/
			DbCommand cmd = s.Connection.CreateCommand();
			cmd.CommandText = "update " + Dialect.QuoteForTableName("foos") + " set long_ = -3";
			await (cmd.ExecuteNonQueryAsync());
			await (s.RefreshAsync(foo));
			Assert.AreEqual(-3L, foo.Long);
			await (s.DeleteAsync(foo));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task AutoFlushAsync()
		{
			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();
			FooProxy foo = new Foo();
			await (s.SaveAsync(foo));
			Assert.AreEqual(1, (await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo").ListAsync())).Count, "autoflush inserted row");
			foo.Char = 'X';
			Assert.AreEqual(1, (await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo where foo.Char='X'").ListAsync())).Count, "autflush updated row");
			await (txn.CommitAsync());
			s.Close();
			s = OpenSession();
			txn = s.BeginTransaction();
			foo = (FooProxy)await (s.LoadAsync(typeof (Foo), foo.Key));
			if (Dialect.SupportsSubSelects)
			{
				foo.Bytes = GetBytes("osama");
				Assert.AreEqual(1, (await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo where 111 in elements(foo.Bytes)").ListAsync())).Count, "autoflush collection update");
				foo.Bytes[0] = 69;
				Assert.AreEqual(1, (await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo where 69 in elements(foo.Bytes)").ListAsync())).Count, "autoflush collection update");
			}

			await (s.DeleteAsync(foo));
			Assert.AreEqual(0, (await (s.CreateQuery("from foo in class NHibernate.DomainModel.Foo").ListAsync())).Count, "autoflush delete");
			await (txn.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task VetoAsync()
		{
			ISession s = OpenSession();
			Vetoer v = new Vetoer();
			await (s.SaveAsync(v));
			object id = await (s.SaveAsync(v));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			await (s.UpdateAsync(v, id));
			await (s.UpdateAsync(v, id));
			await (s.DeleteAsync(v));
			await (s.DeleteAsync(v));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task SerializableTypeAsync()
		{
			ISession s = OpenSession();
			Vetoer v = new Vetoer();
			v.Strings = new string[]{"foo", "bar", "baz"};
			await (s.SaveAsync(v));
			object id = await (s.SaveAsync(v));
			v.Strings[1] = "osama";
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			v = (Vetoer)await (s.LoadAsync(typeof (Vetoer), id));
			Assert.AreEqual("osama", v.Strings[1], "serializable type");
			await (s.DeleteAsync(v));
			await (s.DeleteAsync(v));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task AutoFlushCollectionsAsync()
		{
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			Baz baz = new Baz();
			baz.SetDefaults();
			await (s.SaveAsync(baz));
			await (tx.CommitAsync());
			s.Close();
			s = OpenSession();
			tx = s.BeginTransaction();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			baz.StringArray[0] = "bark";
			IEnumerator e;
			e = (await (s.CreateQuery("select elements(baz.StringArray) from baz in class NHibernate.DomainModel.Baz").EnumerableAsync())).GetEnumerator();
			bool found = false;
			while (e.MoveNext())
			{
				if ("bark".Equals(e.Current))
				{
					found = true;
				}
			}

			Assert.IsTrue(found);
			baz.StringArray = null;
			e = (await (s.CreateQuery("select distinct elements(baz.StringArray) from baz in class NHibernate.DomainModel.Baz").EnumerableAsync())).GetEnumerator();
			Assert.IsFalse(e.MoveNext());
			baz.StringArray = new string[]{"foo", "bar"};
			e = (await (s.CreateQuery("select elements(baz.StringArray) from baz in class NHibernate.DomainModel.Baz").EnumerableAsync())).GetEnumerator();
			Assert.IsTrue(e.MoveNext());
			Foo foo = new Foo();
			await (s.SaveAsync(foo));
			await (s.FlushAsync());
			baz.FooArray = new Foo[]{foo};
			e = (await (s.CreateQuery("select foo from baz in class NHibernate.DomainModel.Baz, foo in elements(baz.FooArray)").EnumerableAsync())).GetEnumerator();
			found = false;
			while (e.MoveNext())
			{
				if (foo == e.Current)
				{
					found = true;
				}
			}

			Assert.IsTrue(found);
			baz.FooArray[0] = null;
			e = (await (s.CreateQuery("select foo from baz in class NHibernate.DomainModel.Baz, foo in elements(baz.FooArray)").EnumerableAsync())).GetEnumerator();
			Assert.IsFalse(e.MoveNext());
			baz.FooArray[0] = foo;
			e = (await (s.CreateQuery("select elements(baz.FooArray) from baz in class NHibernate.DomainModel.Baz").EnumerableAsync())).GetEnumerator();
			Assert.IsTrue(e.MoveNext());
			if (Dialect.SupportsSubSelects && !(Dialect is FirebirdDialect))
			{
				baz.FooArray[0] = null;
				e = (await ((await (s.CreateQuery("from baz in class NHibernate.DomainModel.Baz where ? in elements(baz.FooArray)").SetEntityAsync(0, foo))).EnumerableAsync())).GetEnumerator();
				Assert.IsFalse(e.MoveNext());
				baz.FooArray[0] = foo;
				e = (await (s.CreateQuery("select foo from foo in class NHibernate.DomainModel.Foo where foo in " + "(select elt from baz in class NHibernate.DomainModel.Baz, elt in elements(baz.FooArray))").EnumerableAsync())).GetEnumerator();
				Assert.IsTrue(e.MoveNext());
			}

			await (s.DeleteAsync(foo));
			await (s.DeleteAsync(baz));
			await (tx.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task UserProvidedConnectionAsync()
		{
			IConnectionProvider prov = ConnectionProviderFactory.NewConnectionProvider(cfg.Properties);
			ISession s = sessions.OpenSession(await (prov.GetConnectionAsync()));
			ITransaction tx = s.BeginTransaction();
			await (s.CreateQuery("from foo in class NHibernate.DomainModel.Fo").ListAsync());
			await (tx.CommitAsync());
			DbConnection c = s.Disconnect();
			Assert.IsNotNull(c);
			s.Reconnect(c);
			tx = s.BeginTransaction();
			await (s.CreateQuery("from foo in class NHibernate.DomainModel.Fo").ListAsync());
			await (tx.CommitAsync());
			Assert.AreSame(c, s.Close());
			c.Close();
		}

		[Test]
		public async Task CachedCollectionAsync()
		{
			ISession s = OpenSession();
			Baz baz = new Baz();
			baz.SetDefaults();
			await (s.SaveAsync(baz));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			((FooComponent)baz.TopComponents[0]).Count = 99;
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			Assert.AreEqual(99, ((FooComponent)baz.TopComponents[0]).Count);
			await (s.DeleteAsync(baz));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task ComplicatedQueryAsync()
		{
			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();
			Foo foo = new Foo();
			object id = await (s.SaveAsync(foo));
			Assert.IsNotNull(id);
			Qux q = new Qux("q");
			foo.Dependent.Qux = q;
			await (s.SaveAsync(q));
			q.Foo.String = "foo2";
			IEnumerator enumer = (await (s.CreateQuery("from foo in class Foo where foo.Dependent.Qux.Foo.String = 'foo2'").EnumerableAsync())).GetEnumerator();
			Assert.IsTrue(enumer.MoveNext());
			await (s.DeleteAsync(foo));
			await (txn.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task LoadAfterDeleteAsync()
		{
			ISession s = OpenSession();
			Foo foo = new Foo();
			object id = await (s.SaveAsync(foo));
			await (s.FlushAsync());
			await (s.DeleteAsync(foo));
			bool err = false;
			try
			{
				await (s.LoadAsync(typeof (Foo), id));
			}
			//catch (ObjectDeletedException ode) Changed to have same behavior of H3.2
			catch (ObjectNotFoundException ode)
			{
				Assert.IsNotNull(ode); //getting ride of 'ode' is never used compile warning
				err = true;
			}

			Assert.IsTrue(err);
			await (s.FlushAsync());
			err = false;
			try
			{
				bool proxyBoolean = ((FooProxy)await (s.LoadAsync(typeof (Foo), id))).Boolean;
			}
			catch (ObjectNotFoundException lie)
			{
				// Proxy initialization which failed because the object was not found
				// now throws ONFE instead of LazyInitializationException
				Assert.IsNotNull(lie); //getting ride of 'lie' is never used compile warning
				err = true;
			}

			Assert.IsTrue(err);
			Fo fo = Fo.NewFo();
			id = FumTestAsync.FumKey("abc"); //yuck!
			await (s.SaveAsync(fo, id));
			await (s.FlushAsync());
			await (s.DeleteAsync(fo));
			err = false;
			try
			{
				await (s.LoadAsync(typeof (Fo), id));
			}
			//catch (ObjectDeletedException ode) Changed to have same behavior of H3.2
			catch (ObjectNotFoundException ode)
			{
				Assert.IsNotNull(ode); //getting ride of 'ode' is never used compile warning
				err = true;
			}

			Assert.IsTrue(err);
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task ObjectTypeAsync()
		{
			object gid;
			using (ISession s = OpenSession())
			{
				GlarchProxy g = new Glarch();
				Foo foo = new Foo();
				g.Any = foo;
				gid = await (s.SaveAsync(g));
				await (s.SaveAsync(foo));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				GlarchProxy g = (GlarchProxy)await (s.LoadAsync(typeof (Glarch), gid));
				Assert.IsNotNull(g.Any);
				Assert.IsTrue(g.Any is FooProxy);
				await (s.DeleteAsync(g.Any));
				await (s.DeleteAsync(g));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task AnyAsync()
		{
			ISession s = OpenSession();
			One one = new One();
			BarProxy foo = new Bar();
			foo.Object = one;
			object fid = await (s.SaveAsync(foo));
			object oid = one.Key;
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			IList list = await (s.CreateQuery("from Bar bar where bar.Object.id = ? and bar.Object.class = ?").SetParameter(0, oid, NHibernateUtil.Int64).SetParameter(1, typeof (One).FullName, NHibernateUtil.ClassMetaType).ListAsync());
			Assert.AreEqual(1, list.Count);
			// this is a little different from h2.0.3 because the full type is stored, not
			// just the class name.
			list = await (s.CreateQuery("select one from One one, Bar bar where bar.Object.id = one.id and bar.Object.class LIKE 'NHibernate.DomainModel.One%'").ListAsync());
			Assert.AreEqual(1, list.Count);
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			foo = (BarProxy)await (s.LoadAsync(typeof (Foo), fid));
			Assert.IsNotNull(foo);
			Assert.IsTrue(foo.Object is One);
			Assert.AreEqual(oid, s.GetIdentifier(foo.Object));
			await (s.DeleteAsync(foo));
			await (s.DeleteAsync(foo.Object));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task EmbeddedCompositeIDAsync()
		{
			ISession s = OpenSession();
			Location l = new Location();
			l.CountryCode = "AU";
			l.Description = "foo bar";
			l.Locale = CultureInfo.CreateSpecificCulture("en-AU");
			l.StreetName = "Brunswick Rd";
			l.StreetNumber = 300;
			l.City = "Melbourne";
			await (s.SaveAsync(l));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			s.FlushMode = FlushMode.Never;
			l = (Location)(await (s.CreateQuery("from l in class Location where l.CountryCode = 'AU' and l.Description='foo bar'").ListAsync()))[0];
			Assert.AreEqual("AU", l.CountryCode);
			Assert.AreEqual("Melbourne", l.City);
			Assert.AreEqual(CultureInfo.CreateSpecificCulture("en-AU"), l.Locale);
			s.Close();
			s = OpenSession();
			l.Description = "sick're";
			await (s.UpdateAsync(l));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			l = new Location();
			l.CountryCode = "AU";
			l.Description = "foo bar";
			l.Locale = CultureInfo.CreateSpecificCulture("en-US");
			l.StreetName = "Brunswick Rd";
			l.StreetNumber = 300;
			l.City = "Melbourne";
			Assert.AreSame(l, await (s.LoadAsync(typeof (Location), l)));
			Assert.AreEqual(CultureInfo.CreateSpecificCulture("en-AU"), l.Locale);
			await (s.DeleteAsync(l));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task AutosaveChildrenAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Baz baz = new Baz();
			baz.CascadingBars = new HashSet<BarProxy>();
			await (s.SaveAsync(baz));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			baz.CascadingBars.Add(new Bar());
			baz.CascadingBars.Add(new Bar());
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			Assert.AreEqual(2, baz.CascadingBars.Count);
			IEnumerator enumer = baz.CascadingBars.GetEnumerator();
			Assert.IsTrue(enumer.MoveNext());
			Assert.IsNotNull(enumer.Current);
			baz.CascadingBars.Clear(); // test all-delete-orphan
			await (s.FlushAsync());
			Assert.AreEqual(0, (await (s.CreateQuery("from Bar bar").ListAsync())).Count);
			await (s.DeleteAsync(baz));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task OrphanDeleteAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Baz baz = new Baz();
			IDictionary bars = new Hashtable();
			bars.Add(new Bar(), new object ());
			bars.Add(new Bar(), new object ());
			bars.Add(new Bar(), new object ());
			await (s.SaveAsync(baz));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			baz = (Baz)await (s.LoadAsync(typeof (Baz), baz.Code));
			IEnumerator enumer = bars.GetEnumerator();
			enumer.MoveNext();
			bars.Remove(enumer.Current);
			await (s.DeleteAsync(baz));
			enumer.MoveNext();
			bars.Remove(enumer.Current);
			await (s.FlushAsync());
			Assert.AreEqual(0, (await (s.CreateQuery("from Bar bar").ListAsync())).Count);
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task TransientOrphanDeleteAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Baz baz = new Baz();
			var bars = new HashSet<BarProxy>{new Bar(), new Bar(), new Bar()};
			baz.CascadingBars = bars;
			IList<Foo> foos = new List<Foo>();
			foos.Add(new Foo());
			foos.Add(new Foo());
			baz.FooBag = foos;
			await (s.SaveAsync(baz));
			IEnumerator enumer = new JoinedEnumerable(new IEnumerable[]{foos, bars}).GetEnumerator();
			while (enumer.MoveNext())
			{
				FooComponent cmp = ((Foo)enumer.Current).Component;
				await (s.DeleteAsync(cmp.Glarch));
				cmp.Glarch = null;
			}

			await (t.CommitAsync());
			s.Close();
			var enumerBar = bars.GetEnumerator();
			enumerBar.MoveNext();
			bars.Remove(enumerBar.Current);
			foos.RemoveAt(1);
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(baz));
			Assert.AreEqual(2, (await (s.CreateQuery("from Bar bar").ListAsync())).Count);
			Assert.AreEqual(3, (await (s.CreateQuery("from Foo foo").ListAsync())).Count);
			await (t.CommitAsync());
			s.Close();
			foos.RemoveAt(0);
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(baz));
			enumerBar = bars.GetEnumerator();
			enumerBar.MoveNext();
			bars.Remove(enumerBar.Current);
			await (s.DeleteAsync(baz));
			await (s.FlushAsync());
			Assert.AreEqual(0, (await (s.CreateQuery("from Foo foo").ListAsync())).Count);
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task ProxiesInCollectionsAsync()
		{
			ISession s = OpenSession();
			Baz baz = new Baz();
			Bar bar = new Bar();
			Bar bar2 = new Bar();
			await (s.SaveAsync(bar));
			object bar2id = await (s.SaveAsync(bar2));
			baz.FooArray = new Foo[]{bar, bar2};
			bar = new Bar();
			await (s.SaveAsync(bar));
			baz.FooSet = new HashSet<FooProxy>{bar};
			baz.CascadingBars = new HashSet<BarProxy>{new Bar(), new Bar()};
			var list = new List<Foo>();
			list.Add(new Foo());
			baz.FooBag = list;
			object id = await (s.SaveAsync(baz));
			IEnumerator enumer = baz.CascadingBars.GetEnumerator();
			enumer.MoveNext();
			object bid = ((Bar)enumer.Current).Key;
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			BarProxy barprox = (BarProxy)await (s.LoadAsync(typeof (Bar), bid));
			BarProxy bar2prox = (BarProxy)await (s.LoadAsync(typeof (Bar), bar2id));
			Assert.IsTrue(bar2prox is INHibernateProxy);
			Assert.IsTrue(barprox is INHibernateProxy);
			baz = (Baz)await (s.LoadAsync(typeof (Baz), id));
			enumer = baz.CascadingBars.GetEnumerator();
			enumer.MoveNext();
			BarProxy b1 = (BarProxy)enumer.Current;
			enumer.MoveNext();
			BarProxy b2 = (BarProxy)enumer.Current;
			Assert.IsTrue((b1 == barprox && !(b2 is INHibernateProxy)) || (b2 == barprox && !(b1 is INHibernateProxy))); //one-to-many
			Assert.IsTrue(baz.FooArray[0] is INHibernateProxy); //many-to-many
			Assert.AreEqual(bar2prox, baz.FooArray[1]);
			if (sessions.Settings.IsOuterJoinFetchEnabled)
			{
				enumer = baz.FooBag.GetEnumerator();
				enumer.MoveNext();
				Assert.IsFalse(enumer.Current is INHibernateProxy); // many-to-many outer-join="true"
			}

			enumer = baz.FooSet.GetEnumerator();
			enumer.MoveNext();
			Assert.IsFalse(enumer.Current is INHibernateProxy); //one-to-many
			await (s.DeleteAsync("from o in class Baz"));
			await (s.DeleteAsync("from o in class Foo"));
			await (s.FlushAsync());
			s.Close();
		}

		// Not ported - testService() - not applicable to NHibernate
		[Test]
		public async Task PSCacheAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction txn = s.BeginTransaction())
				{
					for (int i = 0; i < 10; i++)
					{
						await (s.SaveAsync(new Foo()));
					}

					IQuery q = s.CreateQuery("from f in class Foo");
					q.SetMaxResults(2);
					q.SetFirstResult(5);
					Assert.AreEqual(2, (await (q.ListAsync())).Count);
					q = s.CreateQuery("from f in class Foo");
					Assert.AreEqual(10, (await (q.ListAsync())).Count);
					Assert.AreEqual(10, (await (q.ListAsync())).Count);
					q.SetMaxResults(3);
					q.SetFirstResult(3);
					Assert.AreEqual(3, (await (q.ListAsync())).Count);
					q = s.CreateQuery("from f in class Foo");
					Assert.AreEqual(10, (await (q.ListAsync())).Count);
					await (txn.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction txn = s.BeginTransaction())
				{
					IQuery q = s.CreateQuery("from f in class Foo");
					Assert.AreEqual(10, (await (q.ListAsync())).Count);
					q.SetMaxResults(5);
					Assert.AreEqual(5, (await (q.ListAsync())).Count);
					await (s.DeleteAsync("from f in class Foo"));
					await (txn.CommitAsync());
				}
		}

		[Test]
		public async Task FormulaAsync()
		{
			Foo foo = new Foo();
			ISession s = OpenSession();
			object id = await (s.SaveAsync(foo));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			foo = (Foo)(await (s.CreateQuery("from Foo as f where f.id = ?").SetParameter(0, id, NHibernateUtil.String).ListAsync()))[0];
			Assert.AreEqual(4, foo.Formula, "should be 2x 'Int' property that is defaulted to 2");
			await (s.DeleteAsync(foo));
			await (s.FlushAsync());
			s.Close();
		}

		/// <summary>
		/// This test verifies that the AddAll() method works
		/// correctly for a persistent Set.
		/// </summary>
		[Test]
		public async Task AddAllAsync()
		{
			using (ISession s = OpenSession())
			{
				Foo foo1 = new Foo();
				await (s.SaveAsync(foo1));
				Foo foo2 = new Foo();
				await (s.SaveAsync(foo2));
				Foo foo3 = new Foo();
				await (s.SaveAsync(foo3));
				Baz baz = new Baz();
				baz.FooSet = new HashSet<FooProxy>{foo1};
				await (s.SaveAsync(baz));
				Assert.AreEqual(1, baz.FooSet.Count);
				var foos = new List<FooProxy>{foo2, foo3};
				baz.FooSet.UnionWith(foos);
				Assert.AreEqual(3, baz.FooSet.Count);
				await (s.FlushAsync());
				// Clean up
				foreach (Foo foo in baz.FooSet)
				{
					await (s.DeleteAsync(foo));
				}

				await (s.DeleteAsync(baz));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task CopyAsync()
		{
			Baz baz = new Baz();
			baz.SetDefaults();
			using (ISession s = OpenSession())
			{
				Baz persistentBaz = new Baz();
				await (s.SaveAsync(persistentBaz));
				await (s.FlushAsync());
				baz.Code = persistentBaz.Code;
			}

			using (ISession s = OpenSession())
			{
				Baz persistentBaz = await (s.GetAsync(typeof (Baz), baz.Code)) as Baz;
				Baz copiedBaz = s.Merge(baz);
				Assert.AreSame(persistentBaz, copiedBaz);
				await (s.DeleteAsync(persistentBaz));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task ParameterInHavingClauseAsync()
		{
			using (ISession s = OpenSession())
			{
				await (s.CreateQuery("select f.id from Foo f group by f.id having count(f.id) >= ?").SetInt32(0, 0).ListAsync());
			}
		}

		// It's possible that this test only works on MS SQL Server. If somebody complains about
		// the test not working on their DB, I'll put an if around the code to only run on MS SQL.
		[Test]
		public async Task ParameterInOrderByClauseAsync()
		{
			using (ISession s = OpenSession())
			{
				await (s.CreateQuery("from Foo as foo order by case ? when 0 then foo.id else foo.id end").SetInt32(0, 0).ListAsync());
			}
		}
	}
}
#endif