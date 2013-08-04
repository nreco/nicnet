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
using System.Collections;

namespace NI.Data
{
	/// <summary>
	/// Data view interface.
	/// </summary>
	public interface IDbDataView
	{
	
		/// <summary>
		/// Determines whether this dataview matches given sourcename
		/// </summary>
		/// <param name="sourceName">sourcename to test</param>
		bool MatchSourceName(string sourceName);
		
		/// <summary>
		/// Source name of data view origin (optional; can be null)
		/// </summary>
		string SourceNameOrigin { get; }
		
		string FormatSqlCommandText(IDictionary context);
		
		IQueryFieldValueFormatter GetQueryFieldValueFormatter(Query q);
	}
}
