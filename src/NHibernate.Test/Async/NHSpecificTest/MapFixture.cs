#if NET_4_5
using System;
using System.Collections;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.DomainModel.NHSpecific;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class MapFixture : TestCase
	{
		[Test]
		public async Task TestSelectAsync()
		{
			await (TestInsertAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			ICriteria chiefsCriteria = s.CreateCriteria(typeof (Team));
			chiefsCriteria.Add(Expression.Eq("Name", "Chiefs"));
			Team chiefs = (Team)(await (chiefsCriteria.ListAsync()))[0];
			IList<Child> players = chiefs.Players;
			Parent parentDad = (Parent)await (s.LoadAsync(typeof (Parent), 1));
			Child amyJones = (Child)await (s.LoadAsync(typeof (Child), 2));
			Child[] friends = amyJones.Friends;
			Child childOneRef = amyJones.FirstSibling;
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task TestSortAsync()
		{
			await (TestInsertAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Parent bobJones = (Parent)await (s.LoadAsync(typeof (Parent), 1));
			ISet<Parent> friends = bobJones.AdultFriends;
			int currentId = 0;
			int previousId = 0;
			foreach (Parent friend in friends)
			{
				previousId = currentId;
				currentId = friend.Id;
				Assert.IsTrue(currentId > previousId, "Current should have a higher Id than previous");
			}

			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task TestInsertAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			SexType male = new SexType();
			SexType female = new SexType();
			//male.Id = 1;
			male.TypeName = "Male";
			//female.Id = 2;
			female.TypeName = "Female";
			await (s.SaveAsync(male));
			await (s.SaveAsync(female));
			Parent bobJones = new Parent();
			bobJones.Id = 1;
			bobJones.AdultName = "Bob Jones";
			Parent maryJones = new Parent();
			maryJones.Id = 2;
			maryJones.AdultName = "Mary Jones";
			Parent charlieSmith = new Parent();
			charlieSmith.Id = 3;
			charlieSmith.AdultName = "Charlie Smith";
			Parent cindySmith = new Parent();
			cindySmith.Id = 4;
			cindySmith.AdultName = "Cindy Smith";
			bobJones.AddFriend(cindySmith);
			bobJones.AddFriend(charlieSmith);
			bobJones.AddFriend(maryJones);
			maryJones.AddFriend(cindySmith);
			await (s.SaveAsync(bobJones, bobJones.Id));
			await (s.SaveAsync(maryJones, maryJones.Id));
			await (s.SaveAsync(charlieSmith, charlieSmith.Id));
			await (s.SaveAsync(cindySmith, cindySmith.Id));
			Child johnnyJones = new Child();
			Child amyJones = new Child();
			Child brianSmith = new Child();
			Child sarahSmith = new Child();
			johnnyJones.Id = 1;
			johnnyJones.FullName = "Johnny Jones";
			johnnyJones.Dad = bobJones;
			johnnyJones.Mom = maryJones;
			johnnyJones.Sex = male;
			johnnyJones.Friends = new Child[]{brianSmith, sarahSmith};
			johnnyJones.FavoriteDate = DateTime.Parse("2003-08-16");
			amyJones.Id = 2;
			amyJones.FullName = "Amy Jones";
			amyJones.Dad = bobJones;
			amyJones.Mom = maryJones;
			amyJones.Sex = female;
			amyJones.FirstSibling = johnnyJones;
			amyJones.Friends = new Child[]{johnnyJones, sarahSmith};
			brianSmith.Id = 11;
			brianSmith.FullName = "Brian Smith";
			brianSmith.Dad = charlieSmith;
			brianSmith.Mom = cindySmith;
			brianSmith.Sex = male;
			brianSmith.Friends = new Child[]{johnnyJones, amyJones, sarahSmith};
			sarahSmith.Id = 12;
			sarahSmith.FullName = "Sarah Smith";
			sarahSmith.Dad = charlieSmith;
			sarahSmith.Mom = cindySmith;
			sarahSmith.Sex = female;
			sarahSmith.Friends = new Child[]{brianSmith};
			Team royals = new Team();
			royals.Name = "Royals";
			Team chiefs = new Team();
			chiefs.Name = "Chiefs";
			royals.Players = new List<Child>();
			royals.Players.Add(amyJones);
			royals.Players.Add(brianSmith);
			chiefs.Players = new List<Child>();
			chiefs.Players.Add(johnnyJones);
			chiefs.Players.Add(sarahSmith);
			await (s.SaveAsync(johnnyJones, johnnyJones.Id));
			await (s.SaveAsync(amyJones, amyJones.Id));
			await (s.SaveAsync(brianSmith, brianSmith.Id));
			await (s.SaveAsync(sarahSmith, sarahSmith.Id));
			await (s.SaveAsync(royals));
			await (s.SaveAsync(chiefs));
			await (t.CommitAsync());
			s.Close();
		}
	}
}
#endif
