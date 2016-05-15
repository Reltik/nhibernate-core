#if NET_4_5
using System.Collections;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH1801
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class Fixture : BugTestCase
	{
		[Test]
		public async Task TestAsync()
		{
			try
			{
				using (ISession s = OpenSession())
				{
					var a1 = new A{Id = 1, Name = "A1"};
					var a2 = new A{Id = 2, Name = "A2"};
					var b1 = new B{Id = 1, Name = "B1", A = a1};
					var b2 = new B{Id = 2, Name = "B2", A = a2};
					var c1 = new C{Name = "C1", A = a1};
					var c2 = new C{Name = "C2", A = a2};
					await (s.SaveAsync(a1));
					await (s.SaveAsync(a2));
					await (s.SaveAsync(b1));
					await (s.SaveAsync(b2));
					await (s.SaveAsync(c1));
					await (s.SaveAsync(c2));
					await (s.FlushAsync());
				}

				using (ISession s = OpenSession())
				{
					IList res = await (s.CreateQuery("from B b, C c where b.A = c.A and b.Id = :id").SetInt32("id", 1).ListAsync());
					Assert.That(res, Has.Count.EqualTo(1));
					await (s.FlushAsync());
				}
			}
			finally
			{
				using (ISession s = OpenSession())
				{
					await (s.DeleteAsync("from B"));
					await (s.DeleteAsync("from C"));
					await (s.DeleteAsync("from A"));
					await (s.FlushAsync());
				}
			}
		}
	}
}
#endif
