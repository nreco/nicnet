using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NI.Examples.Dalc {
	public class Program {
		static void Main(string[] args) {

			// dataset dalc sample
			Console.WriteLine("1. DatasetDalc sample");
			var dsDalcSample = new DataSetDalcSample();
			dsDalcSample.CreateDalc();
			dsDalcSample.ShowData();
			dsDalcSample.ShowUser("1");
			dsDalcSample.ChangeUserName("2", "Steve");
			dsDalcSample.Delete("3");
			dsDalcSample.ShowData();

			Console.WriteLine();
			Console.WriteLine("2. DbDalc sample");
			var dbDalcSample = new DbDalcSample();
			dbDalcSample.CreateDalc();
			dbDalcSample.LoadSchema();

			dbDalcSample.CreateUser("Joe", "joe@somedomain.com", null);
			dbDalcSample.ShowActiveUsers();

			Console.WriteLine("Compact query syntax also can be used (try for example: users(name like \"%o%\" )[id]");
			var relex = Console.ReadLine();
			dbDalcSample.ShowByRelex(relex);

			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}
	}
}
