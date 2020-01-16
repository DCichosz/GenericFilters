using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ConsoleAppRoslynStringToExpression.Grid.GridOptions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ConsoleAppRoslynStringToExpression.Grid
{
	public static class GridOptionsHandler
	{
		public static IQueryable<TDbModel> ApplyDatabaseDataPagination<TDbModel>(this IQueryable<TDbModel> query, GridPagination pagination)
			where TDbModel : class =>
			pagination?.PageSize > 0
				? query.Skip(pagination.Page * pagination.PageSize)
					.Take(pagination.PageSize)
				: query;

		public static IQueryable<TDbModel> ApplyDatabaseDataOrder<TDbModel>(this IQueryable<TDbModel> query, GridOrder order)
			where TDbModel : class
		{
			if (order == null || !(typeof(TDbModel).GetProperties()
					.FirstOrDefault(prop => prop.Name == order?.GetParentFieldName()) is var property) ||
				property == null) return query;

			if (order.IsNestedObject() && !order.CheckChildNodes(property, order.GetChildrenFieldsNames()))
				return query;
			return query.GetOrderByDynamicQuery(order);
		}

		public static IQueryable<TDbModel> ApplyDatabaseDataFilters<TDbModel>(this IQueryable<TDbModel> query,
			ICollection<GridFilter> gridFilters) where TDbModel : class
		{
			if (gridFilters?.Count > 0)
			{
				gridFilters.ToList().ForEach(filter =>
				{
					if (filter.CanConvertValue<TDbModel>())
						query = query.ApplyFilter(filter);
				});
			}
			return query;
		}

		private static IOrderedQueryable<TDbModel> GetOrderByDynamicQuery<TDbModel>(this IQueryable<TDbModel> query, GridOrder order)
			where TDbModel : class =>
			order.Order switch
			{
				OrderChoice.Ascending => query.OrderBy(CSharpScript.EvaluateAsync<Expression<Func<TDbModel, object>>>($"x=>x.{order.OrderBy}",
					ScriptOptions.Default.AddReferences(typeof(TDbModel).Assembly)).Result),
				OrderChoice.Descending => query.OrderByDescending(CSharpScript.EvaluateAsync<Expression<Func<TDbModel, object>>>($"x=>x.{order.OrderBy}",
					ScriptOptions.Default.AddReferences(typeof(TDbModel).Assembly)).Result),
				_ => query.OrderBy(CSharpScript.EvaluateAsync<Expression<Func<TDbModel, object>>>($"x=>x.{order.OrderBy}",
					ScriptOptions.Default.AddReferences(typeof(TDbModel).Assembly)).Result)
			};

		private static IQueryable<TDbModel> ApplyFilter<TDbModel>(this IQueryable<TDbModel> query,
			GridFilter filter) where TDbModel : class
		{
			GetExpressionPropertyWithParameter(typeof(TDbModel), filter.Field, out _, out var parameter, out var propertyExpression);
			var field = propertyExpression.Type != typeof(string) ? $"{filter.Field}.ToString()" : filter.Field;
			// zajebiste
			// tera wykryjesz zmiany??
			try
			{
				return filter.FilterMethod switch
				{
					FilterMethods.Equal => query.Where(CSharpScript
						.EvaluateAsync<Expression<Func<TDbModel, bool>>>($"x=>x.{filter.Field} == \"{filter.Field}\"",
							ScriptOptions.Default.AddReferences(typeof(TDbModel).Assembly)).Result),
					FilterMethods.NotEqual => query.Where(GetDynamicWhereExpression<TDbModel>(Expression.NotEqual,
						parameter,
						propertyExpression, filter.GetConvertedValueOrNull<TDbModel>())),
					FilterMethods.GreaterThan => query.Where(GetDynamicWhereExpression<TDbModel>(Expression.GreaterThan,
						parameter,
						propertyExpression, filter.GetConvertedValueOrNull<TDbModel>())),
					FilterMethods.GreaterOrEqual => query.Where(GetDynamicWhereExpression<TDbModel>(
						Expression.GreaterThanOrEqual, parameter,
						propertyExpression, filter.GetConvertedValueOrNull<TDbModel>())),
					FilterMethods.LessThan => query.Where(GetDynamicWhereExpression<TDbModel>(Expression.LessThan,
						parameter, propertyExpression, filter.GetConvertedValueOrNull<TDbModel>())),
					FilterMethods.LessThanOrEqual => query.Where(GetDynamicWhereExpression<TDbModel>(
						Expression.LessThan,
						parameter, propertyExpression, filter.GetConvertedValueOrNull<TDbModel>())),
					FilterMethods.Equals => query.Where(GetStringExpression<TDbModel>(parameter,
						propertyExpression, nameof(FilterMethods.Equals), filter.GetConvertedValueOrNull<TDbModel>())),

					FilterMethods.Contains => query.Where(CSharpScript
						.EvaluateAsync<Expression<Func<TDbModel, bool>>>($"x=>x.{field}.Contains(\"{filter.Value}\")",
							ScriptOptions.Default.AddReferences(typeof(TDbModel).Assembly)).Result),


					//FilterMethods.Contains => query.Where(GetStringExpression<TDbModel>(parameter,
					//	propertyExpression, nameof(FilterMethods.Contains), filter.GetConvertedValueOrNull<TDbModel>())),
					FilterMethods.StartsWith => query.Where(GetStringExpression<TDbModel>(parameter,
						propertyExpression, nameof(FilterMethods.StartsWith), filter.GetConvertedValueOrNull<TDbModel>())),
					FilterMethods.EndsWith => query.Where(GetStringExpression<TDbModel>(parameter,
						propertyExpression, nameof(FilterMethods.EndsWith), filter.GetConvertedValueOrNull<TDbModel>())),
					FilterMethods.Default => query,
					_ => query
				};
			}
			catch (InvalidOperationException) { /* Prevent from blocking data - user-friendly , need refactor */}
			return query;
		}

		private static void GetExpressionPropertyWithParameter(Type type, string fieldName, out Type outputType,
			out ParameterExpression parameter, out Expression propertyExpression)
		{
			outputType = type;
			parameter = Expression.Parameter(type, nameof(parameter));
			propertyExpression = fieldName.Contains('.') && fieldName.Split('.').Aggregate<string, Expression>(parameter, Expression.Property) is var propExpression ? propExpression :
				Expression.Property(parameter, fieldName);
		}

		private static Expression<Func<TDbModel, bool>> GetDynamicWhereExpression<TDbModel>(Func<Expression, Expression, Expression> filterExpressionMethod, ParameterExpression parameter, Expression propertyExpression, object value)
			where TDbModel : class =>
			Expression.Lambda<Func<TDbModel, bool>>(
				filterExpressionMethod.Invoke(propertyExpression, Expression.Constant(value)), parameter);

		private static Expression<Func<TDbModel, bool>> GetStringExpression<TDbModel>(ParameterExpression parameter, Expression propertyExpression, string methodName, object value)
			where TDbModel : class
		{
			var method = typeof(string).GetMethod(methodName, new[] { typeof(string) });
			var someValue = Expression.Constant(value);

			var leftFixed = propertyExpression.Type != typeof(string)
				? Expression.Call(propertyExpression, typeof(object).GetMethod("ToString", Type.EmptyTypes))
				: propertyExpression;

			var methodExpression = Expression.Call(leftFixed, method, someValue);

			var lambdaExpression = Expression.Lambda<Func<TDbModel, bool>>(methodExpression, parameter);
			return lambdaExpression;
		}
	}
}