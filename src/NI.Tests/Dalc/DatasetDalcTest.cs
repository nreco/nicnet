using System;
using System.Data;
using System.Collections;
using NI.Data;

using NUnit.Framework;

namespace NI.Tests.Data.Dalc
{

	//TODO: revise test, cover more cases
	[TestFixture]
	[Category("NI.Data")]
	public class DatasetDalcTest
	{

		public static DatasetDalc createDsDalc() {
			DatasetDalc dsDalc = new DatasetDalc();
			
			DataSet ds = new DataSet();

			ds.Tables.Add("users");
			DataColumn idColumn = ds.Tables["users"].Columns.Add("id", typeof(int));
			idColumn.AutoIncrement = false;
			ds.Tables["users"].Columns.Add("name", typeof(string));
			ds.Tables["users"].Columns.Add("role", typeof(string));
			ds.Tables["users"].PrimaryKey = new DataColumn[] { idColumn };
			
			ds.Tables["users"].Rows.Add( new object[] {1, "Vitalik", "1" } );
			ds.Tables["users"].Rows.Add( new object[] {2, "Darina", "1" } );
			ds.Tables["users"].Rows.Add( new object[] {3, "Stas", "2" } );
			
			ds.Tables.Add("roles");
			idColumn = ds.Tables["roles"].Columns.Add("id", typeof(int));
			ds.Tables["roles"].Columns.Add("role", typeof(string));
			ds.Tables["roles"].PrimaryKey = new DataColumn[] { idColumn };
			
			ds.Tables["roles"].Rows.Add( new object[] {1, "admin" } );
			ds.Tables["roles"].Rows.Add( new object[] {2, "user" } );
			
			dsDalc.PersistedDS = ds;

			ds.AcceptChanges();
			
			return dsDalc;
		}

		[Test]
		public void test_LoadRecord() {
			DatasetDalc dsDalc = createDsDalc();
			Query q = new Query("users");
			q.Condition = (QField)"name" == (QConst)"Vitalik";
			var res = dsDalc.LoadRecord(q);
			Assert.NotNull(res,"LoadRecord failed");
			Assert.AreEqual(1, (int)res["id"], "LoadRecord failed");
		}
		
		[Test]
		public void test_Load() {
			DatasetDalc dsDalc = createDsDalc();
			DataSet ds = new DataSet();
			
			Query subQuery = new Query("roles");
			subQuery.Condition = (QField)"role"==(QConst)"admin";
			subQuery.Fields = new QField[] { "id" };
			
			Query q = new Query("users");
			q.Condition = (QField)"role" == subQuery;
			q.Sort = new QSortField[] { "name" };

			dsDalc.Load( q, ds );
			if (ds.Tables["users"].Rows.Count!=2)
				throw new Exception("Load failed");
			if (ds.Tables["users"].Rows[0]["name"].ToString()!="Darina" ||
				ds.Tables["users"].Rows[1]["name"].ToString()!="Vitalik")
				throw new Exception("Load failed");
			
			q.Sort = new QSortField[] { "role", "name DESC" };
			dsDalc.Load( q, ds );
			if (ds.Tables["users"].Rows.Count!=2)
				throw new Exception("Load failed");
			if (ds.Tables["users"].Rows[0]["name"].ToString()!="Vitalik" ||
				ds.Tables["users"].Rows[1]["name"].ToString()!="Darina")
				throw new Exception("Load failed");
			
			q.Condition = (QField)"role" == subQuery & (QField)"id">(QConst)5;
			dsDalc.Load( q, ds );
			if (ds.Tables["users"].Rows.Count!=0)
				throw new Exception("Load failed");
		}
		
		[Test]
		public void test_Delete() {
			DatasetDalc dsDalc = createDsDalc();

			Query subQuery = new Query("roles");
			subQuery.Condition = (QField)"role"==(QConst)"user";
			subQuery.Fields = new QField[] { "id" };
			Query q = new Query("users");
			q.Condition = (QField)"role" == subQuery;
			
			dsDalc.Delete( q );
			
			Hashtable res  = new Hashtable();
			Assert.Null( dsDalc.LoadRecord(q), "Delete failed");
			Assert.AreEqual(2, dsDalc.PersistedDS.Tables["users"].Rows.Count, "Delete failed");
		}
		
		[Test]
		public void test_Update() {
			DatasetDalc dsDalc = createDsDalc();
			DataSet ds = new DataSet();
			Query q = new Query("users");
			q.Condition = (QField)"id" == (QConst)1;
			
			dsDalc.Load( q, ds );
			ds.Tables["users"].Rows[0]["name"] = "Vit";
			var newRow = ds.Tables["users"].Rows.Add(new object[] { 4, "Petya", "2" });
			dsDalc.Update( ds.Tables["users"] );
			
			Assert.AreEqual(4, dsDalc.RecordsCount(new Query("users")), "Update failed"); 

			var res = dsDalc.LoadRecord(q);
			Assert.AreEqual("Vit", res["name"].ToString(), "Update failed");

			ds.Tables["users"].Rows[1].Delete();
			dsDalc.Update( ds.Tables["users"] );
			if (dsDalc.PersistedDS.Tables["users"].Rows.Count!=3)
				throw new Exception("Update failed");			
			
			res = new Hashtable();
			res["name"] = "VVV";
			var affected = dsDalc.Update( q, res );
			Assert.AreEqual(1, affected, "Update by query failed");

			var res2 = dsDalc.LoadRecord(q );
			Assert.AreEqual("VVV", res2["name"], "Update failed");
			
			
		}
		
		
			
		
	}
}
