﻿#if NET_4_5
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Persister.Collection;
using NHibernate.Util;
using System.Threading.Tasks;

namespace NHibernate.Type
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class ArrayType : CollectionType
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name = "st"></param>
		/// <param name = "value"></param>
		/// <param name = "index"></param>
		/// <param name = "session"></param>
		public override async Task NullSafeSetAsync(DbCommand st, object value, int index, ISessionImplementor session)
		{
			await (base.NullSafeSetAsync(st, session.PersistenceContext.GetCollectionHolder(value), index, session));
		}

		public override async Task<object> ReplaceElementsAsync(object original, object target, object owner, IDictionary copyCache, ISessionImplementor session)
		{
			Array org = (Array)original;
			Array result = (Array)target;
			int length = org.Length;
			if (length != result.Length)
			{
				//note: this affects the return value!
				result = (Array)InstantiateResult(original);
			}

			IType elemType = GetElementType(session.Factory);
			for (int i = 0; i < length; i++)
			{
				result.SetValue(await (elemType.ReplaceAsync(org.GetValue(i), null, session, owner, copyCache)), i);
			}

			return result;
		}
	}
}
#endif