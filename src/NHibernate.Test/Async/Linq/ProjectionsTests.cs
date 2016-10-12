﻿#if NET_4_5
using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.DomainModel.Northwind.Entities;
using NUnit.Framework;
using NHibernate.Linq;
using System.Threading.Tasks;

namespace NHibernate.Test.Linq
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class ProjectionsTestsAsync : LinqTestCaseAsync
	{
		[Test]
		public async Task ProjectAnonymousTypeWithWhereAsync()
		{
			var query = await ((
				from user in db.Users
				where user.Name == "ayende"
				select user.Name).FirstAsync());
			Assert.AreEqual("ayende", query);
		}

		[Test]
		public async Task ProjectConditionalsAsync()
		{
			var query = await ((
				from user in db.Users
				orderby user.Id
				select new
				{
				user.Id, GreaterThan2 = user.Id > 2 ? "Yes" : "No"
				}

			).ToListAsync());
			Assert.AreEqual("No", query[0].GreaterThan2);
			Assert.AreEqual("No", query[1].GreaterThan2);
			Assert.AreEqual("Yes", query[2].GreaterThan2);
		}

		[Test]
		public async Task ProjectAnonymousTypeWithMultiplyAsync()
		{
			var query = await ((
				from user in db.Users
				select new
				{
				user.Name, user.Id, Id2 = user.Id * 2
				}

			).ToListAsync());
			Assert.AreEqual(3, query.Count);
			foreach (var user in query)
			{
				Assert.AreEqual(user.Id * 2, user.Id2);
			}
		}

		[Test]
		public async Task ProjectAnonymousTypeWithSubstractionAsync()
		{
			var query = await ((
				from user in db.Users
				select new
				{
				user.Name, user.Id, Id2 = user.Id - 2
				}

			).ToListAsync());
			Assert.AreEqual(3, query.Count);
			foreach (var user in query)
			{
				Assert.AreEqual(user.Id - 2, user.Id2);
			}
		}

		[Test]
		public async Task ProjectAnonymousTypeWithDivisionAsync()
		{
			var query = await ((
				from user in db.Users
				select new
				{
				user.Name, user.Id, Id2 = (user.Id * 10) / 2
				}

			).ToListAsync());
			Assert.AreEqual(3, query.Count);
			foreach (var user in query)
			{
				Assert.AreEqual((user.Id * 10) / 2, user.Id2);
			}
		}

		[Test]
		public async Task ProjectAnonymousTypeWithAdditionAsync()
		{
			var query = await ((
				from user in db.Users
				select new
				{
				user.Name, user.Id, Id2 = (user.Id + 101)}

			).ToListAsync());
			Assert.AreEqual(3, query.Count);
			foreach (var user in query)
			{
				Assert.AreEqual((user.Id + 101), user.Id2);
			}
		}

		[Test]
		public async Task ProjectAnonymousTypeAndConcatenateFieldsAsync()
		{
			var query = await ((
				from user in db.Users
				orderby user.Name
				select new
				{
				DoubleName = user.Name + " " + user.Name, user.RegisteredAt
				}

			).ToListAsync());
			Assert.AreEqual("ayende ayende", query[0].DoubleName);
			Assert.AreEqual("nhibernate nhibernate", query[1].DoubleName);
			Assert.AreEqual("rahien rahien", query[2].DoubleName);
			Assert.AreEqual(new DateTime(2010, 06, 17), query[0].RegisteredAt);
			Assert.AreEqual(new DateTime(2000, 1, 1), query[1].RegisteredAt);
			Assert.AreEqual(new DateTime(1998, 12, 31), query[2].RegisteredAt);
		}

		[Test]
		public async Task ProjectKnownTypeAsync()
		{
			var query = await ((
				from user in db.Users
				orderby user.Id
				select new KeyValuePair<string, DateTime>(user.Name, user.RegisteredAt)).ToListAsync());
			Assert.AreEqual("ayende", query[0].Key);
			Assert.AreEqual("rahien", query[1].Key);
			Assert.AreEqual("nhibernate", query[2].Key);
			Assert.AreEqual(new DateTime(2010, 06, 17), query[0].Value);
			Assert.AreEqual(new DateTime(1998, 12, 31), query[1].Value);
			Assert.AreEqual(new DateTime(2000, 1, 1), query[2].Value);
		}

		[Test]
		public async Task ProjectAnonymousTypeAsync()
		{
			var query = await ((
				from user in db.Users
				orderby user.Id
				select new
				{
				user.Name, user.RegisteredAt
				}

			).ToListAsync());
			Assert.AreEqual("ayende", query[0].Name);
			Assert.AreEqual("rahien", query[1].Name);
			Assert.AreEqual("nhibernate", query[2].Name);
			Assert.AreEqual(new DateTime(2010, 06, 17), query[0].RegisteredAt);
			Assert.AreEqual(new DateTime(1998, 12, 31), query[1].RegisteredAt);
			Assert.AreEqual(new DateTime(2000, 1, 1), query[2].RegisteredAt);
		}

		[Test]
		public async Task ProjectUserNamesAsync()
		{
			var query = await ((
				from user in db.Users
				select user.Name).ToListAsync());
			Assert.AreEqual(3, query.Count);
			Assert.AreEqual(3, query.Intersect(new[]{"ayende", "rahien", "nhibernate"}).Count());
		}

		[Test]
		public async Task CanCallLocalMethodsInSelectAsync()
		{
			var query = await ((
				from user in db.Users
				orderby user.Id
				select FormatName(user.Name, user.LastLoginDate)).ToListAsync());
			Assert.AreEqual(3, query.Count);
			Assert.IsTrue(query[0].StartsWith("User ayende logged in at"));
			Assert.IsTrue(query[1].StartsWith("User rahien logged in at"));
			Assert.IsTrue(query[2].StartsWith("User nhibernate logged in at"));
		}

		[Test]
		public async Task CanCallLocalMethodsInAnonymousTypeInSelectAsync()
		{
			var query = await ((
				from user in db.Users
				orderby user.Id
				select new
				{
				Title = FormatName(user.Name, user.LastLoginDate)}

			).ToListAsync());
			Assert.AreEqual(3, query.Count);
			Assert.IsTrue(query[0].Title.StartsWith("User ayende logged in at"));
			Assert.IsTrue(query[1].Title.StartsWith("User rahien logged in at"));
			Assert.IsTrue(query[2].Title.StartsWith("User nhibernate logged in at"));
		}

		[Test]
		public async Task CanPerformStringOperationsInSelectAsync()
		{
			var query = await ((
				from user in db.Users
				orderby user.Id
				select new
				{
				Title = "User " + user.Name + " logged in at " + user.LastLoginDate
				}

			).ToListAsync());
			Assert.AreEqual(3, query.Count);
			Assert.IsTrue(query[0].Title.StartsWith("User ayende logged in at"));
			Assert.IsTrue(query[1].Title.StartsWith("User rahien logged in at"));
			Assert.IsTrue(query[2].Title.StartsWith("User nhibernate logged in at"));
		}

		[Test]
		public async Task CanUseConstantStringInProjectionAsync()
		{
			var query =
				from user in db.Users
				select new
				{
				user.Name, Category = "something"
				}

			;
			var firstUser = await (query.FirstAsync());
			Assert.IsNotNull(firstUser);
			Assert.That(firstUser.Category, Is.EqualTo("something"));
		}

		[Test]
		public async Task CanProjectManyCollectionsAsync()
		{
			var query = db.Orders.SelectMany(o => o.OrderLines);
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(2155));
		}

		[Test]
		public async Task CanProjectCollectionsAsync()
		{
			var query = db.Orders.Select(o => o.OrderLines);
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(830));
		}

		[Test]
		public async Task CanProjectCollectionsInsideAnonymousTypeAsync()
		{
			var query = db.Orders.Select(o => new
			{
			o.OrderId, o.OrderLines
			}

			);
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(830));
		}

		[Test]
		public async Task ProjectAnonymousTypeWithCollectionAsync()
		{
			// NH-3333
			// done by WCF DS: context.Orders.Expand(o => o.OrderLines) from the client 
			var query =
				from o in db.Orders
				select new
				{
				o, o.OrderLines
				}

			;
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(830));
			Assert.That(result[0].o.OrderLines, Is.EquivalentTo(result[0].OrderLines));
		}

		[Test]
		public async Task ProjectAnonymousTypeWithCollection1Async()
		{
			// NH-3333
			// done by WCF DS: context.Orders.Expand(o => o.OrderLines) from the client 
			var query =
				from o in db.Orders
				select new
				{
				o.OrderLines, o
				}

			;
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(830));
			Assert.That(result[0].o.OrderLines, Is.EquivalentTo(result[0].OrderLines));
		}

		[Test]
		public async Task ProjectAnonymousTypeWithCollection2Async()
		{
			// NH-3333
			// done by WCF DS: context.Orders.Expand(o => o.OrderLines) from the client 
			var query =
				from o in db.Orders
				select new
				{
				o.OrderLines, A = 1, B = 2
				}

			;
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(830));
		}

		[Test]
		public async Task ProjectAnonymousTypeWithCollection3Async()
		{
			// NH-3333
			// done by WCF DS: context.Orders.Expand(o => o.OrderLines) from the client 
			var query =
				from o in db.Orders
				select new
				{
				OrderLines = o.OrderLines.ToList()}

			;
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(830));
		}

		[Test]
		public async Task ProjectKnownTypeWithCollectionAsync()
		{
			var query =
				from o in db.Orders
				select new ExpandedWrapper<Order, ISet<OrderLine>>{ExpandedElement = o, ProjectedProperty0 = o.OrderLines, Description = "OrderLine", ReferenceDescription = "OrderLine"};
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(830));
			Assert.That(result[0].ExpandedElement.OrderLines, Is.EquivalentTo(result[0].ProjectedProperty0));
		}

		[Test]
		public async Task ProjectKnownTypeWithCollection2Async()
		{
			var query =
				from o in db.Orders
				select new ExpandedWrapper<Order, IEnumerable<OrderLine>>{ExpandedElement = o, ProjectedProperty0 = o.OrderLines.Select(x => x), Description = "OrderLine", ReferenceDescription = "OrderLine"};
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(830));
			Assert.That(result[0].ExpandedElement.OrderLines, Is.EquivalentTo(result[0].ProjectedProperty0));
		}

		[Test]
		public async Task ProjectNestedKnownTypeWithCollectionAsync()
		{
			var query =
				from o in db.Products
				select new ExpandedWrapper<Product, ExpandedWrapper<Supplier, IEnumerable<Product>>>{ExpandedElement = o, ProjectedProperty0 = new ExpandedWrapper<Supplier, IEnumerable<Product>>{ExpandedElement = o.Supplier, ProjectedProperty0 = o.Supplier.Products, Description = "Products", ReferenceDescription = ""}, Description = "Supplier", ReferenceDescription = "Supplier"};
			var result = await (query.ToListAsync());
			Assert.That(result, Has.Count.EqualTo(77));
			Assert.That(result[0].ExpandedElement.Supplier, Is.EqualTo(result[0].ProjectedProperty0.ExpandedElement));
			Assert.That(result[0].ExpandedElement.Supplier.Products, Is.EquivalentTo(result[0].ProjectedProperty0.ProjectedProperty0));
		}

		[Test]
		public async Task ProjectNestedAnonymousTypeWithCollectionAsync()
		{
			var query =
				from o in db.Products
				select new
				{
				ExpandedElement = o, ProjectedProperty0 = new
				{
				ExpandedElement = o.Supplier, ProjectedProperty0 = o.Supplier.Products, Description = "Products", ReferenceDescription = ""
				}

				, Description = "Supplier", ReferenceDescription = "Supplier"
				}

			;
			var result = await (query.ToListAsync());
			Assert.That(result, Has.Count.EqualTo(77));
			Assert.That(result[0].ExpandedElement.Supplier, Is.EqualTo(result[0].ProjectedProperty0.ExpandedElement));
			Assert.That(result[0].ExpandedElement.Supplier.Products, Is.EquivalentTo(result[0].ProjectedProperty0.ProjectedProperty0));
		}

		[Test]
		public async Task ProjectNestedAnonymousTypeWithProjectedCollectionAsync()
		{
			var query =
				from o in db.Products
				select new
				{
				ExpandedElement = o, ProjectedProperty0 = new
				{
				ExpandedElement = o.Supplier, ProjectedProperty0 = o.Supplier.Products.Select(x => new
				{
				x.Name
				}

				), Description = "Products", ReferenceDescription = ""
				}

				, Description = "Supplier", ReferenceDescription = "Supplier"
				}

			;
			var result = await (query.ToListAsync());
			Assert.That(result, Has.Count.EqualTo(77));
			Assert.That(result.Single(x => x.ExpandedElement.ProductId == 1).ProjectedProperty0.ProjectedProperty0.Count(), Is.EqualTo(3));
		}

		[Test]
		public async Task CanProjectComplexDictionaryIndexerAsync()
		{
			//NH-3000
			var lookup = new[]{1, 2, 3, 4}.ToDictionary(x => x, x => new
			{
			Codes = new[]{x}}

			);
			var query =
				from item in db.Users
				select new
				{
				index = Array.IndexOf(lookup[1 + item.Id % 4].Codes, 1 + item.Id % 4, 0) / 7, }

			;
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(3));
		}

		[Test]
		public async Task CanProjectComplexParameterDictionaryIndexerAsync()
		{
			//NH-3000
			var lookup = new[]{1, 2, 3, 4}.ToDictionary(x => x, x => new
			{
			Codes = new[]{x}}

			);
			var query =
				from item in db.Users
				select new
				{
				index = lookup[1 + item.Id % 4], }

			;
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(3));
		}

		[Test]
		public async Task CanProjectParameterDictionaryIndexerAsync()
		{
			//NH-3000
			var lookup = new[]{1, 2, 3, 4}.ToDictionary(x => x, x => x);
			var query =
				from item in db.Users
				select new
				{
				index = lookup[1 + item.Id % 4], }

			;
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(3));
		}

		[Test]
		public async Task CanProjectParameterDictionaryContainsKeyAsync()
		{
			//NH-3000
			var lookup = new[]{1, 2, 3, 4}.ToDictionary(x => x, x => x);
			var query =
				from item in db.Users
				select new
				{
				isPresent = lookup.ContainsKey(item.Id), }

			;
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(3));
		}

		[Test]
		public async Task CanProjectParameterArrayContainsAsync()
		{
			//NH-3000
			var lookup = new[]{1, 2, 3, 4};
			var query =
				from item in db.Users
				select new
				{
				isPresent = lookup.Contains(item.Id), }

			;
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(3));
		}

		[Test]
		public async Task CanProjectParameterStringContainsAsync()
		{
			//NH-3000
			var lookup = new[]{1, 2, 3, 4};
			var query =
				from item in db.Users
				select new
				{
				isPresent = lookup.Contains(item.Id), }

			;
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(3));
		}

		[Test]
		public async Task CanProjectParameterSubstringAsync()
		{
			//NH-3000
			const string value = "1234567890";
			var query =
				from item in db.Users
				select new
				{
				Start = item.Id % 10, Value = value.Substring(item.Id % 10), }

			;
			var result = await (query.ToListAsync());
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].Value, Is.EqualTo(value.Substring(result[0].Start)));
			Assert.That(result[1].Value, Is.EqualTo(value.Substring(result[1].Start)));
			Assert.That(result[2].Value, Is.EqualTo(value.Substring(result[2].Start)));
		}

		private string FormatName(string name, DateTime? lastLoginDate)
		{
			return string.Format("User {0} logged in at {1}", name, lastLoginDate);
		}

		[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
		partial 
		/// <summary>
		/// This mimic classes in System.Data.Services.Internal.
		/// </summary>
		class ExpandedWrapper<TExpandedElement>
		{
			public TExpandedElement ExpandedElement
			{
				get;
				set;
			}

			public string Description
			{
				get;
				set;
			}

			public string ReferenceDescription
			{
				get;
				set;
			}
		}

		[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
		partial 
		/// <summary>
		/// This mimic classes in System.Data.Services.Internal.
		/// </summary>
		class ExpandedWrapper<TExpandedElement, TProperty0> : ExpandedWrapper<TExpandedElement>
		{
			public TProperty0 ProjectedProperty0
			{
				get;
				set;
			}
		}
	}
}
#endif
