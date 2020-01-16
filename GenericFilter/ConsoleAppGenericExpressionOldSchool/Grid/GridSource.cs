using System.Collections.Generic;

namespace ConsoleAppGenericExpressionOldSchool.Grid
{
	public class GridSource<T> where T: class
	{
		public IEnumerable<T> Data { get; set; }
		public int Count { get; set; }
	}
}
