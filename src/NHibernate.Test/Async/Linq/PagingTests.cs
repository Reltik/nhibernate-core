﻿#if NET_4_5
using System.Linq;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NUnit.Framework;
using NHibernate.Linq;
using System.Threading.Tasks;

namespace NHibernate.Test.Linq
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class PagingTestsAsync : LinqTestCaseAsync
	{
		[Test]
		public async Task PageBetweenProjectionsAsync()
		{
			// NH-3326
			var list = await (db.Products.Select(p => new
			{
			p.ProductId, p.Name
			}

			).Skip(5).Take(10).Select(a => new
			{
			a.Name, a.ProductId
			}

			).ToListAsync());
			Assert.That(list, Has.Count.EqualTo(10));
		}

		[Test]
		public async Task PageBetweenProjectionsReturningNestedAnonymousAsync()
		{
			// The important part in this query is that the outer select
			// grabs the entire element from the inner select, plus more.
			// NH-3326
			var list = await (db.Products.Select(p => new
			{
			p.ProductId, p.Name
			}

			).Skip(5).Take(10).Select(a => new
			{
			ExpandedElement = a, a.Name, a.ProductId
			}

			).ToListAsync());
			Assert.That(list, Has.Count.EqualTo(10));
		}

		[Test]
		public async Task PageBetweenProjectionsReturningNestedClassAsync()
		{
			// NH-3326
			var list = await (db.Products.Select(p => new ProductProjection{ProductId = p.ProductId, Name = p.Name}).Skip(5).Take(10).Select(a => new
			{
			ExpandedElement = a, a.Name, a.ProductId
			}

			).ToListAsync());
			Assert.That(list, Has.Count.EqualTo(10));
		}

		[Test]
		public async Task PageBetweenProjectionsReturningOrderedNestedAnonymousAsync()
		{
			// Variation of NH-3326 with order
			var list = await (db.Products.Select(p => new
			{
			p.ProductId, p.Name
			}

			).OrderBy(x => x.ProductId).Skip(5).Take(10).Select(a => new
			{
			ExpandedElement = a, a.Name, a.ProductId
			}

			).ToListAsync());
			Assert.That(list, Has.Count.EqualTo(10));
		}

		[Test]
		public async Task PageBetweenProjectionsReturningOrderedNestedClassAsync()
		{
			// Variation of NH-3326 with order
			var list = await (db.Products.Select(p => new ProductProjection{ProductId = p.ProductId, Name = p.Name}).OrderBy(x => x.ProductId).Skip(5).Take(10).Select(a => new
			{
			ExpandedElement = a, a.Name, a.ProductId
			}

			).ToListAsync());
			Assert.That(list, Has.Count.EqualTo(10));
		}

		[Test]
		public async Task PageBetweenProjectionsReturningOrderedConstrainedNestedAnonymousAsync()
		{
			// Variation of NH-3326 with where
			var list = await (db.Products.Select(p => new
			{
			p.ProductId, p.Name
			}

			).Where(p => p.ProductId > 0).OrderBy(x => x.ProductId).Skip(5).Take(10).Select(a => new
			{
			ExpandedElement = a, a.Name, a.ProductId
			}

			).ToListAsync());
			Assert.That(list, Has.Count.EqualTo(10));
		}

		[Test]
		public async Task PageBetweenProjectionsReturningOrderedConstrainedNestedClassAsync()
		{
			// Variation of NH-3326 with where
			var list = await (db.Products.Select(p => new ProductProjection{ProductId = p.ProductId, Name = p.Name}).Where(p => p.ProductId > 0).OrderBy(x => x.ProductId).Skip(5).Take(10).Select(a => new
			{
			ExpandedElement = a, a.Name, a.ProductId
			}

			).ToListAsync());
			Assert.That(list, Has.Count.EqualTo(10));
		}

		[Test, Ignore("Not supported")]
		public async Task PagedProductsWithOuterWhereClauseOrderedNestedAnonymousAsync()
		{
			// NH-2588 and NH-3326
			var inMemoryIds = (await (db.Products.ToListAsync())).OrderByDescending(x => x.ProductId).Select(p => new
			{
			p.ProductId, p.Name, p.UnitsInStock
			}

			).Skip(10).Take(20).Select(a => new
			{
			ExpandedElement = a, a.Name, a.ProductId
			}

			).Where(x => x.ProductId > 0).ToList();
			var ids = await (db.Products.OrderByDescending(x => x.ProductId).Select(p => new
			{
			p.ProductId, p.Name, p.UnitsInStock
			}

			).Skip(10).Take(20).Where(x => x.ProductId > 0).Select(a => new
			{
			ExpandedElement = a, a.Name, a.ProductId
			}

			).ToListAsync());
			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test, Ignore("Not supported")]
		public async Task PagedProductsWithOuterWhereClauseOrderedNestedAnonymousEquivalentAsync()
		{
			// NH-2588 and NH-3326
			var inMemoryIds = (await (db.Products.ToListAsync())).OrderByDescending(x => x.ProductId).Select(p => new
			{
			p.ProductId, p.Name, p.UnitsInStock
			}

			).Skip(10).Take(20).Select(a => new
			{
			ExpandedElement = a, a.Name, a.ProductId
			}

			).Where(x => x.ProductId > 0).ToList();
			var subquery = db.Products.OrderByDescending(x => x.ProductId).Select(p => new
			{
			p.ProductId, p.Name, p.UnitsInStock
			}

			).Skip(10).Take(20);
			var ids = await (db.Products.Select(p => new
			{
			p.ProductId, p.Name, p.UnitsInStock
			}

			).Where(x => subquery.Contains(x)).Where(x => x.ProductId > 0).Select(a => new
			{
			ExpandedElement = a, a.Name, a.ProductId
			}

			).ToListAsync());
			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test, Ignore("Not supported")]
		public async Task PagedProductsWithOuterWhereClauseOrderedNestedClassAsync()
		{
			// NH-2588 and NH-3326
			var inMemoryIds = (await (db.Products.ToListAsync())).OrderByDescending(x => x.ProductId).Select(p => new ProductProjection{ProductId = p.ProductId, Name = p.Name}).Skip(10).Take(20).Select(a => new
			{
			ExpandedElement = a, a.Name, a.ProductId
			}

			).Where(x => x.ProductId > 0).ToList();
			var ids = await (db.Products.OrderByDescending(x => x.ProductId).Select(p => new ProductProjection{ProductId = p.ProductId, Name = p.Name}).Skip(10).Take(20).Select(a => new
			{
			ExpandedElement = a, a.Name, a.ProductId
			}

			).Where(x => x.ProductId > 0).ToListAsync());
			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test, Ignore("Not supported")]
		public async Task PagedProductsWithOuterWhereClauseOrderedNestedClassEquivalentAsync()
		{
			// NH-2588 and NH-3326
			var inMemoryIds = (await (db.Products.ToListAsync())).OrderByDescending(x => x.ProductId).Select(p => new ProductProjection{ProductId = p.ProductId, Name = p.Name}).Skip(10).Take(20).Select(a => new
			{
			ExpandedElement = a, a.Name, a.ProductId
			}

			).Where(x => x.ProductId > 0).ToList();
			var subquery = db.Products.OrderByDescending(x => x.ProductId).Select(p => new ProductProjection{ProductId = p.ProductId, Name = p.Name}).Skip(10).Take(20);
			var ids = await (db.Products.Select(p => new ProductProjection{ProductId = p.ProductId, Name = p.Name}).Where(x => subquery.Contains(x)).Where(x => x.ProductId > 0).Select(a => new
			{
			ExpandedElement = a, a.Name, a.ProductId
			}

			).ToListAsync());
			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task Customers1to5Async()
		{
			var q = (
				from c in db.Customers
				select c.CustomerId).Take(5);
			var query = await (q.ToListAsync());
			Assert.AreEqual(5, query.Count);
		}

		[Test]
		public async Task Customers11to20Async()
		{
			var query = await ((
				from c in db.Customers
				orderby c.CustomerId
				select c.CustomerId).Skip(10).Take(10).ToListAsync());
			Assert.AreEqual(query[0], "BSBEV");
			Assert.AreEqual(10, query.Count);
		}

		[Test]
		public async Task Customers11to20And21to30ShouldNoCacheQueryAsync()
		{
			var query = await ((
				from c in db.Customers
				orderby c.CustomerId
				select c.CustomerId).Skip(10).Take(10).ToListAsync());
			Assert.AreEqual(query[0], "BSBEV");
			Assert.AreEqual(10, query.Count);
			query = await ((
				from c in db.Customers
				orderby c.CustomerId
				select c.CustomerId).Skip(20).Take(10).ToListAsync());
			Assert.AreNotEqual(query[0], "BSBEV");
			Assert.AreEqual(10, query.Count);
			query = await ((
				from c in db.Customers
				orderby c.CustomerId
				select c.CustomerId).Skip(10).Take(20).ToListAsync());
			Assert.AreEqual(query[0], "BSBEV");
			Assert.AreEqual(20, query.Count);
		}

		[Test]
		[Ignore("Multiple Takes (or Skips) not handled correctly")]
		public async Task CustomersChainedTakeAsync()
		{
			var q = (
				from c in db.Customers
				orderby c.CustomerId
				select c.CustomerId).Take(5).Take(6);
			var query = await (q.ToListAsync());
			Assert.AreEqual(5, query.Count);
			Assert.AreEqual("ALFKI", query[0]);
			Assert.AreEqual("BLAUS", query[4]);
		}

		[Test]
		[Ignore("Multiple Takes (or Skips) not handled correctly")]
		public async Task CustomersChainedSkipAsync()
		{
			var q = (
				from c in db.Customers
				select c.CustomerId).Skip(10).Skip(5);
			var query = await (q.ToListAsync());
			Assert.AreEqual(query[0], "CONSH");
			Assert.AreEqual(76, query.Count);
		}

		[Test]
		[Ignore("Count with Skip or Take is incorrect (Skip / Take done on the query not the HQL, so get applied at the wrong point")]
		public async Task CountAfterTakeShouldReportTheCorrectNumberAsync()
		{
			var users = db.Customers.Skip(3).Take(10);
			Assert.AreEqual(10, await (users.CountAsync()));
		}

		[Test]
		public async Task OrderedPagedProductsWithOuterProjectionAsync()
		{
			//NH-3108
			var inMemoryIds = (await (db.Products.ToListAsync())).OrderBy(p => p.ProductId).Skip(10).Take(20).Select(p => p.ProductId).ToList();
			var ids = await (db.Products.OrderBy(p => p.ProductId).Skip(10).Take(20).Select(p => p.ProductId).ToListAsync());
			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task OrderedPagedProductsWithInnerProjectionAsync()
		{
			//NH-3108 (not failing)
			var inMemoryIds = (await (db.Products.ToListAsync())).OrderBy(p => p.ProductId).Select(p => p.ProductId).Skip(10).Take(20).ToList();
			var ids = await (db.Products.OrderBy(p => p.ProductId).Select(p => p.ProductId).Skip(10).Take(20).ToListAsync());
			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task DescendingOrderedPagedProductsWithOuterProjectionAsync()
		{
			//NH-3108
			var inMemoryIds = (await (db.Products.ToListAsync())).OrderByDescending(p => p.ProductId).Skip(10).Take(20).Select(p => p.ProductId).ToList();
			var ids = await (db.Products.OrderByDescending(p => p.ProductId).Skip(10).Take(20).Select(p => p.ProductId).ToListAsync());
			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task DescendingOrderedPagedProductsWithInnerProjectionAsync()
		{
			//NH-3108 (not failing)
			var inMemoryIds = (await (db.Products.ToListAsync())).OrderByDescending(p => p.ProductId).Select(p => p.ProductId).Skip(10).Take(20).ToList();
			var ids = await (db.Products.OrderByDescending(p => p.ProductId).Select(p => p.ProductId).Skip(10).Take(20).ToListAsync());
			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task PagedProductsWithOuterWhereClauseAsync()
		{
			if (Dialect is MySQLDialect)
				Assert.Ignore("MySQL does not support LIMIT in subqueries.");
			//NH-2588
			var inMemoryIds = (await (db.Products.ToListAsync())).OrderByDescending(x => x.ProductId).Skip(10).Take(20).Where(x => x.UnitsInStock > 0).ToList();
			var ids = await (db.Products.OrderByDescending(x => x.ProductId).Skip(10).Take(20).Where(x => x.UnitsInStock > 0).ToListAsync());
			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task PagedProductsWithOuterWhereClauseResortAsync()
		{
			if (Dialect is MySQLDialect)
				Assert.Ignore("MySQL does not support LIMIT in subqueries.");
			//NH-2588
			var inMemoryIds = (await (db.Products.ToListAsync())).OrderByDescending(x => x.ProductId).Skip(10).Take(20).Where(x => x.UnitsInStock > 0).OrderBy(x => x.Name).ToList();
			var ids = db.Products.OrderByDescending(x => x.ProductId).Skip(10).Take(20).Where(x => x.UnitsInStock > 0).OrderBy(x => x.Name).ToList();
			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task PagedProductsWithInnerAndOuterWhereClausesAsync()
		{
			if (Dialect is MySQLDialect)
				Assert.Ignore("MySQL does not support LIMIT in subqueries.");
			//NH-2588
			var inMemoryIds = (await (db.Products.ToListAsync())).Where(x => x.UnitsInStock < 100).OrderByDescending(x => x.ProductId).Skip(10).Take(20).Where(x => x.UnitsInStock > 0).OrderBy(x => x.Name).ToList();
			var ids = db.Products.Where(x => x.UnitsInStock < 100).OrderByDescending(x => x.ProductId).Skip(10).Take(20).Where(x => x.UnitsInStock > 0).OrderBy(x => x.Name).ToList();
			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task PagedProductsWithOuterWhereClauseEquivalentAsync()
		{
			if (Dialect is MySQLDialect)
				Assert.Ignore("MySQL does not support LIMIT in subqueries.");
			//NH-2588
			var inMemoryIds = (await (db.Products.ToListAsync())).OrderByDescending(x => x.ProductId).Skip(10).Take(20).Where(x => x.UnitsInStock > 0).ToList();
			var subquery = db.Products.OrderByDescending(x => x.ProductId).Skip(10).Take(20);
			var ids = db.Products.Where(x => subquery.Contains(x)).Where(x => x.UnitsInStock > 0).OrderByDescending(x => x.ProductId).ToList();
			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task PagedProductsWithOuterWhereClauseAndProjectionAsync()
		{
			if (Dialect is MySQLDialect)
				Assert.Ignore("MySQL does not support LIMIT in subqueries.");
			//NH-2588
			var inMemoryIds = (await (db.Products.ToListAsync())).OrderByDescending(x => x.ProductId).Skip(10).Take(20).Where(x => x.UnitsInStock > 0).Select(x => x.ProductId).ToList();
			var ids = await (db.Products.OrderByDescending(x => x.ProductId).Skip(10).Take(20).Where(x => x.UnitsInStock > 0).Select(x => x.ProductId).ToListAsync());
			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task PagedProductsWithOuterWhereClauseAndComplexProjectionAsync()
		{
			if (Dialect is MySQLDialect)
				Assert.Ignore("MySQL does not support LIMIT in subqueries.");
			//NH-2588
			var inMemoryIds = (await (db.Products.ToListAsync())).OrderByDescending(x => x.ProductId).Skip(10).Take(20).Where(x => x.UnitsInStock > 0).Select(x => new
			{
			x.ProductId
			}

			).ToList();
			var ids = await (db.Products.OrderByDescending(x => x.ProductId).Skip(10).Take(20).Where(x => x.UnitsInStock > 0).Select(x => new
			{
			x.ProductId
			}

			).ToListAsync());
			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}
	}
}
#endif