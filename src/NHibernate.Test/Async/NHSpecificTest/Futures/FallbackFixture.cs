#if NET_4_5
using System.Linq;
using NHibernate.Cfg;
using NHibernate.Connection;
using NHibernate.Criterion;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Linq;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.Futures
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FallbackFixture : FutureFixture
	{
		[Test]
		public async Task FutureOfCriteriaFallsBackToListImplementationWhenQueryBatchingIsNotSupportedAsync()
		{
			using (var session = sessions.OpenSession())
			{
				var results = await (session.CreateCriteria<Person>().FutureAsync<Person>());
				results.GetEnumerator().MoveNext();
			}
		}

		[Test]
		public async Task FutureValueOfCriteriaCanGetSingleEntityWhenQueryBatchingIsNotSupportedAsync()
		{
			int personId = await (CreatePersonAsync());
			using (var session = sessions.OpenSession())
			{
				var futurePerson = await (session.CreateCriteria<Person>().Add(Restrictions.Eq("Id", personId)).FutureValueAsync<Person>());
				Assert.IsNotNull(futurePerson.Value);
			}
		}

		[Test]
		public async Task FutureValueOfCriteriaCanGetScalarValueWhenQueryBatchingIsNotSupportedAsync()
		{
			await (CreatePersonAsync());
			using (var session = sessions.OpenSession())
			{
				var futureCount = await (session.CreateCriteria<Person>().SetProjection(Projections.RowCount()).FutureValueAsync<int>());
				Assert.That(futureCount.Value, Is.EqualTo(1));
			}
		}

		[Test]
		public async Task FutureOfQueryFallsBackToListImplementationWhenQueryBatchingIsNotSupportedAsync()
		{
			using (var session = sessions.OpenSession())
			{
				var results = await (session.CreateQuery("from Person").FutureAsync<Person>());
				results.GetEnumerator().MoveNext();
			}
		}

		[Test]
		public async Task FutureValueOfQueryCanGetSingleEntityWhenQueryBatchingIsNotSupportedAsync()
		{
			int personId = await (CreatePersonAsync());
			using (var session = sessions.OpenSession())
			{
				var futurePerson = await (session.CreateQuery("from Person where Id = :id").SetInt32("id", personId).FutureValueAsync<Person>());
				Assert.IsNotNull(futurePerson.Value);
			}
		}

		[Test]
		public async Task FutureValueOfQueryCanGetScalarValueWhenQueryBatchingIsNotSupportedAsync()
		{
			await (CreatePersonAsync());
			using (var session = sessions.OpenSession())
			{
				var futureCount = await (session.CreateQuery("select count(*) from Person").FutureValueAsync<long>());
				Assert.That(futureCount.Value, Is.EqualTo(1L));
			}
		}

		[Test]
		public async Task FutureValueOfLinqCanGetSingleEntityWhenQueryBatchingIsNotSupportedAsync()
		{
			var personId = await (CreatePersonAsync());
			using (var session = sessions.OpenSession())
			{
				var futurePerson = session.Query<Person>().Where(x => x.Id == personId).ToFutureValue();
				Assert.IsNotNull(futurePerson.Value);
			}
		}

		private async Task<int> CreatePersonAsync()
		{
			using (var session = sessions.OpenSession())
			{
				var person = new Person();
				await (session.SaveAsync(person));
				await (session.FlushAsync());
				return person.Id;
			}
		}
	}
}
#endif
