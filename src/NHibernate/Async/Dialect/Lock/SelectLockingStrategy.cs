using System;
using System.Data;
using NHibernate.Engine;
using NHibernate.Persister.Entity;
using NHibernate.SqlCommand;
using NHibernate.Impl;
using NHibernate.Exceptions;
using System.Threading.Tasks;

namespace NHibernate.Dialect.Lock
{
	/// <summary> 
	/// A locking strategy where the locks are obtained through select statements.
	///  </summary>
	/// <seealso cref = "NHibernate.Dialect.Dialect.GetForUpdateString(NHibernate.LockMode)"/>
	/// <seealso cref = "NHibernate.Dialect.Dialect.AppendLockHint(NHibernate.LockMode, string)"/>
	/// <remarks>
	/// For non-read locks, this is achieved through the Dialect's specific
	/// SELECT ... FOR UPDATE syntax.
	/// </remarks>
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class SelectLockingStrategy : ILockingStrategy
	{
		public async Task LockAsync(object id, object version, object obj, ISessionImplementor session)
		{
			ISessionFactoryImplementor factory = session.Factory;
			try
			{
				IDbCommand st = session.Batcher.PrepareCommand(CommandType.Text, sql, lockable.IdAndVersionSqlTypes);
				IDataReader rs = null;
				try
				{
					lockable.IdentifierType.NullSafeSet(st, id, 0, session);
					if (lockable.IsVersioned)
					{
						lockable.VersionType.NullSafeSet(st, version, lockable.IdentifierType.GetColumnSpan(factory), session);
					}

					rs = await (session.Batcher.ExecuteReaderAsync(st));
					try
					{
						if (!rs.Read())
						{
							if (factory.Statistics.IsStatisticsEnabled)
							{
								factory.StatisticsImplementor.OptimisticFailure(lockable.EntityName);
							}

							throw new StaleObjectStateException(lockable.EntityName, id);
						}
					}
					finally
					{
						rs.Close();
					}
				}
				finally
				{
					session.Batcher.CloseCommand(st, rs);
				}
			}
			catch (HibernateException)
			{
				// Do not call Convert on HibernateExceptions
				throw;
			}
			catch (Exception sqle)
			{
				var exceptionContext = new AdoExceptionContextInfo{SqlException = sqle, Message = "could not lock: " + MessageHelper.InfoString(lockable, id, factory), Sql = sql.ToString(), EntityName = lockable.EntityName, EntityId = id};
				throw ADOExceptionHelper.Convert(session.Factory.SQLExceptionConverter, exceptionContext);
			}
		}
	}
}