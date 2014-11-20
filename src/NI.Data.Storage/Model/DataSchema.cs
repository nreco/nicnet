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

		List<Class> ClassesList { get; set; }
        public IEnumerable<Class> Classes { get { return ClassesList.AsReadOnly(); } }

		List<Property> PropertiesList { get; set; }
		public IEnumerable<Property> Properties { get { return PropertiesList.AsReadOnly(); } }
        
		public IEnumerable<Relationship> Relationships {
			get { return RelationshipList; }
		}

		IList<Relationship> RelationshipList { get; set; }

		IDictionary<string, Class> ClassById = null;
		IDictionary<string, Property> PropertyById = null;
		IDictionary<long, Class> ClassByCompactId = null;
		IDictionary<long, Property> PropertyByCompactId = null;
		IDictionary<string, List<Class>> ClassesByPropertyId = null;
		IDictionary<string, List<Property>> PropertiesByClassId = null;
		IDictionary<string, IDictionary<string, ClassPropertyLocation>> ValueLocationByPropertyClass = null;
		IDictionary<string, List<Relationship>> RelationshipsByClassId = null;
		IDictionary<string, Relationship> RelationshipById = null;

		public DataSchema() : this(null,null) {
		}

        public DataSchema(IEnumerable<Class> classes, IEnumerable<Property> props) {
            ClassesList = new List<Class>();
			ClassById = new Dictionary<string, Class>();
			ClassByCompactId = new Dictionary<long, Class>();
			if (classes!=null)
				foreach (var c in classes) {
					AddClass(c);
				}

            PropertiesList = new List<Property>();
			PropertyById = new Dictionary<string, Property>();
			PropertyByCompactId = new Dictionary<long, Property>();
			if (props!=null)
				foreach (var p in props) {
					AddProperty(p);
				}

			ClassesByPropertyId = new Dictionary<string, List<Class>>();
			PropertiesByClassId = new Dictionary<string, List<Property>>();
			RelationshipsByClassId = new Dictionary<string, List<Relationship>>();
			RelationshipList = new List<Relationship>();
			RelationshipById = new Dictionary<string,Relationship>();
			ValueLocationByPropertyClass = new Dictionary<string, IDictionary<string, ClassPropertyLocation>>();
		}

		public void AddRelationship(Relationship r) {
			RelationshipList.Add(r);
			if (!RelationshipsByClassId.ContainsKey(r.Subject.ID))
				RelationshipsByClassId[r.Subject.ID] = new List<Relationship>();
			RelationshipsByClassId[r.Subject.ID].Add(r);
			if (r.ID!=null)
				RelationshipById[r.ID] = r;
		}

		public void AddClassProperty(ClassPropertyLocation classProp) {
			if (!PropertiesByClassId.ContainsKey(classProp.Class.ID))
				PropertiesByClassId[classProp.Class.ID] = new List<Property>();
			PropertiesByClassId[classProp.Class.ID].Add(classProp.Property);

			if (!ClassesByPropertyId.ContainsKey(classProp.Property.ID))
				ClassesByPropertyId[classProp.Property.ID] = new List<Class>();
			ClassesByPropertyId[classProp.Property.ID].Add(classProp.Class);

			if (!ValueLocationByPropertyClass.ContainsKey(classProp.Property.ID))
				ValueLocationByPropertyClass[classProp.Property.ID] = new Dictionary<string,ClassPropertyLocation>();
			ValueLocationByPropertyClass[ classProp.Property.ID ][ classProp.Class.ID ] = classProp;
		}

		public void AddClass(Class c) {
			if (ClassById.ContainsKey(c.ID))
				throw new InvalidOperationException(String.Format("Class with ID={0} already exists",c.ID));
			if (ClassByCompactId.ContainsKey(c.CompactID)) 
				throw new InvalidOperationException(String.Format("Class with CompactID={0} already exists",c.ID));
			
			ClassById[c.ID] = c;
			ClassByCompactId[c.CompactID] = c;

			ClassesList.Add(c);
			c.Schema = this;
		}


		public void AddProperty(Property p) {
			if (PropertyById.ContainsKey(p.ID))
				throw new InvalidOperationException(String.Format("Property with ID={0} already exists",p.ID));
			if (PropertyByCompactId.ContainsKey(p.CompactID)) 
				throw new InvalidOperationException(String.Format("Property with CompactID={0} already exists",p.ID));

			PropertyById[p.ID] = p;
			PropertyByCompactId[p.CompactID] = p;

			PropertiesList.Add(p);
			p.Schema = this;
		}

		public IEnumerable<Property> FindPropertyByClassID(string classId) {
			return PropertiesByClassId.ContainsKey(classId)?
					PropertiesByClassId[classId].AsEnumerable()
					: new Property[0];
		}

		public ClassPropertyLocation FindClassPropertyLocation(string classId, string propertyId) {
			if (ValueLocationByPropertyClass.ContainsKey(propertyId)) {
				var d = ValueLocationByPropertyClass[propertyId];
				if (d.ContainsKey(classId)) {
					return d[classId];
				}
			}
			return null;
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

		internal Class FindClassByCompactID(long compactId) {
			return ClassByCompactId.ContainsKey(compactId) ? ClassByCompactId[compactId] : null;
		}

		public Property FindPropertyByID(string id) {
			return PropertyById.ContainsKey(id) ? PropertyById[id] : null;
		}

		internal Property FindPropertyByCompactID(long id) {
			return PropertyByCompactId.ContainsKey(id) ? PropertyByCompactId[id] : null;
		}

		public Relationship FindRelationshipByID(string id) {
			return RelationshipById.ContainsKey(id) ? RelationshipById[id] : null;
		}

		public Relationship InferRelationshipByID(string id, Class subjClass) {
			var nextSubj = subjClass;
			var relIds = id.Split('.');
			if (relIds.Length>1) {
				var rels = new Relationship[relIds.Length];
				for (int i=0; i<rels.Length;i++) {
					var r = FindRelationshipByID(relIds[i]);
					if (r==null)
						return null;
					if (r.Subject!=nextSubj && r.Object==nextSubj) {
						r = nextSubj.FindRelationship(r.Predicate, r.Subject, true);
					}
					if (r==null || r.Subject!=nextSubj)
						return null;
					nextSubj = r.Object;
					rels[i] = r;
				}
				return new Relationship( rels[0].Subject, rels, rels[rels.Length-1].Object );
			}
			return null;
		}


    }
}
