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
using System.Runtime.Serialization;
using System.Data;
using System.ServiceModel;
using System.Globalization;

using NI.Data.Storage.Model;
using NI.Data;
using NI.Data.RelationalExpressions;
using NI.Data.Storage.Service.Schema;

namespace NI.Data.Storage.Service.Actions {
	
	public class ChangeRow {

		static Logger log = new Logger(typeof(ChangeRow));

		DataSchema Schema;
		IObjectContainerStorage ObjStorage;
		IDalc StorageDalc;

		public ChangeRow(DataSchema schema, IObjectContainerStorage objStorage, IDalc storageDalc) {
			Schema = schema;
			ObjStorage = objStorage;
			StorageDalc = storageDalc;
		}

		public long? Insert(string tableName, DictionaryItem data) {
			var objClass = Schema.FindClassByID(tableName);
			if (objClass == null) {
				if (Schema.FindRelationshipByID(tableName)!=null) {
					StorageDalc.Insert(tableName, (IDictionary) data.Data);
					return null;
				}
				throw new Exception(String.Format("Unknown table {0}", tableName));
			}
			var objContainer = new ObjectContainer(objClass);
			foreach (var entry in data.Data) {
				var prop = objClass.FindPropertyByID(entry.Key);
				if (prop != null && !prop.PrimaryKey) {
					objContainer[entry.Key] = entry.Value;
				}
			}
			ObjStorage.Insert(objContainer);
			return objContainer.ID;
		}

		public void Update(string tableName, long id, DictionaryItem data) {
			var objClass = Schema.FindClassByID(tableName);
			if (objClass==null)
				throw new Exception(String.Format("Unknown table {0}", tableName) );
			var objContainer = new ObjectContainer(objClass, id);
			foreach (var entry in data.Data) {
				var prop = objClass.FindPropertyByID(entry.Key);
				if (prop!=null && !prop.PrimaryKey) {
					objContainer[ entry.Key ] = entry.Value;
				}
			}
			ObjStorage.Update( objContainer );
		}

		public void Delete(string tableName, long id) {
			var objClass = Schema.FindClassByID(tableName);
			if (objClass == null) {
				throw new Exception(String.Format("Unknown table {0}", tableName));
			}
			var objContainer = new ObjectContainer(objClass, id);
			ObjStorage.Delete(objContainer);
		}

	}


}
