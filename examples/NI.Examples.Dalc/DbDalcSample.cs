using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Data;
using System.Data.SqlClient;

using NI.Data.Dalc;
using NI.Data.Dalc.SqlClient;
using NI.Data.Dalc.Linq;

using NI.Data.RelationalExpressions;

namespace NI.Examples.Dalc {
	
	public class DbDalcSample {
		IDbDalc Dalc;
		DataSet UserDs;

		public void ShowActiveUsers() {
			Console.WriteLine("Active users list:");
			// linq can be easily used with any IDalc...
			var q = from r in Dalc.Linq<DalcRecord>("users")
					where r["is_active"] == true
					select r;
			foreach (var r in q) {
				Console.WriteLine(String.Format("ID={0}\tName={1}\tEmail={2}", r["id"].Value, r["name"].Value, r["email"].Value));
			}
		}

		public void CreateUser(string name, string email, IDictionary<string, object> preferences) {
			var userDs = (DataSet)UserDs.Clone();

			var userTbl = userDs.Tables["users"];
			var userRow = userTbl.NewRow();
			userRow["name"] = name;
			userRow["email"] = email;
			userRow["is_active"] = true;
			userTbl.Rows.Add(userRow);

			Dalc.Update(userDs, "users");

		}

		public void ShowByRelex(string relex) {
			var relexParser = new RelExQueryParser();
			IQuery q;
			try {
				q = relexParser.Parse(relex);
			} catch (Exception ex) {
				Console.WriteLine("Invalid relex: " + ex.Message);
				return;
			}

			try {
				var ds = new DataSet();
				Console.WriteLine("Executing: " + q.ToString());
				Dalc.Load(ds, q);
				foreach (DataRow r in ds.Tables[q.SourceName].Rows) {
					var sb = new StringBuilder();
					foreach (DataColumn c in r.Table.Columns)
						sb.AppendFormat("{0}={1}\t", c.ColumnName, r[c]);
					Console.WriteLine(sb.ToString());
				}
			} catch (Exception ex) {
				Console.WriteLine("Cannot execute query ("+relex+"): " + ex.Message);
			}
		}

		public void LoadSchema() {
			UserDs = new DataSet();
			// just populate DS schema from DB
			Dalc.Load(UserDs, new Query("users", new QueryConditionNode((QConst)1, Conditions.Equal, (QConst)2)));
			// but PK we should set up manually
			var userTbl = UserDs.Tables["users"];
			userTbl.PrimaryKey = new DataColumn[] { userTbl.Columns["id"] };
			userTbl.Columns["id"].AutoIncrement = true;

			Dalc.Load(UserDs, new Query("user_preferences", new QueryConditionNode((QConst)1, Conditions.Equal, (QConst)2)));
			var userPrefTbl = UserDs.Tables["user_preferences"];
			userPrefTbl.PrimaryKey = new DataColumn[] { userPrefTbl.Columns["user_id"], userPrefTbl.Columns["preference"] };

		}


		public void CreateDalc() {
			var dbFileName = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "sampleDb.mdf" );

			// SQL Express is used in this sample
			var factory = new SqlFactory();
			var cmdGen = new DbCommandGenerator(factory);
			var conn = new SqlConnection(@"Data Source=.\SQLEXPRESS;AttachDbFilename="+dbFileName+@";Integrated Security=True;Connect Timeout=30;User Instance=True");

			var dbDalc = new DbDalc();
			dbDalc.AdapterWrapperFactory = factory;
			dbDalc.CommandGenerator = cmdGen;
			dbDalc.Connection = conn;

			Dalc = dbDalc;
		}

	}
}
