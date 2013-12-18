using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NI.Data;
using NI.Data.SqlClient;


using NUnit.Framework;

namespace NI.Tests.Data.Dalc {

	[TestFixture]
	[Category("NI.Data")]
	public class DbCommandGeneratorTest {

		QueryNode createTestQuery() {
			return  (
						(QField)"name" % (QConst)"Anna" | new QueryNegationNode( (QField)"age" >= (QConst)18 )
					) & (
						(QField)"weight" == (QConst)54.3
						&
						new QueryConditionNode(
							(QField)"type",
							Conditions.In, 
							(QConst)new string[] {"Str1", "Str2"}
						)
					) | (
						(QField)"name"!=(QConst)"Petya"
						&
						new QueryConditionNode(
							(QField)"type", Conditions.Not|Conditions.Null,	null)
					);		
		}

		[Test]
		public void test_Select_Speed() {
			var cmdGenerator = new DbCommandGenerator(new NI.Data.SQLite.SQLiteDalcFactory() );
			
			Query q = new Query( "test" );
			q.Condition = createTestQuery();
			q.Fields = new QField[] { "name", "age" };

			// SELECT TEST
			var stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();
			for (int i=0; i<10000; i++) {
				IDbCommand cmd = cmdGenerator.ComposeSelect( q );	
			}
			stopwatch.Stop();

			Console.WriteLine("Speedtest for select command generation (10000 times): {0}", stopwatch.Elapsed); 
		}


		[Test]
		public void test_InsertUpdateDelete_Speed() {
			var cmdGenerator = new DbCommandGenerator(new NI.Data.SQLite.SQLiteDalcFactory());
			var ds = new DataSet();
			var t = ds.Tables.Add("users");
			t.Columns.Add("id", typeof(int)).AutoIncrement = true;
			t.Columns.Add("name", typeof(string));
			t.Columns.Add("userpic", typeof(string));
			t.Columns.Add("create_date", typeof(DateTime));
			t.Columns.Add("update_date", typeof(DateTime));
			t.Columns.Add("age", typeof(int));
			t.PrimaryKey = new[] { t.Columns["id"] };

			var stopwatch = new Stopwatch();

			int iterations = 0;
			stopwatch.Start();
			while (iterations < 500) {
				iterations++;

				cmdGenerator.ComposeAdapterUpdateCommands( new System.Data.SQLite.SQLiteDataAdapter(), t);
			}

			stopwatch.Stop();
			Console.WriteLine("Speedtest for DbCommandGenerator Update commands ({1} times): {0}", stopwatch.Elapsed, iterations);
		}
		
		
		[Test]
		public void test_CommandGenerator() {
			SqlClientDalcFactory factory = new SqlClientDalcFactory();
			DbCommandGenerator cmdGenerator = new DbCommandGenerator(factory);
			
			Query q = new Query( new QSource("test","t") );
			q.Condition = createTestQuery();
			q.Fields = new QField[] { "name", "t.age", new QField("age_months", "t.age*12") };

			// SELECT TEST with prefixes and expressions
			IDbCommand cmd = cmdGenerator.ComposeSelect( q );	
			string masterSQL = "SELECT name,t.age as age,t.age*12 as age_months FROM test t WHERE (((name LIKE @p0) Or (NOT(age>=@p1))) And ((weight=@p2) And (type IN (@p3,@p4)))) Or ((name<>@p5) And (type IS NOT NULL))";
			
			Assert.AreEqual( cmd.CommandText, masterSQL, "Select command generation failed");

			// SELECT WITH TABLE ALIAS TEST
			cmd = cmdGenerator.ComposeSelect(
					new Query("accounts.a",
						new QueryConditionNode( (QField)"a.id", Conditions.In, 
							new Query("dbo.accounts.b", (QField)"a.id"!=(QField)"b.id" ) ) ) );
			masterSQL = "SELECT * FROM accounts a WHERE a.id IN ((SELECT * FROM dbo.accounts b WHERE a.id<>b.id))";
			Assert.AreEqual( masterSQL, cmd.CommandText);

			DataSet ds = new DataSet();
			ds.Tables.Add("test");
			ds.Tables["test"].Columns.Add("name", typeof(string) ).DefaultValue = "name";
			ds.Tables["test"].Columns.Add("age", typeof(int) );
			ds.Tables["test"].Columns.Add("weight", typeof(double) );
			ds.Tables["test"].Columns.Add("type", typeof(string) );
			ds.Tables["test"].PrimaryKey = new DataColumn[] { ds.Tables["test"].Columns["name"] };
			

			// INSERT TEST
			cmd = cmdGenerator.ComposeInsert( ds.Tables["test"] );
			masterSQL = "INSERT INTO test (name,age,weight,type) VALUES (@p0,@p1,@p2,@p3)";
			
			Assert.AreEqual( cmd.CommandText, masterSQL, "Insert command generation failed");
			Assert.AreEqual( cmd.Parameters.Count, 4, "Insert command generation failed");
			
			// UPDATE TEST
			cmd = cmdGenerator.ComposeUpdate( ds.Tables["test"] );
			masterSQL = "UPDATE test SET name=@p0,age=@p1,weight=@p2,type=@p3 WHERE name=@p4";
			
			Assert.AreEqual( cmd.CommandText, masterSQL, "Update command generation failed");
			Assert.AreEqual( cmd.Parameters.Count, 5, "Update command generation failed");
			
			// UPDATE TEST (by query)
			var changes = new Dictionary<string,IQueryValue>() {
				{ "age", (QConst)21 }, { "name", (QConst)"Alexandra" } };
			cmd = cmdGenerator.ComposeUpdate(new Query("test", (QField)"id" == (QConst)1), changes);
			masterSQL = "UPDATE test SET age=@p0,name=@p1 WHERE id=@p2";

			Assert.AreEqual(masterSQL, cmd.CommandText, "Update command generation failed");
			Assert.AreEqual(3, cmd.Parameters.Count, "Update command generation failed");
			
			// DELETE TEST
			cmd = cmdGenerator.ComposeDelete( ds.Tables["test"] );
			masterSQL = "DELETE FROM test WHERE name=@p0";
			
			Assert.AreEqual( cmd.CommandText, masterSQL, "Delete command generation failed");
			Assert.AreEqual( cmd.Parameters.Count, 1, "Delete command generation failed");
			
			// DELETE BY QUERY TEST
			cmd = cmdGenerator.ComposeDelete( new Query("test", (QField)"id"==(QConst)5 ) );
			masterSQL = "DELETE FROM test WHERE id=@p0";
			
			Assert.AreEqual( cmd.CommandText, masterSQL, "Delete command (by query) generation failed" );
			Assert.AreEqual( cmd.Parameters.Count, 1, "Delete command (by query) generation failed");
		}
		

		
		
	}
}
