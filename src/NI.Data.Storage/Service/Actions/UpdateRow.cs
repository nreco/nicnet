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
using System.Runtime.Serialization;
using System.Data;
using System.ServiceModel;
using System.Globalization;

using NI.Data.Storage.Model;
using NI.Data;
using NI.Data.RelationalExpressions;
using NI.Data.Storage.Service.Schema;

namespace NI.Data.Storage.Service.Actions {
	
	public class UpdateRow {

		static Logger log = new Logger(typeof(UpdateRow));

		DataSchema Schema;
		IObjectContainerStorage ObjStorage;

		public UpdateRow(DataSchema schema, IObjectContainerStorage objStorage) {
			Schema = schema;
			ObjStorage = objStorage;
		}

		public void Update(string tableName, long id, IDictionary<string,object> data) {
			var objClass = Schema.FindClassByID(tableName);
			if (objClass==null)
				throw new Exception(String.Format("Unknown table {0}", tableName) );
			var objContainer = new ObjectContainer(objClass, id);
			log.Info("UPDATE {0} (ID={1})", tableName, id);
			foreach (var entry in data) {
				objContainer[ entry.Key ] = entry.Value;
				log.Info( "SET {0} = {1}", entry.Key, entry.Value );
			}
			ObjStorage.Update( objContainer );
		}

	}


}
