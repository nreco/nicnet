#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013 Vitalii Fedorchenko
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

namespace NI.Data.Storage.Service {

	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ErrorBehavior(typeof(ErrorHandler))]
	public class StorageService : IStorageService {

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

		public LoadRelexResult LoadRelex(string relex) {
			try {
				return new LoadRelex(ProvideOntology(), StorageDalc ).Execute(relex); 
			} catch (Exception ex) {
				Console.WriteLine("RELEX ERROR: {0}", ex);
				throw;
			}
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
