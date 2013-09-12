using System;
using System.Data;
using System.Collections;
using System.IO;
using NI.Data;
using NI.Data.SQLite;
using System.Data.SQLite;

using NUnit.Framework;

namespace NI.Tests.Data.Dalc
{

	//TODO: revise test, cover more cases
	[TestFixture]
	[Category("NI.Data.SQLite")]
	public class SQLiteDalcTest
	{
		string dbFileName;
		DbDalc Dalc;

		[TestFixtureSetUp]
		public void SetUp() {
			dbFileName = Path.GetTempFileName()+".db";
			var connStr = String.Format("Data Source={0};FailIfMissing=false;Pooling=True;",dbFileName);

			Dalc = new DbDalc(new SQLiteDalcFactory(), connStr);

			// create tables if not exist
			Dalc.ExecuteNonQuery(@"
				CREATE TABLE [users]  (
					[id] INTEGER PRIMARY KEY AUTOINCREMENT,
					[name] TEXT,
					[role] INTEGER
				)
			");
			Dalc.ExecuteNonQuery(@"
				CREATE TABLE [roles]  (
					[id] INTEGER PRIMARY KEY AUTOINCREMENT,
					[role] TEXT
				)
			");

			Dalc.Insert("users", new Hashtable { { "name", "Vitalik" }, { "role", 1 } });
			Dalc.Insert("users", new Hashtable { { "name", "Darina" }, { "role", 1 } });
			Dalc.Insert("users", new Hashtable { { "name", "Stas" }, { "role", 2 } });
			Dalc.Insert("users", new Hashtable { { "name", "WUserToDelete" }, { "role", 3 } });

			Dalc.Insert("roles", new Hashtable { { "role", "admin" } });
			Dalc.Insert("roles", new Hashtable { { "role", "user" } });
			Dalc.Insert("roles", new Hashtable { { "role", "toDelete" } });
		}

		[TestFixtureTearDown]
		public void CleanUp() {
			((SQLiteConnection)Dalc.Connection).Shutdown();
			GC.Collect();
			if (dbFileName != null && File.Exists(dbFileName))
				File.Delete(dbFileName);
		}


		[Test]
		public void test_LoadRecord() {
			Query q = new Query("users");
			q.Condition = (QField)"name" == (QConst)"Vitalik";
			var res = Dalc.LoadRecord(q);
			Assert.NotNull(res,"LoadRecord failed");
			Assert.AreEqual(1,  Convert.ToInt32( res["id"] ), "LoadRecord failed");
		}
		
		[Test]
		public void test_Load() {
			DataSet ds = new DataSet();
			
			Query subQuery = new Query("roles");
			subQuery.Condition = (QField)"role"==(QConst)"admin";
			subQuery.Fields = new QField[] { "id" };
			
			Query q = new Query("users");
			q.Condition = (QField)"role" == subQuery;
			q.Sort = new QSortField[] { "name" };

			Dalc.Load( q, ds );
			if (ds.Tables["users"].Rows.Count!=2)
				throw new Exception("Load failed");
			if (ds.Tables["users"].Rows[0]["name"].ToString()!="Darina" ||
				ds.Tables["users"].Rows[1]["name"].ToString()!="Vitalik")
				throw new Exception("Load failed");
			
			q.Sort = new QSortField[] { "role", "name DESC" };
			Dalc.Load( q, ds );
			if (ds.Tables["users"].Rows.Count!=2)
				throw new Exception("Load failed");

			Console.Write(ds.GetXml());
			if (ds.Tables["users"].Rows[0]["name"].ToString()!="Vitalik" ||
				ds.Tables["users"].Rows[1]["name"].ToString()!="Darina")
				throw new Exception("Load failed");
			
			q.Condition = (QField)"role" == subQuery & (QField)"id">(QConst)5;
			Dalc.Load( q, ds );
			if (ds.Tables["users"].Rows.Count!=0)
				throw new Exception("Load failed");
		}
		
		[Test]
		public void test_Delete() {
			Query subQuery = new Query("roles");
			subQuery.Condition = (QField)"role" == (QConst)"toDelete";
			subQuery.Fields = new QField[] { "id" };
			Query q = new Query("users");
			q.Condition = (QField)"role" == subQuery;
			
			Dalc.Delete( q );
			
			Hashtable res  = new Hashtable();
			Assert.Null( Dalc.LoadRecord(q), "Delete failed");
			Assert.AreEqual(3, Dalc.RecordsCount(new Query("users")), "Delete failed");
		}
		
		[Test]
		public void test_Update() {
			DataSet ds = new DataSet();
			Query q = new Query("users");
			q.Condition = (QField)"id" == (QConst)1;
			
			Dalc.Load( q, ds );
			ds.Tables["users"].PrimaryKey = new[] { ds.Tables["users"].Columns["id"] };

			ds.Tables["users"].Rows[0]["name"] = "Vit";
			var newRow = ds.Tables["users"].Rows.Add(new object[] { 4, "Petya", 2 });
			Dalc.Update(ds.Tables["users"]);
			
			Assert.AreEqual(4, Dalc.RecordsCount(new Query("users")), "Update failed"); 

			var res = Dalc.LoadRecord(q);
			Assert.AreEqual("Vit", res["name"].ToString(), "Update failed");

			ds.Tables["users"].Rows[1].Delete();
			Dalc.Update( ds.Tables["users"] );
			if (Dalc.RecordsCount(new Query("users")) != 3)
				throw new Exception("Update failed");	
			
			res = new Hashtable();
			res["name"] = "VVV";
			var affected = Dalc.Update( q, res );
			Assert.AreEqual(1, affected, "Update by query failed");

			var res2 = Dalc.LoadRecord(q );
			Assert.AreEqual("VVV", res2["name"], "Update failed");
			
			
		}
		
		
			
		
	}
}
