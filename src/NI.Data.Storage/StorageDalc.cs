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

		protected Func<DataSchema> GetSchema { get; set; }

		public StorageDalc(IDalc dalc, IObjectContainerStorage objContainerStorage, Func<DataSchema> getSchema) {
			UnderlyingDalc = dalc;
			GetSchema = getSchema;
			ObjectContainerStorage = objContainerStorage;
        }


		public DataTable Load(Query query, DataSet ds) {
			var dataClass = GetSchema().FindClassByID(query.Table.Name);
			if (dataClass != null) {
				// special count query
				if (query.Fields!=null && 
					query.Fields.Length == 1 && 
					query.Fields[0].Expression != null && 
					query.Fields[0].Expression.ToLower() == "count(*)") {
					
					if (ds.Tables.Contains(query.Table.Name))
						ds.Tables.Remove(query.Table.Name);
					var t = ds.Tables.Add(query.Table.Name);
					t.Columns.Add("count", typeof(int));
					var cntRow = t.NewRow();
					cntRow["count"] = ObjectContainerStorage.ObjectsCount(query);
					t.Rows.Add(cntRow);
					return t;
				}

				DataTable tbl;
				if (!ds.Tables.Contains(dataClass.ID)) {
					tbl = ds.Tables.Add( dataClass.ID );
				} else {
					tbl = ds.Tables[dataClass.ID];
				}
				// check columns
				IEnumerable<Property> propsToLoad = null;
				bool includeId = query.Fields==null || query.Fields.Where(f=>f.Name=="id" && f.Prefix==query.Table.Alias).Any();

				if (includeId) {
					// ensure special "id" column
					if (!tbl.Columns.Contains("id") || tbl.Columns["id"].DataType!=typeof(long)) {
						if (tbl.Columns.Contains("id"))
							tbl.Columns.Remove("id");
						tbl.Columns.Add("id", typeof(long));
					}
				}
				if (query.Fields==null) {
					propsToLoad = dataClass.Properties;
				} else {
					var queryProps = new List<Property>();
					foreach (var fld in query.Fields) {
						if (fld.Name=="id") continue; // tmp

						var prop = dataClass.FindPropertyByID(fld.Name);
						if (prop==null || fld.Prefix!=query.Table.Alias)
							throw new Exception(String.Format("Unknown field {0}", fld));
						queryProps.Add(prop);
					}
					propsToLoad = queryProps;
				}

				// ensure property columns
				foreach (var p in propsToLoad) {
					if (!tbl.Columns.Contains(p.ID) || tbl.Columns[p.ID].DataType != p.DataType.ValueType) {
						if (tbl.Columns.Contains(p.ID))
							tbl.Columns.Remove(p.ID);
						tbl.Columns.Add(p.ID, p.DataType.ValueType); //todo: handle multivalue
					}
				}
				var ids = ObjectContainerStorage.ObjectIds(query);
				var objects = ObjectContainerStorage.Load(ids, propsToLoad.ToArray());
				foreach (var id in ids) {
					if (objects.ContainsKey(id)) {
						var obj = objects[id];
						var r = tbl.NewRow();

						if (includeId)
							r["id"] = obj.ID.Value;

						foreach (var p in propsToLoad)
							r[p.ID] = obj[p] ?? DBNull.Value;

						tbl.Rows.Add(r);
						r.AcceptChanges();
					}
				}
				tbl.AcceptChanges();
				return tbl;
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
			var dataClass = GetSchema().FindClassByID(query.Table.Name);
			if (dataClass != null) {
				var ids = ObjectContainerStorage.ObjectIds(query);
				return ObjectContainerStorage.Delete(ids);
			} else {
				return UnderlyingDalc.Delete(query);
			}
		}

		public void Insert(string tableName, IDictionary<string, IQueryValue> data) {
			var dataClass = GetSchema().FindClassByID(tableName);
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
			var dataClass = GetSchema().FindClassByID(t.TableName);
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
		}

		protected void DeleteDataRow(Class dataClass, DataRow r) {
			var objId = Convert.ToInt64( r["id",DataRowVersion.Original] );
			var obj = new ObjectContainer(dataClass, objId );
			ObjectContainerStorage.Delete( obj );
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
		}
	}
}
