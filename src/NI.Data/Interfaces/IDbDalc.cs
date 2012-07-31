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
using System.Collections;

namespace NI.Data
{

	/// <summary>
	/// Database DALC
	/// </summary>
	public interface IDbDalc : IDalc {

		/// <summary>
		/// Connection
		/// </summary>
		IDbConnection Connection { get; set; }

		/// <summary>
		/// Transaction
		/// </summary>
		IDbTransaction Transaction { get; set; }


		/// <summary>
		/// Execute SQL command
		/// </summary>
		/// <param name="sqlText">SQL command text</param>
		/// <returns>Records affected</returns>
		int Execute(string sqlText);

		/// <summary>
		/// Execute given raw SQL and return data reader
		/// </summary>
		/// <remarks>
		/// Code that uses this method should manually maintain DB connection state
		/// (it should be opened before calling this method)
		/// </remarks>
		/// <param name="query">query</param>
		/// <returns>data reader with possible command execution results</returns>
		IDataReader ExecuteReader(string sqlText);
		
		/// <summary>
		/// Execute given query and return data reader
		/// </summary>
		/// <remarks>
		/// Code that uses this method should manually maintain DB connection state
		/// (it should be opened before calling this method)
		/// </remarks>
		/// <param name="query">query</param>
		/// <returns>data reader with possible query execution results</returns>
		IDataReader LoadReader(IQuery q);

		/// <summary>
		/// Execute custom SQL command and store result in specified dataset
		/// </summary>
		void Load(DataSet ds, string sqlText);

		/// <summary>
		/// Load first
		/// </summary>
		/// <param name="data"></param>
		/// <param name="sqlCommandText"></param>
		/// <returns>Success flag</returns>
		bool LoadRecord(IDictionary data, string sqlCommandText);
	
	}

}
