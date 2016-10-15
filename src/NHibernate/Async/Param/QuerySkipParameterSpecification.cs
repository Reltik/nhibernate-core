﻿#if NET_4_5
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.Type;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Param
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class QuerySkipParameterSpecification : IParameterSpecification
	{
		public Task BindAsync(DbCommand command, IList<Parameter> sqlQueryParametersList, QueryParameters queryParameters, ISessionImplementor session)
		{
			return BindAsync(command, sqlQueryParametersList, 0, sqlQueryParametersList, queryParameters, session);
		}

		public async Task BindAsync(DbCommand command, IList<Parameter> multiSqlQueryParametersList, int singleSqlParametersOffset, IList<Parameter> sqlQueryParametersList, QueryParameters queryParameters, ISessionImplementor session)
		{
			// The QuerySkipParameterSpecification is unique so we can use multiSqlQueryParametersList
			var effectiveParameterLocations = multiSqlQueryParametersList.GetEffectiveParameterLocations(limitParametersNameForThisQuery).ToArray();
			if (effectiveParameterLocations.Length > 0)
			{
				// if the dialect does not support variable limits the parameter may was removed
				int value = Loader.Loader.GetOffsetUsingDialect(queryParameters.RowSelection, session.Factory.Dialect) ?? queryParameters.RowSelection.FirstRow;
				int position = effectiveParameterLocations[0];
				await (type.NullSafeSetAsync(command, value, position, session));
			}
		}
	}
}
#endif