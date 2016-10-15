﻿#if NET_4_5
using System.Linq;
using NHibernate.Linq;
using NHibernate.DomainModel.Northwind.Entities;
using NUnit.Framework;
using System.Threading.Tasks;
using Exception = System.Exception;
using NHibernate.Util;

namespace NHibernate.Test.Linq
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class EagerLoadTestsAsync : LinqTestCaseAsync
	{
		[Test]
		public async Task CanSelectAndFetchHqlAsync()
		{
			//NH-3075
			var result = await (this.session.CreateQuery("select c from Order o left join o.Customer c left join fetch c.Orders").ListAsync<Customer>());
			session.Close();
			Assert.IsNotEmpty(result);
			Assert.IsTrue(NHibernateUtil.IsInitialized(result[0].Orders));
		}

		[Test]
		public async Task RelationshipsAreLazyLoadedByDefaultAsync()
		{
			var x = await (db.Customers.ToListAsync());
			session.Close();
			Assert.AreEqual(91, x.Count);
			Assert.IsFalse(NHibernateUtil.IsInitialized(x[0].Orders));
		}

		[Test]
		public async Task FetchWithWhereAsync()
		{
			// NH-2381 NH-2362
			await ((
				from p in session.Query<Product>().Fetch(a => a.Supplier)where p.ProductId == 1
				select p).ToListAsync());
		}

		[Test]
		public async Task FetchManyWithWhereAsync()
		{
			// NH-2381 NH-2362
			await ((
				from s in session.Query<Supplier>().FetchMany(a => a.Products)where s.SupplierId == 1
				select s).ToListAsync());
		}

		[Test]
		public async Task FetchAndThenFetchWithWhereAsync()
		{
			// NH-2362
			await ((
				from p in session.Query<User>().Fetch(a => a.Role).ThenFetch(a => a.Entity)where p.Id == 1
				select p).ToListAsync());
		}

		[Test]
		public async Task FetchAndThenFetchManyWithWhereAsync()
		{
			// NH-2362
			await ((
				from p in session.Query<Employee>().Fetch(a => a.Superior).ThenFetchMany(a => a.Orders)where p.EmployeeId == 1
				select p).ToListAsync());
		}

		[Test]
		public async Task FetchManyAndThenFetchWithWhereAsync()
		{
			// NH-2362
			await ((
				from s in session.Query<Supplier>().FetchMany(a => a.Products).ThenFetch(a => a.Category)where s.SupplierId == 1
				select s).ToListAsync());
		}

		[Test]
		public async Task FetchManyAndThenFetchManyWithWhereAsync()
		{
			// NH-2362
			await ((
				from s in session.Query<Supplier>().FetchMany(a => a.Products).ThenFetchMany(a => a.OrderLines)where s.SupplierId == 1
				select s).ToListAsync());
		}

		[Test]
		public async Task WhereBeforeFetchAndOrderByAsync()
		{
			//NH-2915
			var firstOrderId = await (db.Orders.OrderBy(x => x.OrderId).Select(x => x.OrderId).FirstAsync());
			var orders = db.Orders.Where(x => x.OrderId != firstOrderId).Fetch(x => x.Customer).OrderBy(x => x.OrderId).ToList();
			Assert.AreEqual(829, orders.Count);
			Assert.IsTrue(NHibernateUtil.IsInitialized(orders[0].Customer));
		}

		[Test]
		public async Task WhereBeforeFetchManyAndOrderByAsync()
		{
			//NH-2915
			var firstOrderId = await (db.Orders.OrderBy(x => x.OrderId).Select(x => x.OrderId).FirstAsync());
			var orders = db.Orders.Where(x => x.OrderId != firstOrderId).FetchMany(x => x.OrderLines).OrderBy(x => x.OrderId).ToList();
			Assert.AreEqual(829, orders.Count);
			Assert.IsTrue(NHibernateUtil.IsInitialized(orders[0].OrderLines));
		}

		[Test]
		public async Task WhereBeforeFetchManyThenFetchAndOrderByAsync()
		{
			//NH-2915
			var firstOrderId = await (db.Orders.OrderBy(x => x.OrderId).Select(x => x.OrderId).FirstAsync());
			var orders = db.Orders.Where(x => x.OrderId != firstOrderId).FetchMany(x => x.OrderLines).ThenFetch(x => x.Product).OrderBy(x => x.OrderId).ToList();
			Assert.AreEqual(829, orders.Count);
			Assert.IsTrue(NHibernateUtil.IsInitialized(orders[0].OrderLines));
			Assert.IsTrue(NHibernateUtil.IsInitialized(orders[0].OrderLines.First().Product));
		}

		[Test]
		public async Task WhereBeforeFetchAndSelectAsync()
		{
			//NH-3056
			var firstOrderId = await (db.Orders.OrderBy(x => x.OrderId).Select(x => x.OrderId).FirstAsync());
			var orders = await (db.Orders.Where(x => x.OrderId != firstOrderId).Fetch(x => x.Customer).Select(x => x).ToListAsync());
			Assert.AreEqual(829, orders.Count);
			Assert.IsTrue(NHibernateUtil.IsInitialized(orders[0].Customer));
		}

		[Test]
		public async Task WhereBeforeFetchManyAndSelectAsync()
		{
			//NH-3056
			var firstOrderId = await (db.Orders.OrderBy(x => x.OrderId).Select(x => x.OrderId).FirstAsync());
			var orders = await (db.Orders.Where(x => x.OrderId != firstOrderId).FetchMany(x => x.OrderLines).Select(x => x).ToListAsync());
			Assert.AreEqual(829, orders.Count);
			Assert.IsTrue(NHibernateUtil.IsInitialized(orders[0].OrderLines));
		}

		[Test]
		public async Task WhereBeforeFetchManyThenFetchAndSelectAsync()
		{
			//NH-3056
			var firstOrderId = await (db.Orders.OrderBy(x => x.OrderId).Select(x => x.OrderId).FirstAsync());
			var orders = await (db.Orders.Where(x => x.OrderId != firstOrderId).FetchMany(x => x.OrderLines).ThenFetch(x => x.Product).Select(x => x).ToListAsync());
			Assert.AreEqual(829, orders.Count);
			Assert.IsTrue(NHibernateUtil.IsInitialized(orders[0].OrderLines));
			Assert.IsTrue(NHibernateUtil.IsInitialized(orders[0].OrderLines.First().Product));
		}

		[Test]
		public async Task WhereBeforeFetchAndWhereAsync()
		{
			var firstOrderId = await (db.Orders.OrderBy(x => x.OrderId).Select(x => x.OrderId).FirstAsync());
			var orders = await (db.Orders.Where(x => x.OrderId != firstOrderId).Fetch(x => x.Customer).Where(x => true).ToListAsync());
			Assert.AreEqual(829, orders.Count);
			Assert.IsTrue(NHibernateUtil.IsInitialized(orders[0].Customer));
		}

		[Test]
		public async Task WhereBeforeFetchManyAndWhereAsync()
		{
			var firstOrderId = await (db.Orders.OrderBy(x => x.OrderId).Select(x => x.OrderId).FirstAsync());
			var orders = await (db.Orders.Where(x => x.OrderId != firstOrderId).FetchMany(x => x.OrderLines).Where(x => true).ToListAsync());
			Assert.AreEqual(829, orders.Count);
			Assert.IsTrue(NHibernateUtil.IsInitialized(orders[0].OrderLines));
		}

		[Test]
		public async Task WhereBeforeFetchManyThenFetchAndWhereAsync()
		{
			var firstOrderId = await (db.Orders.OrderBy(x => x.OrderId).Select(x => x.OrderId).FirstAsync());
			var orders = await (db.Orders.Where(x => x.OrderId != firstOrderId).FetchMany(x => x.OrderLines).ThenFetch(x => x.Product).Where(x => true).ToListAsync());
			Assert.AreEqual(829, orders.Count);
			Assert.IsTrue(NHibernateUtil.IsInitialized(orders[0].OrderLines));
			Assert.IsTrue(NHibernateUtil.IsInitialized(orders[0].OrderLines.First().Product));
		}

		[Test]
		public async Task WhereAfterFetchAndSingleOrDefaultAsync()
		{
			//NH-3186
			var firstOrderId = await (db.Orders.OrderBy(x => x.OrderId).Select(x => x.OrderId).FirstAsync());
			var order = await (db.Orders.Fetch(x => x.Shipper).SingleOrDefaultAsync(x => x.OrderId == firstOrderId));
			Assert.IsTrue(NHibernateUtil.IsInitialized(order.Shipper));
		}
	}
}
#endif