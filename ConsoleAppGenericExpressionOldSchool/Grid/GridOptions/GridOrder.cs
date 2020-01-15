using System;
using System.Linq;
using System.Reflection;

namespace ConsoleAppGenericExpressionOldSchool.Grid.GridOptions
{
	public class GridOrder : IOrderable
	{
		public string OrderBy { get; set; }
		public OrderChoice Order { get; set; }
		public bool IsNestedObject() => OrderBy.Contains('.');
		public string GetParentFieldName() => OrderBy.Contains('.') ? OrderBy.Split('.').First() : OrderBy;
		public string[] GetChildrenFieldsNames() => GetChildrenFieldsNames(OrderBy);
		public string GetLastChildrenFieldName() => GetChildrenFieldsNames()?.LastOrDefault();
		public Type GetLastChildrenFieldType() => _lastChildrenFieldType;
		public void SetLastChildrenFieldType(Type fieldType) => _lastChildrenFieldType = fieldType;
		public bool CheckChildNodes(PropertyInfo parentField, string[] childrenFieldsNames)
		{
			var result = false;
			if (childrenFieldsNames != null && childrenFieldsNames.Length > 0)
			{
				foreach (var childrenFieldName in childrenFieldsNames)
				{
					if (parentField.PropertyType.GetProperties().FirstOrDefault(prop => prop.Name == childrenFieldName) is var property && property != null)
					{
						result = true;
						parentField = property;
					}
					else
					{
						break;
					}
				}
			}
			_lastChildrenFieldType = parentField.PropertyType;
			return result;
		}
		private Type _lastChildrenFieldType { get; set; }
		private string[] GetChildrenFieldsNames(string field) => (!string.IsNullOrEmpty(field) && field.Contains('.') ? OrderBy.Split('.') : null) is var result ? result?.Where(x => x != result[0])?.ToArray() : null;
	}
}