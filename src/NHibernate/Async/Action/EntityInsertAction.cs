﻿#if NET_4_5
using System;
using System.Diagnostics;
using NHibernate.Cache;
using NHibernate.Cache.Entry;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Persister.Entity;
using System.Threading.Tasks;

namespace NHibernate.Action
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public sealed partial class EntityInsertAction : EntityAction
	{
		public override async Task ExecuteAsync()
		{
			IEntityPersister persister = Persister;
			ISessionImplementor session = Session;
			object instance = Instance;
			object id = Id;
			bool statsEnabled = Session.Factory.Statistics.IsStatisticsEnabled;
			Stopwatch stopwatch = null;
			if (statsEnabled)
			{
				stopwatch = Stopwatch.StartNew();
			}

			bool veto = await (PreInsertAsync());
			// Don't need to lock the cache here, since if someone
			// else inserted the same pk first, the insert would fail
			if (!veto)
			{
				await (persister.InsertAsync(id, state, instance, Session));
				EntityEntry entry = Session.PersistenceContext.GetEntry(instance);
				if (entry == null)
				{
					throw new AssertionFailure("Possible nonthreadsafe access to session");
				}

				entry.PostInsert();
				if (persister.HasInsertGeneratedProperties)
				{
					await (persister.ProcessInsertGeneratedPropertiesAsync(id, instance, state, Session));
					if (persister.IsVersionPropertyGenerated)
					{
						version = Versioning.GetVersion(state, persister);
					}

					entry.PostUpdate(instance, state, version);
				}
			}

			ISessionFactoryImplementor factory = Session.Factory;
			if (IsCachePutEnabled(persister))
			{
				CacheEntry ce = new CacheEntry(state, persister, persister.HasUninitializedLazyProperties(instance, session.EntityMode), version, session, instance);
				cacheEntry = persister.CacheEntryStructure.Structure(ce);
				CacheKey ck = Session.GenerateCacheKey(id, persister.IdentifierType, persister.RootEntityName);
				bool put = persister.Cache.Insert(ck, cacheEntry, version);
				if (put && factory.Statistics.IsStatisticsEnabled)
				{
					factory.StatisticsImplementor.SecondLevelCachePut(Persister.Cache.RegionName);
				}
			}

			await (PostInsertAsync());
			if (statsEnabled && !veto)
			{
				stopwatch.Stop();
				factory.StatisticsImplementor.InsertEntity(Persister.EntityName, stopwatch.Elapsed);
			}
		}

		protected override async Task AfterTransactionCompletionProcessImplAsync(bool success)
		{
			//Make 100% certain that this is called before any subsequent ScheduledUpdate.afterTransactionCompletion()!!
			IEntityPersister persister = Persister;
			if (success && IsCachePutEnabled(persister))
			{
				CacheKey ck = Session.GenerateCacheKey(Id, persister.IdentifierType, persister.RootEntityName);
				bool put = persister.Cache.AfterInsert(ck, cacheEntry, version);
				if (put && Session.Factory.Statistics.IsStatisticsEnabled)
				{
					Session.Factory.StatisticsImplementor.SecondLevelCachePut(Persister.Cache.RegionName);
				}
			}

			if (success)
			{
				await (PostCommitInsertAsync());
			}
		}

		private async Task PostInsertAsync()
		{
			IPostInsertEventListener[] postListeners = Session.Listeners.PostInsertEventListeners;
			if (postListeners.Length > 0)
			{
				PostInsertEvent postEvent = new PostInsertEvent(Instance, Id, state, Persister, (IEventSource)Session);
				foreach (IPostInsertEventListener listener in postListeners)
				{
					await (listener.OnPostInsertAsync(postEvent));
				}
			}
		}

		private async Task PostCommitInsertAsync()
		{
			IPostInsertEventListener[] postListeners = Session.Listeners.PostCommitInsertEventListeners;
			if (postListeners.Length > 0)
			{
				PostInsertEvent postEvent = new PostInsertEvent(Instance, Id, state, Persister, (IEventSource)Session);
				foreach (IPostInsertEventListener listener in postListeners)
				{
					await (listener.OnPostInsertAsync(postEvent));
				}
			}
		}

		private async Task<bool> PreInsertAsync()
		{
			IPreInsertEventListener[] preListeners = Session.Listeners.PreInsertEventListeners;
			bool veto = false;
			if (preListeners.Length > 0)
			{
				var preEvent = new PreInsertEvent(Instance, Id, state, Persister, (IEventSource)Session);
				foreach (IPreInsertEventListener listener in preListeners)
				{
					veto |= await (listener.OnPreInsertAsync(preEvent));
				}
			}

			return veto;
		}
	}
}
#endif