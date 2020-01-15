namespace ConsoleAppRoslynStringToExpression
{
	public interface IOrderable
	{
		string OrderBy { get; set; }
		OrderChoice Order { get; set; }
	}
}