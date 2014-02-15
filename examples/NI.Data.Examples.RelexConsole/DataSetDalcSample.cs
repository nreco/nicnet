using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;

using NI.Data.Dalc;

namespace NI.Examples.Dalc {
	
	public class DataSetDalcSample {

		IDalc Dalc;

		public void ShowData() {
			Console.WriteLine("Selecting from 'users' table (order by name):");
			var ds = new DataSet();
			var q = new Query("users");
			q.Sort = new string[] {"name"};
			Dalc.Load(ds, q);

			foreach (DataRow r in ds.Tables[q.SourceName].Rows)
				Console.WriteLine(String.Format("ID: {0}\tName: {1}", r["id"], r["name"] ));



		}

		public void ShowUser(string id) {
			Console.WriteLine( String.Format( "Selecting only user with ID={0}:", id ) );
			var data = new Hashtable();
			if (Dalc.LoadRecord(data, new Query("users", (QField)"id" == (QConst)id))) {
				Console.WriteLine("Record loaded, name = " + data["name"].ToString());
			}
			else
				Console.WriteLine("Record with ID=" + id + " not found");
		}

		public void ChangeUserName(string id, string newName) {
			// fast method
			if (Dalc.Update(new Hashtable { { "name", newName } }, new Query("users", (QField)"id" == (QConst)id)) > 0)
				Console.WriteLine("Name changed for user ID=" + id);
		}

		public void Delete(string id) {
			if (Dalc.Delete(new Query("users", (QField)"id" == (QConst)id)) > 0)
				Console.WriteLine("User removed: ID=" + id);
		}

		public void CreateDalc() {
			DatasetDalc dsDalc = new DatasetDalc();

			DataSet ds = new DataSet();
			
			ds.Tables.Add("users");
			DataColumn idColumn = ds.Tables["users"].Columns.Add("id", typeof(int));
			ds.Tables["users"].Columns.Add("name", typeof(string));
			ds.Tables["users"].Columns.Add("role", typeof(string));
			ds.Tables["users"].PrimaryKey = new DataColumn[] { idColumn };

			ds.Tables["users"].Rows.Add(new object[] { 1, "Vitalik", "1" });
			ds.Tables["users"].Rows.Add(new object[] { 2, "Bob", "1" });
			ds.Tables["users"].Rows.Add(new object[] { 3, "Joe", "2" });

			ds.Tables.Add("roles");
			idColumn = ds.Tables["roles"].Columns.Add("id", typeof(int));
			ds.Tables["roles"].Columns.Add("role", typeof(string));
			ds.Tables["roles"].PrimaryKey = new DataColumn[] { idColumn };

			ds.Tables["roles"].Rows.Add(new object[] { 1, "admin" });
			ds.Tables["roles"].Rows.Add(new object[] { 2, "user" });

			dsDalc.PersistedDS = ds;



			Dalc = dsDalc;
		}	

	}
}
