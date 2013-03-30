using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace NI.Data {
	
	public static class DataHelper {
		public static DataSet GetDataSetFromXml(string xml) {
			var ds = new DataSet();
			ds.ReadXml(new StringReader(xml));
			return ds;
		}
	}
}
