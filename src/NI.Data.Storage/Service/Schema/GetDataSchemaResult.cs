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
using System.Threading.Tasks;

namespace NI.Data.Storage.Service.Schema {

	[DataContract(Name = "dataschema")]
	public class GetDataSchemaResult {

		[DataMember(Name = "classes")]
		public List<DataSchemaClassInfo> Classes { get; private set; }

		[DataMember(Name = "relationships")]
		public List<DataSchemaRelationshipInfo> Relationships { get; private set; }

		public GetDataSchemaResult() {
			Classes = new List<DataSchemaClassInfo>();
			Relationships = new List<DataSchemaRelationshipInfo>();
		}

	}

	[DataContract(Name = "class")]
	public class DataSchemaClassInfo {
		[DataMember(Name = "id")]
		public string ID { get; set; }
		
		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "properties")]
		public List<DataSchemaPropertyInfo> Properties { get; set; }

		public DataSchemaClassInfo() {
			Properties = new List<DataSchemaPropertyInfo>();
		}
	}

	[DataContract(Name = "property")]
	public class DataSchemaPropertyInfo {
		[DataMember(Name = "id")]
		public string ID { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "datatype")]
		public string DataTypeID { get; set; }

		[DataMember(Name="derived_from")]
		public string DerivedFromPropertyID { get; set; }
	}

	[DataContract(Name = "relationship")]
	public class DataSchemaRelationshipInfo {
		[DataMember(Name = "id")]
		public string ID { get; set; }

		[DataMember(Name = "subject_class")]
		public string SubjectClassID { get; set; }

		[DataMember(Name = "object_class")]
		public string ObjectClassID { get; set; }

		[DataMember(Name = "predicate_name")]
		public string PredicateName { get; set; }

		[DataMember(Name = "subject_multiplicity")]
		public bool SubjectMultiplicity { get; set; }

		[DataMember(Name = "object_multiplicity")]
		public bool ObjectMultiplicity { get; set; }

	}


	

}
