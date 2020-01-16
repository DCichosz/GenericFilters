using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ConsoleAppGenericExpressionOldSchool.Grid.GridOptions
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
