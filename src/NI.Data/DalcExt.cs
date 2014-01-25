using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace NI.Data {
	
	/// <summary>
	/// Extension methods for <see cref="NI.Data.IDalc"/> interface
	/// </summary>
	public static class DalcExt {
		
		/// <summary>
		/// Load first record by query
		/// </summary>
		/// <param name="dalc">IDalc instance</param>
		/// <param name="q">query</param>
		/// <returns>IDictionary with record data or null if no records matched</returns>
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

		/// <summary>
		/// Load first value of first record returned by query
		/// </summary>
		/// <param name="dalc">IDalc instance</param>
		/// <param name="q">query</param>
		/// <returns>object or null if no records matched</returns>
		public static object LoadValue(this IDalc dalc, Query q) {
			object val = null;
			dalc.ExecuteReader(q, (reader) => {
				if (reader.Read()) {
					if (q.Fields != null && q.Fields.Length == 1) {
						var fldMatched = false;
						for (int i=0; i<reader.FieldCount; i++)
							if (reader.GetName(i) == q.Fields[0].Name) {
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

		/// <summary>
		/// Load all records data returned by query
		/// </summary>
		/// <param name="dalc">IDalc instance</param>
		/// <param name="q">query</param>
		/// <returns>IDictionary[] with records data</returns>
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

		/// <summary>
		/// Load list of first values of records returned by query
		/// </summary>
		/// <param name="dalc">IDalc instance</param>
		/// <param name="q">query</param>
		/// <returns>object[] with first values</returns>
		public static object[] LoadAllValues(this IDalc dalc, Query q) {
			var rs = new List<object>();
			if (q.Fields.Length != 1)
				throw new ArgumentException("LoadAllValues expects exactly one field to load");
			dalc.ExecuteReader(q, (reader) => {
				while (reader.Read()) {
					rs.Add(reader[q.Fields[0].Name]);
				}
			});
			return rs.ToArray();
		}

		internal static IDictionary<string, IQueryValue> GetDalcChangeset(IDictionary data) {
			var updateFields = new Dictionary<string, IQueryValue>();
			foreach (DictionaryEntry entry in data)
				updateFields[Convert.ToString(entry.Key)] = entry.Value is IQueryValue ?
					(IQueryValue)entry.Value : new QConst(entry.Value);
			return updateFields;
		}

		/// <summary>
		/// Update records matched by query
		/// </summary>
		/// <param name="dalc">IDalc instance</param>
		/// <param name="q">query</param>
		/// <param name="data">changeset dictionary (field name -> set value)</param>
		/// <returns>number of updated records</returns>
		public static int Update(this IDalc dalc, Query q, IDictionary data) {
			return dalc.Update(q, GetDalcChangeset(data) );
		}

		/// <summary>
		/// Insert new record
		/// </summary>
		/// <param name="dalc">IDalc instance</param>
		/// <param name="tableName">table name</param>
		/// <param name="data">record data (field name -> set value)</param>
		public static void Insert(this IDalc dalc, string tableName, IDictionary data) {
			dalc.Insert(tableName, GetDalcChangeset(data));
		}

		/// <summary>
		/// Get records count matched by query
		/// </summary>
		/// <param name="dalc">IDalc instance</param>
		/// <param name="q"></param>
		/// <returns>number of matched records</returns>
		public static int RecordsCount(this IDalc dalc, Query q) {
			var qCount = new Query(q);
			qCount.Sort = null;
			qCount.Fields = new QField[] { new QField("cnt", "count(*)") };
            qCount.StartRecord = 0;
            qCount.RecordCount = 1;
			return Convert.ToInt32( LoadValue(dalc, qCount ) );
		}

	}
}
