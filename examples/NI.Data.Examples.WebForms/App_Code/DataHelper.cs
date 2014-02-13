using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;

namespace NI.Data.Examples.WebForms.App_Code {
	
	public static class DataHelper {

		public static IDalc GetDalc() {
			DataSet ds = HttpContext.Current.Session["dataset"] as DataSet;
			if (ds == null) {
				ds = CreateBookDS();
				var r = ds.Tables["books"].NewRow();
				r["title"] = "Twenty Thousand Leagues Under the Sea";
				r["description"] = "Twenty Thousand Leagues Under the Sea is a classic science fiction novel by French writer Jules Verne published in 1870. It tells the story of Captain Nemo and his submarine Nautilus, as seen from the perspective of Professor Pierre Aronnax.";
				r["rating"] = 5;
				ds.Tables["books"].Rows.Add(r);
			}
			return new DataSetDalc(ds);
		}

		public static DataSet CreateBookDS() {
			var ds = new DataSet();
			var bookTbl = ds.Tables.Add("books");
			var idCol = bookTbl.Columns.Add("id", typeof(int));
			idCol.AutoIncrement = true;
			idCol.AutoIncrementSeed = 1;
			bookTbl.PrimaryKey = new[] { idCol };

			bookTbl.Columns.Add("title", typeof(string));
			bookTbl.Columns.Add("description", typeof(string));
			bookTbl.Columns.Add("rating", typeof(int));

			return ds;
		}

	}
}