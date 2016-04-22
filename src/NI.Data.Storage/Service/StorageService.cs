#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013-2014 Vitalii Fedorchenko
 * Copyright 2014 NewtonIdeas
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
using System.ServiceModel.Activation;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.ServiceModel;

using NI.Data.Storage.Model;
using NI.Data.Storage.Service.Actions;
using NI.Data.Storage.Service.Schema;

using NI.Data;
using NI.Data.RelationalExpressions;

namespace NI.Data.Storage.Service {

	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ErrorBehavior(typeof(ErrorHandler))]
	public class StorageService : IStorageService {

		static Logger log = new Logger(typeof(StorageService));

		protected IObjectContainerStorage ObjPersister;
		protected Func<DataSchema> ProvideOntology;
		protected IDalc StorageDalc;

		public StorageService(IObjectContainerStorage objPersister, IDalc storageDalc, Func<DataSchema> getOntology) {
			ObjPersister = objPersister;
			ProvideOntology = getOntology;
			StorageDalc = storageDalc;
		}

		public GetDataSchemaResult GetDataSchema() {
			return new GetDataSchema(ProvideOntology()).Execute();
		}

		public LoadRowsResult LoadRows(string relex, bool totalcount) {
			try {
				return new LoadRelex(ProvideOntology(), StorageDalc ).LoadRows(relex, totalcount); 
			} catch (Exception ex) {
				log.Error("LoadRows: {0}", ex);
				throw;
			}
		}

		public LoadValuesResult LoadValues(string relex, bool totalcount) {
			try {
				return new LoadRelex(ProvideOntology(), StorageDalc ).LoadValues(relex, totalcount); 
			} catch (Exception ex) {
				log.Error("LoadValues: {0}", ex);
				throw;
			}
		}

		public long? InsertRow(string tableName, DictionaryItem data) {
			return (new ChangeRow(ProvideOntology(), ObjPersister, StorageDalc)).Insert(tableName, data);
		}

		public void UpdateRow(string tableName, string id, DictionaryItem data) {
			if (data==null)
				throw new ArgumentException("data is null");
			(new ChangeRow(ProvideOntology(), ObjPersister, StorageDalc)).Update( tableName, Convert.ToInt64( id ), data);
		}

		public void DeleteRow(string tableName, string id) {
			(new ChangeRow(ProvideOntology(), ObjPersister, StorageDalc)).Delete(tableName, Convert.ToInt64(id) );
		}

		public int DeleteRows(string relex) {
			var relexParser = new RelExParser();
			var q = relexParser.Parse(relex);
			return StorageDalc.Delete(q);
		}

		public DictionaryItem LoadRow(string tableName, string id) {
			var objId = Convert.ToInt64(id);
			var schema = ProvideOntology();
			var objClass = schema.FindClassByID(tableName);
			if (objClass==null)
				throw new Exception(String.Format("Unknown table {0}", tableName) );

			var idToContainer = ObjPersister.Load(new[] {objId});
			if (!idToContainer.ContainsKey(objId) || idToContainer[objId].GetClass()!=objClass ) {
				throw new Exception(String.Format("Record ID={0} does not exist in table {1}", id, tableName));
			}

			var record = new Dictionary<string,object>();
			foreach (var entry in idToContainer[objId]) {
				record[entry.Key.ID] = entry.Value;
			}
			return new DictionaryItem(record);
		}

		/*protected void RunInTransaction(Action<object> a) {
			var dbTransaction = WebManager.GetService<NI.Common.Transaction.ITransaction>("db-DalcTransaction");
			dbTransaction.Begin();
			try {
				a(null);
				dbTransaction.Commit();
			} finally {
				dbTransaction.Abort();
			}
		}*/

	}



}
