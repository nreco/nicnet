#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
 * Copyright 2008-2013 Vitalii Fedorchenko (changes and v.2)
 * Distributed under the LGPL licence
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

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
				for (int i = 0; i < q.StartRecord; i++)
					reader.Read(); // skip first N records
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
			if (q.Fields==null || q.Fields.Length != 1)
				throw new ArgumentException("LoadValue expects exactly one field to load");
			dalc.ExecuteReader(q, (reader) => {
				for (int i=0; i<q.StartRecord; i++)
					reader.Read(); // skip first N records
				if (reader.Read()) {
					if (reader.FieldCount==1) {
						val = reader[0];
					} else {
						val = reader[q.Fields[0].Name];
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
				int index = 0;
				while (reader.Read() && rs.Count < q.RecordCount) {
					if (index>=q.StartRecord) {
						var data = new Hashtable();
						// fetch all fields & values in hashtable
						for (int i = 0; i < reader.FieldCount; i++)
							data[reader.GetName(i)] = reader.GetValue(i);
						rs.Add(data);
					}
					index++;
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
			if (q.Fields == null || q.Fields.Length != 1)
				throw new ArgumentException("LoadAllValues expects exactly one field to load");
			dalc.ExecuteReader(q, (reader) => {
				int index = 0;
				while (reader.Read() && rs.Count < q.RecordCount ) {
					if (index>=q.StartRecord) {
						rs.Add(reader.FieldCount>1 ? reader[q.Fields[0].Name] : reader[0] );
					}
					index++;
				}
			});
			return rs.ToArray();
		}

		internal static IDictionary<string, IQueryValue> GetDalcChangeset(IDictionary data) {
			if (data == null)
				throw new ArgumentNullException("Cannot prepare DALC changeset from null");
			var updateFields = new Dictionary<string, IQueryValue>(data.Count);
			foreach (DictionaryEntry entry in data)
				updateFields[Convert.ToString(entry.Key)] = entry.Value is IQueryValue ?
					(IQueryValue)entry.Value : new QConst(entry.Value);
			return updateFields;
		}

		internal static IDictionary<string, IQueryValue> GetDalcChangeset(object o) {
			if (o == null)
				throw new ArgumentNullException("Cannot prepare DALC changeset from null");
			var oType = o.GetType();
			var changesetFields = new Dictionary<string, IQueryValue>();
			foreach (var p in oType.GetProperties()) {
				var pVal = p.GetValue(o, null);
				changesetFields[p.Name] = pVal is IQueryValue ? (IQueryValue)pVal : new QConst(pVal);
			}
			return changesetFields;
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
		/// Update records matched by query
		/// </summary>
		/// <param name="dalc">IDalc instance</param>
		/// <param name="q">query</param>
		/// <param name="dto">DTO object with changeset data (properties are used as fields)</param>
		/// <returns>number of updated records</returns>
		public static int Update(this IDalc dalc, Query q, object dto) {
			return dalc.Update(q, GetDalcChangeset(dto));
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
		/// Insert new record
		/// </summary>
		/// <param name="dalc">IDalc instance</param>
		/// <param name="tableName">table name</param>
		/// <param name="data">DTO object with changeset data (properties are used as fields)</param>
		public static void Insert(this IDalc dalc, string tableName, object dto) {
			dalc.Insert(tableName, GetDalcChangeset(dto));
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
