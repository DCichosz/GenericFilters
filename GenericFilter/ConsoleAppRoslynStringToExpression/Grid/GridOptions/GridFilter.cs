using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Castle.Core.Internal;

namespace ConsoleAppRoslynStringToExpression.Grid.GridOptions
{
	public class GridFilter : IFilterable
	{
		public string Field { get; set; }
		public string Value { get; set; }
		public FilterMethods FilterMethod { get; set; }
		public bool IsNestedObject() => Field.Contains('.');
		public string GetParentFieldName() => Field.Contains('.') ? Field.Split('.').First() : Field;
		public string[] GetChildrenFieldsNames() => GetChildrenFieldsNames(Field);
		public string GetLastChildrenFieldName() => GetChildrenFieldsNames()?.LastOrDefault();
		private Type LastChildrenFieldType { get; set; }
		public object GetConvertedValueOrNull<TDbModel>()
		{
			if (!Field.IsNullOrEmpty() && !Value.IsNullOrEmpty() && typeof(TDbModel).GetProperties().FirstOrDefault(prop => prop.Name == GetParentFieldName()) is var property && property != null)
			{
				if (IsNestedObject() && !CheckChildNodes(property, GetChildrenFieldsNames(Field)))
					return null;

				var fieldType = LastChildrenFieldType ?? property.PropertyType;

				fieldType = FilterMethod == FilterMethods.Contains || FilterMethod == FilterMethods.StartsWith ||
							FilterMethod == FilterMethods.EndsWith || FilterMethod == FilterMethods.Equals ? typeof(string) : fieldType;

				if (TypeDescriptor.GetConverter(fieldType) is var converter)
					return converter.IsValid(Value) ? converter.ConvertFromInvariantString(Value) : null;
			}
			return null;
		}

		public bool CanConvertValue<TDbModel>()
		{
			if (!Field.IsNullOrEmpty() && !Value.IsNullOrEmpty() && typeof(TDbModel).GetProperties().FirstOrDefault(prop => prop.Name == GetParentFieldName()) is var property && property != null)
			{
				if (IsNestedObject() && !CheckChildNodes(property, GetChildrenFieldsNames(Field)))
					return false;

				var fieldType = LastChildrenFieldType ?? property.PropertyType;

				fieldType = FilterMethod == FilterMethods.Contains || FilterMethod == FilterMethods.StartsWith ||
				            FilterMethod == FilterMethods.EndsWith || FilterMethod == FilterMethods.Equals ? typeof(string) : fieldType;

				if (TypeDescriptor.GetConverter(fieldType) is var converter)
					return converter.IsValid(Value);
			}
			return false;
		}

		private bool CheckChildNodes(PropertyInfo parentField, string[] childrenFieldsNames)
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

			LastChildrenFieldType = parentField.PropertyType;
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