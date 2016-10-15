﻿#if NET_4_5
using System.Collections;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Dialect;
using NHibernate.DomainModel;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Legacy
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class ParentChildTestAsync : TestCaseAsync
	{
		protected override IList Mappings
		{
			get
			{
				return new string[]{"FooBar.hbm.xml", "Baz.hbm.xml", "Qux.hbm.xml", "Glarch.hbm.xml", "Fum.hbm.xml", "Fumm.hbm.xml", "Fo.hbm.xml", "One.hbm.xml", "Many.hbm.xml", "Immutable.hbm.xml", "Fee.hbm.xml", "Vetoer.hbm.xml", "Holder.hbm.xml", "ParentChild.hbm.xml", "Simple.hbm.xml", "Container.hbm.xml", "Circular.hbm.xml", "Stuff.hbm.xml"};
			}
		}

		[Test]
		public async Task ReplicateAsync()
		{
			ISession s = OpenSession();
			Container baz = new Container();
			Contained f = new Contained();
			IList<Container> list = new List<Container>();
			list.Add(baz);
			f.Bag = list;
			IList<Contained> list2 = new List<Contained>();
			list2.Add(f);
			baz.Bag = list2;
			await (s.SaveAsync(f));
			await (s.SaveAsync(baz));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			await (s.ReplicateAsync(baz, ReplicationMode.Overwrite));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			await (s.ReplicateAsync(baz, ReplicationMode.Ignore));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			await (s.DeleteAsync(baz));
			await (s.DeleteAsync(f));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task QueryOneToOneAsync()
		{
			ISession s;
			ITransaction t;
			object id;
			using (s = OpenSession())
				using (t = s.BeginTransaction())
				{
					id = await (s.SaveAsync(new Parent()));
					Assert.AreEqual(1, (await (s.CreateQuery("from Parent p left join fetch p.Child").ListAsync())).Count);
					await (t.CommitAsync());
					s.Close();
				}

			using (s = OpenSession())
				using (t = s.BeginTransaction())
				{
					Parent p = (Parent)await (s.CreateQuery("from Parent p left join fetch p.Child").UniqueResultAsync());
					Assert.IsNull(p.Child);
					await (s.CreateQuery("from Parent p join p.Child c where c.X > 0").ListAsync());
					await (s.CreateQuery("from Child c join c.Parent p where p.X > 0").ListAsync());
					await (t.CommitAsync());
					s.Close();
				}

			using (s = OpenSession())
				using (t = s.BeginTransaction())
				{
					await (s.DeleteAsync(await (s.GetAsync(typeof (Parent), id))));
					await (t.CommitAsync());
					s.Close();
				}
		}

		[Test]
		public async Task ProxyReuseAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			FooProxy foo = new Foo();
			FooProxy foo2 = new Foo();
			object id = await (s.SaveAsync(foo));
			object id2 = await (s.SaveAsync(foo2));
			foo2.Int = 1234567;
			foo.Int = 1234;
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			foo = (FooProxy)await (s.LoadAsync(typeof (Foo), id));
			foo2 = (FooProxy)await (s.LoadAsync(typeof (Foo), id2));
			Assert.IsFalse(NHibernateUtil.IsInitialized(foo));
			await (NHibernateUtil.InitializeAsync(foo2));
			await (NHibernateUtil.InitializeAsync(foo));
			Assert.AreEqual(3, foo.Component.ImportantDates.Length);
			Assert.AreEqual(3, foo2.Component.ImportantDates.Length);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			foo.Float = 1.2f;
			foo2.Float = 1.3f;
			foo2.Dependent.Key = null;
			foo2.Component.Subcomponent.Fee.Key = null;
			Assert.IsFalse(foo2.Key.Equals(id));
			await (s.SaveAsync(foo, "xyzid"));
			await (s.UpdateAsync(foo2, id)); //intentionally id, not id2!
			Assert.AreEqual(id, foo2.Key);
			Assert.AreEqual(1234567, foo2.Int);
			Assert.AreEqual("xyzid", foo.Key);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			foo = (FooProxy)await (s.LoadAsync(typeof (Foo), id));
			Assert.AreEqual(1234567, foo.Int);
			Assert.AreEqual(3, foo.Component.ImportantDates.Length);
			await (s.DeleteAsync(foo));
			await (s.DeleteAsync(await (s.GetAsync(typeof (Foo), id2))));
			await (s.DeleteAsync(await (s.GetAsync(typeof (Foo), "xyzid"))));
			Assert.AreEqual(3, await (s.DeleteAsync("from System.Object")));
			await (t.CommitAsync());
			s.Close();
			string feekey = foo.Dependent.Key;
			s = OpenSession();
			t = s.BeginTransaction();
			foo.Component.Glarch = null; //no id property!
			await (s.ReplicateAsync(foo, ReplicationMode.Overwrite));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			Foo refoo = (Foo)await (s.GetAsync(typeof (Foo), id));
			Assert.AreEqual(feekey, refoo.Dependent.Key);
			await (s.DeleteAsync(refoo));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task ComplexCriteriaAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Baz baz = new Baz();
			await (s.SaveAsync(baz));
			baz.SetDefaults();
			IDictionary<char, GlarchProxy> topGlarchez = new Dictionary<char, GlarchProxy>();
			baz.TopGlarchez = topGlarchez;
			Glarch g1 = new Glarch();
			g1.Name = "g1";
			await (s.SaveAsync(g1));
			Glarch g2 = new Glarch();
			g2.Name = "g2";
			await (s.SaveAsync(g2));
			g1.ProxyArray = new GlarchProxy[]{g2};
			topGlarchez['1'] = g1;
			topGlarchez['2'] = g2;
			Foo foo1 = new Foo();
			Foo foo2 = new Foo();
			await (s.SaveAsync(foo1));
			await (s.SaveAsync(foo2));
			baz.FooSet.Add(foo1);
			baz.FooSet.Add(foo2);
			baz.FooArray = new FooProxy[]{foo1};
			LockMode lockMode = (Dialect is DB2Dialect) ? LockMode.Read : LockMode.Upgrade;
			ICriteria crit = s.CreateCriteria(typeof (Baz));
			crit.CreateCriteria("TopGlarchez").Add(Expression.IsNotNull("Name")).CreateCriteria("ProxyArray").Add(Expression.EqProperty("Name", "Name")).Add(Expression.Eq("Name", "g2")).Add(Expression.Gt("X", -666));
			crit.CreateCriteria("FooSet").Add(Expression.IsNull("Null")).Add(Expression.Eq("String", "a string")).Add(Expression.Lt("Integer", -665));
			crit.CreateCriteria("FooArray").Add(Expression.Eq("String", "a string")).SetLockMode(lockMode);
			IList list = await (crit.ListAsync());
			Assert.AreEqual(2, list.Count);
			await (s.CreateCriteria(typeof (Glarch)).SetLockMode(LockMode.Upgrade).ListAsync());
			await (s.CreateCriteria(typeof (Glarch)).SetLockMode(CriteriaSpecification.RootAlias, LockMode.Upgrade).ListAsync());
			g2.Name = null;
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			crit = s.CreateCriteria(typeof (Baz)).SetLockMode(lockMode);
			crit.CreateCriteria("TopGlarchez").Add(Expression.Gt("X", -666));
			crit.CreateCriteria("FooSet").Add(Expression.IsNull("Null"));
			list = await (crit.ListAsync());
			Assert.AreEqual(4, list.Count);
			baz = (Baz)await (crit.UniqueResultAsync());
			Assert.IsTrue(NHibernateUtil.IsInitialized(baz.TopGlarchez)); //cos it is nonlazy
			Assert.IsFalse(NHibernateUtil.IsInitialized(baz.FooSet));
			//list = s.CreateCriteria(typeof( Baz ))
			//	.createCriteria("fooSet.foo.component.glarch")
			//		.Add( Expression.eq("name", "xxx") )
			//	.Add( Expression.eq("fooSet.foo.component.glarch.name", "xxx") )
			//	.list();
			//assertTrue( list.size()==0 );
			list = await (s.CreateCriteria(typeof (Baz)).CreateCriteria("FooSet").CreateCriteria("TheFoo").CreateCriteria("Component.Glarch").Add(Expression.Eq("Name", "xxx")).ListAsync());
			Assert.AreEqual(0, list.Count);
			list = await (s.CreateCriteria(typeof (Baz)).CreateAlias("FooSet", "foo").CreateAlias("foo.TheFoo", "foo2").SetLockMode("foo2", lockMode).Add(Expression.IsNull("foo2.Component.Glarch")).CreateCriteria("foo2.Component.Glarch").Add(Expression.Eq("Name", "xxx")).ListAsync());
			Assert.AreEqual(0, list.Count);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			crit = s.CreateCriteria(typeof (Baz));
			crit.CreateCriteria("TopGlarchez").Add(Expression.IsNotNull("Name"));
			crit.CreateCriteria("FooSet").Add(Expression.IsNull("Null"));
			list = await (crit.ListAsync());
			Assert.AreEqual(2, list.Count);
			baz = (Baz)await (crit.UniqueResultAsync());
			Assert.IsTrue(NHibernateUtil.IsInitialized(baz.TopGlarchez)); //cos it is nonlazy
			Assert.IsFalse(NHibernateUtil.IsInitialized(baz.FooSet));
			await (s.DeleteAsync("from Glarch g"));
			await (s.DeleteAsync(await (s.GetAsync(typeof (Foo), foo1.Key))));
			await (s.DeleteAsync(await (s.GetAsync(typeof (Foo), foo2.Key))));
			await (s.DeleteAsync(baz));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task ClassWhereAsync()
		{
			if (Dialect is PostgreSQLDialect)
				return;
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Baz baz = new Baz();
			baz.Parts = new List<Part>();
			Part p1 = new Part();
			p1.Description = "xyz";
			Part p2 = new Part();
			p2.Description = "abc";
			baz.Parts.Add(p1);
			baz.Parts.Add(p2);
			await (s.SaveAsync(baz));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			Assert.AreEqual(1, (await (s.CreateCriteria(typeof (Part)).ListAsync())).Count);
			//there is a where condition on Part mapping
			Assert.AreEqual(1, (await (s.CreateCriteria(typeof (Part)).Add(Expression.Eq("Id", p1.Id)).ListAsync())).Count);
			Assert.AreEqual(1, (await (s.CreateQuery("from Part").ListAsync())).Count);
			Assert.AreEqual(2, (await (s.CreateQuery("from Baz baz join baz.Parts").ListAsync())).Count);
			baz = (Baz)await (s.CreateCriteria(typeof (Baz)).UniqueResultAsync());
			Assert.AreEqual(2, (await ((await (s.CreateFilterAsync(baz.Parts, ""))).ListAsync())).Count);
			//assertTrue( baz.getParts().size()==1 );
			await (s.DeleteAsync("from Part"));
			await (s.DeleteAsync(baz));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CollectionQueryAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Simple s1 = new Simple();
			s1.Name = "s";
			s1.Count = 0;
			Simple s2 = new Simple();
			s2.Count = 2;
			Simple s3 = new Simple();
			s3.Count = 3;
			await (s.SaveAsync(s1, (long)1));
			await (s.SaveAsync(s2, (long)2));
			await (s.SaveAsync(s3, (long)3));
			Container c = new Container();
			IList<Simple> l = new List<Simple>();
			l.Add(s1);
			l.Add(s3);
			l.Add(s2);
			c.OneToMany = l;
			l = new List<Simple>();
			l.Add(s1);
			l.Add(null);
			l.Add(s2);
			c.ManyToMany = l;
			await (s.SaveAsync(c));
			Assert.AreEqual(1, (await (s.CreateQuery("select c from c in class ContainerX, s in class Simple where c.OneToMany[2] = s").ListAsync())).Count);
			Assert.AreEqual(1, (await (s.CreateQuery("select c from c in class ContainerX, s in class Simple where c.ManyToMany[2] = s").ListAsync())).Count);
			Assert.AreEqual(1, (await (s.CreateQuery("select c from c in class ContainerX, s in class Simple where s = c.OneToMany[2]").ListAsync())).Count);
			Assert.AreEqual(1, (await (s.CreateQuery("select c from c in class ContainerX, s in class Simple where s = c.ManyToMany[2]").ListAsync())).Count);
			Assert.AreEqual(1, (await (s.CreateQuery("select c from c in class ContainerX where c.OneToMany[0].Name = 's'").ListAsync())).Count);
			Assert.AreEqual(1, (await (s.CreateQuery("select c from c in class ContainerX where c.ManyToMany[0].Name = 's'").ListAsync())).Count);
			Assert.AreEqual(1, (await (s.CreateQuery("select c from c in class ContainerX where 's' = c.OneToMany[2 - 2].Name").ListAsync())).Count);
			Assert.AreEqual(1, (await (s.CreateQuery("select c from c in class ContainerX where 's' = c.ManyToMany[(3+1)/4-1].Name").ListAsync())).Count);
			if (Dialect.SupportsSubSelects)
			{
				Assert.AreEqual(1, (await (s.CreateQuery("select c from c in class ContainerX where c.ManyToMany[ c.ManyToMany.maxIndex ].Count = 2").ListAsync())).Count);
				Assert.AreEqual(1, (await (s.CreateQuery("select c from c in class ContainerX where c.ManyToMany[ maxindex(c.ManyToMany) ].Count = 2").ListAsync())).Count);
			}

			Assert.AreEqual(1, (await (s.CreateQuery("select c from c in class ContainerX where c.OneToMany[ c.ManyToMany[0].Count ].Name = 's'").ListAsync())).Count);
			Assert.AreEqual(1, (await (s.CreateQuery("select c from c in class ContainerX where c.ManyToMany[ c.OneToMany[0].Count ].Name = 's'").ListAsync())).Count);
			await (s.DeleteAsync(c));
			await (s.DeleteAsync(s1));
			await (s.DeleteAsync(s2));
			await (s.DeleteAsync(s3));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task ParentChildAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Parent p = new Parent();
			Child c = new Child();
			c.Parent = p;
			p.Child = c;
			await (s.SaveAsync(p));
			await (s.SaveAsync(c));
			await (t.CommitAsync());
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Child)await (s.LoadAsync(typeof (Child), c.Id));
			p = c.Parent;
			Assert.IsNotNull(p, "1-1 parent");
			c.Count = 32;
			p.Count = 66;
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Child)await (s.LoadAsync(typeof (Child), c.Id));
			p = c.Parent;
			Assert.AreEqual(66, p.Count, "1-1 update");
			Assert.AreEqual(32, c.Count, "1-1 update");
			Assert.AreEqual(1, (await (s.CreateQuery("from c in class NHibernate.DomainModel.Child where c.Parent.Count=66").ListAsync())).Count, "1-1 query");
			Assert.AreEqual(2, ((object[])(await (s.CreateQuery("from Parent p join p.Child c where p.Count=66").ListAsync()))[0]).Length, "1-1 query");
			await (s.CreateQuery("select c, c.Parent from c in class NHibernate.DomainModel.Child order by c.Parent.Count").ListAsync());
			await (s.CreateQuery("select c, c.Parent from c in class NHibernate.DomainModel.Child where c.Parent.Count=66 order by c.Parent.Count").ListAsync());
			await (s.CreateQuery("select c, c.Parent, c.Parent.Count from c in class NHibernate.DomainModel.Child order by c.Parent.Count").EnumerableAsync());
			Assert.AreEqual(1, (await (s.CreateQuery("FROM p in CLASS NHibernate.DomainModel.Parent WHERE p.Count=?").SetInt32(0, 66).ListAsync())).Count, "1-1 query");
			await (s.DeleteAsync(c));
			await (s.DeleteAsync(p));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task ParentNullChildAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Parent p = new Parent();
			await (s.SaveAsync(p));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			p = (Parent)await (s.LoadAsync(typeof (Parent), p.Id));
			Assert.IsNull(p.Child);
			p.Count = 66;
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			p = (Parent)await (s.LoadAsync(typeof (Parent), p.Id));
			Assert.AreEqual(66, p.Count, "null 1-1 update");
			Assert.IsNull(p.Child);
			await (s.DeleteAsync(p));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task ManyToManyAsync()
		{
			// if( dialect is Dialect.HSQLDialect) return;
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Container c = new Container();
			c.ManyToMany = new List<Simple>();
			c.Bag = new List<Contained>();
			Simple s1 = new Simple();
			Simple s2 = new Simple();
			s1.Count = 123;
			s2.Count = 654;
			Contained c1 = new Contained();
			c1.Bag = new List<Container>();
			c1.Bag.Add(c);
			c.Bag.Add(c1);
			c.ManyToMany.Add(s1);
			c.ManyToMany.Add(s2);
			object cid = await (s.SaveAsync(c));
			await (s.SaveAsync(s1, (long)12));
			await (s.SaveAsync(s2, (long)-1));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Container)await (s.LoadAsync(typeof (Container), cid));
			Assert.AreEqual(1, c.Bag.Count);
			Assert.AreEqual(2, c.ManyToMany.Count);
			foreach (object obj in c.Bag)
			{
				c1 = (Contained)obj;
				break;
			}

			c.Bag.Remove(c1);
			c1.Bag.Remove(c);
			Assert.IsNotNull(c.ManyToMany[0]);
			c.ManyToMany.RemoveAt(0);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Container)await (s.LoadAsync(typeof (Container), cid));
			Assert.AreEqual(0, c.Bag.Count);
			Assert.AreEqual(1, c.ManyToMany.Count);
			c1 = (Contained)await (s.LoadAsync(typeof (Contained), c1.Id));
			Assert.AreEqual(0, c1.Bag.Count);
			Assert.AreEqual(1, await (s.DeleteAsync("from c in class ContainerX")));
			Assert.AreEqual(1, await (s.DeleteAsync("from c in class Contained")));
			Assert.AreEqual(2, await (s.DeleteAsync("from s in class Simple")));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task ContainerAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Container c = new Container();
			Simple x = new Simple();
			x.Count = 123;
			Simple y = new Simple();
			y.Count = 456;
			await (s.SaveAsync(x, (long)1));
			await (s.SaveAsync(y, (long)0));
			IList<Simple> o2m = new List<Simple>();
			o2m.Add(x);
			o2m.Add(null);
			o2m.Add(y);
			IList<Simple> m2m = new List<Simple>();
			m2m.Add(x);
			m2m.Add(null);
			m2m.Add(y);
			c.OneToMany = o2m;
			c.ManyToMany = m2m;
			IList<Container.ContainerInnerClass> comps = new List<Container.ContainerInnerClass>();
			Container.ContainerInnerClass ccic = new Container.ContainerInnerClass();
			ccic.Name = "foo";
			ccic.Simple = x;
			comps.Add(ccic);
			comps.Add(null);
			ccic = new Container.ContainerInnerClass();
			ccic.Name = "bar";
			ccic.Simple = y;
			comps.Add(ccic);
			var compos = new HashSet<Container.ContainerInnerClass>{ccic};
			c.Composites = compos;
			c.Components = comps;
			One one = new One();
			Many many = new Many();
			one.Manies = new HashSet<Many>{many};
			many.One = one;
			ccic.Many = many;
			ccic.One = one;
			await (s.SaveAsync(one));
			await (s.SaveAsync(many));
			await (s.SaveAsync(c));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Container)await (s.LoadAsync(typeof (Container), c.Id));
			ccic = (Container.ContainerInnerClass)c.Components[2];
			Assert.AreEqual(ccic.One, ccic.Many.One);
			Assert.AreEqual(3, c.Components.Count);
			Assert.AreEqual(1, c.Composites.Count);
			Assert.AreEqual(3, c.OneToMany.Count);
			Assert.AreEqual(3, c.ManyToMany.Count);
			for (int i = 0; i < 3; i++)
			{
				Assert.AreEqual(c.ManyToMany[i], c.OneToMany[i]);
			}

			Simple o1 = c.OneToMany[0];
			Simple o2 = c.OneToMany[2];
			c.OneToMany.RemoveAt(2);
			c.OneToMany[0] = o2;
			c.OneToMany[1] = o1;
			Container.ContainerInnerClass comp = c.Components[2];
			c.Components.RemoveAt(2);
			c.Components[0] = comp;
			c.ManyToMany[0] = c.ManyToMany[2];
			c.Composites.Add(comp);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Container)await (s.LoadAsync(typeof (Container), c.Id));
			Assert.AreEqual(1, c.Components.Count); //WAS: 2 - h2.0.3 comment
			Assert.AreEqual(2, c.Composites.Count);
			Assert.AreEqual(2, c.OneToMany.Count);
			Assert.AreEqual(3, c.ManyToMany.Count);
			Assert.IsNotNull(c.OneToMany[0]);
			Assert.IsNotNull(c.OneToMany[1]);
			((Container.ContainerInnerClass)c.Components[0]).Name = "a different name";
			IEnumerator enumer = c.Composites.GetEnumerator();
			enumer.MoveNext();
			((Container.ContainerInnerClass)enumer.Current).Name = "once again";
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Container)await (s.LoadAsync(typeof (Container), c.Id));
			Assert.AreEqual(1, c.Components.Count); //WAS: 2 -> h2.0.3 comment
			Assert.AreEqual(2, c.Composites.Count);
			Assert.AreEqual("a different name", ((Container.ContainerInnerClass)c.Components[0]).Name);
			enumer = c.Composites.GetEnumerator();
			bool found = false;
			while (enumer.MoveNext())
			{
				if (((Container.ContainerInnerClass)enumer.Current).Name.Equals("once again"))
				{
					found = true;
				}
			}

			Assert.IsTrue(found);
			c.OneToMany.Clear();
			c.ManyToMany.Clear();
			c.Composites.Clear();
			c.Components.Clear();
			await (s.DeleteAsync("from s in class Simple"));
			await (s.DeleteAsync("from m in class Many"));
			await (s.DeleteAsync("from o in class One"));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Container)await (s.LoadAsync(typeof (Container), c.Id));
			Assert.AreEqual(0, c.Components.Count);
			Assert.AreEqual(0, c.Composites.Count);
			Assert.AreEqual(0, c.OneToMany.Count);
			Assert.AreEqual(0, c.ManyToMany.Count);
			await (s.DeleteAsync(c));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CascadeCompositeElementsAsync()
		{
			Container c = new Container();
			c.Cascades = new List<Container.ContainerInnerClass>();
			Container.ContainerInnerClass cic = new Container.ContainerInnerClass();
			cic.Many = new Many();
			cic.One = new One();
			c.Cascades.Add(cic);
			ISession s = OpenSession();
			await (s.SaveAsync(c));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			foreach (Container obj in await (s.CreateQuery("from c in class ContainerX").EnumerableAsync()))
			{
				c = obj;
				break;
			}

			foreach (Container.ContainerInnerClass obj in c.Cascades)
			{
				cic = obj;
				break;
			}

			Assert.IsNotNull(cic.Many);
			Assert.IsNotNull(cic.One);
			Assert.AreEqual(1, c.Cascades.Count);
			await (s.DeleteAsync(c));
			await (s.FlushAsync());
			s.Close();
			c = new Container();
			s = OpenSession();
			await (s.SaveAsync(c));
			c.Cascades = new List<Container.ContainerInnerClass>();
			cic = new Container.ContainerInnerClass();
			cic.Many = new Many();
			cic.One = new One();
			c.Cascades.Add(cic);
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			foreach (Container obj in await (s.CreateQuery("from c in class ContainerX").EnumerableAsync()))
			{
				c = obj;
			}

			foreach (Container.ContainerInnerClass obj in c.Cascades)
			{
				cic = obj;
			}

			Assert.IsNotNull(cic.Many);
			Assert.IsNotNull(cic.One);
			Assert.AreEqual(1, c.Cascades.Count);
			await (s.DeleteAsync(c));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task BagAsync()
		{
			//if( dialect is Dialect.HSQLDialect ) return;
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Container c = new Container();
			Contained c1 = new Contained();
			Contained c2 = new Contained();
			c.Bag = new List<Contained>();
			c.Bag.Add(c1);
			c.Bag.Add(c2);
			c1.Bag.Add(c);
			c2.Bag.Add(c);
			await (s.SaveAsync(c));
			c.Bag.Add(c2);
			c2.Bag.Add(c);
			c.LazyBag.Add(c1);
			c1.LazyBag.Add(c);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Container)(await (s.CreateQuery("from c in class ContainerX").ListAsync()))[0];
			Assert.AreEqual(1, c.LazyBag.Count);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Container)(await (s.CreateQuery("from c in class ContainerX").ListAsync()))[0];
			Contained c3 = new Contained();
			// commented out in h2.0.3 also
			//c.Bag.Add(c3);
			//c3.Bag.Add(c);
			c.LazyBag.Add(c3);
			c3.LazyBag.Add(c);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Container)(await (s.CreateQuery("from c in class ContainerX").ListAsync()))[0];
			Contained c4 = new Contained();
			c.LazyBag.Add(c4);
			c4.LazyBag.Add(c);
			Assert.AreEqual(3, c.LazyBag.Count); //forces initialization
			// s.Save(c4); commented in h2.0.3 also
			await (t.CommitAsync());
			s.Close();
			// new test code in here to catch when a lazy bag has an addition then
			// is flushed and then an operation that causes it to read from the db
			// occurs - this used to cause the element in Add(element) to be in there
			// twice and throw off the count by 1 (or by however many additions were 
			// made before the flush
			s = OpenSession();
			c = (Container)(await (s.CreateQuery("from c in class ContainerX").ListAsync()))[0];
			Contained c5 = new Contained();
			c.LazyBag.Add(c5);
			c5.LazyBag.Add(c);
			// this causes the additions to be written to the db - now verify
			// the additions list was cleared and the count returns correctly
			await (s.FlushAsync());
			Assert.AreEqual(4, c.LazyBag.Count, "correct additions clearing after flush");
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Container)(await (s.CreateQuery("from c in class ContainerX").ListAsync()))[0];
			int j = 0;
			foreach (object obj in c.Bag)
			{
				Assert.IsNotNull(obj);
				j++;
			}

			Assert.AreEqual(3, j);
			// this used to be 3 - but since I added an item in the test above for flush
			// I increased it to 4
			Assert.AreEqual(4, c.LazyBag.Count);
			await (s.DeleteAsync(c));
			c.Bag.Remove(c2);
			j = 0;
			foreach (object obj in c.Bag)
			{
				j++;
				await (s.DeleteAsync(obj));
			}

			Assert.AreEqual(2, j);
			await (s.DeleteAsync(await (s.LoadAsync(typeof (Contained), c5.Id))));
			await (s.DeleteAsync(await (s.LoadAsync(typeof (Contained), c4.Id))));
			await (s.DeleteAsync(await (s.LoadAsync(typeof (Contained), c3.Id))));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CircularCascadeAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Circular c = new Circular();
			c.Clazz = typeof (Circular);
			c.Other = new Circular();
			c.Other.Other = new Circular();
			c.Other.Other.Other = c;
			c.AnyEntity = c.Other;
			string id = (string)await (s.SaveAsync(c));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Circular)await (s.LoadAsync(typeof (Circular), id));
			c.Other.Other.Clazz = typeof (Foo);
			await (t.CommitAsync());
			s.Close();
			c.Other.Clazz = typeof (Qux);
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.SaveOrUpdateAsync(c));
			await (t.CommitAsync());
			s.Close();
			c.Other.Other.Clazz = typeof (Bar);
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.SaveOrUpdateAsync(c));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Circular)await (s.LoadAsync(typeof (Circular), id));
			Assert.AreEqual(typeof (Bar), c.Other.Other.Clazz);
			Assert.AreEqual(typeof (Qux), c.Other.Clazz);
			Assert.AreEqual(c, c.Other.Other.Other);
			Assert.AreEqual(c.Other, c.AnyEntity);
			Assert.AreEqual(3, await (s.DeleteAsync("from o in class Universe")));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task DeleteEmptyAsync()
		{
			ISession s = OpenSession();
			Assert.AreEqual(0, await (s.DeleteAsync("from s in class Simple")));
			Assert.AreEqual(0, await (s.DeleteAsync("from o in class Universe")));
			s.Close();
		}

		[Test]
		public async Task LockingAsync()
		{
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			Simple s1 = new Simple();
			s1.Count = 1;
			Simple s2 = new Simple();
			s2.Count = 2;
			Simple s3 = new Simple();
			s3.Count = 3;
			Simple s4 = new Simple();
			s4.Count = 4;
			await (s.SaveAsync(s1, (long)1));
			await (s.SaveAsync(s2, (long)2));
			await (s.SaveAsync(s3, (long)3));
			await (s.SaveAsync(s4, (long)4));
			Assert.AreEqual(LockMode.Write, s.GetCurrentLockMode(s1));
			await (tx.CommitAsync());
			s.Close();
			s = OpenSession();
			tx = s.BeginTransaction();
			s1 = (Simple)await (s.LoadAsync(typeof (Simple), (long)1, LockMode.None));
			Assert.AreEqual(LockMode.Read, s.GetCurrentLockMode(s1));
			s2 = (Simple)await (s.LoadAsync(typeof (Simple), (long)2, LockMode.Read));
			Assert.AreEqual(LockMode.Read, s.GetCurrentLockMode(s2));
			s3 = (Simple)await (s.LoadAsync(typeof (Simple), (long)3, LockMode.Upgrade));
			Assert.AreEqual(LockMode.Upgrade, s.GetCurrentLockMode(s3));
			s4 = (Simple)await (s.LoadAsync(typeof (Simple), (long)4, LockMode.UpgradeNoWait));
			Assert.AreEqual(LockMode.UpgradeNoWait, s.GetCurrentLockMode(s4));
			s1 = (Simple)await (s.LoadAsync(typeof (Simple), (long)1, LockMode.Upgrade)); //upgrade
			Assert.AreEqual(LockMode.Upgrade, s.GetCurrentLockMode(s1));
			s2 = (Simple)await (s.LoadAsync(typeof (Simple), (long)2, LockMode.None));
			Assert.AreEqual(LockMode.Read, s.GetCurrentLockMode(s2));
			s3 = (Simple)await (s.LoadAsync(typeof (Simple), (long)3, LockMode.Read));
			Assert.AreEqual(LockMode.Upgrade, s.GetCurrentLockMode(s3));
			s4 = (Simple)await (s.LoadAsync(typeof (Simple), (long)4, LockMode.Upgrade));
			Assert.AreEqual(LockMode.UpgradeNoWait, s.GetCurrentLockMode(s4));
			await (s.LockAsync(s2, LockMode.Upgrade)); //upgrade
			Assert.AreEqual(LockMode.Upgrade, s.GetCurrentLockMode(s2));
			await (s.LockAsync(s3, LockMode.Upgrade));
			Assert.AreEqual(LockMode.Upgrade, s.GetCurrentLockMode(s3));
			await (s.LockAsync(s1, LockMode.UpgradeNoWait));
			await (s.LockAsync(s4, LockMode.None));
			Assert.AreEqual(LockMode.UpgradeNoWait, s.GetCurrentLockMode(s4));
			await (tx.CommitAsync());
			tx = s.BeginTransaction();
			Assert.AreEqual(LockMode.None, s.GetCurrentLockMode(s3));
			Assert.AreEqual(LockMode.None, s.GetCurrentLockMode(s1));
			Assert.AreEqual(LockMode.None, s.GetCurrentLockMode(s2));
			Assert.AreEqual(LockMode.None, s.GetCurrentLockMode(s4));
			await (s.LockAsync(s1, LockMode.Read)); //upgrade
			Assert.AreEqual(LockMode.Read, s.GetCurrentLockMode(s1));
			await (s.LockAsync(s2, LockMode.Upgrade)); //upgrade
			Assert.AreEqual(LockMode.Upgrade, s.GetCurrentLockMode(s2));
			await (s.LockAsync(s3, LockMode.UpgradeNoWait)); //upgrade
			Assert.AreEqual(LockMode.UpgradeNoWait, s.GetCurrentLockMode(s3));
			await (s.LockAsync(s4, LockMode.None));
			Assert.AreEqual(LockMode.None, s.GetCurrentLockMode(s4));
			s4.Name = "s4";
			await (s.FlushAsync());
			Assert.AreEqual(LockMode.Write, s.GetCurrentLockMode(s4));
			await (tx.CommitAsync());
			tx = s.BeginTransaction();
			Assert.AreEqual(LockMode.None, s.GetCurrentLockMode(s3));
			Assert.AreEqual(LockMode.None, s.GetCurrentLockMode(s1));
			Assert.AreEqual(LockMode.None, s.GetCurrentLockMode(s2));
			Assert.AreEqual(LockMode.None, s.GetCurrentLockMode(s4));
			await (s.DeleteAsync(s1));
			await (s.DeleteAsync(s2));
			await (s.DeleteAsync(s3));
			await (s.DeleteAsync(s4));
			await (tx.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task ObjectTypeAsync()
		{
			ISession s = OpenSession();
			Parent g = new Parent();
			Foo foo = new Foo();
			g.Any = foo;
			await (s.SaveAsync(g));
			await (s.SaveAsync(foo));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			g = (Parent)await (s.LoadAsync(typeof (Parent), g.Id));
			Assert.IsNotNull(g.Any);
			Assert.IsTrue(g.Any is FooProxy);
			await (s.DeleteAsync(g.Any));
			await (s.DeleteAsync(g));
			await (s.FlushAsync());
			s.Close();
		}
	}
}
#endif