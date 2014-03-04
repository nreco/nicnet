using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using NUnit.Framework;

using NI.Text;

namespace NI.Tests.Expressions
{
	/// <summary>
	/// </summary>
	[TestFixture]
	[Category("NI.Expressions")]
	public class EvalTest
	{
		public EvalTest()
		{
		}
		
		[Test]
		public void test_DataBind() {
			var dataBinder = new DataBind();

			var someArr = new[] { "a", "b", "c" };
			Assert.AreEqual(3, (int)dataBinder.Eval(someArr, ".Length"));
			Assert.AreEqual("b", (string)dataBinder.Eval(someArr, "[1]"));

			var dict = new Hashtable() {
				{"name", "AAA"},
				{"k", new KeyValuePair<string,TimeSpan>("a", new TimeSpan(0,1,2) ) }
			};
			Assert.AreEqual("AAA", (string)dataBinder.Eval(dict, "[name]"));
			Assert.IsNull( dataBinder.Eval(dict, "[name1]"));
			Assert.AreEqual("a", (string)dataBinder.Eval(dict, "[k].Key"));
			Assert.AreEqual(2, (int) dataBinder.Eval(dict, "[k].Value.Seconds"));
		}

		[Test]
		public void test_StringTemplate() {
			var st = new StringTemplate();

			var vars = new Hashtable() {
				{"name", "John"},
				{"company", "Johnson&Johnson"},
				{"age", 65},
				{"birthday", new DateTime(1985, 1, 5)}
			};
			var noMarkerStr = "Just a text without markers.";
			Assert.AreEqual(noMarkerStr, st.Eval(vars,noMarkerStr) );

			var simpleMarkersStr = "Hello, {var:name}, you are {var:age} years old! You born in {databind:[birthday].Month}/{var:birthday,dd}.";
			var simpleMarkersStrRes = "Hello, John, you are 65 years old! You born in 1/05.";

			Assert.AreEqual(simpleMarkersStrRes, st.Eval(vars, simpleMarkersStr));

			Assert.AreEqual("Johnson&amp;Johnson", st.Eval(vars, "{xml:{var:company}}"));

		}
		
		
	}
}
