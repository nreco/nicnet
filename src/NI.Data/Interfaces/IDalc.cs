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
	public interface IDalc
	{

		/// <summary>
		/// Execute given query and passes data reader to handler
		/// </summary>
		void ExecuteReader(Query q, Action<IDataReader> handler);		
	
		/// <summary>
		/// Load data into dataset by query
		/// </summary>
		DataTable Load(Query query, DataSet ds);
		
		/// <summary>
		/// Update data from dataset to datasource
		/// </summary>
		/// <param name="ds">DataSet</param>
		/// <param name="sourceName"></param>
		void Update(DataTable t);

		/// <summary>
		/// Update data from dictionary container to datasource by query
		/// </summary>
		/// <param name="query">query</param>
		/// <param name="data">Container with record changes</param>
		int Update(Query query, IDictionary<string,IQueryValue> data);
		
		/// <summary>
		/// Insert data from dictionary container to datasource
		/// </summary>
		/// <param name="sourceName">Source name for data</param>
		/// <param name="data">Container with record data</param>
		void Insert(string sourceName, IDictionary<string,IQueryValue> data);

		/// <summary>
		/// Delete data from dataset by query
		/// </summary>
		/// <param name="query"></param>
		int Delete(Query query);
		
		
	}
}
