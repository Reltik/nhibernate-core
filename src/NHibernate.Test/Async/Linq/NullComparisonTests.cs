﻿#if NET_4_5
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using NHibernate.DomainModel.Northwind.Entities;
using NUnit.Framework;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.Linq
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class NullComparisonTestsAsync : LinqTestCaseAsync
	{
		private static readonly AnotherEntity OutputSet = new AnotherEntity{Output = "output"};
		private static readonly AnotherEntity InputSet = new AnotherEntity{Input = "input"};
		private static readonly AnotherEntity BothSame = new AnotherEntity{Input = "i/o", Output = "i/o"};
		private static readonly AnotherEntity BothNull = new AnotherEntity();
		private static readonly AnotherEntity BothDifferent = new AnotherEntity{Input = "input", Output = "output"};
		[Test]
		public async Task NullEqualityAsync()
		{
			string nullVariable = null;
			string nullVariable2 = null;
			string notNullVariable = "input";
			Assert.AreEqual(5, (await (session.CreateCriteria<AnotherEntity>().ListAsync<AnotherEntity>())).Count);
			IQueryable<AnotherEntity> q;
			// Null literal against itself
			q =
				from x in session.Query<AnotherEntity>()where null == null
				select x;
			await (ExpectAllAsync(q));
			// Null against constants
			q =
				from x in session.Query<AnotherEntity>()where null == "value"
				select x;
			await (ExpectNoneAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where "value" == null
				select x;
			await (ExpectNoneAsync(q));
			// Null against variables
			q =
				from x in session.Query<AnotherEntity>()where null == nullVariable
				select x;
			await (ExpectAllAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where null == notNullVariable
				select x;
			await (ExpectNoneAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where nullVariable == null
				select x;
			await (ExpectAllAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where notNullVariable == null
				select x;
			await (ExpectNoneAsync(q));
			// Null against columns
			q =
				from x in session.Query<AnotherEntity>()where x.Input == null
				select x;
			await (ExpectInputIsNullAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where null == x.Input
				select x;
			await (ExpectInputIsNullAsync(q));
			// All null pairings with two columns.
			q =
				from x in session.Query<AnotherEntity>()where x.Input == null && x.Output == null
				select x;
			await (ExpectAsync(q, BothNull));
			q =
				from x in session.Query<AnotherEntity>()where x.Input != null && x.Output == null
				select x;
			await (ExpectAsync(q, InputSet));
			q =
				from x in session.Query<AnotherEntity>()where x.Input == null && x.Output != null
				select x;
			await (ExpectAsync(q, OutputSet));
			q =
				from x in session.Query<AnotherEntity>()where x.Input != null && x.Output != null
				select x;
			await (ExpectAsync(q, BothSame, BothDifferent));
			// Variables against variables
			q =
				from x in session.Query<AnotherEntity>()where nullVariable == nullVariable2
				select x;
			await (ExpectAllAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where nullVariable == notNullVariable
				select x;
			await (ExpectNoneAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where notNullVariable == nullVariable
				select x;
			await (ExpectNoneAsync(q));
			//// Variables against columns
			q =
				from x in session.Query<AnotherEntity>()where nullVariable == x.Input
				select x;
			await (ExpectInputIsNullAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where notNullVariable == x.Input
				select x;
			await (ExpectAsync(q, InputSet, BothDifferent));
			q =
				from x in session.Query<AnotherEntity>()where x.Input == nullVariable
				select x;
			await (ExpectInputIsNullAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where x.Input == notNullVariable
				select x;
			await (ExpectAsync(q, InputSet, BothDifferent));
			// Columns against columns
			q =
				from x in session.Query<AnotherEntity>()where x.Input == x.Output
				select x;
			await (ExpectAsync(q, BothSame));
		}

		[Test]
		public async Task NullInequalityAsync()
		{
			string nullVariable = null;
			string nullVariable2 = null;
			string notNullVariable = "input";
			IQueryable<AnotherEntity> q;
			// Null literal against itself
			q =
				from x in session.Query<AnotherEntity>()where null != null
				select x;
			await (ExpectNoneAsync(q));
			// Null against constants
			q =
				from x in session.Query<AnotherEntity>()where null != "value"
				select x;
			await (ExpectAllAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where "value" != null
				select x;
			await (ExpectAllAsync(q));
			// Null against variables
			q =
				from x in session.Query<AnotherEntity>()where null != nullVariable
				select x;
			await (ExpectNoneAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where null != notNullVariable
				select x;
			await (ExpectAllAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where nullVariable != null
				select x;
			await (ExpectNoneAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where notNullVariable != null
				select x;
			await (ExpectAllAsync(q));
			// Null against columns.
			q =
				from x in session.Query<AnotherEntity>()where x.Input != null
				select x;
			await (ExpectInputIsNotNullAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where null != x.Input
				select x;
			await (ExpectInputIsNotNullAsync(q));
			// Variables against variables.
			q =
				from x in session.Query<AnotherEntity>()where nullVariable != nullVariable2
				select x;
			await (ExpectNoneAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where nullVariable != notNullVariable
				select x;
			await (ExpectAllAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where notNullVariable != nullVariable
				select x;
			await (ExpectAllAsync(q));
			// Variables against columns.
			q =
				from x in session.Query<AnotherEntity>()where nullVariable != x.Input
				select x;
			await (ExpectInputIsNotNullAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where notNullVariable != x.Input
				select x;
			await (ExpectAsync(q, BothSame));
			q =
				from x in session.Query<AnotherEntity>()where x.Input != nullVariable
				select x;
			await (ExpectInputIsNotNullAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where x.Input != notNullVariable
				select x;
			await (ExpectAsync(q, BothSame));
			// Columns against columns
			q =
				from x in session.Query<AnotherEntity>()where x.Input != x.Output
				select x;
			await (ExpectAsync(q, BothDifferent));
		}

		[Test]
		public async Task NullEqualityInvertedAsync()
		{
			string nullVariable = null;
			string nullVariable2 = null;
			string notNullVariable = "input";
			IQueryable<AnotherEntity> q;
			// Null literal against itself
			q =
				from x in session.Query<AnotherEntity>()where !(null == null)select x;
			await (ExpectNoneAsync(q));
			// Null against constants
			q =
				from x in session.Query<AnotherEntity>()where !(null == "value")select x;
			await (ExpectAllAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !("value" == null)select x;
			await (ExpectAllAsync(q));
			// Null against variables
			q =
				from x in session.Query<AnotherEntity>()where !(null == nullVariable)select x;
			await (ExpectNoneAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(null == notNullVariable)select x;
			await (ExpectAllAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(nullVariable == null)select x;
			await (ExpectNoneAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(notNullVariable == null)select x;
			await (ExpectAllAsync(q));
			// Null against columns
			q =
				from x in session.Query<AnotherEntity>()where !(x.Input == null)select x;
			await (ExpectInputIsNotNullAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(null == x.Input)select x;
			await (ExpectInputIsNotNullAsync(q));
			// All null pairings with two columns.
			q =
				from x in session.Query<AnotherEntity>()where !(x.Input == null && x.Output == null)select x;
			await (ExpectAsync(q, InputSet, OutputSet, BothSame, BothDifferent));
			q =
				from x in session.Query<AnotherEntity>()where !(x.Input != null && x.Output == null)select x;
			await (ExpectAsync(q, OutputSet, BothNull, BothSame, BothDifferent));
			q =
				from x in session.Query<AnotherEntity>()where !(x.Input == null && x.Output != null)select x;
			await (ExpectAsync(q, InputSet, BothSame, BothDifferent, BothNull));
			q =
				from x in session.Query<AnotherEntity>()where !(x.Input != null && x.Output != null)select x;
			await (ExpectAsync(q, InputSet, OutputSet, BothNull));
			// Variables against variables
			q =
				from x in session.Query<AnotherEntity>()where !(nullVariable == nullVariable2)select x;
			await (ExpectNoneAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(nullVariable == notNullVariable)select x;
			await (ExpectAllAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(notNullVariable == nullVariable)select x;
			await (ExpectAllAsync(q));
			// Variables against columns
			q =
				from x in session.Query<AnotherEntity>()where !(nullVariable == x.Input)select x;
			await (ExpectInputIsNotNullAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(notNullVariable == x.Input)select x;
			await (ExpectAsync(q, BothSame));
			q =
				from x in session.Query<AnotherEntity>()where !(x.Input == nullVariable)select x;
			await (ExpectInputIsNotNullAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(x.Input == notNullVariable)select x;
			await (ExpectAsync(q, BothSame));
			// Columns against columns
			q =
				from x in session.Query<AnotherEntity>()where !(x.Input == x.Output)select x;
			await (ExpectAsync(q, BothDifferent));
		}

		[Test]
		public async Task NullInequalityInvertedAsync()
		{
			string nullVariable = null;
			string nullVariable2 = null;
			string notNullVariable = "input";
			IQueryable<AnotherEntity> q;
			// Null literal against itself
			q =
				from x in session.Query<AnotherEntity>()where !(null != null)select x;
			await (ExpectAllAsync(q));
			// Null against constants
			q =
				from x in session.Query<AnotherEntity>()where !(null != "value")select x;
			await (ExpectNoneAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !("value" != null)select x;
			await (ExpectNoneAsync(q));
			// Null against variables
			q =
				from x in session.Query<AnotherEntity>()where !(null != nullVariable)select x;
			await (ExpectAllAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(null != notNullVariable)select x;
			await (ExpectNoneAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(nullVariable != null)select x;
			await (ExpectAllAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(notNullVariable != null)select x;
			await (ExpectNoneAsync(q));
			// Null against columns.
			q =
				from x in session.Query<AnotherEntity>()where !(x.Input != null)select x;
			await (ExpectInputIsNullAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(null != x.Input)select x;
			await (ExpectInputIsNullAsync(q));
			// Variables against variables.
			q =
				from x in session.Query<AnotherEntity>()where !(nullVariable != nullVariable2)select x;
			await (ExpectAllAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(nullVariable != notNullVariable)select x;
			await (ExpectNoneAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(notNullVariable != nullVariable)select x;
			await (ExpectNoneAsync(q));
			// Variables against columns.
			q =
				from x in session.Query<AnotherEntity>()where !(nullVariable != x.Input)select x;
			await (ExpectInputIsNullAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(notNullVariable != x.Input)select x;
			await (ExpectAsync(q, InputSet, BothDifferent));
			q =
				from x in session.Query<AnotherEntity>()where !(x.Input != nullVariable)select x;
			await (ExpectInputIsNullAsync(q));
			q =
				from x in session.Query<AnotherEntity>()where !(x.Input != notNullVariable)select x;
			await (ExpectAsync(q, InputSet, BothDifferent));
			// Columns against columns
			q =
				from x in session.Query<AnotherEntity>()where !(x.Input != x.Output)select x;
			await (ExpectAsync(q, BothSame));
		}

		private Task ExpectAllAsync(IQueryable<AnotherEntity> q)
		{
			return ExpectAsync(q, BothNull, BothSame, BothDifferent, InputSet, OutputSet);
		}

		private Task ExpectNoneAsync(IQueryable<AnotherEntity> q)
		{
			return ExpectAsync(q);
		}

		private Task ExpectInputIsNullAsync(IQueryable<AnotherEntity> q)
		{
			return ExpectAsync(q, BothNull, OutputSet);
		}

		private Task ExpectInputIsNotNullAsync(IQueryable<AnotherEntity> q)
		{
			return ExpectAsync(q, InputSet, BothSame, BothDifferent);
		}

		private async Task ExpectAsync(IQueryable<AnotherEntity> q, params AnotherEntity[] entities)
		{
			IList<AnotherEntity> results = (await (q.ToListAsync())).OrderBy(l => Key(l)).ToList();
			IList<AnotherEntity> check = entities.OrderBy(l => Key(l)).ToList();
			Assert.AreEqual(check.Count, results.Count);
			for (int i = 0; i < check.Count; i++)
				Assert.AreEqual(Key(check[i]), Key(results[i]));
		}

		private string Key(AnotherEntity e)
		{
			return "Input=" + (e.Input ?? "NULL") + ", Output=" + (e.Output ?? "NULL");
		}
	}
}
#endif