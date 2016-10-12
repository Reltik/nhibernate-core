﻿#if NET_4_5
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Linq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH2583
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class MassTestingMoreOperatorsFixtureAsync : AbstractMassTestingFixtureAsync
	{
		protected override async Task<int> TestAndAssertAsync(Expression<Func<MyBO, bool>> condition, ISession session, IEnumerable<int> expectedIds)
		{
			IQueryable<int ? > result = session.Query<MyBO>().Where(condition).Select(bo => (int ? )bo.BO1.Id);
			var forceDBRun = await (result.ToListAsync());
			IEnumerable<int> resultNullTo0 = forceDBRun.Select(i => i ?? 0);
			var expectedBO1Ids = await (session.Query<MyBO>().Where(bo => expectedIds.Contains(bo.Id)).Select(bo => bo.BO1 == null ? 0 : bo.BO1.Id).ToListAsync());
			AreEqual(expectedBO1Ids, resultNullTo0.ToArray());
			// Unused result.
			return -1;
		}

		// Condition pattern: (A && B) && (C || D) SELECT E
		[Test]
		public async Task TestNestedPlusAsync()
		{
			await (RunTestAsync(x => (x.K1 + x.K2) + x.K2 == null || (x.K1 + x.K2) + x.K2 == null, Setters<TK, TK>(MyBO.SetK1, MyBO.SetK2)));
		}

		[Test]
		public async Task TestNestedPlusBehindNotAsync()
		{
			await (RunTestAsync(x => !((x.K1 + x.K2) + x.K2 != null), Setters<TK, TK>(MyBO.SetK1, MyBO.SetK2)));
		}

		[Test]
		public async Task TestNestedPlusBehindNotAndAsync()
		{
			await (RunTestAsync(x => !((x.K1 + x.K2) + x.K2 != null && (x.K1 + x.K2) + x.K2 != null), Setters<TK, TK>(MyBO.SetK1, MyBO.SetK2)));
		}

		[Test]
		public async Task TestNestedPlusBehindNotOrAsync()
		{
			await (RunTestAsync(x => !((x.K1 + x.K2) + x.K2 != null || (x.K1 + x.K2) + x.K2 != null), Setters<TK, TK>(MyBO.SetK1, MyBO.SetK2)));
		}

		[Test]
		public async Task TestNestedPlusBehindOrNavAsync()
		{
			await (RunTestAsync(x => (x.BO1.I1 + x.BO1.I2) + x.BO1.I2 == null || (x.BO1.I1 + x.BO1.I2) + x.BO1.I2 == null, Setters<TBO1_I, TBO1_I>(MyBO.SetBO1_I1, MyBO.SetBO1_I2)));
		}

		[Test]
		public async Task TestNestedPlusBehindNotNavAsync()
		{
			await (RunTestAsync(x => !((x.BO1.I1 + x.BO1.I2) + x.BO1.I2 != null), Setters<TBO1_I, TBO1_I>(MyBO.SetBO1_I1, MyBO.SetBO1_I2)));
		}

		[Test]
		public async Task TestNestedPlusBehindNotAndNavAsync()
		{
			await (RunTestAsync(x => !((x.BO1.I1 + x.BO1.I2) + x.BO1.I2 != null && (x.BO1.I1 + x.BO1.I2) + x.BO1.I2 != null), Setters<TBO1_I, TBO1_I>(MyBO.SetBO1_I1, MyBO.SetBO1_I2)));
		}

		[Test]
		public async Task TestNestedPlusBehindNotOrNavAsync()
		{
			await (RunTestAsync(x => !((x.BO1.I1 + x.BO1.I2) + x.BO1.I2 != null || (x.BO1.I1 + x.BO1.I2) + x.BO1.I2 != null), Setters<TBO1_I, TBO1_I>(MyBO.SetBO1_I1, MyBO.SetBO1_I2)));
		}

		[Test]
		public async Task TestNestedPlusBehindOrNav2Async()
		{
			await (RunTestAsync(x => (x.BO1.I1 + x.BO1.I2) + x.BO1.I2 == null || (x.BO2.J1 + x.BO2.J2) + x.BO2.J2 == null, Setters<TBO1_I, TBO1_I, TBO2_J, TBO2_J>(MyBO.SetBO1_I1, MyBO.SetBO1_I2, MyBO.SetBO2_J1, MyBO.SetBO2_J2)));
		}

		[Test]
		public async Task TestNestedPlusBehindNotOrNav2Async()
		{
			await (RunTestAsync(x => !((x.BO1.I1 + x.BO1.I2) + x.BO1.I2 == null || (x.BO2.J1 + x.BO2.J2) + x.BO2.J2 == null), Setters<TBO1_I, TBO1_I, TBO2_J, TBO2_J>(MyBO.SetBO1_I1, MyBO.SetBO1_I2, MyBO.SetBO2_J1, MyBO.SetBO2_J2)));
		}

		[Test]
		public async Task TestNestedPlusBehindNotAndNav2Async()
		{
			await (RunTestAsync(x => !((x.BO1.I1 + x.BO1.I2) + x.BO1.I2 == null && (x.BO2.J1 + x.BO2.J2) + x.BO2.J2 == null), Setters<TBO1_I, TBO1_I, TBO2_J, TBO2_J>(MyBO.SetBO1_I1, MyBO.SetBO1_I2, MyBO.SetBO2_J1, MyBO.SetBO2_J2)));
		}

		[Test]
		public async Task TestNestedPlusBehindOrNav3Async()
		{
			await (RunTestAsync(x => (x.BO1.I1 + x.BO1.I2) + x.BO2.J2 == null || (x.BO2.J1 + x.BO2.J2) + x.BO1.I2 == null, Setters<TBO1_I, TBO2_J, TBO2_J, TBO1_I>(MyBO.SetBO1_I1, MyBO.SetBO2_J2, MyBO.SetBO2_J1, MyBO.SetBO1_I2)));
		}

		[Test]
		public async Task TestNestedPlusBehindNotOrNav3Async()
		{
			await (RunTestAsync(x => !((x.BO1.I1 + x.BO1.I2) + x.BO2.J2 == null || (x.BO2.J1 + x.BO2.J2) + x.BO1.I2 == null), Setters<TBO1_I, TBO2_J, TBO2_J, TBO1_I>(MyBO.SetBO1_I1, MyBO.SetBO2_J2, MyBO.SetBO2_J1, MyBO.SetBO1_I2)));
		}

		[Test]
		public async Task TestNestedPlusBehindNotAndNav3Async()
		{
			await (RunTestAsync(x => !((x.BO1.I1 + x.BO1.I2) + x.BO2.J2 == null && (x.BO2.J1 + x.BO2.J2) + x.BO1.I2 == null), Setters<TBO1_I, TBO2_J, TBO2_J, TBO1_I>(MyBO.SetBO1_I1, MyBO.SetBO2_J2, MyBO.SetBO2_J1, MyBO.SetBO1_I2)));
		}
	}
}
#endif
