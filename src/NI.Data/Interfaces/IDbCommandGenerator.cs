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
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;

namespace NI.Data
{

	/// <summary>
	/// DbCommand Generator interface
	/// </summary>
	public interface IDbCommandGenerator 
	{
		/// <summary>
		/// Compose IDbCommand instance with SELECT by query
		/// </summary>
		IDbCommand ComposeSelect(Query query);

		/// <summary>
		/// Compose DB adapter update commands (insert,update,delete) for specified DataTable
		/// </summary>
		void ComposeAdapterUpdateCommands(IDbDataAdapter adapter, DataTable table);

		/// <summary>
		/// Compose IDbCommand with SQL insert for specified table name and column name -> value map
		/// </summary>
		IDbCommand ComposeInsert(string tableName, IDictionary<string, IQueryValue> data);

		/// <summary>
		/// Compose IDbCommand with SQL delete by query 
		/// </summary>
		IDbCommand ComposeDelete(Query query);

		/// <summary>
		/// Compose IDbCommand with SQL update by query and specified column name -> value map
		/// </summary>
		IDbCommand ComposeUpdate(Query query, IDictionary<string, IQueryValue> data);
	}
}
