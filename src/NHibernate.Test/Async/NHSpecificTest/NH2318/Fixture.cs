#if NET_4_5
using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NUnit.Framework;
using NHibernate.Linq;
using System.Linq;
using NHibernate.Linq.Functions;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH2318
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class Fixture : BugTestCase
	{
		private async Task AddObjectsAsync()
		{
			ISession s = OpenSession();
			try
			{
				await (s.SaveAsync(new A{Name = "first"}));
				await (s.SaveAsync(new A{Name = "second"}));
				await (s.SaveAsync(new A{Name = "aba"}));
				await (s.FlushAsync());
			}
			finally
			{
				s.Close();
			}
		}

		[Test]
		public async Task CriteriaTrimFunctionsWithParametersAsync()
		{
			await (AddObjectsAsync());
			ISession s = OpenSession();
			try
			{
				ICriteria criteria = s.CreateCriteria(typeof (A)).Add(Restrictions.Eq(Projections.SqlFunction("trim", NHibernateUtil.String, Projections.Constant("f"), Projections.SqlProjection("from", null, null), // Silly hack to get "from" as a second argument.
 Projections.Property("Name")), "irst"));
				IList<A> items = await (criteria.ListAsync<A>());
				Assert.AreEqual(1, items.Count);
				Assert.AreEqual("first", items[0].Name);
			}
			finally
			{
				s.Close();
			}
		}

		[Test]
		public async Task LinqTrimFunctionsWithParametersAsync()
		{
			await (AddObjectsAsync());
			ISession s = OpenSession();
			try
			{
				IList<A> items = s.Query<A>().Where(a => a.Name.TrimLeading("f").TrimTrailing("t") == "irs").ToList();
				Assert.AreEqual(1, items.Count);
				Assert.AreEqual("first", items[0].Name);
				string trimString = "a";
				items = s.Query<A>().Where(a => a.Name.TrimLeading(trimString).TrimTrailing(trimString) == "b").ToList();
				Assert.AreEqual(1, items.Count);
				Assert.AreEqual("aba", items[0].Name);
			}
			finally
			{
				s.Close();
			}
		}

		[Test]
		public async Task HqlTrimFunctionsWithParametersAsync()
		{
			await (AddObjectsAsync());
			ISession s = OpenSession();
			try
			{
				IList<A> items = await (s.CreateQuery("from A a where a.Name = :p0 or a.Name <> :p0 order by a.Name").SetParameter("p0", "first").ListAsync<A>());
				Assert.AreEqual(3, items.Count);
				Assert.AreEqual("aba", items[0].Name);
				Assert.AreEqual("first", items[1].Name);
				Assert.AreEqual("second", items[2].Name);
				items = await (s.CreateQuery("from A a where a.Name = ? or a.Name <> ? order by a.Name").SetParameter(0, "first").SetParameter(1, "first").ListAsync<A>());
				Assert.AreEqual(3, items.Count);
				Assert.AreEqual("aba", items[0].Name);
				Assert.AreEqual("first", items[1].Name);
				Assert.AreEqual("second", items[2].Name);
				items = await (s.CreateQuery("from A a where TRIM(LEADING :p0 FROM a.Name) = 'irst'").SetParameter("p0", "f").ListAsync<A>());
				Assert.AreEqual(1, items.Count);
				Assert.AreEqual("first", items[0].Name);
				items = await (s.CreateQuery("from A a where TRIM(TRAILING :p0 FROM TRIM(LEADING :p1 FROM a.Name)) = 'irs'").SetParameter("p0", "t").SetParameter("p1", "f").ListAsync<A>());
				Assert.AreEqual(1, items.Count);
				Assert.AreEqual("first", items[0].Name);
				items = await (s.CreateQuery("from A a where TRIM(TRAILING :p0 FROM TRIM(LEADING :p0 FROM a.Name)) = 'b'").SetParameter("p0", "a").ListAsync<A>());
				Assert.AreEqual(1, items.Count);
				Assert.AreEqual("aba", items[0].Name);
				items = await (s.CreateQuery("from A a where TRIM(TRAILING :p0 FROM a.Name) = 'firs'").SetParameter("p0", "t").ListAsync<A>());
				Assert.AreEqual(1, items.Count);
				Assert.AreEqual("first", items[0].Name);
				items = await (s.CreateQuery("from A a where TRIM(LEADING ? FROM a.Name) = 'irst'").SetParameter(0, "f").ListAsync<A>());
				Assert.AreEqual(1, items.Count);
				Assert.AreEqual("first", items[0].Name);
				items = await (s.CreateQuery("from A a where TRIM(TRAILING ? FROM a.Name) = 'firs'").SetParameter(0, "t").ListAsync<A>());
				Assert.AreEqual(1, items.Count);
				Assert.AreEqual("first", items[0].Name);
				items = await (s.CreateQuery("from A a where TRIM(TRAILING ? FROM TRIM(LEADING ? FROM a.Name)) = 'irs'").SetParameter(0, "t").SetParameter(1, "f").ListAsync<A>());
				Assert.AreEqual(1, items.Count);
				Assert.AreEqual("first", items[0].Name);
			}
			finally
			{
				s.Close();
			}
		}
	}
}
#endif
