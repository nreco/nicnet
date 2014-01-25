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
using System.IO;

namespace NI.Data
{
	/// <summary>
	/// DataSetFactory creates DataSet with schema for specified table name.
	/// </summary>
	/// <remarks>This component is used by <see cref="NI.Data.DataRowDalcMapper"/>.</remarks>
	public class DataSetFactory : IDataSetFactory
	{
		SchemaDescriptor[] _Schemas;

		/// <summary>
		/// Get or set list of SchemaDescriptor for this factory.
		/// </summary>
		public SchemaDescriptor[] Schemas {
			get { return _Schemas; }
			set { 
				_Schemas = value;
				TableNameDescrHash = null;
			}
		}

		static IDictionary<string, DataSet> DataSetCache = new Dictionary<string,DataSet>();
		static int MaxDataSetCacheSize = 200;

		IDictionary<string, SchemaDescriptor> TableNameDescrHash = null;

		/// <summary>
		/// Initializes new instance of DataSetFactory (Schemas property should be set before calling this component)
		/// </summary>
		public DataSetFactory() {

		}

		/// <summary>
		/// Initializes new instance of DataSetFactory with specified list of known table schemas
		/// </summary>
		/// <param name="schemas"></param>
		public DataSetFactory(SchemaDescriptor[] schemas) {
			Schemas = schemas;
		}

		protected SchemaDescriptor FindDescriptor(string tableName) {
			if (TableNameDescrHash == null) {
				TableNameDescrHash = new Dictionary<string, SchemaDescriptor>();
				foreach (SchemaDescriptor descr in Schemas)
					foreach (string sn in descr.TableNames)
						if (!TableNameDescrHash.ContainsKey(sn))
							TableNameDescrHash[sn] = descr;
			}
			return TableNameDescrHash.ContainsKey(tableName) ? TableNameDescrHash[tableName] : null;
		}

		protected DataSet GetDataSetWithSchema(string xmlSchema) {
			if (DataSetCache.Count>MaxDataSetCacheSize) {
				lock (DataSetCache) {
					DataSetCache.Clear();
				}
			}
			DataSet ds;

			if (DataSetCache.TryGetValue(xmlSchema, out ds)) {
				return ds.Clone();
			}
			ds = new DataSet();
			ds.ReadXmlSchema(new StringReader(xmlSchema));
			lock (DataSetCache) {
				DataSetCache[xmlSchema] = ds;
			}
			return ds.Clone();
		}

		/// <see cref="NI.Data.IDataSetFactory.GetDataSet"/>
		public DataSet GetDataSet(string tableName) {
			if (tableName==null)
				throw new ArgumentNullException("Source name cannot be null");
			SchemaDescriptor schemaDescr = FindDescriptor(tableName);
			if (schemaDescr == null)
				return null; // unknown table
			DataSet ds = GetDataSetWithSchema(schemaDescr.XmlSchema);
			return ds;
		}

		public class SchemaDescriptor {

			public string[] TableNames { get; set; }

			public string XmlSchema { get; set; }

		}
	}
}
