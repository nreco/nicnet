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

	/// <summary>
	/// OWL-compatible data schema persisted in IObjectContainerStorage  
	/// </summary>
	public class OwlEmbeddedSchemaStorage : IDataSchemaStorage {

		protected IObjectContainerStorage ObjectStorage { get; set; }

		public OwlConfiguration OwlConfig { get; set; }


		public OwlEmbeddedSchemaStorage(IObjectContainerStorage objStorage) {
			ObjectStorage = objStorage;
			OwlConfig = OwlConfiguration.Default;
		}

		private DataSchema CachedDataSchema = null;

		public void Refresh() {
			CachedDataSchema = null;
		}

		protected void InitSuperClassSchema(DataSchema schema) {
			var superClass = new Class(OwlConfig.SuperClassID) {
				CompactID = OwlConfig.SuperClassCompactID,
				Name = OwlConfig.SuperClassID,
				ObjectLocation = ClassObjectLocationMode.ObjectTable
			};

			schema.AddClass(superClass);

			var superIdProp = new Property(OwlConfig.SuperIdPropertyID) {
				CompactID = OwlConfig.SuperIdPropertyCompactID,
				Name = OwlConfig.SuperIdPropertyID,
				DataType = PropertyDataType.String
			};
			schema.AddProperty(superIdProp);

			schema.AddClassProperty(new ClassPropertyLocation(superClass, superIdProp, PropertyValueLocationMode.ValueTable, null) );
		}

		Class FindAndAssertMetaClass(DataSchema schema, string classId) {
			var c = schema.FindClassByID(classId);
			if (c==null)
				throw new InvalidOperationException(String.Format("Required metaclass with ID={0} is missed", classId) );
			return c;
		}

		protected void InitMetaSchema(DataSchema schema, IEnumerable<ObjectContainer> metaClassObjects) {
			var metaClasses = new List<Class>();
			foreach (var metaClassObj in metaClassObjects) {
				var id = metaClassObj[OwlConfig.SuperIdPropertyID] as string;
				if (id != null) { 
					var c = new Class(id) {
						CompactID = metaClassObj.ID.Value,
						Name = id
					};
					schema.AddClass(c);
					metaClasses.Add(c);
				}
			}

			foreach (var c in metaClasses) {
				schema.AddClassProperty(new ClassPropertyLocation(c, schema.FindPropertyByID(OwlConfig.SuperIdPropertyID), PropertyValueLocationMode.ValueTable, null) );
			}

			var domainClass = FindAndAssertMetaClass(schema, OwlConfig.DomainClassID);
			var rangeClass = FindAndAssertMetaClass(schema, OwlConfig.RangeClassID);
			var objClass = FindAndAssertMetaClass(schema, OwlConfig.ObjectClassID);
			var objPropClass = FindAndAssertMetaClass(schema, OwlConfig.ObjectPropertyClassID);
			var datatypePropClass = FindAndAssertMetaClass(schema, OwlConfig.DatatypePropertyClassID);
			var datatypeClass = FindAndAssertMetaClass(schema, OwlConfig.DatatypeClassID);
			var labelClass = FindAndAssertMetaClass(schema, OwlConfig.LabelClassID);

			schema.AddRelationship(new Relationship(objPropClass,domainClass, objClass, true, false, null) );
			schema.AddRelationship(new Relationship(objPropClass, rangeClass, objClass, true, false, null) );

			schema.AddRelationship(new Relationship(datatypePropClass, domainClass, objClass, true, false, null) );
			schema.AddRelationship(new Relationship(datatypePropClass, rangeClass, datatypeClass, false, false, null) );

			var labelProp = new Property(labelClass.ID) {
				CompactID = labelClass.CompactID,
				Name = labelClass.ID,
				DataType = PropertyDataType.String
			};
			schema.AddProperty(labelProp);
			schema.AddClassProperty(new ClassPropertyLocation(objClass, labelProp, PropertyValueLocationMode.ValueTable, null));
			schema.AddClassProperty(new ClassPropertyLocation(datatypePropClass, labelProp, PropertyValueLocationMode.ValueTable, null));
			schema.AddClassProperty(new ClassPropertyLocation(objPropClass, labelProp, PropertyValueLocationMode.ValueTable, null));
		}

		protected void InitDataSchema(DataSchema schema) {
			var objectClassIds = ObjectStorage.GetObjectIds(new Query(OwlConfig.ObjectClassID));
			var datatypePropClassIds = ObjectStorage.GetObjectIds(new Query(OwlConfig.DatatypePropertyClassID));
			var objPropClassIds = ObjectStorage.GetObjectIds(new Query(OwlConfig.ObjectPropertyClassID));
			var datatypeIds = ObjectStorage.GetObjectIds(new Query(OwlConfig.DatatypeClassID));
				
			var allIds = new List<long>();
			allIds.AddRange(objectClassIds);
			allIds.AddRange(datatypePropClassIds);
			allIds.AddRange(objPropClassIds);
			allIds.AddRange(datatypeIds);

			var idToObj = ObjectStorage.Load(allIds.ToArray());
			
			foreach (var metaClassObj in objectClassIds.Select( o => idToObj[o] ) ) {
				var id = metaClassObj[OwlConfig.SuperIdPropertyID] as string;
				if (id != null) { 
					var name = (metaClassObj[OwlConfig.LabelClassID] as string) ?? id;
					var c = new Class(id) {
						CompactID = metaClassObj.ID.Value,
						Name = name
					};
					schema.AddClass(c);
				}
			}

			var allDatatypePropObjects = datatypePropClassIds.Select( o => idToObj[o ] ).ToArray();
			foreach (var metaPropObj in allDatatypePropObjects) {
				var id = metaPropObj[OwlConfig.SuperIdPropertyID] as string;
				if (id != null) {
					var name = (metaPropObj[OwlConfig.LabelClassID] as string) ?? id;
					schema.AddProperty( new Property(id) {
						CompactID = metaPropObj.ID.Value,
						Name = name,
						DataType = PropertyDataType.String // default type used for properties without explicit range definition
					});
				}
			}

			var allObjPropObjects = objPropClassIds.Select( o => idToObj[o ] ).ToArray();
			foreach (var obj in allObjPropObjects ) {
				var id = obj[OwlConfig.SuperIdPropertyID] as string;
				if (id != null) { 
					var name = (obj[OwlConfig.LabelClassID] as string) ?? id;
					var c = new Class(id) {
						CompactID = obj.ID.Value,
						Name = name,
						IsPredicate = true
					};
					schema.AddClass(c);
				}
			}

			// resolve datatype properties
			var datatypePropClass = schema.FindClassByID(OwlConfig.DatatypePropertyClassID);
			var domainClass = schema.FindClassByID(OwlConfig.DomainClassID);
			var rangeClass = schema.FindClassByID(OwlConfig.RangeClassID);
			var objClass = schema.FindClassByID(OwlConfig.ObjectClassID);

			var dataTypePropertyRanges = ObjectStorage.LoadRelations(allDatatypePropObjects, 
					new [] { datatypePropClass.FindRelationship( 
								rangeClass, objClass ) } );
			var dataTypeMap = BuildDataTypeMap(datatypeIds, idToObj);
			foreach (var rangeRel in dataTypePropertyRanges) {
				var p = schema.FindPropertyByCompactID( rangeRel.SubjectID );
				if (dataTypeMap.ContainsKey(rangeRel.ObjectID)) { 
 					p.DataType = dataTypeMap[rangeRel.ObjectID];
				}
			}

			// resolve property domains
			var dataTypePropertyDomains = ObjectStorage.LoadRelations(allDatatypePropObjects, 
					new [] { datatypePropClass.FindRelationship( 
								domainClass, objClass ) } );
			foreach (var domainRel in dataTypePropertyDomains) {
				var p = schema.FindPropertyByCompactID( domainRel.SubjectID );
				var c = schema.FindClassByCompactID( domainRel.ObjectID );
				if (c!=null && p!=null)
					schema.AddClassProperty(new ClassPropertyLocation(c,p,PropertyValueLocationMode.ValueTable, null));
			}

			// resolve object properties into relationships
			var objPropClass = schema.FindClassByID(OwlConfig.ObjectPropertyClassID);
			var objPropRangeRel = objPropClass.FindRelationship(rangeClass, objClass );
			var objPropDomainRel = objPropClass.FindRelationship(domainClass, objClass );

			var objPropRels = ObjectStorage.LoadRelations(allObjPropObjects, 
					new [] { objPropClass.FindRelationship(domainClass, objClass ), objPropClass.FindRelationship(rangeClass, objClass ) } );

			foreach (var predicateClass in schema.Classes.Where(c => c.IsPredicate)) {
				var predRels = objPropRels.Where( r => r.SubjectID == predicateClass.CompactID).ToArray();
				foreach (var domainRel in predRels.Where(r=>r.Relation.Predicate==domainClass))
					foreach (var rangeRel in predRels.Where(r => r.Relation.Predicate == rangeClass)) {
						var rSubjClass = schema.FindClassByCompactID( domainRel.ObjectID );
						var rObjClass = schema.FindClassByCompactID( rangeRel.ObjectID );

						//TODO: handle cardinality
						var subjectMultiplicity = true;
						var objectMultiplicity = true;
						var revRelationship = new Relationship(rObjClass, predicateClass, rSubjClass, subjectMultiplicity, true, null);
						schema.AddRelationship(new Relationship(rSubjClass, predicateClass, rObjClass, objectMultiplicity, false, revRelationship));
						schema.AddRelationship(revRelationship);
					}

			}


		}

		public DataSchema GetSchema() {
			if (CachedDataSchema!=null)
				return CachedDataSchema;

			lock (this) {
				if (CachedDataSchema!=null)
					return CachedDataSchema;

				var schema = new DataSchema();
				InitSuperClassSchema(schema);
				CachedDataSchema = schema;

				// load all OWL classes
				var owlClassIds = ObjectStorage.GetObjectIds(new Query(OwlConfig.SuperClassID));
				var owlClassMap = ObjectStorage.Load(owlClassIds);
				InitMetaSchema(schema, owlClassMap.Values );

				// load data schema described in OWL terms
				InitDataSchema(schema);
			}

			return CachedDataSchema;
		}

		private IDictionary<long, PropertyDataType> BuildDataTypeMap(long[] dataTypeIds, IDictionary<long, ObjectContainer> idToObj) {
			var r = new Dictionary<long,PropertyDataType>();
			foreach (var dataTypeId in dataTypeIds) {
				if (!idToObj.ContainsKey(dataTypeId))
					throw new Exception("Unknown datatype object ID="+dataTypeId.ToString());
				var dataTypeObj = idToObj[dataTypeId];
				var dataType = dataTypeObj[OwlConfig.SuperIdPropertyID] as string;
				r[dataTypeId] = PropertyDataType.FindByID(dataType);
			}
			return r;
		}


		public class OwlConfiguration {

			public long SuperClassCompactID { get; set; }
			public long SuperIdPropertyCompactID { get; set; }

			public string SuperClassID { get; set;}
			public string SuperIdPropertyID { get; set; }

			public string ObjectClassID { get; set;}
			public string DatatypePropertyClassID { get; set; }
			public string ObjectPropertyClassID { get; set;}
			public string DatatypeClassID { get; set; }
			public string DomainClassID { get; set; }
			public string RangeClassID { get; set; }
			public string LabelClassID { get; set; }

			public OwlConfiguration() {
			}

			public static readonly OwlConfiguration Default = new OwlConfiguration() {
				SuperClassCompactID = 0,
				SuperIdPropertyCompactID = 0,
				SuperClassID = "rdfs_Class",
				SuperIdPropertyID = "rdf_about",
				
				ObjectClassID = "owl_Class",
				DatatypePropertyClassID = "owl_DatatypeProperty",
				ObjectPropertyClassID = "owl_ObjectProperty",
				DatatypeClassID = "rdfs_Datatype",
				DomainClassID = "rdfs_domain",
				RangeClassID = "rdfs_range",
				LabelClassID = "rdfs_label"
			};
		}




	}

}
