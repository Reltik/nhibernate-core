#if NET_4_5
using System;
using System.Collections;
using NUnit.Framework;
using NHibernate.Criterion;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH1018
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class NH1018Fixture : BugTestCase
	{
		[Test]
		public async Task TestAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction tx = session.BeginTransaction())
				{
					Employer emr1 = new Employer("Test Employer 1");
					Employer emr2 = new Employer("Test Employer 2");
					Employee[] employees = new Employee[5];
					for (int i = 0; i < employees.Length; i++)
					{
						employees[i] = new Employee("Test Employee " + (i + 1).ToString());
					}

					emr1.AddEmployee(employees[0]);
					emr1.AddEmployee(employees[1]);
					emr1.AddEmployee(employees[2]);
					emr2.AddEmployee(employees[3]);
					emr2.AddEmployee(employees[4]);
					await (session.SaveAsync(emr1));
					await (session.SaveAsync(emr2));
					foreach (Employee emp in employees)
					{
						await (session.SaveAsync(emp));
					}

					await (tx.CommitAsync());
				}

			using (ISession session = OpenSession())
			{
				IList employers = await (session.CreateQuery("select emr from Employer emr inner join fetch emr.Employees").SetResultTransformer(CriteriaSpecification.DistinctRootEntity).ListAsync());
				Assert.AreEqual(2, employers.Count);
			}

			using (ISession session = OpenSession())
				using (ITransaction tx = session.BeginTransaction())
				{
					await (session.DeleteAsync("from Employee"));
					await (session.DeleteAsync("from Employer"));
					await (tx.CommitAsync());
				}
		}
	}
}
#endif
