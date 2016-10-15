﻿#if NET_4_5
using System.Collections;
using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;
using System.Linq.Expressions;
using System;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.NHSpecificTest.NH2664
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
				return new[]{"NHSpecificTest.NH2664.Mappings.hbm.xml"};
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
					Product product = new Product();
					product.ProductId = "1";
					product.Properties["Name"] = "First Product";
					product.Properties["Description"] = "First Description";
					await (session.SaveAsync(product));
					product = new Product();
					product.ProductId = "2";
					product.Properties["Name"] = "Second Product";
					product.Properties["Description"] = "Second Description";
					await (session.SaveAsync(product));
					product = new Product();
					product.ProductId = "3";
					product.Properties["Name"] = "val";
					product.Properties["Description"] = "val";
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
		public async Task Query_DynamicComponentAsync()
		{
			using (var session = OpenSession())
			{
				var product = await ((
					from p in session.Query<Product>()where p.Properties["Name"] == "First Product"
					select p).SingleAsync());
				Assert.IsNotNull(product);
				Assert.AreEqual("First Product", product.Properties["Name"]);
			}
		}

		[Test]
		public async Task Multiple_Query_Does_Not_CacheAsync()
		{
			using (var session = OpenSession())
			{
				// Query by name
				var product1 = await ((
					from p in session.Query<Product>()where p.Properties["Name"] == "First Product"
					select p).SingleAsync());
				Assert.That(product1.ProductId, Is.EqualTo("1"));
				// Query by description (this test is to verify that the dictionary
				// index isn't cached from the query above.
				var product2 = await ((
					from p in session.Query<Product>()where p.Properties["Description"] == "Second Description"
					select p).SingleAsync());
				Assert.That(product2.ProductId, Is.EqualTo("2"));
			}
		}
	}
}
#endif