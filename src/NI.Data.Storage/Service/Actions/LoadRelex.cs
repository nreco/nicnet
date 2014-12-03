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

			if (totalcount) {
				res.TotalCount = StorageDalc.RecordsCount( q );
			}

			var ds = new DataSet();
			var tbl = StorageDalc.Load(q, ds);
			
			var data = new RowList();
			foreach (DataRow r in tbl.Rows) {
				data.Add( new DataRowItem(r) );
			}
			res.Data = data;
			return res;
		}

		public LoadValuesResult LoadValues(string relex, bool totalcount) {
			var res = new LoadValuesResult();
			var relexParser = new RelExParser();
			var q = relexParser.Parse(relex);

			if (totalcount) {
				res.TotalCount = StorageDalc.RecordsCount( q );
			}

			var cols = new List<string>();
			var data = new List<object[]>();
			StorageDalc.ExecuteReader(q, (reader) => {
				for (int i = 0; i < q.StartRecord; i++)
					reader.Read(); // skip first N records				
				for (int i = 0; i < reader.FieldCount; i++) {
					 cols.Add( reader.GetName(i) );
				}

				while (reader.Read() && data.Count < q.RecordCount) {
					var values = new object[reader.FieldCount];
					// fetch all fields & values in hashtable
					for (int i = 0; i < reader.FieldCount; i++)
						values[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
					data.Add(values);
				}
			});
			res.Columns = cols.ToArray();
			res.Data = data;
			return res;
		}


	}


}
