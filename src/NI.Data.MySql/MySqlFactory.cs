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
using System.Data.Common;
using MySql.Data.MySqlClient;
using MySql.Data;
using System.ComponentModel;

namespace NI.Data.MySql
{

	public class MySqlDalcFactory : GenericDbProviderFactory
	{

		public MySqlDalcFactory()
			: base(MySqlClientFactory.Instance) {
		}

		public override object GetInsertId(IDbConnection connection) {
			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException("GetInsertId requires opened connection");
			using (var cmd = CreateCommand()) {
				cmd.CommandText = "SELECT LAST_INSERT_ID()";
				cmd.Connection = connection;
				return cmd.ExecuteScalar();
			}
		}		
		
		


	}
}
