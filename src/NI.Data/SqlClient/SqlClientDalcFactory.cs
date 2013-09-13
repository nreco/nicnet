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
using System.Data.Common;
using System.Data.SqlClient;
using System.ComponentModel;

namespace NI.Data.SqlClient
{

	public class SqlClientDalcFactory : DbDalcFactory
	{

		public bool TopOptimization { get; set; }

		public bool ConstOptimization { get; set; }

		public bool NameBrackets { get; set; }

		public SqlClientDalcFactory() : base(SqlClientFactory.Instance) {
			TopOptimization = false;
			ConstOptimization = false;
			NameBrackets = false;
		}

		public override IDbSqlBuilder CreateSqlBuilder(IDbCommand dbCommand) {
			return new SqlClientDbSqlBuilder(dbCommand, this);
		}

	}
}
