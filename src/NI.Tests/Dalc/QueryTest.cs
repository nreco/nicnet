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
			QSortField fld = (QSortField)"name";
			Assert.AreEqual( fld.Name, "name", "QSortField parse error");
			Assert.AreEqual( fld.SortDirection, ListSortDirection.Ascending, "QSortField parse error");
			
			fld = (QSortField)"email desc ";
			Assert.AreEqual( fld.Name, "email", "QSortField parse error");
			Assert.AreEqual( fld.SortDirection, ListSortDirection.Descending, "QSortField parse error");
			
			fld = (QSortField)"email  desc ";
			Assert.AreEqual( fld.Name, "email", "QSortField parse error");
			Assert.AreEqual( fld.SortDirection, ListSortDirection.Descending, "QSortField parse error");

			fld = (QSortField)"  email  desc ";
			Assert.AreEqual( fld.Name, "email", "QSortField parse error");
			Assert.AreEqual( fld.SortDirection, ListSortDirection.Descending, "QSortField parse error");			
			
			fld = (QSortField)"position asc";
			Assert.AreEqual( fld.Name, "position", "QSortField parse error");
			Assert.AreEqual( fld.SortDirection, ListSortDirection.Ascending, "QSortField parse error");
		}
	}
}
