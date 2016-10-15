﻿#if NET_4_5
using System.Collections;
using System.Threading.Tasks;
using Exception = System.Exception;
using NHibernate.Util;

namespace NHibernate.Impl
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FutureQueryBatch : FutureBatch<IQuery, IMultiQuery>
	{
		protected override Task<IList> GetResultsFromAsync(IMultiQuery multiApproach)
		{
			return multiApproach.ListAsync();
		}
	}
}
#endif