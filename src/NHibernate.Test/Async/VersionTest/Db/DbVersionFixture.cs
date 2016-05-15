#if NET_4_5
using System;
using System.Collections;
using System.Threading;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.VersionTest.Db
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class DbVersionFixture : TestCase
	{
		[Test]
		public async Task CollectionVersionAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			var guy = new User{Username = "guy"};
			await (s.PersistAsync(guy));
			var admin = new Group{Name = "admin"};
			await (s.PersistAsync(admin));
			await (t.CommitAsync());
			s.Close();
			DateTime guyTimestamp = guy.Timestamp;
			// For dialects (Oracle8 for example) which do not return "true
			// timestamps" sleep for a bit to allow the db date-time increment...
			Thread.Sleep(1500);
			s = OpenSession();
			t = s.BeginTransaction();
			guy = await (s.GetAsync<User>(guy.Id));
			admin = await (s.GetAsync<Group>(admin.Id));
			guy.Groups.Add(admin);
			admin.Users.Add(guy);
			await (t.CommitAsync());
			s.Close();
			Assert.That(!NHibernateUtil.Timestamp.IsEqual(guyTimestamp, guy.Timestamp), "owner version not incremented");
			guyTimestamp = guy.Timestamp;
			Thread.Sleep(1500);
			s = OpenSession();
			t = s.BeginTransaction();
			guy = await (s.GetAsync<User>(guy.Id));
			guy.Groups.Clear();
			await (t.CommitAsync());
			s.Close();
			Assert.That(!NHibernateUtil.Timestamp.IsEqual(guyTimestamp, guy.Timestamp), "owner version not incremented");
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.DeleteAsync(await (s.LoadAsync<User>(guy.Id))));
			await (s.DeleteAsync(await (s.LoadAsync<Group>(admin.Id))));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CollectionNoVersionAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			var guy = new User{Username = "guy"};
			await (s.PersistAsync(guy));
			var perm = new Permission{Name = "silly", Access = "user", Context = "rw"};
			await (s.PersistAsync(perm));
			await (t.CommitAsync());
			s.Close();
			DateTime guyTimestamp = guy.Timestamp;
			s = OpenSession();
			t = s.BeginTransaction();
			guy = await (s.GetAsync<User>(guy.Id));
			perm = await (s.GetAsync<Permission>(perm.Id));
			guy.Permissions.Add(perm);
			await (t.CommitAsync());
			s.Close();
			const string ownerVersionWasIncremented = "owner version was incremented ({0:o} => {1:o})";
			Assert.That(NHibernateUtil.Timestamp.IsEqual(guyTimestamp, guy.Timestamp), string.Format(ownerVersionWasIncremented, guyTimestamp, guy.Timestamp));
			Console.WriteLine(string.Format(ownerVersionWasIncremented, guyTimestamp, guy.Timestamp));
			s = OpenSession();
			t = s.BeginTransaction();
			guy = await (s.GetAsync<User>(guy.Id));
			guy.Permissions.Clear();
			await (t.CommitAsync());
			s.Close();
			Assert.That(NHibernateUtil.Timestamp.IsEqual(guyTimestamp, guy.Timestamp), string.Format(ownerVersionWasIncremented, guyTimestamp, guy.Timestamp));
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.DeleteAsync(await (s.LoadAsync<User>(guy.Id))));
			await (s.DeleteAsync(await (s.LoadAsync<Permission>(perm.Id))));
			await (t.CommitAsync());
			s.Close();
		}
	}
}
#endif
