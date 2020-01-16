using System.Collections.Generic;
using ConsoleAppRoslynStringToExpression.Grid.GridOptions;

namespace ConsoleAppRoslynStringToExpression
{
	public class GridOptions : GridOptions<GridPagination, GridOrder, GridFilter>
	{
		
	}

	public class GridOptions<TIPageable, TIOrderable, TIFilterable>
	where TIPageable : class, IPageable
	where TIOrderable : class, IOrderable
	where TIFilterable : class, IFilterable
	{
		public TIPageable Pagination { get; set; }
		public TIOrderable Order { get; set; }
		public ICollection<TIFilterable> Filters { get; set; }
	}
}
