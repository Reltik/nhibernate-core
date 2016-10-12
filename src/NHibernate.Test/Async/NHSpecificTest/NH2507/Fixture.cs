﻿#if NET_4_5
using System;
using System.Linq;
using NHibernate.Dialect;
using NHibernate.Linq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH2507
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FixtureAsync : BugTestCaseAsync
	{
		protected override async Task OnSetUpAsync()
		{
			await (base.OnSetUpAsync());
			using (ISession session = this.OpenSession())
			{
				Animal animal = new Animal()
				{Id = 1, BodyWeight = 2, Sex = Sex.Undefined, Description = "Animal", SerialNumber = "111"};
				Lizard lizard = new Lizard()
				{Id = 2, BodyTemperature = 98, BodyWeight = 10, Sex = Sex.Male, Description = "Lizard", SerialNumber = "222", Mother = animal};
				Dog momDog = new Dog()
				{BirthDate = new DateTime(2007, 01, 01), BodyWeight = 65, Sex = Sex.Female, Description = "MyDogMom", Pregnant = false, SerialNumber = "333"};
				Dog dadDog = new Dog()
				{BirthDate = new DateTime(2007, 02, 02), BodyWeight = 75, Sex = Sex.Male, Description = "MyDogDad", Pregnant = false, SerialNumber = "444"};
				Dog puppy = new Dog()
				{BirthDate = new DateTime(2010, 01, 01), BodyWeight = 50, Sex = Sex.Male, Description = "Puppy", Pregnant = false, Father = dadDog, Mother = momDog, SerialNumber = "555"};
				Cat cat = new Cat()
				{BirthDate = new DateTime(2007, 03, 03), BodyWeight = 10, Sex = Sex.Female, Description = "MyCat", Pregnant = true, SerialNumber = "777"};
				await (session.SaveAsync(animal));
				await (session.SaveAsync(lizard));
				await (session.SaveAsync(momDog));
				await (session.SaveAsync(dadDog));
				await (session.SaveAsync(puppy));
				await (session.SaveAsync(cat));
				await (session.FlushAsync());
			}
		}

		protected override async Task OnTearDownAsync()
		{
			await (base.OnTearDownAsync());
			using (ISession session = this.OpenSession())
			{
				const string hql = "from System.Object";
				await (session.DeleteAsync(hql));
				await (session.FlushAsync());
			}
		}

		/// <summary>
		/// Test LINQ query that compares enumeration values while simulating projects with Advanced Build property
		/// value for "Check for arithmetic overflow/underflow" as <b>checked</b>.
		/// </summary>
		[Test]
		public async Task QueryEnumerationWithCheckedArithmeticOverflowTestAsync()
		{
			using (ISession session = this.OpenSession())
			{
				checked
				{
					var query = await (session.Query<Animal>().Where(item => item.Sex == Sex.Undefined).ToListAsync());
					Assert.AreEqual(1, query.Count);
				}
			}
		}

		/// <summary>
		/// Test LINQ query that compares enumeration values while simulating projects with Advanced Build property
		/// value for "Check for arithmetic overflow/underflow" as <b>unchecked</b>.
		/// </summary>
		[Test]
		public async Task QueryEnumerationWithUncheckedArithmeticOverflowTestAsync()
		{
			using (ISession session = this.OpenSession())
			{
				unchecked
				{
					var query = await (session.Query<Animal>().Where(item => item.Sex == Sex.Undefined).ToListAsync());
					Assert.AreEqual(1, query.Count);
				}
			}
		}
	}
}
#endif
