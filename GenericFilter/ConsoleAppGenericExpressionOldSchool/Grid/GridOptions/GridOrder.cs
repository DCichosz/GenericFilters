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
		public Type GetLastChildrenFieldType() => _lastChildrenFieldType;
		public bool CheckChildNodesAndSetLastChildFieldType(PropertyInfo parentField, string[] childrenFieldsNames)
		{
			var result = false;
			if (childrenFieldsNames?.Length > 0)
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
						result = false;
						break;
					}
				}
			}
			_lastChildrenFieldType = parentField.PropertyType;
			return result;
		}
		private Type _lastChildrenFieldType { get; set; }
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