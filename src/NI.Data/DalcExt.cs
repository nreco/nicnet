using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace NI.Data {
	
	public static class DalcExt {
		
		/// <summary>
		/// Load first record by query
		/// </summary>
		public static IDictionary LoadRecord(this IDalc dalc, Query q) {
			IDictionary data = null;
            var oneRecordQuery = new Query(q);
            q.RecordCount = 1;
            dalc.ExecuteReader(oneRecordQuery, (reader) => {
				if (reader.Read()) {
					data = new Hashtable();
					// fetch all fields & values in hashtable
					for (int i = 0; i < reader.FieldCount; i++)
						data[reader.GetName(i)] = reader.GetValue(i);
				}
			});
			return data;
		}

		public static object LoadValue(this IDalc dalc, Query q) {
			object val = null;
			dalc.ExecuteReader(q, (reader) => {
				if (reader.Read()) {
					if (q.Fields != null && q.Fields.Length == 1) {
						var fldMatched = false;
						for (int i=0; i<reader.FieldCount; i++)
							if (reader.GetName(i) == q.Fields[0]) {
								val = reader.GetValue(i);
								fldMatched = true;
							}
						if (!fldMatched)
							val = reader[0];
					} else {
						val = reader[0];
					}
				}
			});
			return val;
		}

		public static IDictionary[] LoadAllRecords(this IDalc dalc, Query q) {
			var rs = new List<IDictionary>();
			dalc.ExecuteReader(q, (reader) => {
				while (reader.Read()) {
					var data = new Hashtable();
					// fetch all fields & values in hashtable
					for (int i = 0; i < reader.FieldCount; i++)
						data[reader.GetName(i)] = reader.GetValue(i);
					rs.Add(data);
				}
			});
			return rs.ToArray();
		}

		public static object[] LoadAllValues(this IDalc dalc, Query q) {
			var rs = new List<object>();
			if (q.Fields.Length != 1)
				throw new ArgumentException("LoadAllValues expects exactly one field to load");
			dalc.ExecuteReader(q, (reader) => {
				while (reader.Read()) {
					rs.Add(reader[q.Fields[0]]);
				}
			});
			return rs.ToArray();
		}

		public static int RecordsCount(this IDalc dalc, Query q) {
			var qCount = new Query(q);
			qCount.Sort = null;
			qCount.Fields = new[] { "count(*)" };
            qCount.StartRecord = 0;
            qCount.RecordCount = 1;
			return Convert.ToInt32( LoadValue(dalc, qCount ) );
		}

	}
}
