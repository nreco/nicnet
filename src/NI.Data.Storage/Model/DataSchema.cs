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
using System.Threading.Tasks;

namespace NI.Data.Storage.Model {
    
    public class DataSchema {
        
        public IEnumerable<Class> Classes { get; private set; }
        public IEnumerable<Property> Properties { get; private set; }
        
		public IEnumerable<Relationship> Relationships {
			get { return RelationshipList; }
		}

		IList<Relationship> RelationshipList { get; set; }

		IDictionary<string, Class> ClassById = null;
		IDictionary<string, Property> PropertyById = null;
		IDictionary<int, Class> ClassByCompactId = null;
		IDictionary<int, Property> PropertyByCompactId = null;
		IDictionary<string, List<Class>> ClassesByPropertyId = null;
		IDictionary<string, List<Property>> PropertiesByClassId = null;
		IDictionary<string, List<Relationship>> RelationshipsByClassId = null;

        public DataSchema(IEnumerable<Class> classes, IEnumerable<Property> props) {
            Classes = classes;
            Properties = props;
			BuildClassIndex();
			BuildPropertyIndex();

			ClassesByPropertyId = new Dictionary<string, List<Class>>();
			PropertiesByClassId = new Dictionary<string, List<Property>>();
			RelationshipsByClassId = new Dictionary<string, List<Relationship>>();
			RelationshipList = new List<Relationship>();
		}

		public void AddRelationship(Relationship r) {
			RelationshipList.Add(r);
			if (!RelationshipsByClassId.ContainsKey(r.Subject.ID))
				RelationshipsByClassId[r.Subject.ID] = new List<Relationship>();
			RelationshipsByClassId[r.Subject.ID].Add(r);
		}

		public void AddClassProperty(Class c, Property p) {
			if (!PropertiesByClassId.ContainsKey(c.ID))
				PropertiesByClassId[c.ID] = new List<Property>();
			PropertiesByClassId[c.ID].Add(p);

			if (!ClassesByPropertyId.ContainsKey(p.ID))
				ClassesByPropertyId[p.ID] = new List<Class>();
			ClassesByPropertyId[p.ID].Add(c);
		}

		protected void BuildClassIndex() {
			ClassById = new Dictionary<string, Class>();
			ClassByCompactId = new Dictionary<int, Class>();
			foreach (var c in Classes) {
				c.Schema = this;
				ClassById[c.ID] = c;
				ClassByCompactId[c.CompactID] = c;
			}
		}

		protected void BuildPropertyIndex() {
			PropertyById = new Dictionary<string, Property>();
			PropertyByCompactId = new Dictionary<int, Property>();
			foreach (var p in Properties) {
				p.Schema = this;
				PropertyById[p.ID] = p;
				PropertyByCompactId[p.CompactID] = p;
			}
		}

		public IEnumerable<Property> FindPropertyByClassID(string classId) {
			return PropertiesByClassId.ContainsKey(classId)?
					PropertiesByClassId[classId].AsEnumerable()
					: new Property[0];
		}

		public IEnumerable<Relationship> FindClassRelationships(string classId) {
			return RelationshipsByClassId.ContainsKey(classId) ?
					RelationshipsByClassId[classId].AsEnumerable()
					: new Relationship[0];
		}

		public IEnumerable<Class> FindPropertyClasses(string propId) {
			return ClassesByPropertyId.ContainsKey(propId) ?
					ClassesByPropertyId[propId].AsEnumerable()
					: new Class[0];
		}


		public Class FindClassByID(string id) {
			return ClassById.ContainsKey(id) ? ClassById[id] : null;
		}

		internal Class FindClassByCompactID(int compactId) {
			return ClassByCompactId.ContainsKey(compactId) ? ClassByCompactId[compactId] : null;
		}

		public Property FindPropertyByID(string id) {
			return PropertyById.ContainsKey(id) ? PropertyById[id] : null;
		}

		internal Property FindPropertyByCompactID(int id) {
			return PropertyByCompactId.ContainsKey(id) ? PropertyByCompactId[id] : null;
		}


    }
}
