﻿#if NET_4_5
using System.Collections.Generic;
using System.Data.Common;
using NHibernate.Cache;
using NHibernate.Cache.Entry;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Id;
using NHibernate.Metadata;
using NHibernate.Persister.Entity;
using NHibernate.Type;
using System.Threading.Tasks;
using Exception = System.Exception;
using NHibernate.Util;

namespace NHibernate.Persister.Collection
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial interface ICollectionPersister
	{
		/// <summary>
		/// Initialize the given collection with the given key
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "session"></param>
		Task InitializeAsync(object key, ISessionImplementor session);
		/// <summary>
		/// Read the key from a row of the <see cref = "DbDataReader"/>
		/// </summary>
		Task<object> ReadKeyAsync(DbDataReader rs, string[] keyAliases, ISessionImplementor session);
		/// <summary>
		/// Read the element from a row of the <see cref = "DbDataReader"/>
		/// </summary>
		 //TODO: the ReadElement should really be a parameterized TElement
		Task<object> ReadElementAsync(DbDataReader rs, object owner, string[] columnAliases, ISessionImplementor session);
		/// <summary>
		/// Read the index from a row of the <see cref = "DbDataReader"/>
		/// </summary>
		 //TODO: the ReadIndex should really be a parameterized TIndex
		Task<object> ReadIndexAsync(DbDataReader rs, string[] columnAliases, ISessionImplementor session);
		/// <summary>
		/// Read the identifier from a row of the <see cref = "DbDataReader"/>
		/// </summary>
		 //TODO: the ReadIdentifier should really be a parameterized TIdentifier
		Task<object> ReadIdentifierAsync(DbDataReader rs, string columnAlias, ISessionImplementor session);
		/// <summary>
		/// Completely remove the persistent state of the collection
		/// </summary>
		/// <param name = "id"></param>
		/// <param name = "session"></param>
		Task RemoveAsync(object id, ISessionImplementor session);
		/// <summary>
		/// (Re)create the collection's persistent state
		/// </summary>
		/// <param name = "collection"></param>
		/// <param name = "key"></param>
		/// <param name = "session"></param>
		Task RecreateAsync(IPersistentCollection collection, object key, ISessionImplementor session);
		/// <summary>
		/// Delete the persistent state of any elements that were removed from the collection
		/// </summary>
		/// <param name = "collection"></param>
		/// <param name = "key"></param>
		/// <param name = "session"></param>
		Task DeleteRowsAsync(IPersistentCollection collection, object key, ISessionImplementor session);
		/// <summary>
		/// Update the persistent state of any elements that were modified
		/// </summary>
		/// <param name = "collection"></param>
		/// <param name = "key"></param>
		/// <param name = "session"></param>
		Task UpdateRowsAsync(IPersistentCollection collection, object key, ISessionImplementor session);
		/// <summary>
		/// Insert the persistent state of any new collection elements
		/// </summary>
		/// <param name = "collection"></param>
		/// <param name = "key"></param>
		/// <param name = "session"></param>
		Task InsertRowsAsync(IPersistentCollection collection, object key, ISessionImplementor session);
		Task<int> GetSizeAsync(object key, ISessionImplementor session);
		Task<bool> IndexExistsAsync(object key, object index, ISessionImplementor session);
		Task<bool> ElementExistsAsync(object key, object element, ISessionImplementor session);
		/// <summary>
		/// Try to find an element by a given index.
		/// </summary>
		/// <param name = "key">The key of the collection (collection-owner identifier)</param>
		/// <param name = "index">The given index.</param>
		/// <param name = "session">The active <see cref = "ISession"/>.</param>
		/// <param name = "owner">The owner of the collection.</param>
		/// <returns>The value of the element where available; otherwise <see cref = "NotFoundObject"/>.</returns>
		Task<object> GetElementByIndexAsync(object key, object index, ISessionImplementor session, object owner);
	}
}
#endif