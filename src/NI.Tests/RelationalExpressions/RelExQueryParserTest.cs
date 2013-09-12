using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Data;

using NI.Data.RelationalExpressions;
using NI.Data;
using NI.Data.SqlClient;


using NUnit.Framework;

namespace NI.Tests.Data.RelationalExpressions
{
	/// <summary>
	/// </summary>
	[TestFixture]
	public class RelExQueryParserTest
	{
		string[] oldRelExSamples = new string[] {
			"tokens(id in custompage_to_token(left_uid=5 or left_uid=6)[right_uid] and type=6)[value]",
			"expenses(expense_report_id = 5)[id, unit_uid, money_equivalent]",
			"users(id>1 or id<=1)[*]",
			"users( id=null and id!=null )[*]" // test for compatibility
		};

		string[] oldRelExCommandTexts = new string[] {
			@"SELECT value FROM tokens WHERE (id IN ((SELECT right_uid FROM custompage_to_token WHERE (left_uid=@p0) Or (left_uid=@p1)))) And (type=@p2)",
			@"SELECT id,unit_uid,money_equivalent FROM expenses WHERE expense_report_id=@p0",
			@"SELECT * FROM users WHERE (id>@p0) Or (id<=@p1)",
			@"SELECT * FROM users WHERE (id IS  NULL) And (id IS NOT NULL)"
		};


		string[] relExSamples = new string[] {
			"accounts(login = \"vitalik\" or id<=parent_id)[*]",
			"accounts(login = \"vit\"\"alik\" or id<=5)[*]",
			"accounts(1=2)[max(id),min(id)]",
			"users(id=\"\")[*]",
			"users_view[count]",
			"users( (id>\"1\" and id<\"5\") or (name like \"AAA\" and age<\"25\") )[*]",
			"users( (<idGroup> id>\"1\" and id<\"5\") or age<\"25\":int32 )[*]",
			"users( id in \"1,2,3\":int32[] and id !in \"4,5\":int32[] and id!=0 )[*]",
			"users( id=null and id!=null )[count(*)]",
			
			"users( (id!=1 and id!=2) and id!=3)[name]",
			"users( (<grname> id!=1 and id!=2) and id!=3)[name]",
			"users( (id!=1 and id!=2) and (id!=3))[name]",
			"users( (id!=1 and id!=2) and (id!=3 and id!=4))[name]",
			"users( (id!=1 and id!=2) and (id!=3 and (id!=4)))[name]",
			"users( (id!=1 and id!=2) and (( (id!=3 and (id!=4))) ))[name]",
			"users( (id!=1 and id!=2) or (id!=3))[name]",
			"users( 1=1 )[name;name,login]",
			"users( 1=1 )[name;\"name desc\"]",
			"users( 1=1 )[name;name desc,id asc,time]"
		};

		string[] relExCommandTexts = new string[] {
			@"SELECT * FROM accounts WHERE (login=@p0) Or (id<=parent_id)",
			@"SELECT * FROM accounts WHERE (login=@p0) Or (id<=@p1)",
			@"SELECT max(id),min(id) FROM accounts WHERE @p0=@p1",
			@"SELECT * FROM users WHERE id=@p0",
			@"SELECT count FROM users_view",
			@"SELECT * FROM users WHERE ((id>@p0) And (id<@p1)) Or ((name LIKE @p2) And (age<@p3))",
			@"SELECT * FROM users WHERE ((id>@p0) And (id<@p1)) Or (age<@p2)",
			@"SELECT * FROM users WHERE (id IN (@p0,@p1,@p2)) And (NOT (id IN (@p3,@p4))) And (id<>@p5)",
			@"SELECT count(*) FROM users WHERE (id IS  NULL) And (id IS NOT NULL)",
			
			@"SELECT name FROM users WHERE (id<>@p0) And (id<>@p1) And (id<>@p2)",
			@"SELECT name FROM users WHERE ((id<>@p0) And (id<>@p1)) And (id<>@p2)",
			@"SELECT name FROM users WHERE (id<>@p0) And (id<>@p1) And (id<>@p2)",
			@"SELECT name FROM users WHERE (id<>@p0) And (id<>@p1) And (id<>@p2) And (id<>@p3)",
			@"SELECT name FROM users WHERE (id<>@p0) And (id<>@p1) And (id<>@p2) And (id<>@p3)",
			@"SELECT name FROM users WHERE (id<>@p0) And (id<>@p1) And (id<>@p2) And (id<>@p3)",
			@"SELECT name FROM users WHERE ((id<>@p0) And (id<>@p1)) Or (id<>@p2)",
			@"SELECT name FROM users WHERE @p0=@p1 ORDER BY name asc,login asc",
			@"SELECT name FROM users WHERE @p0=@p1 ORDER BY name desc",
			@"SELECT name FROM users WHERE @p0=@p1 ORDER BY name desc,id asc,time asc"
	};
		
		[Test]
		public void test_Parse() {
			RelExQueryParser relExParser = new RelExQueryParser();
			// generate SQL by query
			SqlClientDalcFactory factory = new SqlClientDalcFactory();
			DbCommandGenerator cmdGenerator = new DbCommandGenerator(factory);

			for (int i=0; i<oldRelExSamples.Length; i++) {
				string relEx = oldRelExSamples[i];
				Query q = relExParser.Parse(relEx);
				IDbCommand cmd = cmdGenerator.ComposeSelect( q );
				
				Assert.AreEqual(cmd.CommandText, oldRelExCommandTexts[i], "Parse failed (AllowDumpConstations=true): "+i.ToString() );
			}
			
			for (int i=0; i<relExSamples.Length; i++) {
				string relEx = relExSamples[i];
				Query q = relExParser.Parse(relEx);
				IDbCommand cmd = cmdGenerator.ComposeSelect( q );
				
				Assert.AreEqual(cmd.CommandText, relExCommandTexts[i], "Parse failed (AllowDumpConstations=false): "+i.ToString() );
			}

			// test for named nodes
			string relexWithNamedNodes = @"users( (<idGroup> id=null and id!=null) and (<ageGroup> age>5 or age<2) and (<emptyGroup>) )[count(*)]";
			Query qWithGroups = relExParser.Parse(relexWithNamedNodes);
			Assert.AreNotEqual(null, FindNodeByName(qWithGroups.Condition, "idGroup"), "named group not found");
			Assert.AreNotEqual(null, FindNodeByName(qWithGroups.Condition, "ageGroup"), "named group not found");
			Assert.AreNotEqual(null, FindNodeByName(qWithGroups.Condition, "emptyGroup"), "named group not found");
		
			// just a parse test for real complex relex
			string sss = "sourcename( ( ((\"False\"=\"True\") or (\"False\"=\"True\")) and \"contact-of\" in agent_to_agent_role( left_uid in agent_accounts(agent_accounts.id=\"\")[agent_id] and (right_uid=agent_institutions.id) )[role_uid] ) or ( ((agent_institutions.id in events( events.id in event_assignments( person_id in agent_accounts (agent_accounts.id=\"\")[agent_id] )[event_id] )[client_institution_id]) or (agent_institutions.id in events( events.id in event_assignments( person_id in agent_accounts (agent_accounts.id=\"\")[agent_id] )[event_id] )[supplier_institution_id])) and (\"False\"=\"True\") ) or ( (agent_institutions.id in agent_to_agent_role( (left_uid in agent_to_agent_role( left_uid in agent_accounts(agent_accounts.id=\"\")[agent_id] and role_uid='contact-of' )[right_uid]) and role_uid='supplier-of')[right_uid] ) or (agent_institutions.id in events( events.supplier_institution_id in agent_to_agent_role( (agent_to_agent_role.role_uid='contact-of') and (agent_to_agent_role.left_uid in agent_accounts(agent_accounts.id=\"\")[agent_id]) )[agent_to_agent_role.right_uid] )[events.client_institution_id]) or (agent_institutions.id in events( events.client_institution_id in agent_to_agent_role( (agent_to_agent_role.role_uid='contact-of') and (agent_to_agent_role.left_uid in agent_accounts(agent_accounts.id=\"\")[agent_id]) )[agent_to_agent_role.right_uid] )[events.supplier_institution_id]) ) or (\"False\"=\"True\") or ( (\"False\"=\"True\") and (agent_institutions.id in agent_to_agent_role( role_uid='supplier-of' and right_uid = \"\" )[left_uid]) ) or (\"False\"=\"True\") )[*]";
			relExParser.Parse(sss);
		}

		protected QueryNode FindNodeByName(QueryNode node, string name) {
			if (node.Name!=null && node.Name==name)
				return node;
			if (node.Nodes!=null)
				foreach (QueryNode cNode in node.Nodes) {
					QueryNode r = FindNodeByName(cNode, name);
					if (r!=null) return r;
				}

			return null;
		}

		
		[Test]
		public void test_GetLexem() {
			TestRelExQueryParser relExParser = new TestRelExQueryParser();
			
			string expression = relExSamples[0];
			int startIdx = 0;
			int endIdx = 0;
			RelExQueryParser.LexemType lexemType;
			while ( (lexemType=relExParser.TestGetLexemType(expression,startIdx, out endIdx))!=RelExQueryParser.LexemType.Stop) {
				Console.WriteLine("{0}: {1}", lexemType.ToString(), expression.Substring(startIdx, endIdx-startIdx) );
				startIdx = endIdx;
			}
			
		}
		
		
		public class TestRelExQueryParser : RelExQueryParser {
			
			public RelExQueryParser.LexemType TestGetLexemType(string s, int startIdx, out int endIdx) {
				return GetLexemType(s, startIdx, out endIdx);
			}
		
		}
		
		
		
		
		
		
	}
}
