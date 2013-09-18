using System;
using System.ComponentModel;

using NI.Data;

using NUnit.Framework;

namespace NI.Tests.Data.Dalc
{
	/// <summary>
	/// </summary>
	[TestFixture]
	[NUnit.Framework.Category("NI.Data")]
	public class QueryTest
	{
		[Test]
		public void test_QSortField() {
			QSort fld = (QSort)"name";
			Assert.AreEqual( fld.Field.Name, "name", "QSortField parse error");
			Assert.AreEqual( fld.SortDirection, ListSortDirection.Ascending, "QSortField parse error");
			
			fld = (QSort)"email desc ";
			Assert.AreEqual( fld.Field.Name, "email", "QSortField parse error");
			Assert.AreEqual( fld.SortDirection, ListSortDirection.Descending, "QSortField parse error");
			
			fld = (QSort)"email  desc ";
			Assert.AreEqual( fld.Field.Name, "email", "QSortField parse error");
			Assert.AreEqual( fld.SortDirection, ListSortDirection.Descending, "QSortField parse error");

			fld = (QSort)"  email  desc ";
			Assert.AreEqual( fld.Field.Name, "email", "QSortField parse error");
			Assert.AreEqual( fld.SortDirection, ListSortDirection.Descending, "QSortField parse error");			
			
			fld = (QSort)"position asc";
			Assert.AreEqual( fld.Field.Name, "position", "QSortField parse error");
			Assert.AreEqual( fld.SortDirection, ListSortDirection.Ascending, "QSortField parse error");
		}
	}
}
