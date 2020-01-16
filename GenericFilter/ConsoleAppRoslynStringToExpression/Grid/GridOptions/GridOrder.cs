using System.Linq;
using System.Reflection;

namespace ConsoleAppRoslynStringToExpression.Grid.GridOptions
{
	public class GridOrder : IOrderable
	{
		public string OrderBy { get; set; }
		public OrderChoice Order { get; set; }
		public bool IsNestedObject() => OrderBy.Contains('.');
		public string GetParentFieldName() => OrderBy.Contains('.') ? OrderBy.Split('.').First() : OrderBy;
		public string[] GetChildrenFieldsNames() => GetChildrenFieldsNames(OrderBy);
		public bool CheckChildNodes(PropertyInfo parentField, string[] childrenFieldsNames)
		{
			var result = false;
			if (childrenFieldsNames?.Length > 0)
			{
				childrenFieldsNames.ToList().ForEach(childrenFieldName =>
						result = parentField.PropertyType.GetProperties()
									  .FirstOrDefault(prop => prop.Name == childrenFieldName) is var property && property != null);
			}

			return result;
		}
		private string[] GetChildrenFieldsNames(string field)
		{
			if (string.IsNullOrEmpty(field) || !field.Contains('.'))
				return null;

			var result = field.Split('.').ToList();
			result.RemoveAt(0);
			return result.ToArray();
		}
	}
}