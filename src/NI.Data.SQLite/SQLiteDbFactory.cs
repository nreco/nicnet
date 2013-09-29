#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2013 NewtonIdeas
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
using System.Data.Common;
using System.Data.SQLite;
using System.ComponentModel;

namespace NI.Data.SQLite
{

	public class SQLiteDalcFactory : DbDalcFactory
	{
		public SQLiteDalcFactory()
			: base(SQLiteFactory.Instance) {
			ParamPlaceholderFormat = "?";
			
		}

		public override object GetInsertId(IDbConnection connection) {
			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException("GetInsertId requires opened connection");
			return ((SQLiteConnection)connection).LastInsertRowId;
		}

	}
}
