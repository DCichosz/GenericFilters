namespace ConsoleAppRoslynStringToExpression
{
	public interface IFilterable
	{
		string Field { get; set; }
		string Value { get; set; }
		FilterMethods FilterMethod { get; set; }
		object GetConvertedValueOrNull<T>();
	}
}