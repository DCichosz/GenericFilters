using System;
using System.Reflection;

namespace ConsoleAppRoslynStringToExpression.Grid.GridOptions
{
	public interface INestedTypesGridOptions
	{
		bool IsNestedObject();
		string GetParentFieldName();
		string[] GetChildrenFieldsNames();
		Type GetLastChildrenFieldType();
		bool CheckChildNodesAndSetLastChildFieldType(PropertyInfo parentField, string[] childrenFieldsNames);
	}
}
