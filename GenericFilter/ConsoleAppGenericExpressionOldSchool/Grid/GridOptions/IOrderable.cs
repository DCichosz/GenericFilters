using System;
using System.Reflection;

namespace ConsoleAppGenericExpressionOldSchool.Grid.GridOptions
{
	public interface IOrderable
	{
		string OrderBy { get; set; }
		OrderChoice Order { get; set; }
		bool IsNestedObject();
		string GetParentFieldName();
		string[] GetChildrenFieldsNames();
		Type GetLastChildrenFieldType();
		bool CheckChildNodesAndSetLastChildFieldType(PropertyInfo parentField, string[] childrenFieldsNames);
	}
}