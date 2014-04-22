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
using System.Data;
using System.Threading.Tasks;

using NI.Data;
using NI.Data.Storage.Model;

namespace NI.Data.Storage {

	public class DataSchemaDalcStorage : IDataSchemaStorage {

		protected DataRowDalcMapper DbContext { get; set; }

		protected ObjectDalcMapper<Class> ClassPersister { get; set; }
		protected ObjectDalcMapper<Property> PropertyPersister { get; set; }
		protected ObjectDalcMapper<RelationshipData> RelationshipPersister { get; set; }
		protected ObjectDalcMapper<PropertyToClass> PropertyToClassPersister { get; set; }

		public string ClassTableName { get; set; }
		public IDictionary<string, string> ClassFieldMapping { get; private set; }

		public string PropertyTableName { get; set; }
		public IDictionary<string, string> PropertyFieldMapping { get; private set; }

		public string RelationshipTableName { get; set; }
		public IDictionary<string, string> RelationshipFieldMapping { get; private set; }

		public string PropertyToClassTableName { get; set; }
		public IDictionary<string, string> PropertyToClassFieldMapping { get; private set; }

		public DataSchemaDalcStorage(DataRowDalcMapper dbMgr) {
			DbContext = dbMgr;

			ClassTableName = "metadata_classes";
			ClassFieldMapping = new Dictionary<string, string>() {
				{"id", "ID"},
				{"name", "Name"},
				{"hidden", "Hidden"},
				{"indexable", "Indexable"},
				{"predefined", "Predefined"},
				{"predicate", "IsPredicate"},
				{"compact_id", "CompactID"},
				{"object_location", "ObjectLocation"}
			};
			ClassPersister = new ObjectDalcMapper<Class>(DbContext, ClassTableName, ClassFieldMapping);

			PropertyTableName = "metadata_properties";
			PropertyFieldMapping = new Dictionary<string, string>() {
				{"id", "ID"},
				{"name", "Name"},
				{"predefined", "Predefined"},
				{"hidden", "Hidden"},
				{"indexable", "Indexable"},
				{"multivalue", "Multivalue"},
				{"compact_id", "CompactID"},
				{"value_location","ValueLocation"},
				{"primary_key","PrimaryKey"}
			};
			PropertyPersister = new ObjectDalcMapper<Property>(DbContext, PropertyTableName,
				new PropertyMapper(PropertyFieldMapping) );

			RelationshipTableName = "metadata_class_relationships";
			RelationshipFieldMapping = new Dictionary<string, string>() {
				{"subject_class_id", "SubjectClassID"},
				{"predicate_class_id", "PredicateClassID"},
				{"object_class_id", "ObjectClassID"},
				{"subject_multiplicity", "SubjectMultiplicity"},
				{"object_multiplicity", "ObjectMultiplicity"}
			};
			RelationshipPersister = new ObjectDalcMapper<RelationshipData>(DbContext, RelationshipTableName, RelationshipFieldMapping);

			PropertyToClassTableName = "metadata_property_to_class";
			PropertyToClassFieldMapping = new Dictionary<string, string>() {
				{"class_id", "ClassID"},
				{"property_id", "PropertyID"}
			};
			PropertyToClassPersister = new ObjectDalcMapper<PropertyToClass>(DbContext, PropertyToClassTableName, PropertyToClassFieldMapping);
		}

		protected DataSchema CachedDataSchema = null;

		public DataSchema GetSchema() {
			if (CachedDataSchema!=null)
				return CachedDataSchema; // tmp for tests

			var classes = ClassPersister.LoadAll(new Query(ClassTableName) );
			var props = PropertyPersister.LoadAll(new Query(PropertyTableName) );

			var relData = RelationshipPersister.LoadAll(new Query(RelationshipTableName));
			var propToClass = PropertyToClassPersister.LoadAll(new Query(PropertyToClassTableName));
			
			var dataSchema = new DataSchema(classes, props);

			foreach (var p2c in propToClass) {
				var c = dataSchema.FindClassByID(p2c.ClassID);
				var p = dataSchema.FindPropertyByID(p2c.PropertyID);
				if (c != null && p != null)
					dataSchema.AddClassProperty(c, p);
			}

			foreach (var r in relData) {
				var subjClass = dataSchema.FindClassByID(r.SubjectClassID);
				var objClass = dataSchema.FindClassByID(r.ObjectClassID);
				var predClass = dataSchema.FindClassByID(r.PredicateClassID);
				if (subjClass != null && objClass != null && predClass != null) {
					dataSchema.AddRelationship(new Relationship(subjClass, predClass, objClass, r.ObjectMultiplicity, false));
					dataSchema.AddRelationship(new Relationship(objClass, predClass, subjClass, r.SubjectMultiplicity, true));
				}
			}

			CachedDataSchema = dataSchema;
			return dataSchema;
		}

		protected class PropertyToClass {
			public string ClassID { get; set; }
			public string PropertyID { get; set; }
		}

		protected class RelationshipData {
			public string ID { get; set; }
			public string SubjectClassID { get; set; }
			public string PredicateClassID { get; set; }
			public string ObjectClassID { get; set; }
			public bool SubjectMultiplicity { get; set; }
			public bool ObjectMultiplicity { get; set; }
		}

		protected class PropertyMapper : PropertyDataRowMapper {
			public PropertyMapper(IDictionary<string,string> fieldToProperty) : base(fieldToProperty) {
			}
			public override void MapTo(DataRow r, object o) {
				base.MapTo(r, o);
				if (r.Table.Columns.Contains("datatype") && !r.IsNull("datatype") ) {
					var dt = PropertyDataType.FindByID( Convert.ToString(r["datatype"]) );
					if (o is Property)
						((Property)o).DataType = dt;
				}
			}
		}

	}

}
