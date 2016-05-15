#if NET_4_5
using System;
using System.Collections;
using System.Collections.Generic;
using NHibernate.Dialect;
using NHibernate.DomainModel;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Legacy
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class ABCTest : TestCase
	{
		[Test]
		public async Task SubselectAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					B b = new B();
					IDictionary<string, string> map = new Dictionary<string, string>();
					map["a"] = "a";
					map["b"] = "b";
					b.Map = map;
					await (s.SaveAsync(b));
					await (t.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					B b = (B)await (s.CreateQuery("from B").UniqueResultAsync());
					await (t.CommitAsync());
				}

			if (Dialect is FirebirdDialect)
			{
				await (ExecuteStatementAsync("delete from Map"));
				await (ExecuteStatementAsync("delete from A"));
			}
			else
			{
				using (ISession s = OpenSession())
					using (ITransaction t = s.BeginTransaction())
					{
						await (s.DeleteAsync("from B"));
						await (t.CommitAsync());
					}
			}
		}

		[Test]
		public async Task SubclassingAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			C1 c1 = new C1();
			D d = new D();
			d.Amount = 213.34f;
			// id used to be a increment
			c1.Id = 1;
			c1.Address = "foo bar";
			c1.Count = 23432;
			c1.Name = "c1";
			c1.D = d;
			await (s.SaveAsync(c1));
			d.Id = c1.Id;
			await (s.SaveAsync(d));
			Assert.IsTrue((await (s.CreateQuery("from c in class C2 where 1=1 or 1=1").ListAsync())).Count == 0);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c1 = (C1)await (s.LoadAsync(typeof (A), c1.Id));
			Assert.IsTrue(c1.Address.Equals("foo bar") && (c1.Count == 23432) && c1.Name.Equals("c1") && c1.D.Amount > 213.3f);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c1 = (C1)await (s.LoadAsync(typeof (B), c1.Id));
			Assert.IsTrue(c1.Address.Equals("foo bar") && (c1.Count == 23432) && c1.Name.Equals("c1") && c1.D.Amount > 213.3f);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c1 = (C1)await (s.LoadAsync(typeof (C1), c1.Id));
			Assert.IsTrue(c1.Address.Equals("foo bar") && (c1.Count == 23432) && c1.Name.Equals("c1") && c1.D.Amount > 213.3f);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.CreateQuery("from b in class B").ListAsync());
			await (t.CommitAsync());
			s.Close();
			// need to clean up the objects created by this test or Subselect() will fail
			// because there are rows in the table.  There must be some difference in the order
			// that NUnit and JUnit run their tests.
			s = OpenSession();
			t = s.BeginTransaction();
			IList aList = await (s.CreateQuery("from A").ListAsync());
			IList dList = await (s.CreateQuery("from D").ListAsync());
			foreach (A aToDelete in aList)
			{
				await (s.DeleteAsync(aToDelete));
			}

			foreach (D dToDelete in dList)
			{
				await (s.DeleteAsync(dToDelete));
			}

			await (t.CommitAsync());
			s.Close();
		}
	}
}
#endif
