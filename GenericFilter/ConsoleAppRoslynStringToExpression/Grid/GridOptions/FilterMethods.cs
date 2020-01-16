namespace ConsoleAppRoslynStringToExpression.Grid.GridOptions
{
	public enum FilterMethods
	{
		Default,
		// number / date / bool
		Equal,
		NotEqual,
		GreaterThan,
		GreaterOrEqual,
		LessThan,
		LessThanOrEqual,
		// string
		Contains,
		Equals,
		StartsWith,
		EndsWith
	}
}