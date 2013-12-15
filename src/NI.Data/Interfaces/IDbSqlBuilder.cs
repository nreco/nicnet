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

namespace NI.Data
{
	/// <summary>
	/// DB Sql builder interface.
	/// </summary>
	/// <inherit>NI.Data.ISqlBuilder</inherit>
	public interface IDbSqlBuilder : ISqlBuilder
	{
		/// <summary>
		/// Build SQL select command text by query
		/// </summary>
		string BuildSelect(Query query);

		/// <summary>
		/// Build ORDER BY part of SQL command by query
		/// </summary>
		string BuildSort(Query query);

		/// <summary>
		/// Build SQL list of fields to select by
		/// </summary>
		string BuildFields(Query query);

		/// <summary>
		/// Build command parameter by value and return SQL placeholder text
		/// </summary>
		string BuildCommandParameter(object value);

		/// <summary>
		/// Build command parameter by DataColumn and return SQL placeholder text
		/// </summary>
		string BuildCommandParameter(DataColumn col, DataRowVersion sourceVersion);
	}
}
