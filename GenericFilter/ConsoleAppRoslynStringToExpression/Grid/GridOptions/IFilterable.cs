namespace ConsoleAppRoslynStringToExpression.Grid.GridOptions
{
	public interface IFilterable : INestedTypesGridOptions
	{
		string Field { get; set; }
		string Value { get; set; }
		FilterMethods FilterMethod { get; set; }
		object GetConvertedValueOrNull<T>();
		bool CanConvertValue<TDbModel>();
	}
}