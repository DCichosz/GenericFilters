using System.Collections.Generic;

namespace ConsoleAppGenericExpressionOldSchool.Grid.GridOptions
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
		public ICollection<TIOrderable> Order { get; set; }
		public ICollection<TIFilterable> Filters { get; set; }
	}
}
