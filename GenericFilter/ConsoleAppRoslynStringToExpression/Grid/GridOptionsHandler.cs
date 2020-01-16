using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using ConsoleAppRoslynStringToExpression.Grid.GridOptions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ConsoleAppRoslynStringToExpression.Grid
{
	public static class GridOptionsHandler
	{
		public static IQueryable<TDbModel> ApplyDatabaseDataPagination<TDbModel>(
			this IQueryable<TDbModel> query, GridPagination pagination)
			where TDbModel : class =>
			 pagination?.PageSize > 0
				? query.Skip(pagination.Page * pagination.PageSize)
					.Take(pagination.PageSize)
				: query;

		public static IQueryable<TDbModel> ApplyDatabaseDataOrder<TDbModel>(this IQueryable<TDbModel> query, GridOrder order)
			where TDbModel : class
		{
			if (order == null || !(typeof(TDbModel).GetProperties()
					.FirstOrDefault(prop => string.Equals(prop.Name.ToLower(), order?.GetParentFieldName().ToLower(),
						StringComparison.OrdinalIgnoreCase)) is var property) ||
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
			var stringField = GetPropertyExpressionType(typeof(TDbModel), filter.Field) != typeof(string) ? $"{filter.Field}.ToString()" : filter.Field;
			var options = ScriptOptions.Default.AddReferences(typeof(TDbModel).Assembly);
			try
			{
				return filter.FilterMethod switch
				{
					FilterMethods.Equal => query.Where(CSharpScript
						.EvaluateAsync<Expression<Func<TDbModel, bool>>>($"x=>x.{filter.Field} == \"{filter.Field}\"",
							options).Result),
					FilterMethods.NotEqual => query.Where(CSharpScript
						.EvaluateAsync<Expression<Func<TDbModel, bool>>>($"x=>x.{filter.Field} != \"{filter.Field}\"",
							options).Result),
					FilterMethods.GreaterThan => query.Where(CSharpScript
						.EvaluateAsync<Expression<Func<TDbModel, bool>>>($"x=>x.{filter.Field} > \"{filter.Field}\"",
							options).Result),
					FilterMethods.GreaterOrEqual => query.Where(CSharpScript
						.EvaluateAsync<Expression<Func<TDbModel, bool>>>($"x=>x.{filter.Field} >= \"{filter.Field}\"",
							options).Result),
					FilterMethods.LessThan => query.Where(CSharpScript
						.EvaluateAsync<Expression<Func<TDbModel, bool>>>($"x=>x.{filter.Field} < \"{filter.Field}\"",
							options).Result),
					FilterMethods.LessThanOrEqual => query.Where(CSharpScript
						.EvaluateAsync<Expression<Func<TDbModel, bool>>>($"x=>x.{filter.Field} >= \"{filter.Field}\"",
							options).Result),
					FilterMethods.Equals => query.Where(CSharpScript
						.EvaluateAsync<Expression<Func<TDbModel, bool>>>($"x=>x.{stringField}.{nameof(string.Equals)}(\"{filter.Value}\")",
							options).Result),
					FilterMethods.Contains => query.Where(CSharpScript
						.EvaluateAsync<Expression<Func<TDbModel, bool>>>($"x=>x.{stringField}.{nameof(string.Contains)}(\"{filter.Value}\")",
							options).Result),
					FilterMethods.StartsWith => query.Where(CSharpScript
						.EvaluateAsync<Expression<Func<TDbModel, bool>>>($"x=>x.{stringField}.{nameof(string.StartsWith)}(\"{filter.Value}\")",
							options).Result),
					FilterMethods.EndsWith => query.Where(CSharpScript
						.EvaluateAsync<Expression<Func<TDbModel, bool>>>($"x=>x.{stringField}.{nameof(string.EndsWith)}(\"{filter.Value}\")",
							options).Result),
					FilterMethods.Default => query,
					_ => query
				};
			}
			catch (InvalidOperationException) { /* Prevent from blocking data - user-friendly , need refactor */}
			return query;
		}

		private static Type GetPropertyExpressionType(Type type, string fieldName) =>
			fieldName.Contains('.') && fieldName.Split('.').Aggregate<string, Expression>(Expression.Parameter(type, "x"), Expression.Property) is var propExpression ? propExpression.Type :
				Expression.Property(Expression.Parameter(type, "x"), fieldName).Type;
	}
}