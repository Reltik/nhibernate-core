﻿#if NET_4_5
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH3237
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class ByCodeFixtureAsync : TestCaseMappingByCodeAsync
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
				rc.Property(x => x.DateTimeOffsetValue, m => m.Type(typeof (DateTimeOffsetUserType), new DateTimeOffsetUserType(TimeSpan.FromHours(10))));
				rc.Property(x => x.EnumValue, m => m.Type(typeof (EnumUserType), null));
				rc.Property(x => x.IntValue);
				rc.Property(x => x.LongValue);
				rc.Property(x => x.DecimalValue);
				rc.Property(x => x.DoubleValue);
				rc.Property(x => x.FloatValue);
				rc.Property(x => x.DateTimeValue);
				rc.Property(x => x.StringValue);
			}

			);
			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override async Task OnSetUpAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction transaction = session.BeginTransaction())
				{
					var e1 = new Entity{DateTimeOffsetValue = new DateTimeOffset(2012, 08, 06, 11, 0, 0, TimeSpan.FromHours(10)), EnumValue = TestEnum.Zero, IntValue = 1, LongValue = 1L, DecimalValue = 1.2m, DoubleValue = 1.2d, FloatValue = 1.2f, DateTimeValue = new DateTime(2012, 08, 06, 11, 0, 0), StringValue = "a"};
					await (session.SaveAsync(e1));
					var e2 = new Entity{DateTimeOffsetValue = new DateTimeOffset(2012, 08, 06, 12, 0, 0, TimeSpan.FromHours(10)), EnumValue = TestEnum.One, IntValue = 2, LongValue = 2L, DecimalValue = 2.2m, DoubleValue = 2.2d, FloatValue = 2.2f, DateTimeValue = new DateTime(2012, 08, 06, 12, 0, 0), StringValue = "b"};
					await (session.SaveAsync(e2));
					var e3 = new Entity{DateTimeOffsetValue = new DateTimeOffset(2012, 08, 06, 13, 0, 0, TimeSpan.FromHours(10)), EnumValue = TestEnum.Two, IntValue = 3, LongValue = 3L, DecimalValue = 3.2m, DoubleValue = 3.2d, FloatValue = 3.2f, DateTimeValue = new DateTime(2012, 08, 06, 13, 0, 0), StringValue = "c"};
					await (session.SaveAsync(e3));
					await (session.FlushAsync());
					await (transaction.CommitAsync());
				}
		}

		protected override async Task OnTearDownAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction transaction = session.BeginTransaction())
				{
					await (session.DeleteAsync("from System.Object"));
					await (session.FlushAsync());
					await (transaction.CommitAsync());
				}
		}

		[Test]
		public async Task Test_That_DateTimeOffset_UserType_Can_Be_Used_For_Max_And_Min_AggregatesAsync()
		{
			using (ISession session = OpenSession())
				using (session.BeginTransaction())
				{
					var min = await (session.Query<Entity>().MinAsync(e => e.DateTimeOffsetValue));
					Assert.AreEqual(new DateTimeOffset(2012, 08, 06, 11, 0, 0, TimeSpan.FromHours(10)), min);
					var max = await (session.Query<Entity>().MaxAsync(e => e.DateTimeOffsetValue));
					Assert.AreEqual(new DateTimeOffset(2012, 08, 06, 13, 0, 0, TimeSpan.FromHours(10)), max);
				}
		}

		[Test]
		public async Task Test_That_Enum_Type_Can_Be_Used_For_Max_And_Min_AggregatesAsync()
		{
			using (ISession session = OpenSession())
				using (session.BeginTransaction())
				{
					var min = await (session.Query<Entity>().MinAsync(e => e.EnumValue));
					Assert.AreEqual(TestEnum.Zero, min);
					var max = await (session.Query<Entity>().MaxAsync(e => e.EnumValue));
					Assert.AreEqual(TestEnum.Two, max);
				}
		}

		[Test]
		public async Task Test_Max_And_Min_Aggregates_Work_For_IntsAsync()
		{
			using (ISession session = OpenSession())
				using (session.BeginTransaction())
				{
					var min = await (session.Query<Entity>().MinAsync(e => e.IntValue));
					Assert.AreEqual(1, min);
					var max = await (session.Query<Entity>().MaxAsync(e => e.IntValue));
					Assert.AreEqual(3, max);
				}
		}

		[Test]
		public async Task Test_Max_And_Min_Aggregates_Work_For_LongsAsync()
		{
			using (ISession session = OpenSession())
				using (session.BeginTransaction())
				{
					var min = await (session.Query<Entity>().MinAsync(e => e.LongValue));
					Assert.AreEqual(1L, min);
					var max = await (session.Query<Entity>().MaxAsync(e => e.LongValue));
					Assert.AreEqual(3L, max);
				}
		}

		[Test]
		public async Task Test_Max_And_Min_Aggregates_Work_For_DecimalsAsync()
		{
			using (ISession session = OpenSession())
				using (session.BeginTransaction())
				{
					var min = await (session.Query<Entity>().MinAsync(e => e.DecimalValue));
					Assert.AreEqual(1.2m, min);
					var max = await (session.Query<Entity>().MaxAsync(e => e.DecimalValue));
					Assert.AreEqual(3.2m, max);
				}
		}

		[Test]
		public async Task Test_Max_And_Min_Aggregates_Work_For_DoublesAsync()
		{
			using (ISession session = OpenSession())
				using (session.BeginTransaction())
				{
					var min = await (session.Query<Entity>().MinAsync(e => e.DoubleValue));
					Assert.AreEqual(1.2d, min);
					var max = await (session.Query<Entity>().MaxAsync(e => e.DoubleValue));
					Assert.AreEqual(3.2d, max);
				}
		}

		[Test]
		public async Task Test_Max_And_Min_Aggregates_Work_For_FloatsAsync()
		{
			using (ISession session = OpenSession())
				using (session.BeginTransaction())
				{
					var min = await (session.Query<Entity>().MinAsync(e => e.FloatValue));
					Assert.AreEqual(1.2f, min);
					var max = await (session.Query<Entity>().MaxAsync(e => e.FloatValue));
					Assert.AreEqual(3.2f, max);
				}
		}

		[Test]
		public async Task Test_Max_And_Min_Aggregates_Work_For_DateTimesAsync()
		{
			using (ISession session = OpenSession())
				using (session.BeginTransaction())
				{
					var min = await (session.Query<Entity>().MinAsync(e => e.DateTimeValue));
					Assert.AreEqual(new DateTime(2012, 08, 06, 11, 0, 0), min);
					var max = await (session.Query<Entity>().MaxAsync(e => e.DateTimeValue));
					Assert.AreEqual(new DateTime(2012, 08, 06, 13, 0, 0), max);
				}
		}

		[Test]
		public async Task Test_Max_And_Min_Aggregates_Work_For_StringsAsync()
		{
			using (ISession session = OpenSession())
				using (session.BeginTransaction())
				{
					var min = await (session.Query<Entity>().MinAsync(e => e.StringValue));
					Assert.AreEqual("a", min);
					var max = await (session.Query<Entity>().MaxAsync(e => e.StringValue));
					Assert.AreEqual("c", max);
				}
		}
	}
}
#endif
