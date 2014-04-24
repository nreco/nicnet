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
using System.Threading.Tasks;

namespace NI.Data.Storage.Service.Schema {

	[DataContract(Name = "dataschema")]
	public class GetDataSchemaResult {

		[DataMember(Name = "classes")]
		public List<DataSchemaClassInfo> Classes { get; private set; }

		public GetDataSchemaResult() {
			Classes = new List<DataSchemaClassInfo>();
		}

	}

	[DataContract(Name = "class")]
	public class DataSchemaClassInfo {
		[DataMember(Name = "id")]
		public string ID { get; set; }
		[DataMember(Name = "name")]
		public string Name { get; set; }
		[DataMember(Name = "is_predicate")]
		public bool IsPredicate { get; set; }

		[DataMember(Name = "properties")]
		public List<DataSchemaClassPropertyInfo> Properties { get; set; }

		public DataSchemaClassInfo() {
			Properties = new List<DataSchemaClassPropertyInfo>();
		}
	}

	[DataContract(Name = "property")]
	public class DataSchemaClassPropertyInfo {
		[DataMember(Name = "id")]
		public string ID { get; set; }
	}
	

}
