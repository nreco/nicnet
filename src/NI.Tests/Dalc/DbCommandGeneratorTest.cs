using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Collections;
using System.Collections.Specialized;

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

		/*public void test_CommandGeneratorSpeed() {
			IDbFactory dbFactory = new SqlFactoryProvider();
			CommandGenerator cmdGenerator = new CommandGenerator(dbFactory);
			
			Query q = new Query( "test" );
			q.Root = createTestQuery();
			q.Fields = new string[] { "name", "age" };

			// SELECT TEST
			for (int i=0; i<10000; i++) {
				IDbCommand cmd = cmdGenerator.ComposeSelect( q );	
			}
		}*/
		

		void Fail(string message, string received,string expected) {
			Console.Out.WriteLine("Receive: "+received);
			Console.Out.WriteLine("Expected: "+expected);
			throw new Exception(message);
		}		
		
		
		[Test]
		public void test_CommandGenerator() {
			SqlFactory factory = new SqlFactory();
			DbCommandGenerator cmdGenerator = new DbCommandGenerator(factory);
			
			Query q = new Query( "test" );
			q.Condition = createTestQuery();
			q.Fields = new QField[] { "name", "age" };

			// SELECT TEST
			IDbCommand cmd = cmdGenerator.ComposeSelect( q ).Command;	
			string masterSQL = "SELECT name,age FROM test WHERE (((name LIKE @p0) Or (NOT(age>=@p1))) And ((weight=@p2) And (type IN (@p3,@p4)))) Or ((name<>@p5) And (type IS NOT NULL))";
			
			Assert.AreEqual( cmd.CommandText, masterSQL, "Select command generation failed");

			// SELECT WITH TABLE ALIAS TEST
			cmd = cmdGenerator.ComposeSelect(
					new Query("accounts.a",
						new QueryConditionNode( (QField)"a.id", Conditions.In, 
							new Query("dbo.accounts.b", (QField)"a.id"!=(QField)"b.id" ) ) ) ).Command;
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
			cmd = cmdGenerator.ComposeInsert( ds.Tables["test"] ).Command;
			masterSQL = "INSERT INTO test (name,age,weight,type) VALUES (@p0,@p1,@p2,@p3)";
			
			Assert.AreEqual( cmd.CommandText, masterSQL, "Insert command generation failed");
			Assert.AreEqual( cmd.Parameters.Count, 4, "Insert command generation failed");
			
			// UPDATE TEST
			cmd = cmdGenerator.ComposeUpdate( ds.Tables["test"] ).Command;
			masterSQL = "UPDATE test SET name=@p0,age=@p1,weight=@p2,type=@p3 WHERE name=@p4";
			
			Assert.AreEqual( cmd.CommandText, masterSQL, "Update command generation failed");
			Assert.AreEqual( cmd.Parameters.Count, 5, "Update command generation failed");
			
			// UPDATE TEST (by query)
			IDictionary changes = new Hashtable() {
				{ "age", 21 }, { "name", "Alexandra" } };
			cmd = cmdGenerator.ComposeUpdate( changes, new Query("test", (QField)"id" == (QConst)1) ).Command;
			masterSQL = "UPDATE test SET age=@p0,name=@p1 WHERE id=@p2";

			Assert.AreEqual(masterSQL, cmd.CommandText, "Update command generation failed");
			Assert.AreEqual(3, cmd.Parameters.Count, "Update command generation failed");
			
			// DELETE TEST
			cmd = cmdGenerator.ComposeDelete( ds.Tables["test"] ).Command;
			masterSQL = "DELETE FROM test WHERE name=@p0";
			
			Assert.AreEqual( cmd.CommandText, masterSQL, "Delete command generation failed");
			Assert.AreEqual( cmd.Parameters.Count, 1, "Delete command generation failed");
			
			// DELETE BY QUERY TEST
			cmd = cmdGenerator.ComposeDelete( new Query("test", (QField)"id"==(QConst)5 ) ).Command;
			masterSQL = "DELETE FROM test WHERE id=@p0";
			
			Assert.AreEqual( cmd.CommandText, masterSQL, "Delete command (by query) generation failed" );
			Assert.AreEqual( cmd.Parameters.Count, 1, "Delete command (by query) generation failed");
		}
		

		
		
	}
}
