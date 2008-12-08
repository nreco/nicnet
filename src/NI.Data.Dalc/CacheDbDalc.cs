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

namespace NI.Data.Dalc {

    public class CacheDbDalc : CacheDalc, IDbDalc {

		/// <summary>
		/// Get or set underlying DB DALC component
		/// </summary>
		[Dependency]
		public IDbDalc UnderlyingDbDalc {
			get { return (IDbDalc) base.UnderlyingDalc; }
			set { base.UnderlyingDalc = value; }
		}
		
        public System.Data.IDbConnection Connection {
			get { return UnderlyingDbDalc.Connection; }
			set { UnderlyingDbDalc.Connection = value; }
        }

        public System.Data.IDbTransaction Transaction {
			get { return UnderlyingDbDalc.Transaction; }
			set { UnderlyingDbDalc.Transaction = value; }
        }

        public int Execute(string sqlText) {
            return UnderlyingDbDalc.Execute(sqlText);
        }

        public IDataReader ExecuteReader(string sqlText) {
            return UnderlyingDbDalc.ExecuteReader(sqlText);
        }

        public IDataReader LoadReader(IQuery q) {
            return UnderlyingDbDalc.LoadReader(q);
        }

        public void Load(DataSet ds, string sqlText) {
            UnderlyingDbDalc.Load(ds, sqlText);
        }

        public bool LoadRecord(IDictionary data, string sqlCommandText) {
            return UnderlyingDbDalc.LoadRecord(data, sqlCommandText);
        }

    }

}
