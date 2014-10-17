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
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml.Serialization;
using System.ComponentModel;

using NI.Data.Storage.Service;
using NI.Data.Storage.Service.Actions;
using NI.Data.Storage.Service.Schema;

namespace NI.Data.Storage.Service {

	[ServiceContract]
	public interface IStorageService {
		
		[OperationContract]
		[WebInvoke(UriTemplate = "/dataschema", Method = "GET")]
		[FaultContract(typeof(ApiFault))]
		[Description("Get data schema (classes, properties)")]
		GetDataSchemaResult GetDataSchema();

		[OperationContract]
		[WebInvoke(UriTemplate = "/load/rows?q={relex}&totalcount={totalcount}", Method = "GET")]
		[FaultContract(typeof(ApiFault))]
		[Description("Load storage data as rows collection by query (relex expression)")]
		LoadRowsResult LoadRows(string relex, bool totalcount);

		[OperationContract]
		[WebInvoke(UriTemplate = "/load/values?q={relex}&totalcount={totalcount}", Method = "GET")]
		[FaultContract(typeof(ApiFault))]
		[Description("Load storage data as values collection by query (relex expression)")]
		LoadValuesResult LoadValues(string relex, bool totalcount);

		[OperationContract]
		[WebInvoke(UriTemplate = "/data/{tableName}/insert", Method = "POST")]
		long InsertRow(string tableName, DictionaryItem data);

		[OperationContract]
		[WebInvoke(UriTemplate = "/data/{tableName}/{id}/update", Method = "POST")]
		void UpdateRow(string tableName, string id, DictionaryItem data);

		[OperationContract]
		[WebInvoke(UriTemplate = "/data/{tableName}/{id}/delete", Method = "GET")]
		void DeleteRow(string tableName, string id);
	}
}
