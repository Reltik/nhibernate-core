﻿#if NET_4_5
using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.DomainModel.Northwind.Entities;
using NUnit.Framework;
using NHibernate.Linq;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.Linq
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class SelectionTestsAsync : LinqTestCaseAsync
	{
		[Test]
		public async Task CanGetCountOnQueryWithAnonymousTypeAsync()
		{
			var query =
				from user in db.Users
				select new
				{
				user.Name, RoleName = user.Role.Name
				}

			;
			int totalCount = await (query.CountAsync());
			Assert.AreEqual(3, totalCount);
		}

		[Test]
		public async Task CanGetFirstWithAnonymousTypeAsync()
		{
			var query =
				from user in db.Users
				select new
				{
				user.Name, RoleName = user.Role.Name
				}

			;
			var firstUser = await (query.FirstAsync());
			Assert.IsNotNull(firstUser);
		}

		[Test]
		public async Task CanSelectUsingMemberInitExpressionAsync()
		{
			var query =
				from user in db.Users
				select new UserDto(user.Id, user.Name)
				{InvalidLoginAttempts = user.InvalidLoginAttempts};
			var list = await (query.ToListAsync());
			Assert.AreEqual(3, list.Count);
		}

		[Test]
		public async Task CanSelectNestedAnonymousTypeAsync()
		{
			var query =
				from user in db.Users
				select new
				{
				user.Name, Enums = new
				{
				user.Enum1, user.Enum2
				}

				, RoleName = user.Role.Name
				}

			;
			var list = await (query.ToListAsync());
			Assert.AreEqual(3, list.Count);
			//assert role names -- to ensure that the correct values were used to invoke constructor
			Assert.IsTrue(list.All(u => u.RoleName == "Admin" || u.RoleName == "User" || String.IsNullOrEmpty(u.RoleName)));
		}

		[Test]
		public async Task CanSelectNestedAnonymousTypeWithMultipleReferencesAsync()
		{
			var query =
				from user in db.Users
				select new
				{
				user.Name, Enums = new
				{
				user.Enum1, user.Enum2
				}

				, RoleName = user.Role.Name, RoleIsActive = (bool ? )user.Role.IsActive
				}

			;
			var list = await (query.ToListAsync());
			Assert.AreEqual(3, list.Count);
			//assert role names -- to ensure that the correct values were used to invoke constructor
			Assert.IsTrue(list.All(u => u.RoleName == "Admin" || u.RoleName == "User" || String.IsNullOrEmpty(u.RoleName)));
		}

		[Test]
		public async Task CanSelectNestedAnonymousTypeWithComponentReferenceAsync()
		{
			var query =
				from user in db.Users
				select new
				{
				user.Name, Enums = new
				{
				user.Enum1, user.Enum2
				}

				, RoleName = user.Role.Name, ComponentProperty = user.Component.Property1
				}

			;
			var list = await (query.ToListAsync());
			Assert.AreEqual(3, list.Count);
			//assert role names -- to ensure that the correct values were used to invoke constructor
			Assert.IsTrue(list.All(u => u.RoleName == "Admin" || u.RoleName == "User" || String.IsNullOrEmpty(u.RoleName)));
		}

		[Test]
		public async Task CanSelectNestedMemberInitExpressionAsync()
		{
			var query =
				from user in db.Users
				select new UserDto(user.Id, user.Name)
				{InvalidLoginAttempts = user.InvalidLoginAttempts, Dto2 = new UserDto2{RegisteredAt = user.RegisteredAt, Enum = user.Enum2}, RoleName = user.Role.Name};
			var list = await (query.ToListAsync());
			Assert.AreEqual(3, list.Count);
			//assert role names -- to ensure that the correct values were used to invoke constructor
			Assert.IsTrue(list.All(u => u.RoleName == "Admin" || u.RoleName == "User" || String.IsNullOrEmpty(u.RoleName)));
		}

		[Test]
		public async Task CanSelectNestedMemberInitWithinNewExpressionAsync()
		{
			var query =
				from user in db.Users
				select new
				{
				user.Name, user.InvalidLoginAttempts, Dto = new UserDto2{RegisteredAt = user.RegisteredAt, Enum = user.Enum2}, RoleName = user.Role.Name
				}

			;
			var list = await (query.ToListAsync());
			Assert.AreEqual(3, list.Count);
			//assert role names -- to ensure that the correct values were used to invoke constructor
			Assert.IsTrue(list.All(u => u.RoleName == "Admin" || u.RoleName == "User" || String.IsNullOrEmpty(u.RoleName)));
		}

		[Test]
		public async Task CanSelectSinglePropertyAsync()
		{
			var query =
				from user in db.Users
				where user.Name == "ayende"
				select user.RegisteredAt;
			DateTime date = await (query.SingleAsync());
			Assert.AreEqual(new DateTime(2010, 06, 17), date);
		}

		[Test]
		public async Task CanSelectBinaryExpressionsAsync()
		{
			var query =
				from user in db.Users
				select new
				{
				user.Name, IsSmall = (user.Enum1 == EnumStoredAsString.Small)}

			;
			var list = await (query.ToListAsync());
			foreach (var user in list)
			{
				if (user.Name == "rahien")
				{
					Assert.IsTrue(user.IsSmall);
				}
				else
				{
					Assert.IsFalse(user.IsSmall);
				}
			}
		}

		[Test]
		public async Task CanSelectWithMultipleBinaryExpressionsAsync()
		{
			var query =
				from user in db.Users
				select new
				{
				user.Name, IsAyende = (user.Enum1 == EnumStoredAsString.Medium && user.Enum2 == EnumStoredAsInt32.High)}

			;
			var list = await (query.ToListAsync());
			foreach (var user in list)
			{
				if (user.Name == "ayende")
				{
					Assert.IsTrue(user.IsAyende);
				}
				else
				{
					Assert.IsFalse(user.IsAyende);
				}
			}
		}

		[Test]
		public async Task CanSelectWithMultipleBinaryExpressionsWithOrAsync()
		{
			var query =
				from user in db.Users
				select new
				{
				user.Name, IsAyende = (user.Name == "ayende" || user.Name == "rahien")}

			;
			var list = await (query.ToListAsync());
			foreach (var user in list)
			{
				if (user.Name == "ayende" || user.Name == "rahien")
				{
					Assert.IsTrue(user.IsAyende);
				}
				else
				{
					Assert.IsFalse(user.IsAyende);
				}
			}
		}

		[Test]
		public async Task CanSelectWithAnySubQueryAsync()
		{
			var query =
				from timesheet in db.Timesheets
				select new
				{
				timesheet.Id, HasEntries = timesheet.Entries.Any()}

			;
			var list = await (query.ToListAsync());
			Assert.AreEqual(2, list.Count(t => t.HasEntries));
			Assert.AreEqual(1, list.Count(t => !t.HasEntries));
		}

		[Test]
		public async Task CanSelectFirstElementFromChildCollectionAsync()
		{
			Assert.ThrowsAsync<AssertionException>(async () =>
			{
				using (var log = new SqlLogSpy())
				{
					var orders = await (db.Customers.Select(customer => customer.Orders.OrderByDescending(x => x.OrderDate).First()).ToListAsync());
					Assert.That(orders, Has.Count.GreaterThan(0));
					var text = log.GetWholeLog();
					var count = text.Split(new[]{"SELECT"}, StringSplitOptions.None).Length - 1;
					Assert.That(count, Is.EqualTo(1));
				}
			}

			, KnownBug.Issue("NH-3045"));
		}

		[Test]
		public async Task CanProjectWithCastAsync()
		{
			// NH-2463
			// ReSharper disable RedundantCast
			var names1 = await (db.Users.Select(p => new
			{
			p1 = p.Name
			}

			).ToListAsync());
			Assert.AreEqual(3, names1.Count);
			var names2 = await (db.Users.Select(p => new
			{
			p1 = ((User)p).Name
			}

			).ToListAsync());
			Assert.AreEqual(3, names2.Count);
			var names3 = await (db.Users.Select(p => new
			{
			p1 = (p as User).Name
			}

			).ToListAsync());
			Assert.AreEqual(3, names3.Count);
			var names4 = await (db.Users.Select(p => new
			{
			p1 = ((IUser)p).Name
			}

			).ToListAsync());
			Assert.AreEqual(3, names4.Count);
			var names5 = await (db.Users.Select(p => new
			{
			p1 = (p as IUser).Name
			}

			).ToListAsync());
			Assert.AreEqual(3, names5.Count);
		// ReSharper restore RedundantCast
		}

		[Test]
		public async Task CanSelectAfterOrderByAndTakeAsync()
		{
			// NH-3320
			var names = await (db.Users.OrderBy(p => p.Name).Take(3).Select(p => p.Name).ToListAsync());
			Assert.AreEqual(3, names.Count);
		}

		[Test]
		public async Task CanSelectManyWithCastAsync()
		{
			// NH-2688
			// ReSharper disable RedundantCast
			var orders1 = await (db.Customers.Where(c => c.CustomerId == "VINET").SelectMany(o => o.Orders).ToListAsync());
			Assert.AreEqual(5, orders1.Count);
			//$exception	{"c.Orders is not mapped [.SelectMany[NHibernate.DomainModel.Northwind.Entities.Customer,NHibernate.DomainModel.Northwind.Entities.Order](.Where[NHibernate.DomainModel.Northwind.Entities.Customer](NHibernate.Linq.NhQueryable`1[NHibernate.DomainModel.Northwind.Entities.Customer], Quote((c, ) => (String.op_Equality(c.CustomerId, p1))), ), Quote((o, ) => (Convert(o.Orders))), )]"}	System.Exception {NHibernate.Hql.Ast.ANTLR.QuerySyntaxException} 
			// Block OData navigation to detail request requests like 
			// http://localhost:2711/TestWcfDataService.svc/TestEntities(guid&#39;0dd52f6c-1943-4013-a88e-3b63a1fbe11b&#39;)/Details1 
			var orders2 = await (db.Customers.Where(c => c.CustomerId == "VINET").SelectMany(o => (ISet<Order>)o.Orders).ToListAsync());
			Assert.AreEqual(5, orders2.Count);
			//$exception	{"([100001].Orders As ISet`1)"}	System.Exception {System.NotSupportedException} 
			var orders3 = await (db.Customers.Where(c => c.CustomerId == "VINET").SelectMany(o => (o.Orders as ISet<Order>)).ToListAsync());
			Assert.AreEqual(5, orders3.Count);
			var orders4 = await (db.Customers.Where(c => c.CustomerId == "VINET").SelectMany(o => (IEnumerable<Order>)o.Orders).ToListAsync());
			Assert.AreEqual(5, orders4.Count);
			var orders5 = await (db.Customers.Where(c => c.CustomerId == "VINET").SelectMany(o => (o.Orders as IEnumerable<Order>)).ToListAsync());
			Assert.AreEqual(5, orders5.Count);
		// ReSharper restore RedundantCast
		}

		[Test]
		public async Task CanSelectCollectionAsync()
		{
			var orders = await (db.Customers.Where(c => c.CustomerId == "VINET").Select(o => o.Orders).ToListAsync());
			Assert.AreEqual(5, orders[0].Count);
		}

		[Test]
		public async Task CanSelectConditionalKnownTypesAsync()
		{
			var moreThanTwoOrderLinesBool = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? true : false
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesBool.Count(x => x.HasMoreThanTwo == true), Is.EqualTo(410));
			var moreThanTwoOrderLinesNBool = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? true : (bool ? )null
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesNBool.Count(x => x.HasMoreThanTwo == true), Is.EqualTo(410));
			var moreThanTwoOrderLinesShort = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? (short)1 : (short)0
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesShort.Count(x => x.HasMoreThanTwo == 1), Is.EqualTo(410));
			var moreThanTwoOrderLinesNShort = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? (short ? )1 : (short ? )null
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesNShort.Count(x => x.HasMoreThanTwo == 1), Is.EqualTo(410));
			var moreThanTwoOrderLinesInt = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? 1 : 0
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesInt.Count(x => x.HasMoreThanTwo == 1), Is.EqualTo(410));
			var moreThanTwoOrderLinesNInt = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? 1 : (int ? )null
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesNInt.Count(x => x.HasMoreThanTwo == 1), Is.EqualTo(410));
			var moreThanTwoOrderLinesDecimal = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? 1m : 0m
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesDecimal.Count(x => x.HasMoreThanTwo == 1m), Is.EqualTo(410));
			var moreThanTwoOrderLinesNDecimal = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? 1m : (decimal ? )null
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesNDecimal.Count(x => x.HasMoreThanTwo == 1m), Is.EqualTo(410));
			var moreThanTwoOrderLinesSingle = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? 1f : 0f
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesSingle.Count(x => x.HasMoreThanTwo == 1f), Is.EqualTo(410));
			var moreThanTwoOrderLinesNSingle = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? 1f : (float ? )null
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesNSingle.Count(x => x.HasMoreThanTwo == 1f), Is.EqualTo(410));
			var moreThanTwoOrderLinesDouble = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? 1d : 0d
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesDouble.Count(x => x.HasMoreThanTwo == 1d), Is.EqualTo(410));
			var moreThanTwoOrderLinesNDouble = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? 1d : (double ? )null
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesNDouble.Count(x => x.HasMoreThanTwo == 1d), Is.EqualTo(410));
			var moreThanTwoOrderLinesString = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? "yes" : "no"
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesString.Count(x => x.HasMoreThanTwo == "yes"), Is.EqualTo(410));
			var now = DateTime.Now.Date;
			var moreThanTwoOrderLinesDateTime = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? o.OrderDate.Value : now
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesDateTime.Count(x => x.HasMoreThanTwo != now), Is.EqualTo(410));
			var moreThanTwoOrderLinesNDateTime = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? o.OrderDate : null
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesNDateTime.Count(x => x.HasMoreThanTwo != null), Is.EqualTo(410));
			var moreThanTwoOrderLinesGuid = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? o.Shipper.Reference : Guid.Empty
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesGuid.Count(x => x.HasMoreThanTwo != Guid.Empty), Is.EqualTo(410));
			var moreThanTwoOrderLinesNGuid = await (db.Orders.Select(o => new
			{
			Id = o.OrderId, HasMoreThanTwo = o.OrderLines.Count() > 2 ? o.Shipper.Reference : (Guid? )null
			}

			).ToListAsync());
			Assert.That(moreThanTwoOrderLinesNGuid.Count(x => x.HasMoreThanTwo != null), Is.EqualTo(410));
		}

		[Test]
		public async Task CanSelectConditionalEntityAsync()
		{
			var fatherInsteadOfChild = await (db.Animals.Select(a => a.Father.SerialNumber == "5678" ? a.Father : a).ToListAsync());
			Assert.That(fatherInsteadOfChild, Has.Exactly(2).With.Property("SerialNumber").EqualTo("5678"));
		}

		[Test]
		public async Task CanSelectConditionalObjectAsync()
		{
			var fatherIsKnown = await (db.Animals.Select(a => new
			{
			a.SerialNumber, Superior = a.Father.SerialNumber, FatherIsKnown = a.Father.SerialNumber == "5678" ? (object)true : (object)false
			}

			).ToListAsync());
			Assert.That(fatherIsKnown, Has.Exactly(1).With.Property("FatherIsKnown").True);
		}

		[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
		public partial class Wrapper<T>
		{
			public T item;
			public string message;
		}
	}
}
#endif
