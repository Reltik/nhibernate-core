#if NET_4_5
using NHibernate.Driver;
using NHibernate.Linq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.Futures
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class LinqFutureFixture : FutureFixture
	{
		[Test]
		public async Task CoalesceShouldWorkForFuturesAsync()
		{
			int personId;
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					var p1 = new Person{Name = "inserted name"};
					var p2 = new Person{Name = null};
					await (s.SaveAsync(p1));
					await (s.SaveAsync(p2));
					personId = p2.Id;
					await (tx.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (s.BeginTransaction())
				{
					var person = s.Query<Person>().Where(p => (p.Name ?? "e") == "e").ToFutureValue();
					Assert.AreEqual(personId, person.Value.Id);
				}

			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					await (s.DeleteAsync("from Person"));
					await (tx.CommitAsync());
				}
		}

		[Test]
		public async Task CanUseSkipAndFetchManyWithToFutureAsync()
		{
			IgnoreThisTestIfMultipleQueriesArentSupportedByDriver();
			using (var s = sessions.OpenSession())
				using (var tx = s.BeginTransaction())
				{
					var p1 = new Person{Name = "Parent"};
					var p2 = new Person{Parent = p1, Name = "Child"};
					p1.Children.Add(p2);
					await (s.SaveAsync(p1));
					await (s.SaveAsync(p2));
					await (tx.CommitAsync());
					s.Clear(); // we don't want caching
				}

			using (var s = sessions.OpenSession())
			{
				var persons10 = s.Query<Person>().FetchMany(p => p.Children).Skip(5).Take(10).ToFuture();
				var persons5 = s.Query<Person>().ToFuture();
				using (var logSpy = new SqlLogSpy())
				{
					foreach (var person in persons5)
					{
					}

					foreach (var person in persons10)
					{
					}

					var events = logSpy.Appender.GetEvents();
					Assert.AreEqual(1, events.Length);
				}
			}

			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					await (s.DeleteAsync("from Person"));
					await (tx.CommitAsync());
				}
		}

		[Test]
		public async Task CanUseFutureFetchQueryAsync()
		{
			IgnoreThisTestIfMultipleQueriesArentSupportedByDriver();
			using (var s = sessions.OpenSession())
				using (var tx = s.BeginTransaction())
				{
					var p1 = new Person{Name = "Parent"};
					var p2 = new Person{Parent = p1, Name = "Child"};
					p1.Children.Add(p2);
					await (s.SaveAsync(p1));
					await (s.SaveAsync(p2));
					await (tx.CommitAsync());
					s.Clear(); // we don't want caching
				}

			using (var s = sessions.OpenSession())
			{
				var persons = s.Query<Person>().FetchMany(p => p.Children).ToFuture();
				var persons10 = s.Query<Person>().FetchMany(p => p.Children).Take(10).ToFuture();
				using (var logSpy = new SqlLogSpy())
				{
					Assert.That(persons.Any(x => x.Children.Any()), "No children found");
					Assert.That(persons10.Any(x => x.Children.Any()), "No children found");
					var events = logSpy.Appender.GetEvents();
					Assert.AreEqual(1, events.Length);
				}
			}

			using (var s = OpenSession())
				using (var tx = s.BeginTransaction())
				{
					await (s.DeleteAsync("from Person"));
					await (tx.CommitAsync());
				}
		}

		[Test(Description = "NH-2385")]
		public async Task CanCombineSingleFutureValueWithFetchManyAsync()
		{
			int personId;
			using (var s = OpenSession())
				using (var tx = s.BeginTransaction())
				{
					var p1 = new Person{Name = "inserted name"};
					var p2 = new Person{Name = null};
					await (s.SaveAsync(p1));
					await (s.SaveAsync(p2));
					personId = p2.Id;
					await (tx.CommitAsync());
				}

			using (var s = sessions.OpenSession())
			{
				var meContainer = s.Query<Person>().Where(x => x.Id == personId).FetchMany(x => x.Children).ToFutureValue();
				Assert.AreEqual(personId, meContainer.Value.Id);
			}

			using (var s = OpenSession())
				using (var tx = s.BeginTransaction())
				{
					await (s.DeleteAsync("from Person"));
					await (tx.CommitAsync());
				}
		}
	}
}
#endif
