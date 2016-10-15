﻿#if NET_4_5
using System;
using System.Collections;
using System.Data.Common;
using System.Xml;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Util;
using System.Threading.Tasks;

namespace NHibernate.Type
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public abstract partial class AbstractType : IType
	{
		/// <summary>
		/// Disassembles the object into a cacheable representation.
		/// </summary>
		/// <param name = "value">The value to disassemble.</param>
		/// <param name = "session">The <see cref = "ISessionImplementor"/> is not used by this method.</param>
		/// <param name = "owner">optional parent entity object (needed for collections) </param>
		/// <returns>The disassembled, deep cloned state of the object</returns>
		/// <remarks>
		/// This method calls DeepCopy if the value is not null.
		/// </remarks>
		public virtual Task<object> DisassembleAsync(object value, ISessionImplementor session, object owner)
		{
			try
			{
				return Task.FromResult<object>(Disassemble(value, session, owner));
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}

		/// <summary>
		/// Reconstructs the object from its cached "disassembled" state.
		/// </summary>
		/// <param name = "cached">The disassembled state from the cache</param>
		/// <param name = "session">The <see cref = "ISessionImplementor"/> is not used by this method.</param>
		/// <param name = "owner">The parent Entity object is not used by this method</param>
		/// <returns>The assembled object.</returns>
		/// <remarks>
		/// This method calls DeepCopy if the value is not null.
		/// </remarks>
		public virtual Task<object> AssembleAsync(object cached, ISessionImplementor session, object owner)
		{
			try
			{
				return Task.FromResult<object>(Assemble(cached, session, owner));
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}

		public virtual Task BeforeAssembleAsync(object cached, ISessionImplementor session)
		{
			return TaskHelper.CompletedTask;
		}

		/// <summary>
		/// Should the parent be considered dirty, given both the old and current 
		/// field or element value?
		/// </summary>
		/// <param name = "old">The old value</param>
		/// <param name = "current">The current value</param>
		/// <param name = "session">The <see cref = "ISessionImplementor"/> is not used by this method.</param>
		/// <returns>true if the field is dirty</returns>
		/// <remarks>This method uses <c>IType.Equals(object, object)</c> to determine the value of IsDirty.</remarks>
		public virtual Task<bool> IsDirtyAsync(object old, object current, ISessionImplementor session)
		{
			try
			{
				return Task.FromResult<bool>(IsDirty(old, current, session));
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<bool>(ex);
			}
		}

		/// <summary>
		/// Retrieves an instance of the mapped class, or the identifier of an entity 
		/// or collection from a <see cref = "DbDataReader"/>.
		/// </summary>
		/// <param name = "rs">The <see cref = "DbDataReader"/> that contains the values.</param>
		/// <param name = "names">
		/// The names of the columns in the <see cref = "DbDataReader"/> that contain the 
		/// value to populate the IType with.
		/// </param>
		/// <param name = "session">the session</param>
		/// <param name = "owner">The parent Entity</param>
		/// <returns>An identifier or actual object mapped by this IType.</returns>
		/// <remarks>
		/// This method uses the <c>IType.NullSafeGet(DbDataReader, string[], ISessionImplementor, object)</c> method
		/// to Hydrate this <see cref = "AbstractType"/>.
		/// </remarks>
		public virtual Task<object> HydrateAsync(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
		{
			return NullSafeGetAsync(rs, names, session, owner);
		}

		/// <summary>
		/// Maps identifiers to Entities or Collections. 
		/// </summary>
		/// <param name = "value">An identifier or value returned by <c>Hydrate()</c></param>
		/// <param name = "session">The <see cref = "ISessionImplementor"/> is not used by this method.</param>
		/// <param name = "owner">The parent Entity is not used by this method.</param>
		/// <returns>The value.</returns>
		/// <remarks>
		/// There is nothing done in this method other than return the value parameter passed in.
		/// </remarks>
		public virtual Task<object> ResolveIdentifierAsync(object value, ISessionImplementor session, object owner)
		{
			try
			{
				return Task.FromResult<object>(ResolveIdentifier(value, session, owner));
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}

		public virtual Task<object> SemiResolveAsync(object value, ISessionImplementor session, object owner)
		{
			try
			{
				return Task.FromResult<object>(SemiResolve(value, session, owner));
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}

		/// <summary>
		/// Says whether the value has been modified
		/// </summary>
		public virtual Task<bool> IsModifiedAsync(object old, object current, bool[] checkable, ISessionImplementor session)
		{
			return IsDirtyAsync(old, current, session);
		}

		public virtual async Task<object> ReplaceAsync(object original, object target, ISessionImplementor session, object owner, IDictionary copyCache, ForeignKeyDirection foreignKeyDirection)
		{
			bool include;
			if (IsAssociationType)
			{
				IAssociationType atype = (IAssociationType)this;
				include = atype.ForeignKeyDirection == foreignKeyDirection;
			}
			else
			{
				include = ForeignKeyDirection.ForeignKeyFromParent.Equals(foreignKeyDirection);
			}

			return include ? await (ReplaceAsync(original, target, session, owner, copyCache)) : target;
		}

		public abstract Task<object> ReplaceAsync(object original, object current, ISessionImplementor session, object owner, IDictionary copiedAlready);

		/// <include file='..\..\Type\IType.cs.xmldoc' 
		///		path='//members[@type="IType"]/member[@name="M:IType.NullSafeGet(DbDataReader, string[], ISessionImplementor, object)"]/*'
		/// /> 
		public abstract Task<object> NullSafeGetAsync(DbDataReader rs, string[] names, ISessionImplementor session, object owner);

		/// <include file='..\..\Type\IType.cs.xmldoc' 
		///		path='//members[@type="IType"]/member[@name="M:IType.NullSafeGet(DbDataReader, string, ISessionImplementor, object)"]/*'
		/// /> 
		public abstract Task<object> NullSafeGetAsync(DbDataReader rs, string name, ISessionImplementor session, Object owner);

		/// <include file='..\..\Type\IType.cs.xmldoc' 
		///		path='//members[@type="IType"]/member[@name="M:IType.NullSafeSet(settable)"]/*'
		/// /> 
		public abstract Task NullSafeSetAsync(DbCommand st, object value, int index, bool[] settable, ISessionImplementor session);

		/// <include file='..\..\Type\IType.cs.xmldoc' 
		///		path='//members[@type="IType"]/member[@name="M:IType.NullSafeSet"]/*'
		/// /> 
		public abstract Task NullSafeSetAsync(DbCommand st, object value, int index, ISessionImplementor session);
		public abstract Task<bool> IsDirtyAsync(object old, object current, bool[] checkable, ISessionImplementor session);
	}
}
#endif