﻿#if NET_4_5
using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Linq;
using NHibernate.DomainModel.Northwind.Entities;
using NUnit.Framework;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.Linq
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class MiscellaneousTextFixtureAsync : LinqTestCaseAsync
	{
		[Category("COUNT/SUM/MIN/MAX/AVG")]
		[Test(Description = "This sample uses Count to find the number of Orders placed before yesterday in the database.")]
		public async Task CountWithWhereClauseAsync()
		{
			var q =
				from o in db.Orders
				where o.OrderDate <= DateTime.Today.AddDays(-1)select o;
			var count = await (q.CountAsync());
			Console.WriteLine(count);
		}

		[Category("From NHUser list")]
		[Test(Description = "Telerik grid example, http://www.telerik.com/community/forums/aspnet-mvc/grid/grid-and-nhibernate-linq.aspx")]
		public async Task TelerikGridWhereClauseAsync()
		{
			Expression<Func<Customer, bool>> filter = c => c.ContactName.ToLower().StartsWith("a");
			IQueryable<Customer> value = db.Customers;
			var results = await (value.Where(filter).ToListAsync());
			Assert.IsFalse(results.Where(c => !c.ContactName.ToLower().StartsWith("a")).Any());
		}

		[Category("From NHUser list")]
		[Test(Description = "Predicated count on a child list")]
		public async Task PredicatedCountOnChildListAsync()
		{
			var results = await ((
				from c in db.Customers
				select new
				{
				c.ContactName, Count = c.Orders.Count(o => o.Employee.EmployeeId == 4)}

			).ToListAsync());
			Assert.AreEqual(91, results.Count());
			Assert.AreEqual(2, results.Where(c => c.ContactName == "Maria Anders").Single().Count);
			Assert.AreEqual(4, results.Where(c => c.ContactName == "Thomas Hardy").Single().Count);
			Assert.AreEqual(0, results.Where(c => c.ContactName == "Elizabeth Brown").Single().Count);
		}

		[Category("From NHUser list")]
		[Test(Description = "Reference an outer object in a predicate")]
		public async Task ReferenceToOuterAsync()
		{
			var results =
				from c in db.Customers
				where c.Orders.Any(o => o.ShippedTo == c.CompanyName)select c;
			Assert.AreEqual(85, await (results.CountAsync()));
		}

		[Category("Paging")]
		[Test(Description = "This sample uses a where clause and the Skip and Take operators to select " + "the second, third and fourth pages of products")]
		public async Task TriplePageSelectionAsync()
		{
			IQueryable<Product> q = (
				from p in db.Products
				where p.ProductId > 1
				orderby p.ProductId
				select p);
			IQueryable<Product> page2 = q.Skip(5).Take(5);
			IQueryable<Product> page3 = q.Skip(10).Take(5);
			IQueryable<Product> page4 = q.Skip(15).Take(5);
			var firstResultOnPage2 = await (page2.FirstAsync());
			var firstResultOnPage3 = await (page3.FirstAsync());
			var firstResultOnPage4 = await (page4.FirstAsync());
			Assert.AreNotEqual(firstResultOnPage2.ProductId, firstResultOnPage3.ProductId);
			Assert.AreNotEqual(firstResultOnPage3.ProductId, firstResultOnPage4.ProductId);
			Assert.AreNotEqual(firstResultOnPage2.ProductId, firstResultOnPage4.ProductId);
		}

		[Test]
		public async Task SelectFromObjectAsync()
		{
			using (var s = OpenSession())
			{
				var hql = await (s.CreateQuery("from System.Object o").ListAsync());
				var r =
					from o in s.Query<object>()select o;
				var l = await (r.ToListAsync());
				Assert.AreEqual(hql.Count, l.Count);
			}
		}
	}
}
#endif
