using System;
using System.Linq;
using System.Reflection;

namespace ConsoleAppRoslynStringToExpression
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
			if (childrenFieldsNames != null && childrenFieldsNames.Length > 0)
			{
				childrenFieldsNames.ToList().ForEach(childrenFieldName =>
						result = parentField.PropertyType.GetProperties()
									  .FirstOrDefault(prop => prop.Name == childrenFieldName) is var property && property != null);
			}

			return result;
		}
		private string[] GetChildrenFieldsNames(string field) => (!string.IsNullOrEmpty(field) && field.Contains('.') ? OrderBy.Split('.') : null) is var result ? result?.Where(x => x != result[0])?.ToArray() : null;
	}
}