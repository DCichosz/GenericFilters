using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConsoleAppGenericExpressionOldSchool.Grid;
using ConsoleAppGenericExpressionOldSchool.Grid.GridOptions;

namespace ConsoleAppGenericExpressionOldSchool
{
	class Program
	{
		static void Main(string[] args)
		{
			var gridOptions = new GridOptions
			{
				Order = new List<GridOrder>
				{
					new GridOrder
					{
						Order = OrderChoice.Ascending,
						OrderBy = "Name"
					}
				},
				Filters = new List<GridFilter>
				{
					new GridFilter
					{
						Field = "Date.Date.Month",
						FilterMethod = FilterMethods.Equal,
						Value = "2"
					}
				}
			};

			var list = TestModel.CreateElements(10);
			list.Insert(0, new TestModel { Boolean = true, Date = DateTime.Now.AddDays(-5), Name = "test", Value = 2 });
			list.Insert(0, new TestModel { Boolean = true, Date = DateTime.Now.AddDays(-5), Name = "test", Value = 3 });
			list.Insert(0, new TestModel { Boolean = true, Date = DateTime.Now.AddMonths(1), Name = "test", Value = 3 });
			list.Insert(0, new TestModel { Boolean = true, Date = DateTime.Now.AddDays(-5), Name = "testd2", Value = 2 });
			var lista = list.AsQueryable().ApplyDatabaseDataOrder(gridOptions.Order).ApplyDatabaseDataFilters(gridOptions.Filters).ToList();
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
