﻿#if NET_4_5
using System;
using System.Collections;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Persister.Collection;
using NHibernate.UserTypes;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Type
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class CustomCollectionType : CollectionType
	{
		public override Task<bool> ContainsAsync(object collection, object entity, ISessionImplementor session)
		{
			try
			{
				return Task.FromResult<bool>(Contains(collection, entity, session));
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<bool>(ex);
			}
		}

		public override Task<object> ReplaceElementsAsync(object original, object target, object owner, IDictionary copyCache, ISessionImplementor session)
		{
			try
			{
				return Task.FromResult<object>(ReplaceElements(original, target, owner, copyCache, session));
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}
	}
}
#endif