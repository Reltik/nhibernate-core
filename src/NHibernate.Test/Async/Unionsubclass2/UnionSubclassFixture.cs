#if NET_4_5
using System.Collections;
using System.Collections.Generic;
using NHibernate.Criterion;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Unionsubclass2
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class UnionSubclassFixture : TestCase
	{
		[Test]
		public async Task UnionSubclassAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					Employee mark = new Employee();
					mark.Name = "Mark";
					mark.Title = "internal sales";
					mark.Sex = 'M';
					mark.Address.address = "buckhead";
					mark.Address.zip = "30305";
					mark.Address.country = "USA";
					Customer joe = new Customer();
					joe.Name = "Joe";
					joe.Address.address = "San Francisco";
					joe.Address.zip = "XXXXX";
					joe.Address.country = "USA";
					joe.Comments = "Very demanding";
					joe.Sex = 'M';
					joe.Salesperson = mark;
					Person yomomma = new Person();
					yomomma.Name = "mum";
					yomomma.Sex = 'F';
					await (s.SaveAsync(yomomma));
					await (s.SaveAsync(mark));
					await (s.SaveAsync(joe));
					// TODO NH : This line is present in H3.2.5 test; ReadCommitted ?
					//Assert.AreEqual(0, s.CreateQuery("from System.Object").List().Count);
					Assert.AreEqual(3, (await (s.CreateQuery("from Person").ListAsync())).Count);
					Assert.AreEqual(1, (await (s.CreateQuery("from Person p where p.class = Customer").ListAsync())).Count);
					Assert.AreEqual(1, (await (s.CreateQuery("from Person p where p.class = Person").ListAsync())).Count);
					s.Clear();
					IList<Customer> customers = await (s.CreateQuery("from Customer c left join fetch c.salesperson").ListAsync<Customer>());
					foreach (Customer c in customers)
					{
						Assert.IsTrue(NHibernateUtil.IsInitialized(c.Salesperson));
						Assert.AreEqual("Mark", c.Salesperson.Name);
					}

					Assert.AreEqual(1, customers.Count);
					s.Clear();
					customers = await (s.CreateQuery("from Customer").ListAsync<Customer>());
					foreach (Customer c in customers)
					{
						Assert.IsFalse(NHibernateUtil.IsInitialized(c.Salesperson));
						Assert.AreEqual("Mark", c.Salesperson.Name);
					}

					Assert.AreEqual(1, customers.Count);
					s.Clear();
					mark = await (s.GetAsync<Employee>(mark.Id));
					joe = await (s.GetAsync<Customer>(joe.Id));
					mark.Address.zip = "30306";
					Assert.AreEqual(1, (await (s.CreateQuery("from Person p where p.address.zip = '30306'").ListAsync())).Count);
					await (s.DeleteAsync(mark));
					await (s.DeleteAsync(joe));
					await (s.DeleteAsync(yomomma));
					Assert.AreEqual(0, (await (s.CreateQuery("from Person").ListAsync())).Count);
					await (t.CommitAsync());
					s.Close();
				}
		}

		[Test]
		public async Task QuerySubclassAttributeAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					Person p = new Person();
					p.Name = "Emmanuel";
					p.Sex = 'M';
					await (s.PersistAsync(p));
					Employee q = new Employee();
					q.Name = "Steve";
					q.Sex = 'M';
					q.Title = "Mr";
					q.Salary = 1000m;
					await (s.PersistAsync(q));
					IList result = await (s.CreateQuery("from Person p where p.salary > 100").ListAsync());
					Assert.AreEqual(1, result.Count);
					Assert.AreSame(q, result[0]);
					result = await (s.CreateQuery("from Person p where p.salary > 100 or p.name like 'E%'").ListAsync());
					Assert.AreEqual(2, result.Count);
					if (!TestDialect.HasBrokenDecimalType)
					{
						result = await (s.CreateCriteria(typeof (Person)).Add(Property.ForName("salary").Gt(100m)).ListAsync());
						Assert.AreEqual(1, result.Count);
						Assert.AreSame(q, result[0]);
					}

					result = await (s.CreateQuery("select p.salary from Person p where p.salary > 100").ListAsync());
					Assert.AreEqual(1, result.Count);
					Assert.AreEqual(1000m, (decimal)result[0]);
					await (s.DeleteAsync(p));
					await (s.DeleteAsync(q));
					await (t.CommitAsync());
					s.Close();
				}
		}
	}
}
#endif
