#if NET_4_5
using NHibernate;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH1419
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class Tests : BugTestCase
	{
		[Test]
		public async Task BugAsync()
		{
			using (ISession session = OpenSession())
			{
				ITransaction transaction = session.BeginTransaction();
				Blog blog = new Blog();
				blog.Name = "Test Blog 1";
				Entry entry = new Entry();
				entry.Subject = "Test Entry 1";
				blog.AddEntry(entry);
				await (session.SaveOrUpdateAsync(blog));
				await (transaction.CommitAsync());
			}

			using (ISession session = OpenSession())
			{
				ITransaction transaction = session.BeginTransaction();
				await (session.DeleteAsync("from Blog"));
				await (transaction.CommitAsync());
			}
		}

		[Test]
		public async Task WithEmptyCollectionAsync()
		{
			using (ISession session = OpenSession())
			{
				ITransaction transaction = session.BeginTransaction();
				Blog blog = new Blog();
				blog.Name = "Test Blog 1";
				await (session.SaveOrUpdateAsync(blog));
				await (transaction.CommitAsync());
			}

			using (ISession session = OpenSession())
			{
				ITransaction transaction = session.BeginTransaction();
				await (session.DeleteAsync("from Blog"));
				await (transaction.CommitAsync());
			}
		}
	}
}
#endif
