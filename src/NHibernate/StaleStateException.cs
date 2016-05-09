using System;
using System.Runtime.Serialization;

namespace NHibernate
{
	[Serializable]
	public partial class StaleStateException : HibernateException
	{
		public StaleStateException(string message) : base(message)
		{
		}

		protected StaleStateException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}