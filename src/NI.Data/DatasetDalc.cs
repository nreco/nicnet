#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012: NewtonIdeas, modifications and v2 by Vitaliy Fedorchenko
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
using System.Data;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace NI.Data
{
	/// <summary>
	/// Dataset-based (in-memory) DALC implementation.
	/// </summary>
	/// <sort>2</sort>
	public class DataSetDalc : IDalc
	{
		/// <summary>
		/// Get or set underlying DataSet with persisted data
		/// </summary>
		public DataSet PersistedDS { get; set; }
		
		protected ISqlBuilder SqlBuilder { get; private set; }

		/// <summary>
		/// Initialize new instance of DataSetDalc (property PersistedDS should be initialized before component usage)
		/// </summary>
		public DataSetDalc() : this (new DataSet()) {
		}

		/// <summary>
		/// Initialize new instance of DataSetDalc with underlying DataSet
		/// </summary>
		public DataSetDalc(DataSet ds) {
			PersistedDS = ds;
			SqlBuilder = new DataSetSqlBuilder(this);
		}


		/// <see cref="NI.Data.IDalc.Load(NI.Data.Query,System.Data.DataSet)"/>
		public virtual DataTable Load(Query query, DataSet ds) {
			if (!PersistedDS.Tables.Contains(query.Table.Name))
				throw new Exception("Persisted dataset does not contain table with name "+query.Table.Name);

			string whereExpression = SqlBuilder.BuildExpression(query.Condition);
			string sortExpression = BuildSort( query );
			DataRow[] result = PersistedDS.Tables[query.Table.Name].Select( whereExpression, sortExpression );

			if (!ds.Tables.Contains(query.Table.Name))
				ds.Tables.Add(PersistedDS.Tables[query.Table.Name].Clone());
			else
				ds.Tables[query.Table.Name].Rows.Clear();
			
			if (query.Fields != null && query.Fields.Length != 0) {
				if (query.Fields.Length == 1 && query.Fields[0].Expression!=null && query.Fields[0].Expression.ToLower() == "count(*)") {
					ds.Tables.Remove(query.Table.Name);
					var t = ds.Tables.Add(query.Table.Name);
					t.Columns.Add("count", typeof(int));
					var cntRow = t.NewRow();
					cntRow["count"] = result.Length;
					t.Rows.Add(cntRow);
					return t;
				}

				for (int i=0; i<query.Fields.Length; i++) {
					string fld = query.Fields[i].Name;
					if (ds.Tables[query.Table].Columns.Contains(fld))
						continue;
					DataColumn column = new DataColumn();
					int idx = fld.LastIndexOf(')');
					if (idx == -1) {
						column.ColumnName = fld;
					} else {
						column.ColumnName = fld.Substring(idx + 1).Trim();
						column.Expression = fld.Substring(0, idx + 1).Trim();
					}
					if (ds.Tables[query.Table].Columns.Contains(column.ColumnName))
						ds.Tables[query.Table].Columns.Remove(column.ColumnName);
					ds.Tables[query.Table].Columns.Add(column);
				}
			}
			for (int i=0; i<result.Length; i++)
				ds.Tables[query.Table.Name].ImportRow(result[i]);

			return ds.Tables[query.Table.Name];
		}

		/// <see cref="NI.Data.IDalc.Update(System.Data.DataTable)"/>
		public void Update(DataTable t) {
			if (!PersistedDS.Tables.Contains(t.TableName))
				throw new Exception("Persisted dataset does not contain table with name "+t.TableName);
			var pTable = PersistedDS.Tables[t.TableName];
			if (pTable.PrimaryKey==null || pTable.PrimaryKey.Length==0)
				throw new Exception("Cannot update table without primary key: "+t.TableName);
			
			Func<DataRow, DataRow> findRowById = (r) => {
				foreach (DataRow pr in pTable.Rows) {
					var matched = true;
					foreach (DataColumn c in pTable.PrimaryKey) {
						if (!r.Table.Columns.Contains(c.ColumnName) 
							||
							Convert.ToString(pr[c.ColumnName]) != Convert.ToString(r[c.ColumnName, r.HasVersion(DataRowVersion.Current) ? DataRowVersion.Current : DataRowVersion.Original]))
							matched = false;
					}
					if (matched)
						return pr;
				}
				return null;
			};

			// remove deleted / import new
			var deleteRows = new List<DataRow>();
			foreach (DataRow r in t.Rows) {
				if (r.RowState == DataRowState.Added) {
					DataRow rToImport = pTable.NewRow();
					foreach (DataColumn c in pTable.Columns) {
						if (!c.AutoIncrement && t.Columns.Contains(c.ColumnName)) {
							rToImport[c.ColumnName] = r[c.ColumnName];
						}
					}
					pTable.Rows.Add(rToImport);
					foreach (DataColumn c in pTable.Columns)
						if (c.AutoIncrement && t.Columns.Contains(c.ColumnName))
							r[c.ColumnName] = rToImport[c];
				}
				if (r.RowState == DataRowState.Deleted) {
					var pr = findRowById(r);
					if (pr == null)
						throw new DBConcurrencyException("Cannot find datarow for deletion");
					deleteRows.Add(pr);
				}
				if (r.RowState == DataRowState.Modified) {
					var pr = findRowById(r);
					if (pr == null)
						throw new DBConcurrencyException("Cannot find datarow for update");

					foreach (DataColumn c in pTable.Columns) {
						if (!c.AutoIncrement && t.Columns.Contains(c.ColumnName)) {
							pr[c.ColumnName] = r[c.ColumnName];
						}
					}

				}
			}
			// remove rows marked for deletion
			foreach (DataRow pr in deleteRows)
				pr.Delete();

			pTable.AcceptChanges();
			t.AcceptChanges();
		}

		/// <see cref="NI.Data.IDalc.Update(NI.Data.Query,System.Collections.Generic.IDictionary<System.String,NI.Data.IQueryValue>)"/>
		public int Update(Query query, IDictionary<string,IQueryValue> data) {
			if (!PersistedDS.Tables.Contains(query.Table))
				throw new Exception("Persisted dataset does not contain table with name "+query.Table);

			string whereExpression = SqlBuilder.BuildExpression(query.Condition);
			DataRow[] result = PersistedDS.Tables[query.Table].Select( whereExpression );
			for (int i=0; i<result.Length; i++) {
				foreach (var fieldValue in data) {
					if (fieldValue.Value!=null && !(fieldValue.Value is QConst))
						throw new NotSupportedException(
							String.Format("DatasetDalc doesn't support {0} as value for Update", fieldValue.Value.GetType() ) );
					result[i][fieldValue.Key] = fieldValue.Value!=null ? ((QConst)fieldValue.Value).Value : DBNull.Value;
				}
			}
			PersistedDS.AcceptChanges();
			return result.Length;
		}


		/// <see cref="NI.Data.IDalc.Insert(System.String,System.Collections.Generic.IDictionary<System.String,NI.Data.IQueryValue>)"/>
        public void Insert(string tableName, IDictionary<string,IQueryValue> data) {
			if (!PersistedDS.Tables.Contains(tableName))
				throw new Exception("Persisted dataset does not contain table with name "+tableName);
			
			DataRow row = PersistedDS.Tables[tableName].NewRow();
			foreach (var fldVal in data) {
				if (fldVal.Value != null && !(fldVal.Value is QConst))
					throw new NotSupportedException(
						String.Format("DatasetDalc doesn't support {0} as value for Insert", fldVal.Value.GetType()));
				row[fldVal.Key] = fldVal.Value != null ? ((QConst)fldVal.Value).Value : DBNull.Value;
			}
			PersistedDS.Tables[tableName].Rows.Add( row );
			PersistedDS.AcceptChanges();
			
		}

		/// <see cref="NI.Data.IDalc.Delete(NI.Data.Query)"/>
		public int Delete(Query query) {
			if (!PersistedDS.Tables.Contains(query.Table))
				throw new Exception("Persisted dataset does not contain table with name "+query.Table);

			string whereExpression = SqlBuilder.BuildExpression(query.Condition);
			DataRow[] result = PersistedDS.Tables[query.Table].Select( whereExpression );
			for (int i=0; i<result.Length; i++)
				result[i].Delete();
			PersistedDS.AcceptChanges();
			return result.Length;
		}

		/// <see cref="NI.Data.IDalc.ExecuteReader(NI.Data.Query,System.Action<System.Data.IDataReader>)"/>
		public void ExecuteReader(Query q, Action<IDataReader> handler) {
			var ds = new DataSet();
			var tbl = Load(q, ds);
			var rdr = new DataTableReader(tbl);
			handler(rdr);
		}
		
		
		protected virtual string BuildSort(Query q) {
			if (q.Sort!=null && q.Sort.Length>0)
				return string.Join(",", q.Sort.Select(v=>(string)v).ToArray() );
			return null;
		}


		
		internal class DataSetSqlBuilder : SqlBuilder {

			DataSetDalc dsDalc;

			internal DataSetSqlBuilder(DataSetDalc dalc) {
				dsDalc = dalc;
			}

			public override string BuildValue(IQueryValue value) {
				if (value is Query) {
					Query q = (Query)value;
					if (q.Fields == null || q.Fields.Length != 1)
						throw new Exception("Invalid nested query");
					string whereExpression = BuildExpression(q.Condition);
					string sortExpression = dsDalc.BuildSort(q);
					DataRow[] result = dsDalc.PersistedDS.Tables[q.Table].Select(whereExpression, sortExpression);
					if (result.Length == 1)
						return base.BuildValue(new QConst(result[0][q.Fields[0].Name]));
					if (result.Length > 1) {
						// build array
						object[] resValues = new object[result.Length];
						for (int i = 0; i < resValues.Length; i++)
							resValues[i] = result[i][q.Fields[0].Name];
						return base.BuildValue(new QConst(resValues));
					}

					return "NULL";
				}
				return base.BuildValue(value);
			}

			protected override string BuildValue(QConst value) {
				object constValue = value.Value;

				// special processing for arrays
				if (constValue is IList)
					return BuildValue((IList)constValue);

				if (constValue is DateTime) {
					// Date values should be enclosed within pound signs (#). (MSDN)
					return "#" + constValue.ToString() + "#";
				}

				if (constValue is string)
					return "'" + constValue.ToString().Replace("'", "''") + "'";

				if (constValue == DBNull.Value)
					return "NULL";

				return constValue.ToString();
			}

			protected override string BuildValue(QField fieldValue) {
				if (!String.IsNullOrEmpty(fieldValue.Expression))
					return fieldValue.Expression;
				return fieldValue.Name;
			}

		}

		
		
	}
}
