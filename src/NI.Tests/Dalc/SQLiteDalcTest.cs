using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
		public DbDalc Dalc;

		[TestFixtureSetUp]
		public void SetUp() {
			dbFileName = Path.GetTempFileName()+".db";
			var connStr = String.Format("Data Source={0};FailIfMissing=false;Pooling=False;",dbFileName);

			Dalc = new DbDalc(new SQLiteDalcFactory(), connStr);
			var usersViewSql = @"
select @SqlFields from users u
left join roles r on (u.role=r.id)
@SqlWhere[where {0}]
@SqlOrderBy[order by {0};order by u.id desc]
			".Trim();

			Dalc.CommandGenerator = new DbCommandGenerator(Dalc.DbFactory) {
				Views = new[] {
					new DbDalcView("users_view", usersViewSql, "u.*,r.role as role_name","count(u.id)") {
						FieldMapping = new Dictionary<string,string>() { {"role_name", "r.role"} }
					}
				}
			};

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

			Dalc.Insert("users", new Hashtable { { "name", "Mike" }, { "role", 1 } });
			Dalc.Insert("users", new Hashtable { { "name", "Joe" }, { "role", 1 } });
			Dalc.Insert("users", new Hashtable { { "name", "Stas" }, { "role", 2 } });
			Dalc.Insert("users", new Hashtable { { "name", "WUserToDelete" }, { "role", 3 } });

			Dalc.Insert("roles", new Hashtable { { "role", "admin" } });
			Dalc.Insert("roles", new Hashtable { { "role", "user" } });
			Dalc.Insert("roles", new Hashtable { { "role", "toDelete" } });
		}

		[TestFixtureTearDown]
		public void CleanUp() {
			
			((SQLiteConnection)Dalc.Connection).Dispose();
			SQLiteConnection.ClearAllPools();
			GC.Collect();
			if (dbFileName != null && File.Exists(dbFileName))
				File.Delete(dbFileName);
		}


		[Test]
		public void test_LoadRecord() {
			Query q = new Query("users");
			q.Condition = (QField)"name" == (QConst)"Mike";
			var res = Dalc.LoadRecord(q);
			Assert.NotNull(res,"LoadRecord failed");
			Assert.AreEqual(1,  Convert.ToInt32( res["id"] ), "LoadRecord failed");
		}

		[Test]
		public void test_DataView() {
			var rs = Dalc.LoadAllRecords(new Query("users_view", (QField)"role_name" == (QConst)"user"));

			Assert.AreEqual(1, rs.Length, "Load data from data view failed");
			Assert.AreEqual("user", rs[0]["role_name"].ToString(), "Load data from data view failed");

			// check order by
			Assert.AreEqual(4, Dalc.RecordsCount(new Query("users_view")), "Invalid record count for dalc view");
			

			var testSet = new Dictionary<Query,string>() {
				{new Query("users_view") { Fields = new QField[] { "id" } }, @"
select u.*,r.role as role_name from users u
left join roles r on (u.role=r.id)

order by u.id desc".Trim()},

				{new Query("users_view") { Sort = new QSort[] { "role_name desc" } }, @"
select u.*,r.role as role_name from users u
left join roles r on (u.role=r.id)

order by r.role desc".Trim()},


			};

			foreach (var testCase in testSet) {
				var testView = ((DbCommandGenerator)Dalc.CommandGenerator).Views[0];
				using (var testCmd = Dalc.DbFactory.CreateCommand()) {
					var sqlFactory = Dalc.DbFactory.CreateSqlBuilder(testCmd);
					var s = testView.ComposeSelect(testCase.Key, sqlFactory);
					Assert.AreEqual( testCase.Value, s );
				}
			}
		}
		
		[Test]
		public void test_Load() {
			DataSet ds = new DataSet();
			
			Query subQuery = new Query("roles");
			subQuery.Condition = (QField)"role"==(QConst)"admin";
			subQuery.Fields = new QField[] { "id" };
			
			Query q = new Query("users");
			q.Condition = (QField)"role" == subQuery;
			q.Sort = new QSort[] { "name" };

			Dalc.Load( q, ds );
			Assert.AreEqual(2, ds.Tables["users"].Rows.Count, "Load failed: rows count");
			
			Assert.AreEqual("Joe", ds.Tables["users"].Rows[0]["name"].ToString(), "Load failed: ivalid order");
			Assert.AreEqual("Mike", ds.Tables["users"].Rows[1]["name"].ToString(), "Load failed: ivalid order");
			
			q.Sort = new QSort[] { "role", "name DESC" };
			ds.Clear();
			Dalc.Load( q, ds );

			Assert.AreEqual(2, ds.Tables["users"].Rows.Count, "Load failed: rows count");

			Assert.AreEqual("Mike", ds.Tables["users"].Rows[0]["name"].ToString(), "Load failed: ivalid order");
			Assert.AreEqual("Joe", ds.Tables["users"].Rows[1]["name"].ToString(), "Load failed: ivalid order");
			
			q.Condition = (QField)"role" == subQuery & (QField)"id">(QConst)5;
			ds.Clear();
			Dalc.Load( q, ds );
			Assert.AreEqual(0, ds.Tables["users"].Rows.Count, "Load failed");
		}

		[Test]
		public void test_CRUD_Speed() {
			var stopwatch = new Stopwatch();

			int iterations = 0;
			stopwatch.Start();
			while (iterations < 100) {
				iterations++;

				var ds = new DataSet();
				Dalc.Load( new Query("users", new QField("1")==new QConst(2) ), ds);
				var usersTbl = ds.Tables["users"];

				usersTbl.PrimaryKey = new[] { usersTbl.Columns["id"] };
				usersTbl.Columns["id"].AutoIncrement = true;

				var r = usersTbl.NewRow();
				r["name"] = "TEST";
				usersTbl.Rows.Add(r);

				Dalc.Update(usersTbl);

				r["name"] = "TEST1";
				Dalc.Update(usersTbl);

				r.Delete();
				Dalc.Update(usersTbl);
			}

			stopwatch.Stop();
			Console.WriteLine("Speedtest for SQLite CRUD datarow operations ({1} times): {0}", stopwatch.Elapsed, iterations); 
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
