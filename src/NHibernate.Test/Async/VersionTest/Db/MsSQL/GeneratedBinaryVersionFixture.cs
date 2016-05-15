#if NET_4_5
using System.Collections;
using NHibernate.Dialect;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.VersionTest.Db.MsSQL
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class GeneratedBinaryVersionFixture : TestCase
	{
		[Test]
		public async Task ShouldRetrieveVersionAfterFlushAsync()
		{
			// Note : if you are using identity-style strategy the value of version
			// is available inmediately after save.
			var e = new SimpleVersioned{Something = "something"};
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					Assert.That(e.LastModified, Is.Null);
					await (s.SaveAsync(e));
					await (s.FlushAsync());
					Assert.That(e.LastModified, Is.Not.Null);
					await (s.DeleteAsync(e));
					await (tx.CommitAsync());
				}
			}
		}

		[Test]
		public async Task ShouldChangeAfterUpdateAsync()
		{
			object savedId = await (PersistANewSomethingAsync());
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					var fetched = await (s.GetAsync<SimpleVersioned>(savedId));
					var freshVersion = fetched.LastModified;
					fetched.Something = "make it dirty";
					await (s.UpdateAsync(fetched));
					await (s.FlushAsync()); // force flush to hit DB
					Assert.That(fetched.LastModified, Is.Not.SameAs(freshVersion));
					await (s.DeleteAsync(fetched));
					await (tx.CommitAsync());
				}
			}
		}

		private async Task<object> PersistANewSomethingAsync()
		{
			object savedId;
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					var e = new SimpleVersioned{Something = "something"};
					savedId = await (s.SaveAsync(e));
					await (tx.CommitAsync());
				}
			}

			return savedId;
		}

		[Test]
		public async Task ShouldCheckStaleStateAsync()
		{
			var versioned = new SimpleVersioned{Something = "original string"};
			try
			{
				using (ISession session = OpenSession())
				{
					await (session.SaveAsync(versioned));
					await (session.FlushAsync());
					using (ISession concurrentSession = OpenSession())
					{
						var sameVersioned = await (concurrentSession.GetAsync<SimpleVersioned>(versioned.Id));
						sameVersioned.Something = "another string";
						await (concurrentSession.FlushAsync());
					}

					versioned.Something = "new string";
					await (session.FlushAsync());
				}

				Assert.Fail("Expected exception was not thrown");
			}
			catch (StaleObjectStateException)
			{
			// as expected
			}
			finally
			{
				using (ISession session = OpenSession())
				{
					await (session.DeleteAsync("from SimpleVersioned"));
					await (session.FlushAsync());
				}
			}
		}
	}
}
#endif
