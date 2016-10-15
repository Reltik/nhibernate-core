﻿#if NET_4_5
using NHibernate.Engine;
using System.Threading.Tasks;
using Exception = System.Exception;
using NHibernate.Util;

namespace NHibernate.Type
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial interface ICacheAssembler
	{
		/// <summary> Return a cacheable "disassembled" representation of the object.</summary>
		/// <param name = "value">the value to cache </param>
		/// <param name = "session">the session </param>
		/// <param name = "owner">optional parent entity object (needed for collections) </param>
		/// <returns> the disassembled, deep cloned state </returns>		
		Task<object> DisassembleAsync(object value, ISessionImplementor session, object owner);
		/// <summary> Reconstruct the object from its cached "disassembled" state.</summary>
		/// <param name = "cached">the disassembled state from the cache </param>
		/// <param name = "session">the session </param>
		/// <param name = "owner">the parent entity object </param>
		/// <returns> the the object </returns>
		Task<object> AssembleAsync(object cached, ISessionImplementor session, object owner);
		/// <summary>
		/// Called before assembling a query result set from the query cache, to allow batch fetching
		/// of entities missing from the second-level cache.
		/// </summary>
		Task BeforeAssembleAsync(object cached, ISessionImplementor session);
	}
}
#endif