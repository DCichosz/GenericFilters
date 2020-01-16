using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ConsoleAppRoslynStringToExpression.Grid;
using ConsoleAppRoslynStringToExpression.Grid.GridOptions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ConsoleAppRoslynStringToExpression
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var gridOptions = new GridOptions
			{
				Order = new GridOrder
				{
					OrderBy = "Date.Day",
					Order = OrderChoice.Ascending
				},
				Filters = new List<GridFilter>
				{
					new GridFilter
					{
						Field = "Name",
						FilterMethod = FilterMethods.Equal,
						Value = "Name1"
					}
				}
			};
			var list = TestModel.CreateElements(10);
			var expression = CSharpScript
				.EvaluateAsync<Func<TestModel, int>>("x=>x.Value", ScriptOptions.Default.AddReferences(typeof(TestModel).Assembly)).Result;

			//var lista = list.AsQueryable().ApplyDatabaseDataFilters(gridOptions.Filters).ToList();
			var lista = list.AsQueryable().ApplyDatabaseDataOrder(gridOptions.Order).ToList();
			lista.ForEach(x => Console.WriteLine(JsonSerializer.Serialize(x)));
			Console.ReadLine();
		}
	}

	public class TestModel
	{
		public string Name { get; set; }
		public int Value { get; set; }
		public bool Boolean { get; set; }
		public DateTime Date { get; set; }

		public static List<TestModel> CreateElements(int quantity)
		{
			var list = new List<TestModel>();
			for (int i = 0; i < quantity; i++)
			{
				list.Add(new TestModel
				{
					Name = "Name" + i,
					Value = i,
					Boolean = i % 2 == 0,
					Date = DateTime.Now
				});
			}

			return list;
		}
	}
}
