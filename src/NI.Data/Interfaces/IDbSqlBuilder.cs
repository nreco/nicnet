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

namespace NI.Data
{
	/// <summary>
	/// DB Sql builder interface.
	/// </summary>
	public interface IDbSqlBuilder : ISqlBuilder
	{
		string BuildSelect(Query query);
		string BuildSort(Query query);
		string BuildFields(Query query);
		string BuildCommandParameter(object value);
		string BuildCommandParameter(DataColumn col, DataRowVersion sourceVersion);
	}
}
