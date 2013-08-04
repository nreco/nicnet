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

	public class SQLiteFactory : IDbCommandWrapperFactory, IDbDataAdapterWrapperFactory
	{
		public IQueryFieldValueFormatter QueryFieldValueFormatter { get; set; }
	
		IDbCommandWrapper IDbCommandWrapperFactory.CreateInstance() {
			var cmdWrapper = new SQLiteCommandWrapper( new SQLiteCommand() );
			cmdWrapper.QueryFieldValueFormatter = QueryFieldValueFormatter;
			return cmdWrapper;
		}

		IDbDataAdapterWrapper IDbDataAdapterWrapperFactory.CreateInstance() {
			return new SQLiteAdapterWrapper( new SQLiteDataAdapter() );
		}
		
		
		
		


	}
}
