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

		//TODO: rest service that reflects IObjectPersister for rest

		[OperationContract]
		[WebInvoke(UriTemplate = "/relex?q={relex}", Method = "GET")] //, ResponseFormat=WebMessageFormat.Json
		[FaultContract(typeof(ApiFault))]
		[Description("Query storage data with relex expression")]
		LoadRelexResult LoadRelex(string relex);
	}
}
