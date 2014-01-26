using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NI.Data;
using NI.Data.Linq;
using NUnit.Framework;

namespace NI.Tests.Data {
	public class LinqTests {

		[Test]
		public void LinqDalcRecord() {
			var dsDalc = DataSetDalcTest.createDsDalc();

			var q1 = from r in dsDalc.Linq<DalcRecord>("users")
						where r["name"] == "Joe"
						select r;
			Assert.AreEqual("Joe", q1.FirstOrDefault()["name"].Value.ToString());

			var q2 = dsDalc.Linq<DalcRecord>("users").OrderByDescending(r => r["id"]);
			var q2arr = q2.ToArray();

			Assert.AreEqual(3, q2arr.Length);
			Assert.AreEqual(3, Convert.ToInt32(q2arr[0]["id"].Value));

			// single value
			var q3 = dsDalc.Linq<DalcRecord>("users").OrderBy(r => r["id"]).Select(r => r["id"]);
			var q3val = q3.Single();
			Assert.AreEqual(1, Convert.ToInt32(q3val.Value));

		}

		[Test]
		public void LinqDto() {
			var dsDalc = DataSetDalcTest.createDsDalc();

			var q1 = from u in dsDalc.Linq<User>("users")
					 where u.id == 2
					 select u;
			var q1res = q1.First();
			Assert.AreEqual("Joe", q1res.name);
		}

		public class User {
			public int id { get; set; }
			public string name { get; set; }
			public string role { get; set; }
		}


	}
}
