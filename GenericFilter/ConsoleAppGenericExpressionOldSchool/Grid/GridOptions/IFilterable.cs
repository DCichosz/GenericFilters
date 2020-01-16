using System.Reflection;

namespace ConsoleAppGenericExpressionOldSchool.Grid.GridOptions
{
	public interface IFilterable
	{
		string Field { get; set; }
		string Value { get; set; }
		FilterMethods FilterMethod { get; set; }
		object GetConvertedValueOrNull<T>();
		bool IsNestedObject();
		string GetParentFieldName();
		string[] GetChildrenFieldsNames();
		bool CheckChildNodesAndSetLastChildFieldType(PropertyInfo parentField, string[] childrenFieldsNames);
		bool CanConvertValue<TDbModel>();
	}
}