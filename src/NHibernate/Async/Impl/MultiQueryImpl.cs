using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NHibernate.Cache;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.Engine.Query.Sql;
using NHibernate.Exceptions;
using NHibernate.Hql;
using NHibernate.Loader.Custom;
using NHibernate.Loader.Custom.Sql;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NHibernate.Type;
using NHibernate.Util;
using System.Threading.Tasks;

namespace NHibernate.Impl
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class MultiQueryImpl : IMultiQuery
	{
		private async Task<IList> ListUsingQueryCacheAsync()
		{
			IQueryCache queryCache = session.Factory.GetQueryCache(cacheRegion);
			ISet<FilterKey> filterKeys = FilterKey.CreateFilterKeys(session.EnabledFilters, session.EntityMode);
			ISet<string> querySpaces = new HashSet<string>();
			List<IType[]> resultTypesList = new List<IType[]>(Translators.Count);
			for (int i = 0; i < Translators.Count; i++)
			{
				ITranslator queryTranslator = Translators[i];
				querySpaces.UnionWith(queryTranslator.QuerySpaces);
				resultTypesList.Add(queryTranslator.ReturnTypes);
			}

			int[] firstRows = new int[Parameters.Count];
			int[] maxRows = new int[Parameters.Count];
			for (int i = 0; i < Parameters.Count; i++)
			{
				RowSelection rowSelection = Parameters[i].RowSelection;
				firstRows[i] = rowSelection.FirstRow;
				maxRows[i] = rowSelection.MaxRows;
			}

			MultipleQueriesCacheAssembler assembler = new MultipleQueriesCacheAssembler(resultTypesList);
			QueryKey key = new QueryKey(session.Factory, SqlString, combinedParameters, filterKeys, null).SetFirstRows(firstRows).SetMaxRows(maxRows);
			IList result = await (assembler.GetResultFromQueryCacheAsync(session, combinedParameters, querySpaces, queryCache, key));
			if (result == null)
			{
				log.Debug("Cache miss for multi query");
				var list = await (DoListAsync());
				await (queryCache.PutAsync(key, new ICacheAssembler[]{assembler}, new object[]{list}, false, session));
				result = list;
			}

			return GetResultList(result);
		}

		public async Task<IList> ListAsync()
		{
			using (new SessionIdLoggingContext(session.SessionId))
			{
				bool cacheable = session.Factory.Settings.IsQueryCacheEnabled && isCacheable;
				combinedParameters = CreateCombinedQueryParameters();
				if (log.IsDebugEnabled)
				{
					log.DebugFormat("Multi query with {0} queries.", queries.Count);
					for (int i = 0; i < queries.Count; i++)
					{
						log.DebugFormat("Query #{0}: {1}", i, queries[i]);
					}
				}

				try
				{
					Before();
					return cacheable ? await (ListUsingQueryCacheAsync()) : await (ListIgnoreQueryCacheAsync());
				}
				finally
				{
					After();
				}
			}
		}

		public async Task<object> GetResultAsync(string key)
		{
			if (queryResults == null)
			{
				queryResults = await (ListAsync());
			}

			int queryResultPosition;
			if (!queryResultPositions.TryGetValue(key, out queryResultPosition))
				throw new InvalidOperationException(String.Format("The key '{0}' is unknown", key));
			return queryResults[queryResultPosition];
		}

		protected async Task<List<object>> DoListAsync()
		{
			bool statsEnabled = session.Factory.Statistics.IsStatisticsEnabled;
			var stopWatch = new Stopwatch();
			if (statsEnabled)
			{
				stopWatch.Start();
			}

			int rowCount = 0;
			var results = new List<object>();
			var hydratedObjects = new List<object>[Translators.Count];
			List<EntityKey[]>[] subselectResultKeys = new List<EntityKey[]>[Translators.Count];
			bool[] createSubselects = new bool[Translators.Count];
			try
			{
				using (var reader = await (resultSetsCommand.GetReaderAsync(commandTimeout != RowSelection.NoValue ? commandTimeout : (int ? )null)))
				{
					if (log.IsDebugEnabled)
					{
						log.DebugFormat("Executing {0} queries", translators.Count);
					}

					for (int i = 0; i < translators.Count; i++)
					{
						ITranslator translator = Translators[i];
						QueryParameters parameter = Parameters[i];
						int entitySpan = translator.Loader.EntityPersisters.Length;
						hydratedObjects[i] = entitySpan > 0 ? new List<object>() : null;
						RowSelection selection = parameter.RowSelection;
						int maxRows = Loader.Loader.HasMaxRows(selection) ? selection.MaxRows : int.MaxValue;
						if (!dialect.SupportsLimitOffset || !translator.Loader.UseLimit(selection, dialect))
						{
							Loader.Loader.Advance(reader, selection);
						}

						if (parameter.HasAutoDiscoverScalarTypes)
						{
							translator.Loader.AutoDiscoverTypes(reader);
						}

						LockMode[] lockModeArray = translator.Loader.GetLockModes(parameter.LockModes);
						EntityKey optionalObjectKey = Loader.Loader.GetOptionalObjectKey(parameter, session);
						createSubselects[i] = translator.Loader.IsSubselectLoadingEnabled;
						subselectResultKeys[i] = createSubselects[i] ? new List<EntityKey[]>() : null;
						translator.Loader.HandleEmptyCollections(parameter.CollectionKeys, reader, session);
						EntityKey[] keys = new EntityKey[entitySpan]; // we can reuse it each time
						if (log.IsDebugEnabled)
						{
							log.Debug("processing result set");
						}

						IList tempResults = new List<object>();
						int count;
						for (count = 0; count < maxRows && reader.Read(); count++)
						{
							if (log.IsDebugEnabled)
							{
								log.Debug("result set row: " + count);
							}

							rowCount++;
							object result = await (translator.Loader.GetRowFromResultSetAsync(reader, session, parameter, lockModeArray, optionalObjectKey, hydratedObjects[i], keys, true));
							tempResults.Add(result);
							if (createSubselects[i])
							{
								subselectResultKeys[i].Add(keys);
								keys = new EntityKey[entitySpan]; //can't reuse in this case
							}
						}

						if (log.IsDebugEnabled)
						{
							log.Debug(string.Format("done processing result set ({0} rows)", count));
						}

						results.Add(tempResults);
						if (log.IsDebugEnabled)
						{
							log.DebugFormat("Query {0} returned {1} results", i, tempResults.Count);
						}

						reader.NextResult();
					}

					for (int i = 0; i < translators.Count; i++)
					{
						ITranslator translator = translators[i];
						QueryParameters parameter = parameters[i];
						await (translator.Loader.InitializeEntitiesAndCollectionsAsync(hydratedObjects[i], reader, session, false));
						if (createSubselects[i])
						{
							translator.Loader.CreateSubselects(subselectResultKeys[i], parameter, session);
						}
					}
				}
			}
			catch (Exception sqle)
			{
				var message = string.Format("Failed to execute multi query: [{0}]", resultSetsCommand.Sql);
				log.Error(message, sqle);
				throw ADOExceptionHelper.Convert(session.Factory.SQLExceptionConverter, sqle, "Failed to execute multi query", resultSetsCommand.Sql);
			}

			if (statsEnabled)
			{
				stopWatch.Stop();
				session.Factory.StatisticsImplementor.QueryExecuted(string.Format("{0} queries (MultiQuery)", translators.Count), rowCount, stopWatch.Elapsed);
			}

			return results;
		}

		private async Task<IList> ListIgnoreQueryCacheAsync()
		{
			return GetResultList(await (DoListAsync()));
		}

		private async Task AggregateQueriesInformationAsync()
		{
			int queryIndex = 0;
			foreach (AbstractQueryImpl query in queries)
			{
				query.VerifyParameters();
				QueryParameters queryParameters = query.GetQueryParameters();
				queryParameters.ValidateParameters();
				foreach (var translator in await (query.GetTranslatorsAsync(session, queryParameters)))
				{
					translators.Add(translator);
					translatorQueryMap.Add(queryIndex);
					parameters.Add(queryParameters);
					ISqlCommand singleCommand = translator.Loader.CreateSqlCommand(queryParameters, session);
					resultSetsCommand.Append(singleCommand);
				}

				queryIndex++;
			}
		}

		public async Task<IMultiQuery> SetEntityAsync(string name, object val)
		{
			foreach (IQuery query in queries)
			{
				await (query.SetEntityAsync(name, val));
			}

			return this;
		}
	}
}