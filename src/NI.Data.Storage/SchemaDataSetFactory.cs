#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2014 Vitalii Fedorchenko
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using NI.Data;
using NI.Data.Storage.Model;

namespace NI.Data.Storage {
	
	/// <summary>
	/// <see cref="IDataSetFactory"/> implementation based on <see cref="DataSchema"/>. 
	/// </summary>
	public class SchemaDataSetFactory : IDataSetFactory {

		protected Func<DataSchema> GetSchema { get; set; }

		/// <summary>
		/// Initializes new instance of SchemaDataSetFactory with specified DataSchema provider.
		/// </summary>
		/// <param name="getSchema">delegate that returns DataSchema structure</param>
		public SchemaDataSetFactory(Func<DataSchema> getSchema) {
			GetSchema = getSchema;
		}

		/// <summary>
		/// Construct DataSet object with DataTable schema for specifed table name.
		/// </summary>
		/// <param name="tableName">table name</param>
		/// <returns>DataSet with DataTable for specified table name</returns>
		public DataSet GetDataSet(string tableName) {
			if (String.IsNullOrEmpty(tableName))
				throw new ArgumentNullException("tableName is empty");	
			var schema = GetSchema();
			var ds = new DataSet();
			var dataClass = schema.FindClassByID(tableName);
			if (dataClass==null)
				return null;

			var tbl = dataClass.CreateDataTable();
			ds.Tables.Add(tbl);
			return ds;
		}
	}
}
