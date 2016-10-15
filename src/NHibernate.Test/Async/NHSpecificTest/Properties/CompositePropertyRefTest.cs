﻿#if NET_4_5
using System;
using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.Properties
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class CompositePropertyRefTestAsync : BugTestCaseAsync
	{
		private long p_id;
		private long p2_id;
		protected override async Task OnSetUpAsync()
		{
			using (var s = OpenSession())
			{
				using (var tx = s.BeginTransaction())
				{
					var p = new Person{Name = "Steve", UserId = "steve"};
					var a = new Address{Addr = "Texas", Country = "USA", Person = p};
					var p2 = new Person{Name = "Max", UserId = "max"};
					var act = new Account{Type = Convert.ToChar("c"), User = p2};
					p2.Accounts.Add(act);
					p_id = (long)await (s.SaveAsync(p));
					await (s.SaveAsync(a));
					p2_id = (long)await (s.SaveAsync(p2));
					await (s.SaveAsync(act));
					await (tx.CommitAsync());
				}
			}
		}

		protected override async Task OnTearDownAsync()
		{
			using (var s = OpenSession())
			{
				using (var tx = s.BeginTransaction())
				{
					await (s.DeleteAsync("from Account"));
					await (s.DeleteAsync("from Address"));
					await (s.DeleteAsync("from Person"));
					await (tx.CommitAsync());
				}
			}
		}

		[Test]
		public async Task MappingOuterJoinAsync()
		{
			using (var s = OpenSession())
			{
				using (s.BeginTransaction())
				{
					var p = await (s.GetAsync<Person>(p_id)); //get address reference by outer join
					var p2 = await (s.GetAsync<Person>(p2_id)); //get null address reference by outer join
					Assert.IsNull(p2.Address);
					Assert.IsNotNull(p.Address);
					var l = await (s.CreateQuery("from Person").ListAsync()); //pull address references for cache
					Assert.AreEqual(l.Count, 2);
					Assert.IsTrue(l.Contains(p) && l.Contains(p2));
				}
			}
		}

		[Test]
		public async Task AddressBySequentialSelectAsync()
		{
			using (var s = OpenSession())
			{
				using (s.BeginTransaction())
				{
					var l = await (s.CreateQuery("from Person p order by p.Name").ListAsync<Person>());
					Assert.AreEqual(l.Count, 2);
					Assert.IsNull(l[0].Address);
					Assert.IsNotNull(l[1].Address);
				}
			}
		}

		[Test]
		public async Task AddressOuterJoinAsync()
		{
			using (var s = OpenSession())
			{
				using (s.BeginTransaction())
				{
					var l = await (s.CreateQuery("from Person p left join fetch p.Address a order by a.Country").ListAsync<Person>());
					Assert.AreEqual(l.Count, 2);
					if (l[0].Name.Equals("Max"))
					{
						Assert.IsNull(l[0].Address);
						Assert.IsNotNull(l[1].Address);
					}
					else
					{
						Assert.IsNull(l[1].Address);
						Assert.IsNotNull(l[0].Address);
					}
				}
			}
		}

		[Test]
		public async Task AccountsOuterJoinAsync()
		{
			using (var s = OpenSession())
			{
				using (s.BeginTransaction())
				{
					var l = await (s.CreateQuery("from Person p left join p.Accounts").ListAsync());
					for (var i = 0; i < 2; i++)
					{
						var row = (object[])l[i];
						var px = (Person)row[0];
						var accounts = px.Accounts;
						Assert.IsFalse(NHibernateUtil.IsInitialized(accounts));
						Assert.IsTrue(px.Accounts.Count > 0 || row[1] == null);
					}
				}
			}
		}

		[Test]
		public async Task AccountsOuterJoinVerifyInitializationAsync()
		{
			using (var s = OpenSession())
			{
				using (s.BeginTransaction())
				{
					var l = await (s.CreateQuery("from Person p left join fetch p.Accounts a order by p.Name").ListAsync<Person>());
					var p0 = l[0];
					Assert.IsTrue(NHibernateUtil.IsInitialized(p0.Accounts));
					Assert.AreEqual(p0.Accounts.Count, 1);
					Assert.AreSame(p0.Accounts.First().User, p0);
					var p1 = l[1];
					Assert.IsTrue(NHibernateUtil.IsInitialized(p1.Accounts));
					Assert.AreEqual(p1.Accounts.Count, 0);
				}
			}
		}
	}
}
#endif