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
using System.Collections;
using System.Collections.Generic;


namespace NI.Data
{
	/// <summary>
	/// Dataset-based (in-memory) DALC implementation.
	/// </summary>
	public class DatasetDalc : SqlBuilder, IDalc
	{
		DataSet _PersistedDS;
		
		public DataSet PersistedDS {
			get { return _PersistedDS; }
			set { _PersistedDS = value; }
		}
	
		public DatasetDalc()
		{
		}

		/// <summary>
		/// Load data from data source to dataset
		/// </summary>
		public virtual DataTable Load(Query query, DataSet ds) {
			if (!PersistedDS.Tables.Contains(query.SourceName))
				throw new Exception("Persisted dataset does not contain table with name "+query.SourceName);
			
			string whereExpression = BuildExpression( query.Condition );
			string sortExpression = BuildSort( query );
			DataRow[] result = PersistedDS.Tables[query.SourceName].Select( whereExpression, sortExpression );

			if (!ds.Tables.Contains(query.SourceName))
				ds.Tables.Add(PersistedDS.Tables[query.SourceName].Clone());
			else
				ds.Tables[query.SourceName].Rows.Clear();
			
			if (query.Fields != null && query.Fields.Length != 0) {
				if (query.Fields.Length == 1 && query.Fields[0] == "count(*)") {
					ds.Tables.Remove(query.SourceName);
					var t = ds.Tables.Add(query.SourceName);
					t.Columns.Add("count", typeof(int));
					var cntRow = t.NewRow();
					cntRow["count"] = result.Length;
					t.Rows.Add(cntRow);
					return t;
				}

				for (int i=0; i<query.Fields.Length; i++) {
					string fld = query.Fields[i];
					if (ds.Tables[query.SourceName].Columns.Contains(fld))
						continue;
					DataColumn column = new DataColumn();
					int idx = fld.LastIndexOf(')');
					if (idx == -1) {
						column.ColumnName = fld;
					} else {
						column.ColumnName = fld.Substring(idx + 1).Trim();
						column.Expression = fld.Substring(0, idx + 1).Trim();
					}
					if (ds.Tables[query.SourceName].Columns.Contains(column.ColumnName))
						ds.Tables[query.SourceName].Columns.Remove(column.ColumnName);
					ds.Tables[query.SourceName].Columns.Add(column);
				}
			}
			for (int i=0; i<result.Length; i++)
				ds.Tables[query.SourceName].ImportRow(result[i]);

			return ds.Tables[query.SourceName];
		}
		
		/// <summary>
		/// Update data from dataset to datasource
		/// </summary>
		/// <param name="ds">DataSet</param>
		/// <param name="tableName"></param>
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

		/// <summary>
		/// Update data from dictionary container to datasource by query
		/// </summary>
		/// <param name="data">Container with record changes</param>
		/// <param name="query">query</param>
		public int Update(Query query, IDictionary data) {
			if (!PersistedDS.Tables.Contains(query.SourceName))
				throw new Exception("Persisted dataset does not contain table with name "+query.SourceName);
			
			string whereExpression = BuildExpression( query.Condition );
			DataRow[] result = PersistedDS.Tables[query.SourceName].Select( whereExpression );
			for (int i=0; i<result.Length; i++) {
				foreach (object columnName in data.Keys)
					result[i][ columnName.ToString() ] = data[columnName];
			}
			PersistedDS.AcceptChanges();
			return result.Length;
		}


		/// <summary>
		/// Insert data from dictionary container to datasource
		/// </summary>
		/// <param name="data">Container with record changes</param>
		/// <param name="sourceName">source name</param>
        public void Insert(string sourceName, IDictionary data) {
			if (!PersistedDS.Tables.Contains(sourceName))
				throw new Exception("Persisted dataset does not contain table with name "+sourceName);
			
			DataRow row = PersistedDS.Tables[sourceName].NewRow();
			foreach (object columnName in data.Keys)
				row[ columnName.ToString() ] = data[columnName];
			PersistedDS.Tables[sourceName].Rows.Add( row );
			PersistedDS.AcceptChanges();
			
		}

		
		/// <summary>
		/// Delete data from dataset by query
		/// </summary>
		/// <param name="query"></param>
		public int Delete(Query query) {
			if (!PersistedDS.Tables.Contains(query.SourceName))
				throw new Exception("Persisted dataset does not contain table with name "+query.SourceName);
			
			string whereExpression = BuildExpression( query.Condition );
			DataRow[] result = PersistedDS.Tables[query.SourceName].Select( whereExpression );
			for (int i=0; i<result.Length; i++)
				result[i].Delete();
			PersistedDS.AcceptChanges();
			return result.Length;
		}
		
		public void ExecuteReader(Query q, Action<IDataReader> handler) {
			var ds = new DataSet();
			var tbl = Load(q, ds);
			var rdr = new DataTableReader(tbl);
			handler(rdr);
		}
		
		
		protected override string BuildValue(IQueryValue value) {
			if (value is Query) {
				Query q = (Query)value;
				if (q.Fields==null || q.Fields.Length!=1)
					throw new Exception("Invalid nested query");
				string whereExpression = BuildExpression( q.Condition );
				string sortExpression = BuildSort( q );
				DataRow[] result = PersistedDS.Tables[q.SourceName].Select( whereExpression, sortExpression );
				if (result.Length==1)
					return base.BuildValue( new QConst(result[0][q.Fields[0]]) );
				if (result.Length>1) {
					// build array
					object[] resValues = new object[result.Length];
					for (int i=0; i<resValues.Length; i++)
						resValues[i] = result[i][q.Fields[0]];
					return base.BuildValue( new QConst(resValues) );
				}
				
				return "NULL";
			}
			return base.BuildValue( value );
		}
		
		protected virtual string BuildSort(Query q) {
			if (q.Sort!=null && q.Sort.Length>0)
				return string.Join(",", q.Sort);
			return null;
		}

		/// <summary>
		/// Special implementation for dataset constants
		/// </summary>
		protected override string BuildValue(QConst value) {
			object constValue = value.Value;
				
			// special processing for arrays
			if (constValue is IList)
				return BuildValue( (IList)constValue );
			
			if (constValue is DateTime) {
				// Date values should be enclosed within pound signs (#). (MSDN)
				return "#"+constValue.ToString()+"#";
			}

			if (constValue is string)
				return "'"+constValue.ToString().Replace("'", "''")+"'";

			if (constValue == DBNull.Value)
				return "NULL";
									
			return constValue.ToString();
		}
		

		
		
	}
}
