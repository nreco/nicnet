#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013 Vitalii Fedorchenko
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Threading.Tasks;

using NI.Data.Storage.Model;
using NI.Data;

namespace NI.Data.Storage
{
    public class StorageDalc : IDalc {

		protected IObjectContainerStorage ObjectContainerStorage { get; set; }
		protected IDalc UnderlyingDalc { get; set; }
		
		protected DataSchema Schema { get; set; }

		public StorageDalc(IDalc dalc, IObjectContainerStorage objContainerStorage, Func<DataSchema> getSchema) {
			UnderlyingDalc = dalc;
			Schema = getSchema();
			ObjectContainerStorage = objContainerStorage;
        }


		public DataTable Load(Query query, DataSet ds) {
			var dataClass = Schema.FindClassByID(query.Table.Name);
			if (dataClass != null) {
				throw new NotImplementedException();
			} else {
				return UnderlyingDalc.Load(query, ds);
			}
		}

		public void ExecuteReader(Query q, Action<IDataReader> handler) {
			var ds = new DataSet();
			Load(q, ds);
			handler( new DataTableReader(ds.Tables[q.Table.Name]) );
		}

		public int Delete(Query query) {
			var srcName = new QTable(query.Table.Name);
			var dataClass = Schema.FindClassByID(query.Table.Name);
			if (dataClass != null) {
				return 0;
			} else {
				return UnderlyingDalc.Delete(query);
			}
		}

		public void Insert(string tableName, IDictionary<string, IQueryValue> data) {
			var dataClass = Schema.FindClassByID(tableName);
			if (dataClass != null) {
				var objContainer = new ObjectContainer(dataClass);
				foreach (var changeEntry in data) {
					if (!(changeEntry.Value is QConst))
						throw new NotSupportedException(
							String.Format("{0} value type is not supported", changeEntry.Value.GetType() ) );

					objContainer[changeEntry.Key] = ((QConst)changeEntry.Value).Value;
				}
				ObjectContainerStorage.Insert(objContainer);
			} else {
				UnderlyingDalc.Insert(tableName,data);
			}
		}

		public int Update(Query query, IDictionary<string, IQueryValue> data) {
			return UnderlyingDalc.Update(query, data);
		}

		public void Update(DataTable t) {
			var dataClass = Schema.FindClassByID(t.TableName);
			if (dataClass!=null) {
				
				foreach (DataRow r in t.Rows) {
					switch (r.RowState) {
						case DataRowState.Added:
							InsertDataRow(dataClass, r);
							break;
						case DataRowState.Modified:
							UpdateDataRow(dataClass, r);
							break;
						case DataRowState.Deleted:
							DeleteDataRow(dataClass, r);
							break;
					}
				}
				t.AcceptChanges();
			} else {
				UnderlyingDalc.Update(t);
			}
		}

		protected void InsertDataRow(Class dataClass, DataRow r) {
			var obj = new ObjectContainer(dataClass);
			foreach (DataColumn c in r.Table.Columns)
				if (!c.AutoIncrement) {
					var val = r[c];
					if (val==DBNull.Value)
						val = null;
					obj[ c.ColumnName ] = val;
				}
			ObjectContainerStorage.Insert(obj);
			r["id"] = obj.ID.Value;
			r.AcceptChanges();
		}

		protected void DeleteDataRow(Class dataClass, DataRow r) {
			var objId = Convert.ToInt64( r["id",DataRowVersion.Original] );
			var obj = new ObjectContainer(dataClass, objId );
			ObjectContainerStorage.Delete( obj );
			r.AcceptChanges();
		}

		protected void UpdateDataRow(Class dataClass, DataRow r) {
			var objId = Convert.ToInt64(r["id"]);
			var obj = new ObjectContainer(dataClass, objId);
			foreach (DataColumn c in r.Table.Columns)
				if (!c.AutoIncrement) {
					var val = r[c];
					if (val == DBNull.Value)
						val = null;
					obj[c.ColumnName] = val;
				}

			ObjectContainerStorage.Update(obj);
			r.AcceptChanges();
		}


		protected Query TransformQuery(Query q, Class mainClass) {
			return q;
		}
	}
}
