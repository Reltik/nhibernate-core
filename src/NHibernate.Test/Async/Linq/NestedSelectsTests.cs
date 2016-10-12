﻿#if NET_4_5
using System;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using NHibernate.Linq;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.Linq
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class NestedSelectsTestsAsync : LinqTestCaseAsync
	{
		[Test]
		public async Task OrdersIdWithOrderLinesIdAsync()
		{
			var orders = await (db.Orders.Select(o => new
			{
			o.OrderId, OrderLinesIds = o.OrderLines.Select(ol => ol.Id).ToArray()}

			).ToListAsync());
			Assert.That(orders.Count, Is.EqualTo(830));
		}

		[Test]
		public async Task OrdersOrderLinesIdAsync()
		{
			var orders = await (db.Orders.Select(o => new
			{
			OrderLinesIds = o.OrderLines.Select(ol => ol.Id).ToArray()}

			).ToListAsync());
			Assert.That(orders.Count, Is.EqualTo(830));
		}

		[Test]
		public async Task OrdersIdWithOrderLinesIdShouldBeNotLazyAsync()
		{
			var orders = await (db.Orders.Select(o => new
			{
			o.OrderId, OrderLinesIds = o.OrderLines.Select(ol => ol.Id)}

			).ToListAsync());
			Assert.That(orders.Count, Is.EqualTo(830));
			Assert.That(orders[0].OrderLinesIds, Is.InstanceOf<ReadOnlyCollection<long>>());
		}

		[Test]
		public async Task OrdersIdWithOrderLinesIdAndDiscountAsync()
		{
			var orders = await (db.Orders.Select(o => new
			{
			o.OrderId, OrderLines = o.OrderLines.Select(ol => new
			{
			ol.Id, ol.Discount
			}

			).ToArray()}

			).ToListAsync());
			Assert.That(orders.Count, Is.EqualTo(830));
		}

		[Test]
		public async Task OrdersIdAndDateWithOrderLinesIdAndDiscountAsync()
		{
			var orders = await (db.Orders.Select(o => new
			{
			o.OrderId, o.OrderDate, OrderLines = o.OrderLines.Select(ol => new
			{
			ol.Id, ol.Discount
			}

			).ToArray()}

			).ToListAsync());
			Assert.That(orders.Count, Is.EqualTo(830));
		}

		[Test]
		public async Task TimesheetIdAndUserLastLoginDatesAsync()
		{
			var timesheets = await (db.Timesheets.Select(o => new
			{
			o.Id, Users = o.Users.Select(x => x.LastLoginDate).ToArray()}

			).ToListAsync());
			Assert.That(timesheets.Count, Is.EqualTo(3));
			Assert.That(timesheets[0].Users, Is.Not.Empty);
		}

		[Test]
		public async Task TimesheetIdAndUserLastLoginDatesAndEntriesIdsAsync()
		{
			var timesheets = await (db.Timesheets.Select(o => new
			{
			o.Id, LastLoginDates = o.Users.Select(u => u.LastLoginDate).ToArray(), EntriesIds = o.Entries.Select(e => e.Id).ToArray()}

			).ToListAsync());
			Assert.That(timesheets.Count, Is.EqualTo(3));
			Assert.That(timesheets[0].LastLoginDates, Is.Not.Empty);
		}

		[Test(Description = "NH-2986")]
		public async Task TimesheetIdAndUsersTransparentProjectionAsync()
		{
			var timesheets = await (db.Timesheets.Select(o => new
			{
			o.Id, Users = o.Users.Select(x => x)}

			).ToListAsync());
			Assert.That(timesheets.Count, Is.EqualTo(3));
			Assert.That(timesheets[0].Users, Is.Not.Empty);
		}

		[Test(Description = "NH-2986")]
		public async Task TimesheetAndUsersTransparentProjectionAsync()
		{
			var timesheets = await (db.Timesheets.Select(o => new
			{
			o, Users = o.Users.Select(x => x)}

			).ToListAsync());
			Assert.That(timesheets.Count, Is.EqualTo(3));
			Assert.That(timesheets[0].Users, Is.Not.Empty);
		}

		[Test(Description = "NH-2986")]
		public async Task TimesheetUsersTransparentProjectionAsync()
		{
			var timesheets = await (db.Timesheets.Select(o => new
			{
			Users = o.Users.Select(x => x)}

			).ToListAsync());
			Assert.That(timesheets.Count, Is.EqualTo(3));
			Assert.That(timesheets[0].Users, Is.Not.Empty);
		}

		[Test(Description = "NH-2986")]
		public async Task TimesheetIdAndUsersAndEntriesTransparentProjectionAsync()
		{
			var timesheets = await (db.Timesheets.Select(o => new
			{
			o.Id, Users = o.Users.Select(x => x), Entries = o.Entries.Select(x => x)}

			).ToListAsync());
			Assert.That(timesheets.Count, Is.EqualTo(3));
			Assert.That(timesheets[0].Users, Is.Not.Empty);
		}

		[Test(Description = "NH-2986")]
		public async Task TimesheetAndUsersAndEntriesTransparentProjectionAsync()
		{
			var timesheets = await (db.Timesheets.Select(o => new
			{
			o, Users = o.Users.Select(x => x), Entries = o.Entries.Select(x => x)}

			).ToListAsync());
			Assert.That(timesheets.Count, Is.EqualTo(3));
			Assert.That(timesheets[0].Users, Is.Not.Empty);
		}

		[Test(Description = "NH-2986")]
		public async Task TimesheetUsersAndEntriesTransparentProjectionAsync()
		{
			var timesheets = await (db.Timesheets.Select(o => new
			{
			Users = o.Users.Select(x => x), Entries = o.Entries.Select(x => x)}

			).ToListAsync());
			Assert.That(timesheets.Count, Is.EqualTo(3));
			Assert.That(timesheets[0].Users, Is.Not.Empty);
		}

		[Test(Description = "NH-3333")]
		public async Task TimesheetIdAndUsersAsync()
		{
			var timesheets = await (db.Timesheets.Select(o => new
			{
			o.Id, o.Users
			}

			).ToListAsync());
			Assert.That(timesheets.Count, Is.EqualTo(3));
			Assert.That(timesheets[0].Users, Is.Not.Empty);
		}

		[Test(Description = "NH-3333")]
		public async Task TimesheetAndUsersAsync()
		{
			var timesheets = await (db.Timesheets.Select(o => new
			{
			o, o.Users
			}

			).ToListAsync());
			Assert.That(timesheets.Count, Is.EqualTo(3));
			Assert.That(timesheets[0].Users, Is.Not.Empty);
		}

		[Test(Description = "NH-3333")]
		public async Task TimesheetUsersAsync()
		{
			var timesheets = await (db.Timesheets.Select(o => new
			{
			o.Users
			}

			).ToListAsync());
			Assert.That(timesheets.Count, Is.EqualTo(3));
			Assert.That(timesheets[0].Users, Is.Not.Empty);
		}

		[Test(Description = "NH-3333")]
		public async Task TimesheetIdAndUsersAndEntriesAsync()
		{
			var timesheets = await (db.Timesheets.Select(o => new
			{
			o.Id, o.Users, o.Entries
			}

			).ToListAsync());
			Assert.That(timesheets.Count, Is.EqualTo(3));
			Assert.That(timesheets[0].Users, Is.Not.Empty);
		}

		[Test(Description = "NH-3333")]
		public async Task TimesheetAndUsersAndEntriesAsync()
		{
			var timesheets = await (db.Timesheets.Select(o => new
			{
			o, o.Users, o.Entries
			}

			).ToListAsync());
			Assert.That(timesheets.Count, Is.EqualTo(3));
			Assert.That(timesheets[0].Users, Is.Not.Empty);
		}

		[Test(Description = "NH-3333")]
		public async Task TimesheetUsersAndEntriesAsync()
		{
			var timesheets = await (db.Timesheets.Select(o => new
			{
			o.Users, o.Entries
			}

			).ToListAsync());
			Assert.That(timesheets.Count, Is.EqualTo(3));
			Assert.That(timesheets[0].Users, Is.Not.Empty);
		}

		[Test]
		public async Task EmployeesIdAndWithSubordinatesIdAsync()
		{
			var emplyees = await (db.Employees.Select(o => new
			{
			o.EmployeeId, SubordinatesIds = o.Subordinates.Select(so => so.EmployeeId).ToArray()}

			).ToListAsync());
			Assert.That(emplyees.Count, Is.EqualTo(9));
		}

		[Test]
		public async Task OrdersIdWithOrderLinesNestedWhereIdAsync()
		{
			var orders = await (db.Orders.Select(o => new
			{
			o.OrderId, OrderLinesIds = o.OrderLines.Where(ol => ol.Discount > 1).Select(ol => ol.Id).ToArray()}

			).ToListAsync());
			Assert.That(orders.Count, Is.EqualTo(830));
			Assert.That(orders[0].OrderLinesIds, Is.Empty);
		}
	}
}
#endif
