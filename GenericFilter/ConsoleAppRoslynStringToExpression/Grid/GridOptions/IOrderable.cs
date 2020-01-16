namespace ConsoleAppRoslynStringToExpression.Grid.GridOptions
{
	public interface IOrderable : INestedTypesGridOptions
	{
		string OrderBy { get; set; }
		OrderChoice Order { get; set; }
	}
}