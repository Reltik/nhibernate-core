using System;
using System.Data;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using System.Data.Common;
using System.Threading.Tasks;

namespace NHibernate.Id.Insert
{
	/// <summary> 
	/// <see cref="IInsertGeneratedIdentifierDelegate"/> implementation where the
	/// underlying strategy causes the generated identitifer to be returned, as an
	/// effect of performing the insert statement, in a Output parameter.
	/// Thus, there is no need for an additional sql statement to determine the generated identitifer. 
	/// </summary>
	public class OutputParamReturningDelegate : AbstractReturningDelegate
	{
		private const string ReturnParameterName = "nhIdOutParam";
		private readonly ISessionFactoryImplementor factory;
		private readonly string idColumnName;
		private readonly SqlType paramType;
		private string driveGeneratedParamName = ReturnParameterName;

		public OutputParamReturningDelegate(IPostInsertIdentityPersister persister, ISessionFactoryImplementor factory)
			: base(persister)
		{
			if (Persister.RootTableKeyColumnNames.Length > 1)
			{
				throw new HibernateException("identity-style generator cannot be used with multi-column keys");
			}
			paramType = Persister.IdentifierType.SqlTypes(factory)[0];
			idColumnName = Persister.RootTableKeyColumnNames[0];
			this.factory = factory;
		}

		#region Overrides of AbstractReturningDelegate

		public override IdentifierGeneratingInsert PrepareIdentifierGeneratingInsert()
		{
			return new ReturningIdentifierInsert(factory, idColumnName, ReturnParameterName);
		}

		protected internal override async Task<DbCommand> Prepare(SqlCommandInfo insertSQL, ISessionImplementor session)
		{
			DbCommand command = await session.Batcher.PrepareCommand(CommandType.Text, insertSQL.Text, insertSQL.ParameterTypes).ConfigureAwait(false);
			//Add the output parameter
			IDbDataParameter idParameter = factory.ConnectionProvider.Driver.GenerateParameter(command, ReturnParameterName,
			                                                                                         paramType);
			driveGeneratedParamName = idParameter.ParameterName;

            if (factory.Dialect.InsertGeneratedIdentifierRetrievalMethod == InsertGeneratedIdentifierRetrievalMethod.OutputParameter)
                idParameter.Direction = ParameterDirection.Output;
            else if (factory.Dialect.InsertGeneratedIdentifierRetrievalMethod == InsertGeneratedIdentifierRetrievalMethod.ReturnValueParameter)
                idParameter.Direction = ParameterDirection.ReturnValue;
            else
                throw new System.NotImplementedException("Unsupported InsertGeneratedIdentifierRetrievalMethod: " + factory.Dialect.InsertGeneratedIdentifierRetrievalMethod);

			command.Parameters.Add(idParameter);
			return command;
		}

		public override async Task<object> ExecuteAndExtract(DbCommand insert, ISessionImplementor session)
		{
			await session.Batcher.ExecuteNonQuery(insert).ConfigureAwait(false);
			return Convert.ChangeType(((IDbDataParameter) insert.Parameters[driveGeneratedParamName]).Value, Persister.IdentifierType.ReturnedClass);
		}

		#endregion
	}
}