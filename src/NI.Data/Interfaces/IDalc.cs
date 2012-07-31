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
	/// Data access layer component interface
	/// </summary>
	public interface IDalc
	{
		
	
		/// <summary>
		/// Load data from data source to dataset
		/// </summary>
		/// <param name="ds">Destination dataset</param>
		/// <param name="query">Query</param>
		void Load(DataSet ds, IQuery query);
		
		/// <summary>
		/// Update data from dataset to datasource
		/// </summary>
		/// <param name="ds">DataSet</param>
		/// <param name="sourceName"></param>
		void Update(DataSet ds, string sourceName);

		/// <summary>
		/// Update data from dictionary container to datasource by query
		/// </summary>
		/// <param name="data">Container with record changes</param>
		/// <param name="query">query</param>
		int Update(IDictionary data, IQuery query);
		
		/// <summary>
		/// Insert data from dictionary container to datasource
		/// </summary>
		/// <param name="data">Container with record data</param>
		/// <param name="sourceName">Source name for data</param>
		void Insert(IDictionary data, string sourceName);

		/// <summary>
		/// Delete data from dataset by query
		/// </summary>
		/// <param name="query"></param>
		int Delete(IQuery query);
		
		/// <summary>
		/// Load first record by query
		/// </summary>
		/// <param name="data">Container for record data</param>
		/// <param name="query">query</param>
		/// <returns>Success flag</returns>
		bool LoadRecord(IDictionary data, IQuery query);
		
		/// <summary>
		/// Count the number of records
		/// </summary>
		/// <param name="sourceName">source name</param>
		/// <param name="conditions">additional conditions (can be null)</param>
		/// <returns>the number of records</returns>
		int RecordsCount(string sourceName, IQueryNode conditions);
		
		
	}
}
