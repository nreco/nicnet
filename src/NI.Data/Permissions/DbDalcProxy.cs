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
using System.Data;
using System.Collections;

using NI.Data;

namespace NI.Data.Permissions
{
	/// <summary>
	/// DbDALC proxy
	/// </summary>
	/*public class DbDalcProxy : BaseDalcProxy, ISqlDalc
	{
		ISqlDalc _UnderlyingDbDalc;
		
		protected override IDalc Dalc {
			get { return _UnderlyingDbDalc; }
		}
		
		/// <summary>
		/// Get or set underlying DB DALC component
		/// </summary>
		public ISqlDalc UnderlyingDbDalc {
			get { return _UnderlyingDbDalc; }
			set { _UnderlyingDbDalc = value; }
		}
		
		
		public DbDalcProxy()
		{
		}

        public int ExecuteNonQuery(string sqlText) {
            return UnderlyingDbDalc.ExecuteNonQuery(sqlText);
		}

		public void ExecuteReader(string sqlText, Action<IDataReader> callback) {
			UnderlyingDbDalc.ExecuteReader(sqlText, callback);
		}

		public IDbConnection Connection {
			get { return UnderlyingDbDalc.Connection; }
			set { UnderlyingDbDalc.Connection = value; }
		}

		public IDbTransaction Transaction {
			get { return UnderlyingDbDalc.Transaction; }
			set { UnderlyingDbDalc.Transaction = value; }
		}


        public virtual void Load(string sqlText, DataSet ds) {
			UnderlyingDbDalc.Load(sqlText, ds);
		}

	}*/
}
