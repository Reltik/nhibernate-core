using System;
using System.Collections;
using System.Collections.Specialized;

namespace NHibernate.Dialect.Function
{
	public partial interface IFunctionGrammar
	{
		bool IsSeparator(string token);
		bool IsKnownArgument(string token);
	}
}
