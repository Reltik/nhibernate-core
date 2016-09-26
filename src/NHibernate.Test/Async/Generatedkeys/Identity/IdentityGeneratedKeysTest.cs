#if NET_4_5
using System.Collections;
using NHibernate.Cfg;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Generatedkeys.Identity
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class IdentityGeneratedKeysTestAsync : TestCaseAsync
	{
		protected override IList Mappings
		{
			get
			{
				return new string[]{"Generatedkeys.Identity.MyEntity.hbm.xml"};
			}
		}

		protected override string MappingsAssembly
		{
			get
			{
				return "NHibernate.Test";
			}
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return dialect.SupportsIdentityColumns;
		}

		protected override void Configure(Configuration configuration)
		{
			base.Configure(configuration);
			configuration.SetProperty(Environment.GenerateStatistics, "true");
		}

		[Test]
		public async Task IdentityColumnGeneratedIdsAsync()
		{
			ISession s = OpenSession();
			s.BeginTransaction();
			MyEntity myEntity = new MyEntity("test");
			long id = (long)await (s.SaveAsync(myEntity));
			Assert.IsNotNull(id, "identity column did not force immediate insert");
			Assert.AreEqual(id, myEntity.Id);
			await (s.DeleteAsync(myEntity));
			await (s.Transaction.CommitAsync());
			s.Close();
		}

		[Test, Ignore("Not supported yet.")]
		public async Task PersistOutsideTransactionAsync()
		{
			ISession s = OpenSession();
			// first test save() which should force an immediate insert...
			MyEntity myEntity1 = new MyEntity("test-save");
			long id = (long)await (s.SaveAsync(myEntity1));
			Assert.IsNotNull(id, "identity column did not force immediate insert");
			Assert.AreEqual(id, myEntity1.Id);
			// next test persist() which should cause a delayed insert...
			long initialInsertCount = Sfi.Statistics.EntityInsertCount;
			MyEntity myEntity2 = new MyEntity("test-persist");
			await (s.PersistAsync(myEntity2));
			Assert.AreEqual(initialInsertCount, Sfi.Statistics.EntityInsertCount, "persist on identity column not delayed");
			Assert.AreEqual(0, myEntity2.Id);
			// an explicit flush should cause execution of the delayed insertion
			await (s.FlushAsync());
			Assert.AreEqual(initialInsertCount + 1, Sfi.Statistics.EntityInsertCount, "delayed persist insert not executed on flush");
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			await (s.DeleteAsync(myEntity1));
			await (s.DeleteAsync(myEntity2));
			await (s.Transaction.CommitAsync());
			s.Close();
		}

		[Test, Ignore("Not supported yet.")]
		public async Task PersistOutsideTransactionCascadedToNonInverseCollectionAsync()
		{
			long initialInsertCount = Sfi.Statistics.EntityInsertCount;
			ISession s = OpenSession();
			MyEntity myEntity = new MyEntity("test-persist");
			myEntity.NonInverseChildren.Add(new MyChild("test-child-persist-non-inverse"));
			await (s.PersistAsync(myEntity));
			Assert.AreEqual(initialInsertCount, Sfi.Statistics.EntityInsertCount, "persist on identity column not delayed");
			Assert.AreEqual(0, myEntity.Id);
			await (s.FlushAsync());
			Assert.AreEqual(initialInsertCount + 2, Sfi.Statistics.EntityInsertCount, "delayed persist insert not executed on flush");
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			await (s.DeleteAsync("from MyChild"));
			await (s.DeleteAsync("from MyEntity"));
			await (s.Transaction.CommitAsync());
			s.Close();
		}

		[Test, Ignore("Not supported yet.")]
		public async Task PersistOutsideTransactionCascadedToInverseCollectionAsync()
		{
			long initialInsertCount = Sfi.Statistics.EntityInsertCount;
			ISession s = OpenSession();
			MyEntity myEntity2 = new MyEntity("test-persist-2");
			MyChild child = new MyChild("test-child-persist-inverse");
			myEntity2.InverseChildren.Add(child);
			child.InverseParent = myEntity2;
			await (s.PersistAsync(myEntity2));
			Assert.AreEqual(initialInsertCount, Sfi.Statistics.EntityInsertCount, "persist on identity column not delayed");
			Assert.AreEqual(0, myEntity2.Id);
			await (s.FlushAsync());
			Assert.AreEqual(initialInsertCount + 2, Sfi.Statistics.EntityInsertCount, "delayed persist insert not executed on flush");
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			await (s.DeleteAsync("from MyChild"));
			await (s.DeleteAsync("from MyEntity"));
			await (s.Transaction.CommitAsync());
			s.Close();
		}

		[Test, Ignore("Not supported yet.")]
		public async Task PersistOutsideTransactionCascadedToManyToOneAsync()
		{
			long initialInsertCount = Sfi.Statistics.EntityInsertCount;
			ISession s = OpenSession();
			MyEntity myEntity = new MyEntity("test-persist");
			myEntity.Sibling = new MySibling("test-persist-sibling-out");
			await (s.PersistAsync(myEntity));
			Assert.AreEqual(initialInsertCount, Sfi.Statistics.EntityInsertCount, "persist on identity column not delayed");
			Assert.AreEqual(0, myEntity.Id);
			await (s.FlushAsync());
			Assert.AreEqual(initialInsertCount + 2, Sfi.Statistics.EntityInsertCount, "delayed persist insert not executed on flush");
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			await (s.DeleteAsync("from MyEntity"));
			await (s.DeleteAsync("from MySibling"));
			await (s.Transaction.CommitAsync());
			s.Close();
		}

		[Test, Ignore("Not supported yet.")]
		public async Task PersistOutsideTransactionCascadedFromManyToOneAsync()
		{
			long initialInsertCount = Sfi.Statistics.EntityInsertCount;
			ISession s = OpenSession();
			MyEntity myEntity2 = new MyEntity("test-persist-2");
			MySibling sibling = new MySibling("test-persist-sibling-in");
			sibling.Entity = myEntity2;
			await (s.PersistAsync(sibling));
			Assert.AreEqual(initialInsertCount, Sfi.Statistics.EntityInsertCount, "persist on identity column not delayed");
			Assert.AreEqual(0, myEntity2.Id);
			await (s.FlushAsync());
			Assert.AreEqual(initialInsertCount + 2, Sfi.Statistics.EntityInsertCount, "delayed persist insert not executed on flush");
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			await (s.DeleteAsync("from MySibling"));
			await (s.DeleteAsync("from MyEntity"));
			await (s.Transaction.CommitAsync());
			s.Close();
		}
	}
}
#endif
