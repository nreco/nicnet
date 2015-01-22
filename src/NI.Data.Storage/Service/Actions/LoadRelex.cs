#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013-2014 Vitalii Fedorchenko
 * Copyright 2014 NewtonIdeas
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
using System.Runtime.Serialization;
using System.Data;
using System.ServiceModel;
using System.Globalization;

using NI.Data.Storage.Model;
using NI.Data;
using NI.Data.RelationalExpressions;
using NI.Data.Storage.Service.Schema;

namespace NI.Data.Storage.Service.Actions {
	
	public class LoadRelex {
		
		DataSchema Schema;
		IDalc StorageDalc;

		public LoadRelex(DataSchema schema, IDalc storageDalc) {
			Schema = schema;
			StorageDalc = storageDalc;
		}

		public LoadRowsResult LoadRows(string relex, bool totalcount) {
			var res = new LoadRowsResult();
			var relexParser = new RelExParser();
			var q = relexParser.Parse(relex);
			
			var fldMapping = new FieldMapping(q.Fields);
			q.Fields = fldMapping.CompactFields;

			if (totalcount) {
				res.TotalCount = StorageDalc.RecordsCount( q );
			}

			var data = new RowList();
			StorageDalc.ExecuteReader(q, (reader) => {
				for (int i = 0; i < q.StartRecord; i++)
					reader.Read(); // skip first N records

				var cols = new List<string>();
				for (int i = 0; i < reader.FieldCount; i++) {
					var fName = reader.GetName(i);
					cols.Add( fldMapping.RevMapping.ContainsKey(fName) ? fldMapping.RevMapping[fName] : fName );
				}

				while (reader.Read() && data.Count < q.RecordCount) {
					var values = new object[reader.FieldCount];
					reader.GetValues(values);
					var row = new Dictionary<string, object>(values.Length);
					for (int i = 0; i < reader.FieldCount; i++)
						row[ cols[i] ] = DBNull.Value.Equals(values[i]) ? null : values[i];
					data.Add(new DictionaryItem(row) );
				}
			});

			res.Data = data;
			return res;
		}

		public LoadValuesResult LoadValues(string relex, bool totalcount) {
			var res = new LoadValuesResult();
			var relexParser = new RelExParser();
			var q = relexParser.Parse(relex);

			var fldMapping = new FieldMapping(q.Fields);
			q.Fields = fldMapping.CompactFields;

			if (totalcount) {
				res.TotalCount = StorageDalc.RecordsCount( q );
			}

			var cols = new List<string>();
			var data = new List<object[]>();
			StorageDalc.ExecuteReader(q, (reader) => {
				for (int i = 0; i < q.StartRecord; i++)
					reader.Read(); // skip first N records
				for (int i = 0; i < reader.FieldCount; i++) {
					var fName = reader.GetName(i);
					cols.Add( fldMapping.RevMapping.ContainsKey(fName) ? fldMapping.RevMapping[fName] : fName );
				}

				while (reader.Read() && data.Count < q.RecordCount) {
					var values = new object[reader.FieldCount];
					reader.GetValues(values);
					for (int i = 0; i < reader.FieldCount; i++)
						if (DBNull.Value.Equals(values[i]))
							values[i] = null;
					data.Add(values);
				}
			});
			res.Columns = cols.ToArray();
			res.Data = data;
			return res;
		}

		internal class FieldMapping {
			QField[] Fields;
			internal QField[] CompactFields;
			internal IDictionary<string,string> RevMapping;
			internal FieldMapping(QField[] fields) {
				Fields = fields;
				RevMapping = new Dictionary<string,string>();
				if (fields != null && fields.Length > 0) { 
					CompactFields = new QField[Fields.Length];
					for (int i = 0; i < CompactFields.Length; i++) {
						var f = Fields[i];
						if (f.Prefix != null && f.Expression==null) {
							var originalFieldName = f.ToString().Replace('.', '_');
							var compactName = "f_"+i.ToString()+"_"+f.Name;
							RevMapping[compactName] = originalFieldName;
							CompactFields[i] = new QField( compactName, f.ToString() );
						} else {
							CompactFields[i] = f;
						}
					}
				} else {
					CompactFields = Fields;
				}
				

			}

		}

	}


}
