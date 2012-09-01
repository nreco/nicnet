using System;
using System.Data;
using System.Collections;
using NI.Data;

using NUnit.Framework;

namespace NI.Tests.Data.Dalc
{
	/// <summary>
	/// </summary>
	[TestFixture]
	[Category("NI.Data")]
	public class DatasetDalcTest
	{

		public static DatasetDalc createDsDalc() {
			DatasetDalc dsDalc = new DatasetDalc();
			
			DataSet ds = new DataSet();

			ds.Tables.Add("users");
			DataColumn idColumn = ds.Tables["users"].Columns.Add("id", typeof(int));
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


			
			return dsDalc;
		}

		[Test]
		public void test_LoadRecord() {
			DatasetDalc dsDalc = createDsDalc();
			Hashtable res = new Hashtable();
			Query q = new Query("users");
			q.Condition = (QField)"name" == (QConst)"Vitalik";
			if (!dsDalc.LoadRecord(res, q))
				throw new Exception("LoadRecord failed");
			if ((int)res["id"]!=1)
				throw new Exception("LoadRecord failed");
		}
		
		[Test]
		public void test_Load() {
			DatasetDalc dsDalc = createDsDalc();
			DataSet ds = new DataSet();
			
			Query subQuery = new Query("roles");
			subQuery.Condition = (QField)"role"==(QConst)"admin";
			subQuery.Fields = new string[] { "id" };
			
			Query q = new Query("users");
			q.Condition = (QField)"role" == subQuery;
			q.Sort = new string[] { "name" };

			dsDalc.Load( ds, q );
			if (ds.Tables["users"].Rows.Count!=2)
				throw new Exception("Load failed");
			if (ds.Tables["users"].Rows[0]["name"].ToString()!="Darina" ||
				ds.Tables["users"].Rows[1]["name"].ToString()!="Vitalik")
				throw new Exception("Load failed");
			
			q.Sort = new string[] { "role", "name DESC" };
			dsDalc.Load( ds, q );
			if (ds.Tables["users"].Rows.Count!=2)
				throw new Exception("Load failed");
			if (ds.Tables["users"].Rows[0]["name"].ToString()!="Vitalik" ||
				ds.Tables["users"].Rows[1]["name"].ToString()!="Darina")
				throw new Exception("Load failed");
			
			q.Condition = (QField)"role" == subQuery & (QField)"id">(QConst)5;
			dsDalc.Load( ds, q );
			if (ds.Tables["users"].Rows.Count!=0)
				throw new Exception("Load failed");
		}
		
		[Test]
		public void test_Delete() {
			DatasetDalc dsDalc = createDsDalc();

			Query subQuery = new Query("roles");
			subQuery.Condition = (QField)"role"==(QConst)"user";
			subQuery.Fields = new string[] { "id" };
			Query q = new Query("users");
			q.Condition = (QField)"role" == subQuery;
			
			dsDalc.Delete( q );
			
			Hashtable res  = new Hashtable();
			if (dsDalc.LoadRecord( res, q ) || dsDalc.PersistedDS.Tables["users"].Rows.Count!=2)
				throw new Exception("Delete failed");		
		}
		
		[Test]
		public void test_Update() {
			DatasetDalc dsDalc = createDsDalc();
			DataSet ds = new DataSet();
			Query q = new Query("users");
			q.Condition = (QField)"id" == (QConst)1;
			
			dsDalc.Load( ds, q );
			ds.Tables["users"].Rows[0]["name"] = "Vit";
			ds.Tables["users"].Rows.Add( new object[] {4, "Petya", "2" } );
			dsDalc.Update( ds, "users" );
			
			if (dsDalc.PersistedDS.Tables["users"].Rows.Count!=4)
				throw new Exception("Update failed");
			
			Hashtable res = new Hashtable();
			dsDalc.LoadRecord( res, q );
			if (res["name"].ToString()!="Vit")
				throw new Exception("Update failed");
				
			ds.Tables["users"].Rows[0].Delete();
			dsDalc.Update( ds, "users" );
			if (dsDalc.PersistedDS.Tables["users"].Rows.Count!=3)
				throw new Exception("Update failed");			
			
			res = new Hashtable();
			res["name"] = "VVV";
			dsDalc.Update( res, q );

			dsDalc.LoadRecord( res, q );
			if (res["name"].ToString()!="VVV")
				throw new Exception("Update failed");
			
			
		}
		
		
			
		
	}
}
