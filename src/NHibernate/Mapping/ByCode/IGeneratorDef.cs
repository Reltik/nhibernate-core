namespace NHibernate.Mapping.ByCode
{
	public partial interface IGeneratorDef
	{
		string Class { get; }
		object Params { get; }
		System.Type DefaultReturnType { get; }
		bool SupportedAsCollectionElementId { get; }
	}
}