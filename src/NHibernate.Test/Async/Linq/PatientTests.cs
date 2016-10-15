﻿#if NET_4_5
using System.Linq;
using NHibernate.DomainModel.Northwind.Entities;
using NUnit.Framework;
using NHibernate.Linq;
using System.Threading.Tasks;

namespace NHibernate.Test.Linq
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class PatientTestsAsync : LinqTestCaseAsync
	{
		[Test]
		public async Task CanQueryOnPropertyOfComponentAsync()
		{
			var query = await ((
				from pr in db.PatientRecords
				where pr.Name.LastName == "Doe"
				select pr).ToListAsync());
			Assert.AreEqual(2, query.Count);
		}

		[Test]
		public async Task CanQueryOnManyToOneOfComponentAsync()
		{
			var florida = await (db.States.FirstOrDefaultAsync(x => x.Abbreviation == "FL"));
			var query = await ((
				from pr in db.PatientRecords
				where pr.Address.State == florida
				select pr).ToListAsync());
			Assert.AreEqual(2, query.Count);
		}

		[Test]
		public async Task CanQueryOnPropertyOfManyToOneOfComponentAsync()
		{
			var query = await ((
				from pr in db.PatientRecords
				where pr.Address.State.Abbreviation == "FL"
				select pr).ToListAsync());
			Assert.AreEqual(2, query.Count);
		}

		[Test]
		public async Task CanQueryOnPropertyOfOneToManyAsync()
		{
			var query = await ((
				from p in db.Patients
				where p.PatientRecords.Any(x => x.Gender == Gender.Unknown)select p).ToListAsync());
			Assert.AreEqual(1, query.Count);
		}

		[Test]
		public async Task CanQueryOnPropertyOfManyToOneAsync()
		{
			var query = await ((
				from pr in db.PatientRecords
				where pr.Patient.Active == true
				select pr).ToListAsync());
			Assert.AreEqual(2, query.Count);
		}

		[Test]
		public async Task CanQueryOnManyToOneOfManyToOneAsync()
		{
			var drWatson = await (db.Physicians.FirstOrDefaultAsync(x => x.Name == "Dr Watson"));
			var query = await ((
				from pr in db.PatientRecords
				where pr.Patient.Physician == drWatson
				select pr).ToListAsync());
			Assert.AreEqual(2, query.Count);
		}

		[Test]
		public async Task CanQueryOnPropertyOfManyToOneOfManyToOneAsync()
		{
			var query = await ((
				from pr in db.PatientRecords
				where pr.Patient.Physician.Name == "Dr Watson"
				select pr).ToListAsync());
			Assert.AreEqual(2, query.Count);
		}
	}
}
#endif