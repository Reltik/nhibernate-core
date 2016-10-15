﻿#if NET_4_5
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.Param;
using NHibernate.Persister.Entity;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NHibernate.Type;
using NHibernate.Util;
using System.Threading.Tasks;

namespace NHibernate.Loader.Criteria
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class CriteriaLoader : OuterJoinLoader
	{
		public async Task<IList> ListAsync(ISessionImplementor session)
		{
			return await (ListAsync(session, translator.GetQueryParameters(), querySpaces, resultTypes));
		}

		protected override async Task<object> GetResultColumnOrRowAsync(object[] row, IResultTransformer customResultTransformer, DbDataReader rs, ISessionImplementor session)
		{
			return ResolveResultTransformer(customResultTransformer).TransformTuple(await (GetResultRowAsync(row, rs, session)), ResultRowAliases);
		}

		protected override async Task<object[]> GetResultRowAsync(object[] row, DbDataReader rs, ISessionImplementor session)
		{
			object[] result;
			if (translator.HasProjection)
			{
				result = new object[ResultTypes.Length];
				for (int i = 0, position = 0; i < result.Length; i++)
				{
					int numColumns = ResultTypes[i].GetColumnSpan(session.Factory);
					if (numColumns > 1)
					{
						string[] typeColumnAliases = ArrayHelper.Slice(cachedProjectedColumnAliases, position, numColumns);
						result[i] = await (ResultTypes[i].NullSafeGetAsync(rs, typeColumnAliases, session, null));
					}
					else
					{
						result[i] = await (ResultTypes[i].NullSafeGetAsync(rs, cachedProjectedColumnAliases[position], session, null));
					}

					position += numColumns;
				}
			}
			else
			{
				result = ToResultRow(row);
			}

			return result;
		}
	}
}
#endif