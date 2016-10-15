﻿#if NET_4_5
using System.Collections;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Cascade.OneToOneCascadeDelete.Fk.Bidirectional
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public abstract partial class DeleteOneToOneOrphansTestAsync : TestCaseAsync
	{
		protected override string MappingsAssembly
		{
			get
			{
				return "NHibernate.Test";
			}
		}

		protected override async Task OnSetUpAsync()
		{
			await (base.OnSetUpAsync());
			using (var s = OpenSession())
				using (var t = s.BeginTransaction())
				{
					var emp = new Employee{Name = "Julius Caesar"};
					emp.Info = new EmployeeInfo(emp);
					await (s.SaveAsync(emp));
					await (t.CommitAsync());
				}
		}

		protected override async Task OnTearDownAsync()
		{
			await (base.OnTearDownAsync());
			using (var session = OpenSession())
				using (var tx = session.BeginTransaction())
				{
					await (session.DeleteAsync("from EmployeeInfo"));
					await (session.DeleteAsync("from Employee"));
					await (tx.CommitAsync());
				}
		}

		[Test]
		public async Task TestOrphanedWhileManagedAsync()
		{
			long empId;
			using (var s = OpenSession())
				using (var t = s.BeginTransaction())
				{
					var empInfoResults = await (s.CreateQuery("from EmployeeInfo").ListAsync<EmployeeInfo>());
					Assert.AreEqual(1, empInfoResults.Count);
					var empResults = await (s.CreateQuery("from Employee").ListAsync<Employee>());
					Assert.AreEqual(1, empResults.Count);
					var emp = empResults[0];
					Assert.NotNull(emp);
					empId = emp.Id;
					emp.Info = null;
					await (t.CommitAsync());
				}

			using (var s = OpenSession())
				using (var t = s.BeginTransaction())
				{
					var emp = await (s.GetAsync<Employee>(empId));
					Assert.Null(emp.Info);
					var empInfoResults = await (s.CreateQuery("from EmployeeInfo").ListAsync<EmployeeInfo>());
					Assert.AreEqual(0, empInfoResults.Count);
					var empResults = await (s.CreateQuery("from Employee").ListAsync<Employee>());
					Assert.AreEqual(1, empResults.Count);
					await (t.CommitAsync());
				}
		}
	}
}
#endif