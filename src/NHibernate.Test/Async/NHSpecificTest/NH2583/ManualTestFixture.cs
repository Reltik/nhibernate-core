#if NET_4_5
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.NHSpecificTest.NH2583
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class ManualTestFixtureAsync : BugTestCaseAsync
	{
		/// <summary>
		/// This setup is used in most tests in here - but not all; and we might want to
		/// twist it for special tests. Therefore, the OnSetUp mechanism is not used.
		/// </summary>
		private async Task StandardSetUpAsync()
		{
			using (var session = OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var i1001 = new MyRef1{Id = 1001, I1 = null, I2 = 101};
					var i1101 = new MyRef1{Id = 1101, I1 = null, I2 = 111};
					var i1002 = new MyRef1{Id = 1002, I1 = 12, I2 = 102};
					var i1003 = new MyRef1{Id = 1003, I1 = 13, I2 = 103};
					await (session.SaveAsync(i1001));
					await (session.SaveAsync(i1101));
					await (session.SaveAsync(i1002));
					await (session.SaveAsync(i1003));
					var j2001 = new MyRef2{Id = 2001, J1 = null, J2 = 201};
					var j2002 = new MyRef2{Id = 2002, J1 = 22, J2 = 202};
					var j2003 = new MyRef2{Id = 2003, J1 = null, J2 = 203};
					await (session.SaveAsync(j2001));
					await (session.SaveAsync(j2002));
					await (session.SaveAsync(j2003));
					var b10 = new MyBO{Id = 10, Name = "1:1001,o1:NULL,2:NULL", BO1 = i1001, OtherBO1 = null, BO2 = null};
					var b11 = new MyBO{Id = 11, Name = "1:1001,o1:1101,2:NULL", BO1 = i1001, OtherBO1 = i1101, BO2 = null};
					var b20 = new MyBO{Id = 20, Name = "1:1002,o1:NULL,2:2002", BO1 = i1002, OtherBO1 = null, BO2 = j2002};
					var b30 = new MyBO{Id = 30, Name = "1:NULL,o1:NULL,2:2003", BO1 = null, OtherBO1 = null, BO2 = j2003};
					await (session.SaveAsync(b10));
					await (session.SaveAsync(b11));
					await (session.SaveAsync(b20));
					await (session.SaveAsync(b30));
					await (tx.CommitAsync());
				}
			}
		}

		[Test]
		public async Task OrWithTrueShouldBeEqualToTrueAsync()
		{
			await (StandardSetUpAsync());
			int compareCt;
			using (var session = OpenSession())
			{
				var result = session.Query<MyBO>();
				// 3.1.0/2011-03-19: OK - select mybo0_.Id as Id0_, mybo0_.Name as Name0_, mybo0_.BO1Key as BO3_0_, mybo0_.OtherBO1Key as OtherBO4_0_, mybo0_.BO2Key as BO5_0_ from MyBO mybo0_
				var resultList = result.ToList();
				compareCt = resultList.Count;
				Assert.IsTrue(compareCt > 0);
			}

			using (var session = OpenSession())
			{
				var result = session.Query<MyBO>().Where(bo => true);
				// 3.1.0/2011-03-19: OK - exec sp_executesql N'select mybo0_.Id as Id0_, mybo0_.Name as Name0_, mybo0_.BO1Key as BO3_0_, mybo0_.OtherBO1Key as OtherBO4_0_, mybo0_.BO2Key as BO5_0_ from MyBO mybo0_ where @p0=1',N'@p0 bit',@p0=1
				var resultList = result.ToList();
				Assert.AreEqual(compareCt, resultList.Count);
			}

			using (var session = OpenSession())
			{
				var result = session.Query<MyBO>().Where(bo => bo.BO1 != null && bo.BO1.I2 == 101 || true);
				// 3.1.0/2011-03-19: WRONG - exec sp_executesql N'select mybo0_.Id as Id0_, mybo0_.Name as Name0_, mybo0_.BO1Key as BO3_0_, mybo0_.OtherBO1Key as OtherBO4_0_, mybo0_.BO2Key as BO5_0_ from MyBO mybo0_, MyRef1 myref1x1_ where mybo0_.BO1Key=myref1x1_.Id and ((mybo0_.BO1Key is not null) and myref1x1_.I2=@p0 or @p1=1)',N'@p0 int,@p1 bit',@p0=101,@p1=1
				var resultList = result.ToList();
				Assert.AreEqual(compareCt, resultList.Count);
			}

			using (var session = OpenSession())
			{
				var result = session.Query<MyBO>().Where(bo => bo.BO1 != null && bo.BO1.I2 == 101 || bo.Id == bo.Id + 0);
				// 3.1.0/2011-03-19: WRONG - exec sp_executesql N'select mybo0_.Id as Id0_, mybo0_.Name as Name0_, mybo0_.BO1Key as BO3_0_, mybo0_.OtherBO1Key as OtherBO4_0_, mybo0_.BO2Key as BO5_0_ from MyBO mybo0_, MyRef1 myref1x1_ where mybo0_.BO1Key=myref1x1_.Id and ((mybo0_.BO1Key is not null) and myref1x1_.I2=@p0 or mybo0_.Id=mybo0_.Id+@p1)',N'@p0 int,@p1 int',@p0=101,@p1=0
				var resultList = result.ToList();
				Assert.AreEqual(compareCt, resultList.Count);
			}
		}

		[Test]
		public async Task OrAndNavigationsShouldUseOuterJoinsAsync()
		{
			await (StandardSetUpAsync());
			using (var session = OpenSession())
			{
				var result = session.Query<MyBO>().Where(bo => bo.BO1 != null && bo.BO1.I2 == 101// || bo.BO2 != null && bo.BO2.J2 == 203 - is added below!
				);
				var resultList = result.ToList();
				Assert.IsTrue(resultList.Count > 0);
			}

			using (var session = OpenSession())
			{
				var result = session.Query<MyBO>().Where(bo => bo.BO1 != null && bo.BO1.I2 == 101 || bo.BO2 != null && bo.BO2.J2 == 203);
				var resultList = result.ToList();
				Assert.IsTrue(resultList.Count > 0);
			}

			using (var session = OpenSession())
			{
				var result = session.Query<MyBO>().Where(bo => bo.BO1.I2 == 101 && bo.BO1 != null || bo.BO2.J2 == 203 && bo.BO2 != null);
				var resultList = result.ToList();
				Assert.IsTrue(resultList.Count > 0);
			}
		}

		[Test]
		public async Task OrShouldBeCompatibleWithAdditionForNullReferencesAsync()
		{
			await (StandardSetUpAsync());
			using (var session = OpenSession())
			{
				List<MyBO> leftResult;
				List<MyBO> rightResult;
				List<MyBO> orResult;
				TestCoreOrShouldBeCompatibleWithSum(session, bo => bo.BO1.I2 == null, bo => bo.BO2.J2 == null, bo => bo.BO1.I2 == null || bo.BO2.J2 == null, out leftResult, out rightResult, out orResult);
			//Assert.AreEqual(0, leftResult.Count);
			//Assert.AreEqual(0, rightResult.Count);
			//Assert.AreEqual(0, orResult.Count);
			}
		}

		private static void TestCoreOrShouldBeCompatibleWithSum(ISession session, Expression<Func<MyBO, bool>> left, Expression<Func<MyBO, bool>> right, Expression<Func<MyBO, bool>> both, out List<MyBO> leftResult, out List<MyBO> rightResult, out List<MyBO> orResult)
		{
			leftResult = session.Query<MyBO>().Where(left).ToList();
			rightResult = session.Query<MyBO>().Where(right).ToList();
			orResult = session.Query<MyBO>().Where(both).ToList();
			Assert.IsTrue(orResult.Count <= leftResult.Count + rightResult.Count);
		}

		[Test]
		public async Task OrShouldBeCompatibleWithAdditionForNonNullReferencesAsync()
		{
			await (StandardSetUpAsync());
			using (var session = OpenSession())
			{
				List<MyBO> leftResult;
				List<MyBO> rightResult;
				List<MyBO> orResult;
				TestCoreOrShouldBeCompatibleWithSum(session, bo => bo.BO1.I1 == null, bo => bo.BO2.J1 == null, bo => bo.BO1.I1 == null || bo.BO2.J1 == null, out leftResult, out rightResult, out orResult);
				Assert.That(() => leftResult.Count, Is.GreaterThan(0));
				Assert.That(() => rightResult.Count, Is.GreaterThan(0));
				Assert.That(() => orResult.Count, Is.GreaterThan(0));
			}
		}

		[Test, Ignore("Pure Outer Join semantics has projection anomaly!")]
		public async Task ProjectionDoesNotChangeResultAsync()
		{
			// This tests against the "projection anomaly" of (||-4) semantics.
			using (var session = OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var i1001 = new MyRef1{Id = 1001, I1 = null, I2 = 101};
					var i1101 = new MyRef1{Id = 1101, I1 = null, I2 = 111};
					await (session.SaveAsync(i1001));
					await (session.SaveAsync(i1101));
					var b1 = new MyBO{Id = 1, Name = "1:1001", BO1 = i1001};
					var b2 = new MyBO{Id = 2, Name = "2:1101", BO1 = i1101};
					var b3 = new MyBO{Id = 3, Name = "3:1101", BO1 = i1101};
					var b4 = new MyBO{Id = 4, Name = "4:NULL", BO1 = null};
					await (session.SaveAsync(b1));
					await (session.SaveAsync(b2));
					await (session.SaveAsync(b3));
					await (session.SaveAsync(b4));
					await (tx.CommitAsync());
				}
			}

			using (var session = OpenSession())
			{
				var directResult = session.Query<MyRef1>().Where(bo => bo.I1 == null).ToList().Select(bo => bo.Id);
				var resultViaProjection = (
					from bo in session.Query<MyBO>()where bo.BO1.I1 == null || bo.BO2.J2 == 999
					select bo.BO1).Distinct().ToList().Select(bo => bo.Id);
				// With "projection anomaly", the previous Select will fail, as one "bo" is null,
				// and hence bo.Id throws a NRE.
				Assert.That(() => resultViaProjection.ToList(), Is.EquivalentTo(directResult.ToList()));
			}
		}

		[Test, Explicit("Exploratory Test")]
		public async Task NHibernateLinqExploratoryTestWhichProvesNothingAsync()
		{
			await (StandardSetUpAsync());
			using (var session = OpenSession())
			{
				//var result = session.Query<MyBO>()
				//    .Where(bo =>
				//        // ReSharper disable EqualExpressionComparison
				//            bo.BO1.I2 == bo.BO1.I2
				//    // ReSharper restore EqualExpressionComparison
				//            );
				//result.ToList();
				var result =
					from r in session.Query<MyRef1>()orderby (r.Id == 1101 || r.Id == 1102 ? r.Id - 1000 : r.Id)select (r.Id == 1101 || r.Id == 1102 ? r.Id + 1000 : r.Id);
				CollectionAssert.AreEqual(new[]{2101, 1001, 1002, 1003}, result.ToList());
				var someBos =
					from r in session.Query<MyBO>()where r.BO1.BO2.J2 == 201
					select r;
				Assert.IsTrue(someBos.ToList().Count == 0);
				var someBos1 =
					from r in session.Query<MyBO>()where (r.BO1.BO2 == null ? r.BO2 : r.BO1.BO2).J2 == 201
					select r.Id;
				Assert.That(() => someBos1.ToList(), Is.EquivalentTo(new[]{10}));
			}
		}

		protected override async Task OnTearDownAsync()
		{
			using (var session = OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					foreach (var bo in session.Query<MyBO>())
					{
						bo.LeftSon = null;
						bo.RightSon = null;
					}

					await (session.FlushAsync());
					await (DeleteAllAsync<MyBO>(session));
					await (DeleteAllAsync<MyRef1>(session));
					await (DeleteAllAsync<MyRef2>(session));
					await (DeleteAllAsync<MyRef3>(session));
					await (tx.CommitAsync());
				}
			}
		}

		private static async Task DeleteAllAsync<T>(ISession session)
		{
			await (session.CreateQuery("delete from " + typeof (T).Name).ExecuteUpdateAsync());
		}
	}
}
#endif
