using System.Threading.Tasks;
using System;
using NHibernate.Util;

namespace NHibernate.Event
{
	/// <summary> 
	/// Defines the contract for handling of collection initialization events 
	/// generated by a session. 
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial interface IInitializeCollectionEventListener
	{
		Task OnInitializeCollectionAsync(InitializeCollectionEvent @event);
	}
}