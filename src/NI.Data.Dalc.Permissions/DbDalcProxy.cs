#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2008 NewtonIdeas
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
using NI.Data.Dalc;

namespace NI.Data.Dalc.Permissions
{
	/// <summary>
	/// DbDALC proxy
	/// </summary>
	public class DbDalcProxy : BaseDalcProxy, IDbDalc
	{
		IDbDalc _UnderlyingDbDalc;
		
		protected override IDalc Dalc {
			get { return _UnderlyingDbDalc; }
		}
		
		/// <summary>
		/// Get or set underlying DB DALC component
		/// </summary>
		[Dependency]
		public IDbDalc UnderlyingDbDalc {
			get { return _UnderlyingDbDalc; }
			set { _UnderlyingDbDalc = value; }
		}
		
		
		public DbDalcProxy()
		{
		}

		public bool LoadRecord(IDictionary data, string sqlCommandText) {
			return UnderlyingDbDalc.LoadRecord(data, sqlCommandText);
		}

		public int Execute(string sqlText) {
			return UnderlyingDbDalc.Execute(sqlText);
		}

		public IDataReader ExecuteReader(string sqlText) {
			return UnderlyingDbDalc.ExecuteReader(sqlText);
		}

		public IDbConnection Connection {
			get { return UnderlyingDbDalc.Connection; }
			set { UnderlyingDbDalc.Connection = value; }
		}

		public IDbTransaction Transaction {
			get { return UnderlyingDbDalc.Transaction; }
			set { UnderlyingDbDalc.Transaction = value; }
		}


		public void Load(DataSet ds, string sqlText) {
			UnderlyingDbDalc.Load(ds, sqlText);
		}
		
		public IDataReader LoadReader(IQuery q) {
			if (Enabled) {
				IQuery modifiedQuery = AddPermissionCondition(DalcOperation.Retrieve, q);
				return UnderlyingDbDalc.LoadReader(modifiedQuery);
			} else {
				return UnderlyingDbDalc.LoadReader(q);
			}		
		}



	}
}
