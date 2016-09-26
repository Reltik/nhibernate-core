#if NET_4_5
using System.Collections;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Stat;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Stats
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class StatsFixtureAsync : TestCaseAsync
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
				return new string[]{"Stats.Continent.hbm.xml"};
			}
		}

		protected override void Configure(Cfg.Configuration configuration)
		{
			configuration.SetProperty(Cfg.Environment.GenerateStatistics, "true");
		}

		private static async Task<Continent> FillDbAsync(ISession s)
		{
			Continent europe = new Continent();
			europe.Name = "Europe";
			Country france = new Country();
			france.Name = "France";
			europe.Countries = new HashSet<Country>();
			europe.Countries.Add(france);
			await (s.SaveAsync(france));
			await (s.SaveAsync(europe));
			return europe;
		}

		private static async Task CleanDbAsync(ISession s)
		{
			await (s.DeleteAsync("from Locality"));
			await (s.DeleteAsync("from Country"));
			await (s.DeleteAsync("from Continent"));
		}

		[Test]
		public async Task CollectionFetchVsLoadAsync()
		{
			IStatistics stats = sessions.Statistics;
			stats.Clear();
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			Continent europe = await (FillDbAsync(s));
			await (tx.CommitAsync());
			s.Clear();
			tx = s.BeginTransaction();
			Assert.AreEqual(0, stats.CollectionLoadCount);
			Assert.AreEqual(0, stats.CollectionFetchCount);
			Continent europe2 = await (s.GetAsync<Continent>(europe.Id));
			Assert.AreEqual(0, stats.CollectionLoadCount, "Lazy true: no collection should be loaded");
			Assert.AreEqual(0, stats.CollectionFetchCount);
			int cc = europe2.Countries.Count;
			Assert.AreEqual(1, stats.CollectionLoadCount);
			Assert.AreEqual(1, stats.CollectionFetchCount, "Explicit fetch of the collection state");
			await (tx.CommitAsync());
			s.Close();
			s = OpenSession();
			tx = s.BeginTransaction();
			stats.Clear();
			europe = await (FillDbAsync(s));
			await (tx.CommitAsync());
			s.Clear();
			tx = s.BeginTransaction();
			Assert.AreEqual(0, stats.CollectionLoadCount);
			Assert.AreEqual(0, stats.CollectionFetchCount);
			europe2 = await (s.CreateQuery("from Continent a join fetch a.Countries where a.id = " + europe.Id).UniqueResultAsync<Continent>());
			Assert.AreEqual(1, stats.CollectionLoadCount);
			Assert.AreEqual(0, stats.CollectionFetchCount, "collection should be loaded in the same query as its parent");
			await (tx.CommitAsync());
			s.Close();
			Mapping.Collection coll = cfg.GetCollectionMapping("NHibernate.Test.Stats.Continent.Countries");
			coll.FetchMode = FetchMode.Join;
			coll.IsLazy = false;
			ISessionFactory sf = cfg.BuildSessionFactory();
			stats = sf.Statistics;
			stats.Clear();
			stats.IsStatisticsEnabled = true;
			s = sf.OpenSession();
			tx = s.BeginTransaction();
			europe = await (FillDbAsync(s));
			await (tx.CommitAsync());
			s.Clear();
			tx = s.BeginTransaction();
			Assert.AreEqual(0, stats.CollectionLoadCount);
			Assert.AreEqual(0, stats.CollectionFetchCount);
			europe2 = await (s.GetAsync<Continent>(europe.Id));
			Assert.AreEqual(1, stats.CollectionLoadCount);
			Assert.AreEqual(0, stats.CollectionFetchCount, "Should do direct load, not indirect second load when lazy false and JOIN");
			await (tx.CommitAsync());
			s.Close();
			await (sf.CloseAsync());
			coll = cfg.GetCollectionMapping("NHibernate.Test.Stats.Continent.Countries");
			coll.FetchMode = FetchMode.Select;
			coll.IsLazy = false;
			sf = cfg.BuildSessionFactory();
			stats = sf.Statistics;
			stats.Clear();
			stats.IsStatisticsEnabled = true;
			s = sf.OpenSession();
			tx = s.BeginTransaction();
			europe = await (FillDbAsync(s));
			await (tx.CommitAsync());
			s.Clear();
			tx = s.BeginTransaction();
			Assert.AreEqual(0, stats.CollectionLoadCount);
			Assert.AreEqual(0, stats.CollectionFetchCount);
			europe2 = await (s.GetAsync<Continent>(europe.Id));
			Assert.AreEqual(1, stats.CollectionLoadCount);
			Assert.AreEqual(1, stats.CollectionFetchCount, "Should do explicit collection load, not part of the first one");
			foreach (Country country in europe2.Countries)
			{
				await (s.DeleteAsync(country));
			}

			await (CleanDbAsync(s));
			await (tx.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task QueryStatGatheringAsync()
		{
			IStatistics stats = sessions.Statistics;
			stats.Clear();
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			await (FillDbAsync(s));
			await (tx.CommitAsync());
			s.Close();
			s = OpenSession();
			tx = s.BeginTransaction();
			string continents = "from Continent";
			int results = (await (s.CreateQuery(continents).ListAsync())).Count;
			QueryStatistics continentStats = stats.GetQueryStatistics(continents);
			Assert.IsNotNull(continentStats, "stats were null");
			Assert.AreEqual(1, continentStats.ExecutionCount, "unexpected execution count");
			Assert.AreEqual(results, continentStats.ExecutionRowCount, "unexpected row count");
			var maxTime = continentStats.ExecutionMaxTime;
			Assert.AreEqual(maxTime, stats.QueryExecutionMaxTime);
			Assert.AreEqual(continents, stats.QueryExecutionMaxTimeQueryString);
			IEnumerable itr = await (s.CreateQuery(continents).EnumerableAsync());
			// Enumerable() should increment the execution count
			Assert.AreEqual(2, continentStats.ExecutionCount, "unexpected execution count");
			// but should not effect the cumulative row count
			Assert.AreEqual(results, continentStats.ExecutionRowCount, "unexpected row count");
			NHibernateUtil.Close(itr);
			await (tx.CommitAsync());
			s.Close();
			// explicitly check that statistics for "split queries" get collected
			// under the original query
			stats.Clear();
			s = OpenSession();
			tx = s.BeginTransaction();
			string localities = "from Locality";
			results = (await (s.CreateQuery(localities).ListAsync())).Count;
			QueryStatistics localityStats = stats.GetQueryStatistics(localities);
			Assert.IsNotNull(localityStats, "stats were null");
			// ...one for each split query
			Assert.AreEqual(2, localityStats.ExecutionCount, "unexpected execution count");
			Assert.AreEqual(results, localityStats.ExecutionRowCount, "unexpected row count");
			maxTime = localityStats.ExecutionMaxTime;
			Assert.AreEqual(maxTime, stats.QueryExecutionMaxTime);
			Assert.AreEqual(localities, stats.QueryExecutionMaxTimeQueryString);
			await (tx.CommitAsync());
			s.Close();
			Assert.IsFalse(s.IsOpen);
			// native sql queries
			stats.Clear();
			s = OpenSession();
			tx = s.BeginTransaction();
			string sql = "select Id, Name from Country";
			results = (await (s.CreateSQLQuery(sql).AddEntity(typeof (Country)).ListAsync())).Count;
			QueryStatistics sqlStats = stats.GetQueryStatistics(sql);
			Assert.IsNotNull(sqlStats, "sql stats were null");
			Assert.AreEqual(1, sqlStats.ExecutionCount, "unexpected execution count");
			Assert.AreEqual(results, sqlStats.ExecutionRowCount, "unexpected row count");
			maxTime = sqlStats.ExecutionMaxTime;
			Assert.AreEqual(maxTime, stats.QueryExecutionMaxTime);
			Assert.AreEqual(sql, stats.QueryExecutionMaxTimeQueryString);
			await (tx.CommitAsync());
			s.Close();
			s = OpenSession();
			tx = s.BeginTransaction();
			await (CleanDbAsync(s));
			await (tx.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task IncrementQueryExecutionCount_WhenExplicitQueryIsExecutedAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					await (FillDbAsync(s));
					await (tx.CommitAsync());
				}

			IStatistics stats = sessions.Statistics;
			stats.Clear();
			using (ISession s = OpenSession())
			{
				var r = await (s.CreateCriteria<Country>().ListAsync());
			}

			Assert.AreEqual(1, stats.QueryExecutionCount);
			stats.Clear();
			using (ISession s = OpenSession())
			{
				var r = await (s.CreateQuery("from Country").ListAsync());
			}

			Assert.AreEqual(1, stats.QueryExecutionCount);
			stats.Clear();
			var driver = sessions.ConnectionProvider.Driver;
			if (driver.SupportsMultipleQueries)
			{
				using (var s = OpenSession())
				{
					var r = await (s.CreateMultiQuery().Add("from Country").Add("from Continent").ListAsync());
				}

				Assert.AreEqual(1, stats.QueryExecutionCount);
				stats.Clear();
				using (var s = OpenSession())
				{
					var r = await (s.CreateMultiCriteria().Add(DetachedCriteria.For<Country>()).Add(DetachedCriteria.For<Continent>()).ListAsync());
				}

				Assert.AreEqual(1, stats.QueryExecutionCount);
			}

			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					await (CleanDbAsync(s));
					await (tx.CommitAsync());
				}
		}
	}
}
#endif
