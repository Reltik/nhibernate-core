﻿#if NET_4_5
using System.Collections;
using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;
using System.Linq.Expressions;
using System;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.NHSpecificTest.NH3571
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FixtureAsync : TestCaseAsync
	{
		protected override string MappingsAssembly
		{
			get
			{
				return "NHibernate.Test";
			}
		}

		protected override IList Mappings
		{
			get
			{
				return new[]{"NHSpecificTest.NH3571.Mappings.hbm.xml"};
			}
		}

		/// <summary>
		/// push some data into the database
		/// Really functions as a save test also 
		/// </summary>
		protected override async Task OnSetUpAsync()
		{
			await (base.OnSetUpAsync());
			using (var session = OpenSession())
			{
				using (var tran = session.BeginTransaction())
				{
					var product = new Product{ProductId = "1"};
					product.Details.Properties["Name"] = "First Product";
					product.Details.Properties["Description"] = "First Description";
					await (session.SaveAsync(product));
					product = new Product{ProductId = "2"};
					product.Details.Properties["Name"] = "Second Product";
					product.Details.Properties["Description"] = "Second Description";
					await (session.SaveAsync(product));
					product = new Product{ProductId = "3"};
					product.Details.Properties["Name"] = "val";
					product.Details.Properties["Description"] = "val";
					await (session.SaveAsync(product));
					await (tran.CommitAsync());
				}
			}
		}

		protected override async Task OnTearDownAsync()
		{
			await (base.OnTearDownAsync());
			using (var session = OpenSession())
			{
				using (var tran = session.BeginTransaction())
				{
					await (session.DeleteAsync("from Product"));
					await (tran.CommitAsync());
				}
			}
		}

		[Test]
		public async Task CanQueryDynamicComponentInComponentAsync()
		{
			using (var session = OpenSession())
			{
				var query = session.CreateQuery("from Product p where p.Details.Properties.Name=:name");
				query.SetString("name", "First Product");
				//var product = query.List<Product>().FirstOrDefault();
				var product = await ((
					from p in session.Query<Product>()where (string)p.Details.Properties["Name"] == "First Product"
					select p).SingleAsync());
				Assert.IsNotNull(product);
				Assert.AreEqual("First Product", product.Details.Properties["Name"]);
			}
		}

		[Test]
		public async Task MultipleQueriesShouldNotCacheAsync()
		{
			using (var session = OpenSession())
			{
				// Query by name
				var product1 = await ((
					from p in session.Query<Product>()where (string)p.Details.Properties["Name"] == "First Product"
					select p).SingleAsync());
				Assert.That(product1.ProductId, Is.EqualTo("1"));
				// Query by description (this test is to verify that the dictionary
				// index isn't cached from the query above.
				var product2 = await ((
					from p in session.Query<Product>()where (string)p.Details.Properties["Description"] == "Second Description"
					select p).SingleAsync());
				Assert.That(product2.ProductId, Is.EqualTo("2"));
			}
		}
	}
}
#endif
