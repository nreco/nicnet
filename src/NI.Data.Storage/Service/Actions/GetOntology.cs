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
using System.ServiceModel;

using NI.Data.Storage.Model;

namespace NI.Data.Storage.Service.Actions {
	
	public class GetOntology {
		
		Ontology Ontology;

		public GetOntology(Ontology ontology) {
			Ontology = ontology;
		}

		public GetOntologyResult Execute() {
			var r = new GetOntologyResult();

			foreach (var c in Ontology.Classes) {
				var cInfo = new OntologyClassInfo() {
					ID = c.ID,
					Name = c.Name,
					IsPredicate = c.IsPredicate
				};
				foreach (var cProp in c.Properties) {
					cInfo.Properties.Add( new OntologyClassPropertyInfo() { ID = cProp.ID } );
				}
				r.Classes.Add(cInfo);
			}

			return r;
		}

	}

	[DataContract(Name="ontology")]
	public class GetOntologyResult {
		
		[DataMember(Name="classes")]
		public List<OntologyClassInfo> Classes { get; private set; }

		public GetOntologyResult() {
			Classes = new List<OntologyClassInfo>();
		}



	}

	[DataContract(Name="class")]
	public class OntologyClassInfo {
		[DataMember(Name = "id")]
		public string ID { get; set; }
		[DataMember(Name = "name")]
		public string Name { get; set; }
		[DataMember(Name = "is_predicate")]
		public bool IsPredicate { get; set; }

		[DataMember(Name = "properties")]
		public List<OntologyClassPropertyInfo> Properties { get; set; }

		public OntologyClassInfo() {
			Properties = new List<OntologyClassPropertyInfo>();
		}
	}

	[DataContract(Name="property")]
	public class OntologyClassPropertyInfo {
		[DataMember(Name="id")]
		public string ID { get; set; }
	}

}
