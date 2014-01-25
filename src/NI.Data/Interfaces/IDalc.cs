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
using System.Collections;
using System.Collections.Generic;

namespace NI.Data
{

	/// <summary>
	/// Data access layer component interface
	/// </summary>
	/// <remarks>Represents set of methods for accessing and updating tabular data source.</remarks>
	public interface IDalc
	{

		/// <summary>
		/// Execute given query and passes data reader to handler
		/// </summary>
		/// <param name="q">query to execute</param>
		/// <param name="handler">execution result callback</param>
		void ExecuteReader(Query q, Action<IDataReader> handler);		
	
		/// <summary>
		/// Load data into dataset by query
		/// </summary>
		/// <param name="q">query to load</param>
		/// <param name="ds">DataSet for results data</param>
		/// <returns>DataTable loaded with query data</returns>
		DataTable Load(Query q, DataSet ds);
		
		/// <summary>
		/// Update data in datasource according to changes of specified DataTable
		/// </summary>
		/// <param name="t">DataTable with changed rows</param>
		void Update(DataTable t);

		/// <summary>
		/// Update records in data source matched by query with specified changeset
		/// </summary>
		/// <param name="query">query</param>
		/// <param name="data">Container with record changes</param>
		/// <returns>number of updated records</returns>
		int Update(Query query, IDictionary<string,IQueryValue> data);
		
		/// <summary>
		/// Insert data from dictionary container to datasource
		/// </summary>
		/// <param name="tableName">Source name for data</param>
		/// <param name="data">Container with record data</param>
		void Insert(string tableName, IDictionary<string,IQueryValue> data);

		/// <summary>
		/// Delete data from dataset by query
		/// </summary>
		/// <param name="query"></param>
		int Delete(Query query);
		
		
	}
}
