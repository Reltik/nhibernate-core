using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using NHibernate.AdoNet;
using NHibernate.Cache;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Engine.Query;
using NHibernate.Engine.Query.Sql;
using NHibernate.Event;
using NHibernate.Exceptions;
using NHibernate.Hql;
using NHibernate.Loader.Custom;
using NHibernate.Loader.Custom.Sql;
using NHibernate.Persister.Entity;
using NHibernate.Transaction;
using NHibernate.Type;
using System.Threading.Tasks;

namespace NHibernate.Impl
{
	

	/// <summary> Functionality common to stateless and stateful sessions </summary>
	[Serializable]
	public abstract class AbstractSessionImpl : ISessionImplementor
	{
		[NonSerialized]
		private ISessionFactoryImplementor factory;

		private readonly Guid sessionId = Guid.NewGuid();
		private bool closed;

		public ITransactionContext TransactionContext
		{
			get; set;
		}

		private bool isAlreadyDisposed;

		private static readonly IInternalLogger logger = LoggerProvider.LoggerFor(typeof(AbstractSessionImpl));

		public Guid SessionId
		{
			get { return sessionId; }
		}

		internal AbstractSessionImpl() { }

		protected internal AbstractSessionImpl(ISessionFactoryImplementor factory)
		{
			this.factory = factory;
		}

		protected internal AbstractSessionImpl(ISessionFactoryImplementor factory, Guid sessionId)
			: this(factory)
		{
			this.sessionId = sessionId;
		}

		#region ISessionImplementor Members

		public void Initialize()
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				CheckAndUpdateSessionStatus();
			}
		}

		public abstract Task InitializeCollection(IPersistentCollection collection, bool writing);
		public abstract object InternalLoad(string entityName, object id, bool eager, bool isNullable);
		public abstract Task<object> InternalLoadAsync(string entityName, object id, bool eager, bool isNullable);
		public abstract object ImmediateLoad(string entityName, object id);
		public abstract Task<object> ImmediateLoadAsync(string entityName, object id);
		public abstract long Timestamp { get; }
		public abstract object CustomContext { get; }

		public EntityKey GenerateEntityKey(object id, IEntityPersister persister)
		{
			return GenerateEntityKey(id, persister, EntityMode);
		}

		protected EntityKey GenerateEntityKey(object id, IEntityPersister persister, EntityMode entityMode)
		{
			return new EntityKey(id, persister, entityMode);
		}

		public CacheKey GenerateCacheKey(object id, IType type, string entityOrRoleName)
		{
			return new CacheKey(id, type, entityOrRoleName, EntityMode, Factory);
		}

		public ISessionFactoryImplementor Factory
		{
			get { return factory; }
			protected set { factory = value; }
		}
		public abstract EntityMode EntityMode { get; }

		public abstract IBatcher Batcher { get; }
		public abstract void CloseSessionFromDistributedTransaction();

		[Obsolete("Use overload with IQueryExpression")]
		public virtual IList List(string query, QueryParameters parameters)
		{
			return ListAsync(query.ToQueryExpression(), parameters).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		[Obsolete("Use overload with IQueryExpression")]
		public virtual void List(string query, QueryParameters queryParameters, IList results)
		{
			ListAsync(query.ToQueryExpression(), queryParameters, results).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		[Obsolete("Use overload with IQueryExpression")]
		public virtual IList<T> List<T>(string query, QueryParameters queryParameters)
		{
			return ListAsync<T>(query.ToQueryExpression(), queryParameters).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public virtual IList List(IQueryExpression queryExpression, QueryParameters parameters)
		{
			return ListAsync(queryExpression, parameters).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public virtual async Task<IList> ListAsync(IQueryExpression queryExpression, QueryParameters parameters)
		{
			var results = (IList) typeof (List<>).MakeGenericType(queryExpression.Type)
												 .GetConstructor(System.Type.EmptyTypes)
												 .Invoke(null);
			await ListAsync(queryExpression, parameters, results).ConfigureAwait(false);
			return results;
		}

		public void List(IQueryExpression queryExpression, QueryParameters queryParameters, IList results)
		{
			ListAsync(queryExpression, queryParameters, results).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public abstract Task ListAsync(IQueryExpression queryExpression, QueryParameters queryParameters, IList results);

		public virtual IList<T> List<T>(IQueryExpression query, QueryParameters parameters)
		{
			return ListAsync<T>(query, parameters).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public virtual async Task<IList<T>> ListAsync<T>(IQueryExpression query, QueryParameters parameters)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				var results = new List<T>();
				await ListAsync(query, parameters, results).ConfigureAwait(false);
				return results;
			}
		}

		public virtual IList<T> List<T>(CriteriaImpl criteria)
		{
			return ListAsync<T>(criteria).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public virtual async Task<IList<T>> ListAsync<T>(CriteriaImpl criteria)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				var results = new List<T>();
				await ListAsync(criteria, results).ConfigureAwait(false);
				return results;
			}
		}

		public void List(CriteriaImpl criteria, IList results)
		{
			ListAsync(criteria, results).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public abstract Task ListAsync(CriteriaImpl criteria, IList results);

		public virtual IList List(CriteriaImpl criteria)
		{
			return ListAsync(criteria).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public virtual async Task<IList> ListAsync(CriteriaImpl criteria)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				var results = new List<object>();
				await ListAsync(criteria, results).ConfigureAwait(false);
				return results;
			}
		}

		public abstract IList ListFilter(object collection, string filter, QueryParameters parameters);
		public abstract Task<IList> ListFilterAsync(object collection, string filter, QueryParameters parameters);
		public abstract IList<T> ListFilter<T>(object collection, string filter, QueryParameters parameters);
		public abstract Task<IList<T>> ListFilterAsync<T>(object collection, string filter, QueryParameters parameters);
		public abstract IEnumerable EnumerableFilter(object collection, string filter, QueryParameters parameters);
		public abstract Task<IEnumerable> EnumerableFilterAsync(object collection, string filter, QueryParameters parameters);
		public abstract IEnumerable<T> EnumerableFilter<T>(object collection, string filter, QueryParameters parameters);
		public abstract Task<IEnumerable<T>> EnumerableFilterAsync<T>(object collection, string filter, QueryParameters parameters);
		public abstract IEntityPersister GetEntityPersister(string entityName, object obj);
		public abstract void AfterTransactionBegin(ITransaction tx);
		public abstract Task BeforeTransactionCompletion(ITransaction tx);
		public abstract Task AfterTransactionCompletion(bool successful, ITransaction tx);
		public abstract object GetContextEntityIdentifier(object obj);
		public abstract object Instantiate(string clazz, object id);

		public virtual IList List(NativeSQLQuerySpecification spec, QueryParameters queryParameters)
		{
			return ListAsync(spec, queryParameters).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public virtual async Task<IList> ListAsync(NativeSQLQuerySpecification spec, QueryParameters queryParameters)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				var results = new List<object>();
				await ListAsync(spec, queryParameters, results).ConfigureAwait(false);
				return results;
			}
		}

		public virtual void List(NativeSQLQuerySpecification spec, QueryParameters queryParameters, IList results)
		{
			ListAsync(spec, queryParameters, results).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public virtual async Task ListAsync(NativeSQLQuerySpecification spec, QueryParameters queryParameters, IList results)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				var query = new SQLCustomQuery(
					spec.SqlQueryReturns,
					spec.QueryString,
					spec.QuerySpaces,
					Factory);
				await ListCustomQueryAsync(query, queryParameters, results).ConfigureAwait(false);
			}
		}

		public virtual IList<T> List<T>(NativeSQLQuerySpecification spec, QueryParameters queryParameters)
		{
			return ListAsync<T>(spec, queryParameters).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public virtual async Task<IList<T>> ListAsync<T>(NativeSQLQuerySpecification spec, QueryParameters queryParameters)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				var results = new List<T>();
				await ListAsync(spec, queryParameters, results).ConfigureAwait(false);
				return results;
			}
		}

		public void ListCustomQuery(ICustomQuery customQuery, QueryParameters queryParameters, IList results)
		{
			ListCustomQueryAsync(customQuery, queryParameters, results).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public abstract Task ListCustomQueryAsync(ICustomQuery customQuery, QueryParameters queryParameters, IList results);

		public virtual IList<T> ListCustomQuery<T>(ICustomQuery customQuery, QueryParameters queryParameters)
		{
			return ListCustomQueryAsync<T>(customQuery, queryParameters).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public virtual async Task<IList<T>> ListCustomQueryAsync<T>(ICustomQuery customQuery, QueryParameters queryParameters)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				var results = new List<T>();
				await ListCustomQueryAsync(customQuery, queryParameters, results).ConfigureAwait(false);
				return results;
			}
		}

		public abstract object GetFilterParameterValue(string filterParameterName);
		public abstract IType GetFilterParameterType(string filterParameterName);
		public abstract IDictionary<string, IFilter> EnabledFilters { get; }

		public virtual IQuery GetNamedSQLQuery(string name)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				CheckAndUpdateSessionStatus();
				NamedSQLQueryDefinition nsqlqd = factory.GetNamedSQLQuery(name);
				if (nsqlqd == null)
				{
					throw new MappingException("Named SQL query not known: " + name);
				}
				IQuery query = new SqlQueryImpl(nsqlqd, this,
												factory.QueryPlanCache.GetSQLParameterMetadata(nsqlqd.QueryString));
				query.SetComment("named native SQL query " + name);
				InitQuery(query, nsqlqd);
				return query;
			}
		}

		[Obsolete("Use overload with IQueryExpression")]
		public virtual IQueryTranslator[] GetQueries(string query, bool scalar)
		{
			return GetQueries(query.ToQueryExpression(), scalar).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public int ExecuteNativeUpdate(NativeSQLQuerySpecification specification, QueryParameters queryParameters)
		{
			return ExecuteNativeUpdateAsync(specification, queryParameters).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public abstract Task<IQueryTranslator[]> GetQueries(IQueryExpression query, bool scalar);
		public abstract IInterceptor Interceptor { get; }
		public abstract EventListeners Listeners { get; }
		public abstract int DontFlushFromFind { get; }
		public abstract ConnectionManager ConnectionManager { get; }
		public abstract bool IsEventSource { get; }
		public abstract object GetEntityUsingInterceptor(EntityKey key);
		public abstract IPersistenceContext PersistenceContext { get; }
		public abstract CacheMode CacheMode { get; set; }
		public abstract bool IsOpen { get; }
		public abstract bool IsConnected { get; }
		public abstract FlushMode FlushMode { get; set; }
		public abstract string FetchProfile { get; set; }
		public abstract string BestGuessEntityName(object entity);
		public abstract string GuessEntityName(object entity);
		public abstract IDbConnection Connection { get; }
		public abstract Task<int> ExecuteNativeUpdateAsync(NativeSQLQuerySpecification specification, QueryParameters queryParameters);
		public abstract FutureCriteriaBatch FutureCriteriaBatch { get; protected internal set; }
		public abstract FutureQueryBatch FutureQueryBatch { get; protected internal set; }
		public abstract Task<DbConnection> GetConnection();

		public virtual IQuery GetNamedQuery(string queryName)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				CheckAndUpdateSessionStatus();
				NamedQueryDefinition nqd = factory.GetNamedQuery(queryName);
				IQuery query;
				if (nqd != null)
				{
					string queryString = nqd.QueryString;
					query = new QueryImpl(queryString, nqd.FlushMode, this, GetHQLQueryPlan(queryString.ToQueryExpression(), false).ParameterMetadata);
					query.SetComment("named HQL query " + queryName);
				}
				else
				{
					NamedSQLQueryDefinition nsqlqd = factory.GetNamedSQLQuery(queryName);
					if (nsqlqd == null)
					{
						throw new MappingException("Named query not known: " + queryName);
					}
					query = new SqlQueryImpl(nsqlqd, this,
											 factory.QueryPlanCache.GetSQLParameterMetadata(nsqlqd.QueryString));
					query.SetComment("named native SQL query " + queryName);
					nqd = nsqlqd;
				}
				InitQuery(query, nqd);
				return query;
			}
		}

		public bool IsClosed
		{
			get { return closed; }
		}

		protected internal virtual void CheckAndUpdateSessionStatus()
		{
			ErrorIfClosed();
			EnlistInAmbientTransactionIfNeeded();
		}

		protected internal virtual void ErrorIfClosed()
		{
			if (IsClosed || IsAlreadyDisposed)
			{
				throw new ObjectDisposedException("ISession", "Session is closed!");
			}
		}

		protected bool IsAlreadyDisposed
		{
			get { return isAlreadyDisposed; }
			set { isAlreadyDisposed = value; }
		}

		public abstract void Flush();
		public abstract Task FlushAsync();

		public abstract bool TransactionInProgress { get; }

		#endregion

		protected internal void SetClosed()
		{
			try
			{
				if (TransactionContext != null)
					TransactionContext.Dispose();
			}
			catch (Exception)
			{
				//ignore
			}
			closed = true;
		}

		private void InitQuery(IQuery query, NamedQueryDefinition nqd)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				query.SetCacheable(nqd.IsCacheable);
				query.SetCacheRegion(nqd.CacheRegion);
				if (nqd.Timeout != -1)
				{
					query.SetTimeout(nqd.Timeout);
				}
				if (nqd.FetchSize != -1)
				{
					query.SetFetchSize(nqd.FetchSize);
				}
				if (nqd.CacheMode.HasValue)
					query.SetCacheMode(nqd.CacheMode.Value);

				query.SetReadOnly(nqd.IsReadOnly);
				if (nqd.Comment != null)
				{
					query.SetComment(nqd.Comment);
				}
				query.SetFlushMode(nqd.FlushMode);
			}
		}

		public virtual IQuery CreateQuery(IQueryExpression queryExpression)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				CheckAndUpdateSessionStatus();
				var queryPlan = GetHQLQueryPlan(queryExpression, false);
				var query = new ExpressionQueryImpl(queryPlan.QueryExpression, this, queryPlan.ParameterMetadata);
				query.SetComment("[expression]");
				return query;
			}
		}

		public virtual IQuery CreateQuery(string queryString)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				CheckAndUpdateSessionStatus();
				var queryPlan = GetHQLQueryPlan(queryString.ToQueryExpression(), false);
				var query = new QueryImpl(queryString, this, queryPlan.ParameterMetadata);
				query.SetComment(queryString);
				return query;
			}
		}

		public virtual ISQLQuery CreateSQLQuery(string sql)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				CheckAndUpdateSessionStatus();
				SqlQueryImpl query = new SqlQueryImpl(sql, this, factory.QueryPlanCache.GetSQLParameterMetadata(sql));
				query.SetComment("dynamic native SQL query");
				return query;
			}
		}

		[Obsolete("Please use overload with IQueryExpression")]
		protected internal virtual IQueryPlan GetHQLQueryPlan(string query, bool shallow)
		{
			return GetHQLQueryPlan(query.ToQueryExpression(), shallow);
		}

		protected internal virtual IQueryExpressionPlan GetHQLQueryPlan(IQueryExpression queryExpression, bool shallow)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				return factory.QueryPlanCache.GetHQLQueryPlan(queryExpression, shallow, EnabledFilters);
			}
		}

		protected internal virtual NativeSQLQueryPlan GetNativeSQLQueryPlan(NativeSQLQuerySpecification spec)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				return factory.QueryPlanCache.GetNativeSQLQueryPlan(spec);
			}
		}

		protected Exception Convert(Exception sqlException, string message)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				return ADOExceptionHelper.Convert(factory.SQLExceptionConverter, sqlException, message);
			}
		}

		protected async Task AfterOperation(bool success)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				if (!ConnectionManager.IsInActiveTransaction)
				{
					ConnectionManager.AfterNonTransactionalQuery(success);
					await AfterTransactionCompletion(success, null).ConfigureAwait(false);
				}
			}
		}

		protected void EnlistInAmbientTransactionIfNeeded()
		{
			factory.TransactionFactory.EnlistInDistributedTransactionIfNeeded(this);
		}

		internal IOuterJoinLoadable GetOuterJoinLoadable(string entityName)
		{
			using (new SessionIdLoggingContext(SessionId))
			{
				var persister = Factory.GetEntityPersister(entityName) as IOuterJoinLoadable;
				if (persister == null)
				{
					throw new MappingException("class persister is not OuterJoinLoadable: " + entityName);
				}
				return persister;
			}
		}

		public abstract IEnumerable Enumerable(IQueryExpression queryExpression, QueryParameters queryParameters);

		public abstract Task<IEnumerable> EnumerableAsync(IQueryExpression queryExpression, QueryParameters queryParameters);

		[Obsolete("Use overload with IQueryExpression")]
		public virtual IEnumerable Enumerable(string query, QueryParameters queryParameters)
		{
			return Enumerable(query.ToQueryExpression(), queryParameters);
		}

		[Obsolete("Use overload with IQueryExpression")]
		public virtual IEnumerable<T> Enumerable<T>(string query, QueryParameters queryParameters)
		{
			return Enumerable<T>(query.ToQueryExpression(), queryParameters);
		}

		public abstract IEnumerable<T> Enumerable<T>(IQueryExpression queryExpression, QueryParameters queryParameters);

		public abstract Task<IEnumerable<T>> EnumerableAsync<T>(IQueryExpression queryExpression, QueryParameters queryParameters);

		[Obsolete("Use overload with IQueryExpression")]
		public virtual int ExecuteUpdate(string query, QueryParameters queryParameters)
		{
			return ExecuteUpdateAsync(query.ToQueryExpression(), queryParameters).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public virtual int ExecuteUpdate(IQueryExpression queryExpression, QueryParameters queryParameters)
		{
			return ExecuteUpdateAsync(queryExpression, queryParameters).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public abstract Task<int> ExecuteUpdateAsync(IQueryExpression queryExpression, QueryParameters queryParameters);
	}
}
