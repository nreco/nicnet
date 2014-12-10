using System;
using System.ComponentModel;
using System.Collections.Generic;

using NI.Data;

using NUnit.Framework;

namespace NI.Tests.Data
{
	/// <summary>
	/// </summary>
	[TestFixture]
	[NUnit.Framework.Category("NI.Data")]
	public class SimpleStringTemplateTest
	{
		[Test]
		public void FormatTemplate() {
			var testStr = "TEST @SqlOrderBy[order by {0};order by u.id desc] TEST";
			var strTpl = new SimpleStringTemplate(testStr);
			strTpl.ReplaceMissedTokens = false;
			Assert.AreEqual(testStr, strTpl.FormatTemplate(new Dictionary<string, object>()));
			strTpl.ReplaceMissedTokens = true;
			Assert.AreEqual("TEST order by u.id desc TEST", strTpl.FormatTemplate(new Dictionary<string, object>()));

			Assert.AreEqual("TEST order by name TEST", strTpl.FormatTemplate(
				new Dictionary<string, object>() {
					{"SqlOrderBy", "name"}
				} ));


			Assert.AreEqual("1+2",
				new SimpleStringTemplate("@A[{0}+@B]",2).FormatTemplate(new Dictionary<string, object>() {
					{"A", 1}, {"B", 2}
				})
			);

			Assert.AreEqual("No replace: @Test",
				new SimpleStringTemplate("No replace: @@Test").FormatTemplate(new Dictionary<string, object>() {
					{"Test", "bla"}
				})
			);

			Assert.AreEqual(
				"and 1=2",
				new SimpleStringTemplate(
					"@class_id[and id in metadata_property_to_class(class_id=\"class_id\":var)[property_id]];and 1=2]").FormatTemplate(
					new Dictionary<string, object>() {
						{"class_id", ""}
					}
				)
			);

			Assert.AreEqual(
				"zzz@WAW;[]",
				new SimpleStringTemplate(
					"zzz@A[@WAW;;[]]]").FormatTemplate(
					new Dictionary<string, object>() {
						{"A", "1"}
					}
				)
			);
			Assert.AreEqual(
				"zzz [] ",
				new SimpleStringTemplate(
					"zzz@A[\\;; [\\] ]").FormatTemplate(
					new Dictionary<string, object>() {
						{"A", ""}
					}
				)
			);

		}
	}
}
