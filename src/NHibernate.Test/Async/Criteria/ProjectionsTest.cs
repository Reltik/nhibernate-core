#if NET_4_5
using System.Collections;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Dialect;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Criteria
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class ProjectionsTest : TestCase
	{
		[Test]
		public async Task UsingSqlFunctions_ConcatAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				string result = await (session.CreateCriteria(typeof (Student)).SetProjection(new SqlFunctionProjection("concat", NHibernateUtil.String, Projections.Property("Name"), new ConstantProjection(" "), Projections.Property("Name"))).UniqueResultAsync<string>());
				Assert.AreEqual("ayende ayende", result);
			}
		}

		[Test]
		public async Task UsingSqlFunctions_Concat_WithCastAsync()
		{
			if (Dialect is Oracle8iDialect)
			{
				Assert.Ignore("Not supported by the active dialect:{0}.", Dialect);
			}

			using (ISession session = sessions.OpenSession())
			{
				string result = await (session.CreateCriteria(typeof (Student)).SetProjection(Projections.SqlFunction("concat", NHibernateUtil.String, Projections.Cast(NHibernateUtil.String, Projections.Id()), Projections.Constant(" "), Projections.Property("Name"))).UniqueResultAsync<string>());
				Assert.AreEqual("27 ayende", result);
			}
		}

		[Test]
		public async Task CanUseParametersWithProjectionsAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				long result = await (session.CreateCriteria(typeof (Student)).SetProjection(new AddNumberProjection("id", 15)).UniqueResultAsync<long>());
				Assert.AreEqual(42L, result);
			}
		}

		[Test]
		public async Task UsingConditionalsAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				string result = await (session.CreateCriteria(typeof (Student)).SetProjection(Projections.Conditional(Expression.Eq("id", 27L), Projections.Constant("yes"), Projections.Constant("no"))).UniqueResultAsync<string>());
				Assert.AreEqual("yes", result);
				result = await (session.CreateCriteria(typeof (Student)).SetProjection(Projections.Conditional(Expression.Eq("id", 42L), Projections.Constant("yes"), Projections.Constant("no"))).UniqueResultAsync<string>());
				Assert.AreEqual("no", result);
			}
		}

		[Test]
		public async Task UseInWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.In(Projections.Id(), new object[]{27})).ListAsync<Student>());
				Assert.AreEqual(27L, list[0].StudentNumber);
			}
		}

		[Test]
		public async Task UseLikeWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.Like(Projections.Property("Name"), "aye", MatchMode.Start)).ListAsync<Student>());
				Assert.AreEqual(27L, list[0].StudentNumber);
			}
		}

		[Test]
		public async Task UseInsensitiveLikeWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.InsensitiveLike(Projections.Property("Name"), "AYE", MatchMode.Start)).ListAsync<Student>());
				Assert.AreEqual(27L, list[0].StudentNumber);
			}
		}

		[Test]
		public async Task UseIdEqWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.IdEq(Projections.Id())).ListAsync<Student>());
				Assert.AreEqual(27L, list[0].StudentNumber);
			}
		}

		[Test]
		public async Task UseEqWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.Eq(Projections.Id(), 27L)).ListAsync<Student>());
				Assert.AreEqual(27L, list[0].StudentNumber);
			}
		}

		[Test]
		public async Task UseGtWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.Gt(Projections.Id(), 2L)).ListAsync<Student>());
				Assert.AreEqual(27L, list[0].StudentNumber);
			}
		}

		[Test]
		public async Task UseLtWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.Lt(Projections.Id(), 200L)).ListAsync<Student>());
				Assert.AreEqual(27L, list[0].StudentNumber);
			}
		}

		[Test]
		public async Task UseLeWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.Le(Projections.Id(), 27L)).ListAsync<Student>());
				Assert.AreEqual(27L, list[0].StudentNumber);
			}
		}

		[Test]
		public async Task UseGeWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.Ge(Projections.Id(), 27L)).ListAsync<Student>());
				Assert.AreEqual(27L, list[0].StudentNumber);
			}
		}

		[Test]
		public async Task UseBetweenWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.Between(Projections.Id(), 10L, 28L)).ListAsync<Student>());
				Assert.AreEqual(27L, list[0].StudentNumber);
			}
		}

		[Test]
		public async Task UseIsNullWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.IsNull(Projections.Id())).ListAsync<Student>());
				Assert.AreEqual(0, list.Count);
			}
		}

		[Test]
		public async Task UseIsNotNullWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.IsNotNull(Projections.Id())).ListAsync<Student>());
				Assert.AreEqual(1, list.Count);
			}
		}

		[Test]
		public async Task UseEqPropertyWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.EqProperty(Projections.Id(), Projections.Id())).ListAsync<Student>());
				Assert.AreEqual(1, list.Count);
			}
		}

		[Test]
		public async Task UseGePropertyWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.GeProperty(Projections.Id(), Projections.Id())).ListAsync<Student>());
				Assert.AreEqual(1, list.Count);
			}
		}

		[Test]
		public async Task UseGtPropertyWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.GtProperty(Projections.Id(), Projections.Id())).ListAsync<Student>());
				Assert.AreEqual(0, list.Count);
			}
		}

		[Test]
		public async Task UseLtPropertyWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.LtProperty(Projections.Id(), Projections.Id())).ListAsync<Student>());
				Assert.AreEqual(0, list.Count);
			}
		}

		[Test]
		public async Task UseLePropertyWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.LeProperty(Projections.Id(), Projections.Id())).ListAsync<Student>());
				Assert.AreEqual(1, list.Count);
			}
		}

		[Test]
		public async Task UseNotEqPropertyWithProjectionAsync()
		{
			using (ISession session = sessions.OpenSession())
			{
				IList<Student> list = await (session.CreateCriteria(typeof (Student)).Add(Expression.NotEqProperty("id", Projections.Id())).ListAsync<Student>());
				Assert.AreEqual(0, list.Count);
			}
		}
	}
}
#endif
