#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
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

using NI.Common;

namespace NI.Data
{
	/// <summary>
	/// Dataset Data Access Layer Component.
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

		virtual public int RecordsCount(string sourceName, IQueryNode conditions) {
			Query q = new Query(sourceName, conditions);
			// overhead. TODO: write more optimal implementation here
			DataSet ds = new DataSet();
			Load(ds, q);
			return ds.Tables[sourceName].Rows.Count;
		}


		/// <summary>
		/// Load data from data source to dataset
		/// </summary>
		/// <param name="ds">Destination dataset</param>
		/// <param name="query">Query</param>
		public virtual void Load(DataSet ds, IQuery query) {
			if (!PersistedDS.Tables.Contains(query.SourceName))
				throw new Exception("Persisted dataset does not contain table with name "+query.SourceName);
			
			string whereExpression = BuildExpression( query.Root );
			string sortExpression = BuildSort( query );
			if (!ds.Tables.Contains(query.SourceName))
				ds.Tables.Add( PersistedDS.Tables[query.SourceName].Clone() );
			else
				ds.Tables[query.SourceName].Rows.Clear();
			DataRow[] result = PersistedDS.Tables[query.SourceName].Select( whereExpression, sortExpression );
			if (query.Fields != null && query.Fields.Length != 0) {
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
		}
		
		/// <summary>
		/// Update data from dataset to datasource
		/// </summary>
		/// <param name="ds">DataSet</param>
		/// <param name="tableName"></param>
		public void Update(DataSet ds, string tableName) {
			if (!PersistedDS.Tables.Contains(tableName))
				throw new Exception("Persisted dataset does not contain table with name "+tableName);
			
			DiffProcessor diffProcessor = new DiffProcessor();
			DataRowDiffHandler diffHandler = new DataRowDiffHandler(PersistedDS.Tables[tableName]);
			diffProcessor.DiffHandler = diffHandler;
			diffProcessor.Sync( ds.Tables[tableName].Rows, PersistedDS.Tables[tableName].Rows );

			foreach (DataRow r in diffHandler.ForImport) {
				DataRow rToImport = PersistedDS.Tables[tableName].NewRow();
				for (int i = 0; i < PersistedDS.Tables[tableName].Columns.Count; ++i) {
					var c = PersistedDS.Tables[tableName].Columns[i];
					if (!c.AutoIncrement) {
						rToImport[c.ColumnName] = r[c.ColumnName];
					}
				}
				PersistedDS.Tables[tableName].Rows.Add(rToImport);
			}
			PersistedDS.Tables[tableName].AcceptChanges();
			ds.Tables[tableName].AcceptChanges();
		}

		/// <summary>
		/// Update data from dictionary container to datasource by query
		/// </summary>
		/// <param name="data">Container with record changes</param>
		/// <param name="query">query</param>
		public int Update(IDictionary data, IQuery query) {
			if (!PersistedDS.Tables.Contains(query.SourceName))
				throw new Exception("Persisted dataset does not contain table with name "+query.SourceName);
			
			string whereExpression = BuildExpression( query.Root );
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
		public void Insert(IDictionary data, string sourceName) {
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
		public int Delete(IQuery query) {
			if (!PersistedDS.Tables.Contains(query.SourceName))
				throw new Exception("Persisted dataset does not contain table with name "+query.SourceName);
			
			string whereExpression = BuildExpression( query.Root );
			DataRow[] result = PersistedDS.Tables[query.SourceName].Select( whereExpression );
			for (int i=0; i<result.Length; i++)
				result[i].Delete();
			PersistedDS.AcceptChanges();
			return result.Length;
		}
		
		/// <summary>
		/// Load first record by query
		/// </summary>
		/// <param name="data">Container for record data</param>
		/// <param name="query">query</param>
		/// <returns>Success flag</returns>
		public bool LoadRecord(IDictionary data, IQuery query) {
			if (!PersistedDS.Tables.Contains(query.SourceName))
				throw new Exception("Persisted dataset does not contain table with name " + query.SourceName);

			string whereExpression = BuildExpression(query.Root);
			string sortExpression = BuildSort(query);
			DataRow[] result = PersistedDS.Tables[query.SourceName].Select(whereExpression, sortExpression);
			if (result.Length == 0) return false;
			if (query.Fields.Length > 0 && query.Fields[0].ToLower() == "count(*)") {
				data[query.Fields[0]] = result.Length;
			} else {
				foreach (DataColumn c in PersistedDS.Tables[query.SourceName].Columns)
					data[c.ColumnName] = result[0][c.ColumnName];
			}
			return true;
		}
		
		
		protected override string BuildValue(IQueryValue value) {
			if (value is IQuery) {
				IQuery q = (IQuery)value;
				if (q.Fields==null || q.Fields.Length!=1)
					throw new Exception("Invalid nested query");
				string whereExpression = BuildExpression( q.Root );
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
		
		protected virtual string BuildSort(IQuery q) {
			if (q.Sort!=null && q.Sort.Length>0)
				return string.Join(",", q.Sort);
			return null;
		}

		/// <summary>
		/// Special implementation for dataset constants
		/// </summary>
		protected override string BuildValue(IQueryConstantValue value) {
			object constValue = ((IQueryConstantValue)value).Value;
				
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
		

#region Nested classes

		class DataRowDiffHandler : IDiffHandler {
			DataTable DestDataTable;
			public ArrayList ForImport;
			
			
			public DataRowDiffHandler(DataTable destDataTable) {
				DestDataTable = destDataTable;
				ForImport = new ArrayList();
			}
			
			string ExtractUid(object arg) {
				if (arg is DataRow) {
					DataRow r = (DataRow)arg;
					string[] pk = new string[r.Table.PrimaryKey.Length];
					if (pk.Length==0)
						throw new Exception("Cannot synchronize tables without primary key");
					for (int i=0; i<pk.Length; i++)
						pk[i] = r[ r.Table.PrimaryKey[i], r.HasVersion(DataRowVersion.Current)?DataRowVersion.Current:DataRowVersion.Original ].ToString();
					return String.Join(",", pk);
				}
				throw new ArgumentException();
			}
			
			/// <summary>
			/// Compare two elements
			/// </summary>
			public int Compare(object arg1, object arg2) {
				// if we are adding the row it cannot be present in the destination, so it isn't equal to any row in the destination
				if (arg1 is DataRow && (((DataRow)arg1).RowState == DataRowState.Added || ((DataRow)arg1).RowState == DataRowState.Detached)) {
					return 1;
				}
				return ExtractUid(arg1).CompareTo( ExtractUid(arg2) );
			}
		
			/// <summary>
			/// Merge action for two elements
			/// </summary>
			public void Merge(object arg1, object arg2) {
				DataRow sourceRow = (DataRow)arg1;
				DataRow destRow = (DataRow)arg2;
				if (sourceRow.RowState==DataRowState.Deleted)
					destRow.Delete();
				else
					destRow.ItemArray = sourceRow.ItemArray;
			}

			/// <summary>
			/// Add action
			/// </summary>
			public void Add(object arg) {
				DataRow r = (DataRow)arg;
				ForImport.Add(r);
			}

			/// <summary>
			/// Remove action
			/// </summary>
			public void Remove(object arg) {
				// no action here ...
			}
			
		}


#endregion

		
		
	}
}
