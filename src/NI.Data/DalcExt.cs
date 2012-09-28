﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace NI.Data {
	
	public static class DalcExt {
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
					val = reader[0];
				}
			});
			return val;
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
