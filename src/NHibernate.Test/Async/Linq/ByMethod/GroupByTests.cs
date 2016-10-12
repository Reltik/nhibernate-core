﻿#if NET_4_5
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NHibernate.Dialect;
using NHibernate.DomainModel.Northwind.Entities;
using NHibernate.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.Linq.ByMethod
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class GroupByTestsAsync : LinqTestCaseAsync
	{
		[Test]
		public async Task SingleKeyGroupAndCountAsync()
		{
			var orderCounts = await (db.Orders.GroupBy(o => o.Customer).Select(g => g.Count()).ToListAsync());
			Assert.AreEqual(89, orderCounts.Count);
			Assert.AreEqual(830, orderCounts.Sum());
		}

		[Test]
		public async Task MultipleKeyGroupAndCountAsync()
		{
			var orderCounts = await (db.Orders.GroupBy(o => new
			{
			o.Customer, o.Employee
			}

			).Select(g => g.Count()).ToListAsync());
			Assert.AreEqual(464, orderCounts.Count);
			Assert.AreEqual(830, orderCounts.Sum());
		}

		[Test]
		public async Task SingleKeyGroupingAsync()
		{
			var orders = await (db.Orders.GroupBy(o => o.Customer).ToListAsync());
			Assert.That(orders.Count, Is.EqualTo(89));
			Assert.That(orders.Sum(o => o.Count()), Is.EqualTo(830));
			CheckGrouping(orders, o => o.Customer);
		}

		[Test]
		public async Task MultipleKeyGroupingAsync()
		{
			var orders = await (db.Orders.GroupBy(o => new
			{
			o.Customer, o.Employee
			}

			).ToListAsync());
			Assert.That(orders.Count, Is.EqualTo(464));
			Assert.That(orders.Sum(o => o.Count()), Is.EqualTo(830));
			CheckGrouping(orders.Select(g => new TupGrouping<Customer, Employee, Order>(g.Key.Customer, g.Key.Employee, g)), o => o.Customer, o => o.Employee);
		}

		[Test]
		public async Task GroupBySelectKeyShouldUseServerSideGroupingAsync()
		{
			using (var spy = new SqlLogSpy())
			{
				var orders = await ((
					from o in db.Orders
					group o by o.OrderDate into g
						select g.Key).ToListAsync());
				Assert.That(orders.Count, Is.EqualTo(481));
				Assert.That(Regex.Replace(spy.GetWholeLog(), @"\s+", " "), Is.StringContaining("group by order0_.OrderDate"));
			}
		}

		[Test]
		public async Task SingleKeyGroupAndOrderByKeyAsync()
		{
			//NH-2452
			var result = await (db.Products.GroupBy(i => i.UnitPrice).OrderBy(g => g.Key).Select(g => new
			{
			UnitPrice = g.Max(i => i.UnitPrice), TotalUnitsInStock = g.Sum(i => i.UnitsInStock)}

			).ToListAsync());
			Assert.That(result.Count, Is.EqualTo(62));
			AssertOrderedBy.Ascending(result, x => x.UnitPrice);
		}

		[Test]
		public async Task SingleKeyPropertyGroupAndOrderByCountBeforeProjectionAsync()
		{
			// NH-3026, variation of NH-2560.
			// This is a variation of SingleKeyPropertyGroupAndOrderByProjectedCount()
			// that puts the ordering expression inside the OrderBy, without first
			// going through a select clause.
			var orderCounts = await (db.Orders.GroupBy(o => o.Customer.CustomerId).OrderByDescending(g => g.Count()).Select(g => new
			{
			CustomerId = g.Key, OrderCount = g.Count()}

			).ToListAsync());
			AssertOrderedBy.Descending(orderCounts, oc => oc.OrderCount);
		}

		[Test]
		public async Task SingleKeyPropertyGroupWithOrderByCountAsync()
		{
			// The problem with this test (as of 2014-07-25) is that the generated SQL will
			// try to select columns that are not included in the group-by clause. But on MySQL and
			// sqlite, it's apparently ok.
			// The try-catch in this clause aim to ignore the test on dialects where it shouldn't work,
			// but give us a warning if it does start to work.
			try
			{
				var result = await (db.Orders.GroupBy(o => o.Customer).OrderByDescending(g => g.Count()) // it seems like there we should do order on client-side
				.Select(g => g.Key).ToListAsync());
				Assert.That(result.Count, Is.EqualTo(89));
			}
			catch (Exception)
			{
				if (Dialect is MySQLDialect || Dialect is SQLiteDialect)
					throw;
				Assert.Ignore("Known bug NH-3027, discovered as part of NH-2560.");
			}

			if (Dialect is MySQLDialect || Dialect is SQLiteDialect)
				return;
			Assert.Fail("Unexpected success in test. Maybe something was fixed and the test needs to be updated?");
		}

		[Test]
		public async Task GroupByWithAndAlsoContainsInWhereClauseAsync()
		{
			//NH-3032
			var collection = await (db.Products.Select(x => x.Supplier).ToListAsync());
			var result = await (db.Products.Where(x => x.Discontinued == true && collection.Contains(x.Supplier)).GroupBy(x => x.UnitPrice).Select(x => new
			{
			x.Key, Count = x.Count()}

			).ToListAsync());
			Assert.That(result.Count, Is.EqualTo(8));
		}

		[Test]
		public async Task GroupByWithContainsInWhereClauseAsync()
		{
			//NH-3032
			var collection = await (db.Products.Select(x => x.Supplier).ToListAsync());
			var result = await (db.Products.Where(x => collection.Contains(x.Supplier)).GroupBy(x => x.UnitPrice).Select(x => new
			{
			x.Key, Count = x.Count()}

			).ToListAsync());
			Assert.That(result.Count, Is.EqualTo(62));
		}

		[Test]
		public async Task SelectTupleKeyCountOfOrderLinesAsync()
		{
			Assert.ThrowsAsync<AssertionException>(async () =>
			{
				var list = (
					from o in db.Orders.ToList()group o by o.OrderDate into g
						select new
						{
						g.Key, Count = g.SelectMany(x => x.OrderLines).Count()}

				).ToList();
				var query = await ((
					from o in db.Orders
					group o by o.OrderDate into g
						select new
						{
						g.Key, Count = g.SelectMany(x => x.OrderLines).Count()}

				).ToListAsync());
				Assert.That(query.Count, Is.EqualTo(481));
				Assert.That(query, Is.EquivalentTo(list));
			}

			, KnownBug.Issue("NH-3025"));
		}

		[Test]
		public async Task GroupByTwoFieldsWhereOneOfThemIsTooDeepAsync()
		{
			var query = await ((
				from ol in db.OrderLines
				let superior = ol.Order.Employee.Superior
				group ol by new
				{
				ol.Order.OrderId, SuperiorId = (int ? )superior.EmployeeId
				}

					into temp
					select new
					{
					OrderId = (int ? )temp.Key.OrderId, SuperiorId = temp.Key.SuperiorId, Count = temp.Count(), }

			).ToListAsync());
			Assert.That(query.Count, Is.EqualTo(830));
		}

		[Test]
		public async Task GroupByAndTakeAsync()
		{
			//NH-2566
			var names = await (db.Users.GroupBy(p => p.Name).Select(g => g.Key).Take(3).ToListAsync());
			Assert.That(names.Count, Is.EqualTo(3));
		}

		[Test]
		public async Task GroupByAndTake2Async()
		{
			//NH-2566
			var results = await ((
				from o in db.Orders
				group o by o.Customer into g
					select g.Key.CustomerId).OrderBy(customerId => customerId).Skip(10).Take(10).ToListAsync());
			Assert.That(results.Count, Is.EqualTo(10));
		}

		[Test]
		public async Task GroupByAndAnyAsync()
		{
			//NH-2566
			var namesAreNotEmpty = !await (db.Users.GroupBy(p => p.Name).Select(g => g.Key).AnyAsync(name => name.Length == 0));
			Assert.That(namesAreNotEmpty, Is.True);
		}

		[Test]
		public async Task SelectFirstElementFromProductsGroupedByUnitPriceAsync()
		{
			//NH-3180
			var result = await (db.Products.GroupBy(x => x.UnitPrice).Select(x => new
			{
			x.Key, Count = x.Count()}

			).OrderByDescending(x => x.Key).FirstAsync());
			Assert.That(result.Key, Is.EqualTo(263.5M));
			Assert.That(result.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task SelectFirstOrDefaultElementFromProductsGroupedByUnitPriceAsync()
		{
			//NH-3180
			var result = await (db.Products.GroupBy(x => x.UnitPrice).Select(x => new
			{
			x.Key, Count = x.Count()}

			).OrderByDescending(x => x.Key).FirstOrDefaultAsync());
			Assert.That(result.Key, Is.EqualTo(263.5M));
			Assert.That(result.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task SelectSingleElementFromProductsGroupedByUnitPriceAsync()
		{
			//NH-3180
			var result = await (db.Products.GroupBy(x => x.UnitPrice).Select(x => new
			{
			x.Key, Count = x.Count()}

			).Where(x => x.Key == 263.5M).OrderByDescending(x => x.Key).SingleAsync());
			Assert.That(result.Key, Is.EqualTo(263.5M));
			Assert.That(result.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task SelectSingleOrDefaultElementFromProductsGroupedByUnitPriceAsync()
		{
			//NH-3180
			var result = await (db.Products.GroupBy(x => x.UnitPrice).Select(x => new
			{
			x.Key, Count = x.Count()}

			).Where(x => x.Key == 263.5M).OrderByDescending(x => x.Key).SingleOrDefaultAsync());
			Assert.That(result.Key, Is.EqualTo(263.5M));
			Assert.That(result.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task ProjectingCountWithPredicateAsync()
		{
			var result = await (db.Products.GroupBy(x => x.Supplier.CompanyName).Select(x => new
			{
			x.Key, Count = x.Count(y => y.UnitPrice == 9.50M)}

			).OrderByDescending(x => x.Key).FirstAsync());
			Assert.That(result.Key, Is.EqualTo("Zaanse Snoepfabriek"));
			Assert.That(result.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task FilteredByCountWithPredicateAsync()
		{
			var result = await (db.Products.GroupBy(x => x.Supplier.CompanyName).Where(x => x.Count(y => y.UnitPrice == 12.75M) == 1).Select(x => new
			{
			x.Key, Count = x.Count()}

			).FirstAsync());
			Assert.That(result.Key, Is.EqualTo("Zaanse Snoepfabriek"));
			Assert.That(result.Count, Is.EqualTo(2));
		}

		[Test]
		public async Task FilteredByCountFromSubQueryAsync()
		{
			//Not really an aggregate filter, but included to ensure that this kind of query still works
			var result = await (db.Products.GroupBy(x => x.Supplier.CompanyName).Where(x => db.Products.Count(y => y.Supplier.CompanyName == x.Key && y.UnitPrice == 12.75M) == 1).Select(x => new
			{
			x.Key, Count = x.Count()}

			).FirstAsync());
			Assert.That(result.Key, Is.EqualTo("Zaanse Snoepfabriek"));
			Assert.That(result.Count, Is.EqualTo(2));
		}

		[Test]
		public async Task FilteredByAndProjectingSumWithPredicateAsync()
		{
			var result = await (db.Products.GroupBy(x => x.Supplier.CompanyName).Where(x => x.Sum(y => y.UnitPrice == 12.75M ? y.UnitPrice : 0M) == 12.75M).Select(x => new
			{
			x.Key, Sum = x.Sum(y => y.UnitPrice)}

			).FirstAsync());
			Assert.That(result.Key, Is.EqualTo("Zaanse Snoepfabriek"));
			Assert.That(result.Sum, Is.EqualTo(12.75M + 9.50M));
		}

		[Test]
		public async Task FilteredByKeyAndProjectedWithAggregatePredicatesAsync()
		{
			var result = await (db.Products.GroupBy(x => x.Supplier.CompanyName).Where(x => x.Key == "Zaanse Snoepfabriek").Select(x => new
			{
			x.Key, Sum = x.Sum(y => y.UnitPrice == 12.75M ? y.UnitPrice : 0M), Avg = x.Average(y => y.UnitPrice == 12.75M ? y.UnitPrice : 0M), Count = x.Count(y => y.UnitPrice == 12.75M), Max = x.Max(y => y.UnitPrice == 12.75M ? y.UnitPrice : 0M), Min = x.Min(y => y.UnitPrice == 12.75M ? y.UnitPrice : 0M)}

			).FirstAsync());
			Assert.That(result.Key, Is.EqualTo("Zaanse Snoepfabriek"));
			Assert.That(result.Sum, Is.EqualTo(12.75M));
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Avg, Is.EqualTo(12.75M / 2));
			Assert.That(result.Max, Is.EqualTo(12.75M));
			Assert.That(result.Min, Is.EqualTo(0M));
		}

		[Test]
		public async Task ProjectingWithSubQueriesFilteredByTheAggregateKeyAsync()
		{
			var result = await (db.Products.GroupBy(x => x.Supplier.Address.Country).OrderBy(x => x.Key).Select(x => new
			{
			x.Key, MaxFreight = db.Orders.Where(y => y.ShippingAddress.Country == x.Key).Max(y => y.Freight), FirstOrder = db.Orders.Where(o => o.Employee.FirstName.StartsWith("A")).OrderBy(o => o.OrderId).Select(y => y.OrderId).First()}

			).ToListAsync());
			Assert.That(result.Count, Is.EqualTo(16));
			Assert.That(result[15].MaxFreight, Is.EqualTo(830.75M));
			Assert.That(result[15].FirstOrder, Is.EqualTo(10255));
		}

		[Test(Description = "NH-3681")]
		public async Task SelectManyGroupByAggregateProjectionAsync()
		{
			var result = await ((
				from o in db.Orders
				from ol in o.OrderLines
				group ol by ol.Product.ProductId into grp
					select new
					{
					ProductId = grp.Key, Sum = grp.Sum(x => x.UnitPrice), Count = grp.Count(), Avg = grp.Average(x => x.UnitPrice), Min = grp.Min(x => x.UnitPrice), Max = grp.Max(x => x.UnitPrice), }

			).ToListAsync());
			Assert.That(result.Count, Is.EqualTo(77));
		}

		[Test(Description = "NH-3797")]
		public async Task GroupByComputedValueAsync()
		{
			var orderGroups = await (db.Orders.GroupBy(o => o.Customer.CustomerId == null ? 0 : 1).Select(g => new
			{
			Key = g.Key, Count = g.Count()}

			).ToListAsync());
			Assert.AreEqual(830, orderGroups.Sum(g => g.Count));
		}

		[Test(Description = "NH-3797")]
		public async Task GroupByComputedValueInAnonymousTypeAsync()
		{
			var orderGroups = await (db.Orders.GroupBy(o => new
			{
			Key = o.Customer.CustomerId == null ? 0 : 1
			}

			).Select(g => new
			{
			Key = g.Key, Count = g.Count()}

			).ToListAsync());
			Assert.AreEqual(830, orderGroups.Sum(g => g.Count));
		}

		[Test(Description = "NH-3797")]
		public async Task GroupByComputedValueInObjectArrayAsync()
		{
			var orderGroups = await (db.Orders.GroupBy(o => new[]{o.Customer.CustomerId == null ? 0 : 1, }).Select(g => new
			{
			Key = g.Key, Count = g.Count()}

			).ToListAsync());
			Assert.AreEqual(830, orderGroups.Sum(g => g.Count));
		}

		[Test(Description = "NH-3474")]
		public async Task GroupByConstantAsync()
		{
			var totals = await (db.Orders.GroupBy(o => 1).Select(g => new
			{
			Key = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Freight)}

			).ToListAsync());
			Assert.That(totals.Count, Is.EqualTo(1));
			Assert.That(totals, Has.All.With.Property("Key").EqualTo(1));
		}

		[Test(Description = "NH-3474")]
		public async Task GroupByConstantAnonymousTypeAsync()
		{
			var totals = await (db.Orders.GroupBy(o => new
			{
			A = 1
			}

			).Select(g => new
			{
			Key = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Freight)}

			).ToListAsync());
			Assert.That(totals.Count, Is.EqualTo(1));
			Assert.That(totals, Has.All.With.Property("Key").With.Property("A").EqualTo(1));
		}

		[Test(Description = "NH-3474")]
		public async Task GroupByConstantArrayAsync()
		{
			var totals = await (db.Orders.GroupBy(o => new object[]{1}).Select(g => new
			{
			Key = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Freight)}

			).ToListAsync());
			Assert.That(totals.Count, Is.EqualTo(1));
			Assert.That(totals, Has.All.With.Property("Key").EqualTo(new object[]{1}));
		}

		[Test(Description = "NH-3474")]
		public async Task GroupByKeyWithConstantInAnonymousTypeAsync()
		{
			var totals = await (db.Orders.GroupBy(o => new
			{
			A = 1, B = o.Shipper.ShipperId
			}

			).Select(g => new
			{
			Key = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Freight)}

			).ToListAsync());
			Assert.That(totals.Count, Is.EqualTo(3));
			Assert.That(totals, Has.All.With.Property("Key").With.Property("A").EqualTo(1));
		}

		[Test(Description = "NH-3474")]
		public async Task GroupByKeyWithConstantInArrayAsync()
		{
			var totals = await (db.Orders.GroupBy(o => new[]{1, o.Shipper.ShipperId}).Select(g => new
			{
			Key = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Freight)}

			).ToListAsync());
			Assert.That(totals.Count, Is.EqualTo(3));
			Assert.That(totals, Has.All.With.Property("Key").Contains(1));
		}

		private int constKey;
		[Test(Description = "NH-3474")]
		public async Task GroupByKeyWithConstantFromVariableAsync()
		{
			constKey = 1;
			var q1 = db.Orders.GroupBy(o => constKey).Select(g => new
			{
			Key = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Freight)}

			);
			var q1a = db.Orders.GroupBy(o => "").Select(g => new
			{
			Key = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Freight)}

			);
			var q2 = db.Orders.GroupBy(o => new
			{
			A = constKey
			}

			).Select(g => new
			{
			Key = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Freight)}

			);
			var q3 = db.Orders.GroupBy(o => new object[]{constKey}).Select(g => new
			{
			Key = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Freight)}

			);
			var q3a = db.Orders.GroupBy(o => (IEnumerable<object>)new object[]{constKey}).Select(g => new
			{
			Key = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Freight)}

			);
			var q4 = db.Orders.GroupBy(o => new
			{
			A = constKey, B = o.Shipper.ShipperId
			}

			).Select(g => new
			{
			Key = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Freight)}

			);
			var q5 = db.Orders.GroupBy(o => new[]{constKey, o.Shipper.ShipperId}).Select(g => new
			{
			Key = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Freight)}

			);
			var q5a = db.Orders.GroupBy(o => (IEnumerable<int>)new[]{constKey, o.Shipper.ShipperId}).Select(g => new
			{
			Key = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Freight)}

			);
			var r1_1 = await (q1.ToListAsync());
			Assert.That(r1_1.Count, Is.EqualTo(1));
			Assert.That(r1_1, Has.All.With.Property("Key").EqualTo(1));
			var r1a_1 = await (q1a.ToListAsync());
			Assert.That(r1a_1.Count, Is.EqualTo(1));
			Assert.That(r1a_1, Has.All.With.Property("Key").EqualTo(""));
			var r2_1 = await (q2.ToListAsync());
			Assert.That(r2_1.Count, Is.EqualTo(1));
			Assert.That(r2_1, Has.All.With.Property("Key").With.Property("A").EqualTo(1));
			var r3_1 = await (q3.ToListAsync());
			Assert.That(r3_1.Count, Is.EqualTo(1));
			Assert.That(r3_1, Has.All.With.Property("Key").EquivalentTo(new object[]{1}));
			var r3a_1 = await (q3a.ToListAsync());
			Assert.That(r3a_1.Count, Is.EqualTo(1));
			Assert.That(r3a_1, Has.All.With.Property("Key").EquivalentTo(new object[]{1}));
			var r4_1 = await (q4.ToListAsync());
			Assert.That(r4_1.Count, Is.EqualTo(3));
			Assert.That(r4_1, Has.All.With.Property("Key").With.Property("A").EqualTo(1));
			var r5_1 = await (q5.ToListAsync());
			Assert.That(r5_1.Count, Is.EqualTo(3));
			Assert.That(r5_1, Has.All.With.Property("Key").Contains(1));
			var r6_1 = await (q5a.ToListAsync());
			Assert.That(r6_1.Count, Is.EqualTo(3));
			Assert.That(r6_1, Has.All.With.Property("Key").Contains(1));
			constKey = 2;
			var r1_2 = await (q1.ToListAsync());
			Assert.That(r1_2.Count, Is.EqualTo(1));
			Assert.That(r1_2, Has.All.With.Property("Key").EqualTo(2));
			var r2_2 = await (q2.ToListAsync());
			Assert.That(r2_2.Count, Is.EqualTo(1));
			Assert.That(r2_2, Has.All.With.Property("Key").With.Property("A").EqualTo(2));
			var r3_2 = await (q3.ToListAsync());
			Assert.That(r3_2.Count, Is.EqualTo(1));
			Assert.That(r3_2, Has.All.With.Property("Key").EquivalentTo(new object[]{2}));
			var r3a_2 = await (q3a.ToListAsync());
			Assert.That(r3a_2.Count, Is.EqualTo(1));
			Assert.That(r3a_2, Has.All.With.Property("Key").EquivalentTo(new object[]{2}));
			var r4_2 = await (q4.ToListAsync());
			Assert.That(r4_2.Count, Is.EqualTo(3));
			Assert.That(r4_2, Has.All.With.Property("Key").With.Property("A").EqualTo(2));
			var r5_2 = await (q5.ToListAsync());
			Assert.That(r5_2.Count, Is.EqualTo(3));
			Assert.That(r5_2, Has.All.With.Property("Key").Contains(2));
			var r6_2 = await (q5.ToListAsync());
			Assert.That(r6_2.Count, Is.EqualTo(3));
			Assert.That(r6_2, Has.All.With.Property("Key").Contains(2));
		}

		[Test(Description = "NH-3801")]
		public async Task GroupByComputedValueWithJoinOnObjectAsync()
		{
			var orderGroups = await (db.OrderLines.GroupBy(o => o.Order.Customer == null ? 0 : 1).Select(g => new
			{
			Key = g.Key, Count = g.Count()}

			).ToListAsync());
			Assert.AreEqual(2155, orderGroups.Sum(g => g.Count));
		}

		[Test(Description = "NH-3801")]
		public async Task GroupByComputedValueWithJoinOnIdAsync()
		{
			var orderGroups = await (db.OrderLines.GroupBy(o => o.Order.Customer.CustomerId == null ? 0 : 1).Select(g => new
			{
			Key = g.Key, Count = g.Count()}

			).ToListAsync());
			Assert.AreEqual(2155, orderGroups.Sum(g => g.Count));
		}

		[Test(Description = "NH-3801")]
		public async Task GroupByComputedValueInAnonymousTypeWithJoinOnObjectAsync()
		{
			var orderGroups = await (db.OrderLines.GroupBy(o => new
			{
			Key = o.Order.Customer == null ? 0 : 1
			}

			).Select(g => new
			{
			Key = g.Key, Count = g.Count()}

			).ToListAsync());
			Assert.AreEqual(2155, orderGroups.Sum(g => g.Count));
		}

		[Test(Description = "NH-3801")]
		public async Task GroupByComputedValueInAnonymousTypeWithJoinOnIdAsync()
		{
			var orderGroups = await (db.OrderLines.GroupBy(o => new
			{
			Key = o.Order.Customer.CustomerId == null ? 0 : 1
			}

			).Select(g => new
			{
			Key = g.Key, Count = g.Count()}

			).ToListAsync());
			Assert.AreEqual(2155, orderGroups.Sum(g => g.Count));
		}

		[Test(Description = "NH-3801")]
		public async Task GroupByComputedValueInObjectArrayWithJoinOnObjectAsync()
		{
			var orderGroups = await (db.OrderLines.GroupBy(o => new[]{o.Order.Customer == null ? 0 : 1}).Select(g => new
			{
			Key = g.Key, Count = g.Count()}

			).ToListAsync());
			Assert.AreEqual(2155, orderGroups.Sum(g => g.Count));
		}

		[Test(Description = "NH-3801")]
		public async Task GroupByComputedValueInObjectArrayWithJoinOnIdAsync()
		{
			var orderGroups = await (db.OrderLines.GroupBy(o => new[]{o.Order.Customer.CustomerId == null ? 0 : 1}).Select(g => new
			{
			Key = g.Key, Count = g.Count()}

			).ToListAsync());
			Assert.AreEqual(2155, orderGroups.Sum(g => g.Count));
		}

		[Test(Description = "NH-3801")]
		public async Task GroupByComputedValueInObjectArrayWithJoinInRightSideOfCaseAsync()
		{
			var orderGroups = await (db.OrderLines.GroupBy(o => new[]{o.Order.Customer.CustomerId == null ? "unknown" : o.Order.Customer.CompanyName}).Select(g => new
			{
			Key = g.Key, Count = g.Count()}

			).ToListAsync());
			Assert.AreEqual(2155, orderGroups.Sum(g => g.Count));
		}

		private static void CheckGrouping<TKey, TElement>(IEnumerable<IGrouping<TKey, TElement>> groupedItems, Func<TElement, TKey> groupBy)
		{
			var used = new HashSet<object>();
			foreach (IGrouping<TKey, TElement> group in groupedItems)
			{
				Assert.IsFalse(used.Contains(group.Key));
				used.Add(group.Key);
				foreach (var item in group)
				{
					Assert.AreEqual(group.Key, groupBy(item));
				}
			}
		}

		private static void CheckGrouping<TKey1, TKey2, TElement>(IEnumerable<TupGrouping<TKey1, TKey2, TElement>> groupedItems, Func<TElement, TKey1> groupBy1, Func<TElement, TKey2> groupBy2)
		{
			var used = new HashSet<object>();
			foreach (IGrouping<Tup<TKey1, TKey2>, TElement> group in groupedItems)
			{
				Assert.IsFalse(used.Contains(group.Key));
				used.Add(group.Key);
				foreach (var item in group)
				{
					Assert.AreEqual(group.Key.Item1, groupBy1(item));
					Assert.AreEqual(group.Key.Item2, groupBy2(item));
				}
			}
		}

		[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
		private partial class TupGrouping<TKey1, TKey2, TElement> : IGrouping<Tup<TKey1, TKey2>, TElement>
		{
			private IEnumerable<TElement> Elements
			{
				get;
				set;
			}

			public TupGrouping(TKey1 key1, TKey2 key2, IEnumerable<TElement> elements)
			{
				Key = new Tup<TKey1, TKey2>(key1, key2);
				Elements = elements;
			}

			public IEnumerator<TElement> GetEnumerator()
			{
				return Elements.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public Tup<TKey1, TKey2> Key
			{
				get;
				private set;
			}
		}

		[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
		private partial class Tup<T1, T2>
		{
			public T1 Item1
			{
				get;
				private set;
			}

			public T2 Item2
			{
				get;
				private set;
			}

			public Tup(T1 item1, T2 item2)
			{
				Item1 = item1;
				Item2 = item2;
			}

			public override bool Equals(object obj)
			{
				if (obj == null)
					return false;
				if (obj.GetType() != GetType())
					return false;
				var other = (Tup<T1, T2>)obj;
				return Equals(Item1, other.Item1) && Equals(Item2, other.Item2);
			}

			public override int GetHashCode()
			{
				return Item1.GetHashCode() ^ Item2.GetHashCode();
			}
		}

		[Test(Description = "NH-3446")]
		public async Task GroupByOrderByKeySelectToClassAsync()
		{
			Assert.ThrowsAsync<HibernateException>(async () =>
			{
				await (db.Products.GroupBy(x => x.Supplier.CompanyName).OrderBy(x => x.Key).Select(x => new GroupInfo{Key = x.Key, ItemCount = x.Count(), HasSubgroups = false, Items = x}).ToListAsync());
			}

			, KnownBug.Issue("NH-3446"));
		}

		[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
		private partial class GroupInfo
		{
			public object Key
			{
				get;
				set;
			}

			public int ItemCount
			{
				get;
				set;
			}

			public bool HasSubgroups
			{
				get;
				set;
			}

			public IEnumerable Items
			{
				get;
				set;
			}
		}
	}
}
#endif
