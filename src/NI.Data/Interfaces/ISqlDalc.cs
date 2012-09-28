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
	public interface ISqlDalc : IDalc {

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
		int ExecuteNonQuery(string sqlText);

		/// <summary>
		/// Execute given raw SQL and return data reader
		/// </summary>
		void ExecuteReader(string sqlText, Action<IDataReader> handler);


		/// <summary>
		/// Execute custom SQL command and store result in specified dataset
		/// </summary>
		void Load(string sqlText, DataSet ds);

	
	}

}
