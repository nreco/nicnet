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
using System.Collections.Specialized;

using NI.Common;
using NI.Common.Providers;

namespace NI.Data
{
	/// <summary>
	/// Data list provider that uses DALC to load data.
	/// </summary>
	public class DalcObjectListProvider : IObjectListProvider, IObjectProvider
	{
		IQueryProvider _QueryProvider;
		IDalc _Dalc;
		protected bool UseDataReader;
		
		/// <summary>
		/// Get or set query provider that specifies data list to load
		/// </summary>
		public IQueryProvider QueryProvider {
			get { return _QueryProvider; }
			set { _QueryProvider = value; }
		}
		
		/// <summary>
		/// Get or set DALC component used to load list data
		/// </summary>
		public IDalc Dalc {
			get { return _Dalc; }
			set { _Dalc = value; }	
		}
	
		public DalcObjectListProvider()
		{
			// for legacy - disable in derived classes
			UseDataReader = this.GetType() == typeof(DalcObjectListProvider);
		}
		
		/// <summary>
		/// <see cref="IObjectListProvider.GetObjectList"/>
		/// </summary>
		public IList GetObjectList(object context) {
			IQuery q = GetQuery(context);
			object[] result = null;
			
			// datareader-based optimized logic
			if (UseDataReader && Dalc is IDbDalc) {
				IDbDalc dbDalc = (IDbDalc)Dalc;
				// store data here
				ArrayList listData = new ArrayList();
				// ensure that connection is open
				bool closeConnection = false;
				if (dbDalc.Connection.State != ConnectionState.Open) {
					dbDalc.Connection.Open();
					closeConnection = true;
				}
				try {
					IDataReader rdr = dbDalc.LoadReader(q);
					int index = 0;
                    while (rdr.Read() && index < (long)q.RecordCount + (long)q.StartRecord)
                    {
						if (index>=q.StartRecord) {
							Hashtable recordInfo = new Hashtable();
							// fetch all fields & values in dictionary
							for (int i = 0; i < rdr.FieldCount; i++) {
								recordInfo[rdr.GetName(i)] = rdr.GetValue(i);
								if (i==0) recordInfo[0] = rdr.GetValue(i);
							}
							listData.Add(recordInfo);
						}
						index++;
					}
					rdr.Close();
				} finally {
					// close only if was opened
					if (closeConnection)
						dbDalc.Connection.Close();
				}
				// data fetched. Lets prepare result.
				result = new object[listData.Count];
				for (int i=0; i<result.Length; i++)
					result[i] = PrepareObject( (IDictionary)listData[i], q.Fields);
			} else {
				// classic dataset-based logic
				DataSet ds = new DataSet();
				Dalc.Load(ds, q);
				
				result = new object[ds.Tables[q.SourceName].Rows.Count];
				for (int i=0; i<result.Length; i++)
					result[i] = PrepareObject(ds.Tables[q.SourceName].Rows[i], q.Fields) ;
			}
			
			return result;
		}

		/// <summary>
		/// Prepare list object by data from DALC
		/// </summary>
		/// <param name="row">DataRow instance</param>
		/// <param name="fields">fields to include in result object (can be null)</param>
		/// <returns>data row object</returns>
		protected virtual object PrepareObject(DataRow row, string[] fields) {
			if (fields!=null && fields.Length>0 && row.Table.Columns.Contains(fields[0]) )
				return row.IsNull(fields[0]) ? null : row[fields[0]];
			return row.IsNull(0) ? null : row[0];
		}

		protected virtual object PrepareObject(IDictionary row, string[] fields) {
			if (fields != null && fields.Length > 0 && row.Contains(fields[0]))
				return row[fields[0]]==DBNull.Value ? null : row[fields[0]];
			return row.Keys.Count==0 || row[0]==DBNull.Value ? null : row[0];
		}
		
		
		protected virtual IQuery GetQuery(object context) {
			return QueryProvider.GetQuery(context);
		}
		
		/// <summary>
		/// <see cref="IObjectProvider.GetObject"/>
		/// </summary>
		public object GetObject(object context) {
			return GetObjectList(context);
		}

	}
}
