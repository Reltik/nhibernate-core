#if NET_4_5
using System.Collections;
using NHibernate.Criterion;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH295
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class SubclassFixture : TestCase
	{
		[Test]
		public async Task LoadByIDFailureSameSessionAsync()
		{
			User ui1 = new User("User1");
			ISession s = OpenSession();
			s.BeginTransaction();
			object uid1 = await (s.SaveAsync(ui1));
			await (s.Transaction.CommitAsync());
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			Assert.IsNotNull(await (s.GetAsync(typeof (User), uid1)));
			UserGroup ug = (UserGroup)await (s.GetAsync(typeof (UserGroup), uid1));
			Assert.IsNull(ug);
			await (s.Transaction.CommitAsync());
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			await (s.DeleteAsync("from Party"));
			await (s.Transaction.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task LoadByIDFailureAsync()
		{
			UserGroup ug1 = new UserGroup();
			ug1.Name = "Group1";
			User ui1 = new User("User1");
			ISession s = OpenSession();
			s.BeginTransaction();
			object gid1 = await (s.SaveAsync(ug1));
			object uid1 = await (s.SaveAsync(ui1));
			await (s.Transaction.CommitAsync());
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			//Load user with USER NAME: 
			ICriteria criteria1 = s.CreateCriteria(typeof (User));
			criteria1.Add(Expression.Eq("Name", "User1"));
			Assert.AreEqual(1, (await (criteria1.ListAsync())).Count);
			await (s.Transaction.CommitAsync());
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			//Load group with USER NAME: 
			ICriteria criteria2 = s.CreateCriteria(typeof (UserGroup));
			criteria2.Add(Expression.Eq("Name", "User1"));
			Assert.AreEqual(0, (await (criteria2.ListAsync())).Count);
			await (s.Transaction.CommitAsync());
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			//Load group with GROUP NAME
			ICriteria criteria3 = s.CreateCriteria(typeof (UserGroup));
			criteria3.Add(Expression.Eq("Name", "Group1"));
			Assert.AreEqual(1, (await (criteria3.ListAsync())).Count);
			await (s.Transaction.CommitAsync());
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			//Load user with GROUP NAME
			ICriteria criteria4 = s.CreateCriteria(typeof (User));
			criteria4.Add(Expression.Eq("Name", "Group1"));
			Assert.AreEqual(0, (await (criteria4.ListAsync())).Count);
			await (s.Transaction.CommitAsync());
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			//Load group with USER IDENTITY
			ug1 = (UserGroup)await (s.GetAsync(typeof (UserGroup), uid1));
			Assert.IsNull(ug1);
			await (s.Transaction.CommitAsync());
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			ui1 = (User)await (s.GetAsync(typeof (User), gid1));
			Assert.IsNull(ui1);
			await (s.Transaction.CommitAsync());
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			Party p = (Party)await (s.GetAsync(typeof (Party), uid1));
			Assert.IsTrue(p is User);
			p = (Party)await (s.GetAsync(typeof (Party), gid1));
			Assert.IsTrue(p is UserGroup);
			await (s.Transaction.CommitAsync());
			s.Close();
			s = OpenSession();
			s.BeginTransaction();
			await (s.DeleteAsync("from Party"));
			await (s.Transaction.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task ListAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					User user = new User();
					user.Name = "user";
					UserGroup group = new UserGroup();
					group.Name = "user";
					group.Users.Add(user);
					await (s.SaveAsync(group));
					await (s.SaveAsync(user));
					await (s.CreateCriteria(typeof (Party)).ListAsync());
					await (s.DeleteAsync("from Party"));
					await (t.CommitAsync());
				}
		}
	}
}
#endif
