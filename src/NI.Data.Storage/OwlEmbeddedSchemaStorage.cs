#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2014 Vitalii Fedorchenko
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

		public string ObjectPkColumn { get; set; }

		public OwlEmbeddedSchemaStorage(IObjectContainerStorage objStorage) {
			ObjectStorage = objStorage;
			OwlConfig = OwlConfiguration.Default;
			ObjectPkColumn = "id";
		}

		private DataSchema CachedDataSchema = null;

		public void Refresh() {
			CachedDataSchema = null;
		}

		protected void InitSuperClassSchema(DataSchema schema) {
			var superClass = new Class(OwlConfig.SuperClassID) {
				CompactID = OwlConfig.SuperClassCompactID,
				Name = OwlConfig.SuperClassID,
				ObjectLocation = ObjectLocationType.ObjectTable
			};

			schema.AddClass(superClass);

			var superIdProp = new Property(OwlConfig.SuperIdPropertyID) {
				CompactID = OwlConfig.SuperIdPropertyCompactID,
				Name = OwlConfig.SuperIdPropertyID,
				DataType = PropertyDataType.String
			};
			schema.AddProperty(superIdProp);

			schema.AddClassProperty(new ClassPropertyLocation(superClass, superIdProp) );
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
				schema.AddClassProperty(new ClassPropertyLocation(c, schema.FindPropertyByID(OwlConfig.SuperIdPropertyID)) );
			}

			var superClass = schema.FindClassByID(OwlConfig.SuperClassID);
			var domainClass = FindAndAssertMetaClass(schema, OwlConfig.DomainClassID);
			var rangeClass = FindAndAssertMetaClass(schema, OwlConfig.RangeClassID);
			var objClass = FindAndAssertMetaClass(schema, OwlConfig.ObjectClassID);
			var objPropClass = FindAndAssertMetaClass(schema, OwlConfig.ObjectPropertyClassID);
			var datatypePropClass = FindAndAssertMetaClass(schema, OwlConfig.DatatypePropertyClassID);
			var datatypeClass = FindAndAssertMetaClass(schema, OwlConfig.DatatypeClassID);
			var labelClass = FindAndAssertMetaClass(schema, OwlConfig.LabelClassID);
			var rdfTypeClass = FindAndAssertMetaClass(schema, OwlConfig.RdfTypeClassID);
			var funcPropClass = FindAndAssertMetaClass(schema, OwlConfig.FunctionalPropertyClassID);
			var invFuncPropClass = FindAndAssertMetaClass(schema, OwlConfig.InverseFunctionalPropertyClassID);
			var pkPropClass = FindAndAssertMetaClass(schema, OwlConfig.PkPropertyID);

			// object property relations
			var objPropDomainRelRev = new Relationship(objClass,domainClass,objPropClass,true,true,null);
			schema.AddRelationship(new Relationship(objPropClass,domainClass, objClass, true, false, objPropDomainRelRev) );
			
			var objPropRangeRelRev = new Relationship(objClass,rangeClass,objPropClass,true,true,null);
			schema.AddRelationship(new Relationship(objPropClass, rangeClass, objClass, true, false, objPropRangeRelRev) );

			// for superclass (TBD: subclass-of should resolve these duplicate relationships) 
			var objPropDomainForSuperRelRev = new Relationship(superClass,domainClass,objPropClass,true,true,null);
			schema.AddRelationship(new Relationship(objPropClass,domainClass, superClass, true, false, objPropDomainForSuperRelRev) );
			var objPropRangeForSuperRelRev = new Relationship(superClass,rangeClass,objPropClass,true,true,null);
			schema.AddRelationship(new Relationship(objPropClass, rangeClass, superClass, true, false, objPropRangeForSuperRelRev) );

			var objPropTypeRelRev = new Relationship(superClass,rdfTypeClass,objPropClass,true,true,null);
			schema.AddRelationship(new Relationship(objPropClass, rdfTypeClass, superClass, true, false, objPropTypeRelRev) );

			// datatype properties relations
			var dtPropDomainRelRev = new Relationship(objClass,domainClass,datatypePropClass,true,true,null);
			schema.AddRelationship(new Relationship(datatypePropClass, domainClass, objClass, true, false, dtPropDomainRelRev) );
			
			var dtPropRangeRelRev = new Relationship(objClass,rangeClass,datatypePropClass,true,true,null);
			schema.AddRelationship(new Relationship(datatypePropClass, rangeClass, datatypeClass, false, false, dtPropRangeRelRev) );

			var dtPropTypeRelRev = new Relationship(superClass,rdfTypeClass,datatypePropClass,true,true,null);
			schema.AddRelationship(new Relationship(datatypePropClass, rdfTypeClass, superClass, true, false, dtPropTypeRelRev) );

			var labelProp = new Property(labelClass.ID) {
				CompactID = labelClass.CompactID,
				Name = labelClass.ID,
				DataType = PropertyDataType.String
			};
			schema.AddProperty(labelProp);
			schema.AddClassProperty(new ClassPropertyLocation(objClass, labelProp));
			schema.AddClassProperty(new ClassPropertyLocation(datatypePropClass, labelProp));
			schema.AddClassProperty(new ClassPropertyLocation(objPropClass, labelProp));

			var pkProp = new Property(pkPropClass.ID) {
				CompactID = pkPropClass.CompactID,
				Name = pkPropClass.ID,
				DataType = PropertyDataType.Integer,
				PrimaryKey = true
			};
			schema.AddProperty(pkProp);
			schema.AddClassProperty(new ClassPropertyLocation(datatypeClass, pkProp, ObjectPkColumn));
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
					schema.AddClassProperty(new ClassPropertyLocation(c, schema.FindPropertyByID(OwlConfig.PkPropertyID), ObjectPkColumn));
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
						Multivalue = true, // by default all props are multivalue in OWL
						PrimaryKey = id==OwlConfig.PkPropertyID,
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

			var superClass = schema.FindClassByID(OwlConfig.SuperClassID);
			var datatypePropClass = schema.FindClassByID(OwlConfig.DatatypePropertyClassID);
			var datatypeClass = schema.FindClassByID(OwlConfig.DatatypeClassID);
			var domainClass = schema.FindClassByID(OwlConfig.DomainClassID);
			var rangeClass = schema.FindClassByID(OwlConfig.RangeClassID);
			var objClass = schema.FindClassByID(OwlConfig.ObjectClassID);
			var rdfTypeClass = schema.FindClassByID(OwlConfig.RdfTypeClassID);
			var funcPropClass = schema.FindClassByID(OwlConfig.FunctionalPropertyClassID);
			var invFuncPropClass = schema.FindClassByID(OwlConfig.InverseFunctionalPropertyClassID);

			// resolve datatype properties
			var dataTypePropRels = ObjectStorage.LoadRelations(allDatatypePropObjects, 
					new [] { 
						datatypePropClass.FindRelationship(rangeClass, datatypeClass ),
						datatypePropClass.FindRelationship(domainClass, objClass),
						datatypePropClass.FindRelationship(rdfTypeClass, superClass )
					} );
			var dataTypeMap = BuildDataTypeMap(datatypeIds, idToObj);
			foreach (var rangeRel in dataTypePropRels.Where( r=>r.Relation.Predicate==rangeClass) ) {
				var p = schema.FindPropertyByCompactID( rangeRel.SubjectID );
				if (dataTypeMap.ContainsKey(rangeRel.ObjectID)) { 
 					p.DataType = dataTypeMap[rangeRel.ObjectID];
				}
			}

			foreach (var funcPropRel in dataTypePropRels.Where( r=>r.Relation.Predicate==rdfTypeClass && r.ObjectID == funcPropClass.CompactID) ) {
				var p = schema.FindPropertyByCompactID( funcPropRel.SubjectID );
				p.Multivalue = false;
			}

			foreach (var domainRel in dataTypePropRels.Where( r=>r.Relation.Predicate==domainClass ) ) {
				var p = schema.FindPropertyByCompactID( domainRel.SubjectID );
				var c = schema.FindClassByCompactID( domainRel.ObjectID );
				if (c!=null && p!=null)
					AddClassProperty( schema, new ClassPropertyLocation(c,p));
			}

			// resolve object properties into relationships
			var objPropClass = schema.FindClassByID(OwlConfig.ObjectPropertyClassID);
			var objPropRangeRel = objPropClass.FindRelationship(rangeClass, objClass );
			var objPropDomainRel = objPropClass.FindRelationship(domainClass, objClass );

			var objPropRels = ObjectStorage.LoadRelations(allObjPropObjects, 
					new [] { 
						objPropClass.FindRelationship(domainClass, objClass ), 
						objPropClass.FindRelationship(rangeClass, objClass ),
						objPropClass.FindRelationship(rdfTypeClass, superClass ),
						// TBD: subclass-of to avoid these duplicate relationships
						objPropClass.FindRelationship(domainClass, superClass ), 
						objPropClass.FindRelationship(rangeClass, superClass )
					} );

			foreach (var predicateClass in schema.Classes.Where(c => c.IsPredicate)) {
				var predRels = objPropRels.Where( r => r.SubjectID == predicateClass.CompactID).ToArray();
				foreach (var domainRel in predRels.Where(r=>r.Relation.Predicate==domainClass))
					foreach (var rangeRel in predRels.Where(r => r.Relation.Predicate == rangeClass)) {
						var rSubjClass = schema.FindClassByCompactID( domainRel.ObjectID );
						var rObjClass = schema.FindClassByCompactID( rangeRel.ObjectID );

						var subjectMultiplicity = !predRels.Any( r => r.Relation.Predicate==rdfTypeClass && r.ObjectID == invFuncPropClass.CompactID );
						var objectMultiplicity = !predRels.Any( r => r.Relation.Predicate==rdfTypeClass && r.ObjectID == funcPropClass.CompactID );
						var revRelationship = new Relationship(rObjClass, predicateClass, rSubjClass, subjectMultiplicity, true, null);
						schema.AddRelationship(new Relationship(rSubjClass, predicateClass, rObjClass, objectMultiplicity, false, revRelationship));
						schema.AddRelationship(revRelationship);
					}

			}


		}

		protected virtual void AddClassProperty(DataSchema schema, ClassPropertyLocation pLoc) {
			schema.AddClassProperty(pLoc);
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

		public void CreateClass(string id, string label) {
			var schema = GetSchema();
			var owlClass = schema.FindClassByID(OwlConfig.ObjectClassID);

			var newObj = new ObjectContainer(owlClass);
			newObj[OwlConfig.SuperIdPropertyID] = id;
			newObj[OwlConfig.LabelClassID] = label;
			ObjectStorage.Insert(newObj);
			Refresh();
		}

		public void CreateObjectProperty(string id, string label, bool isFunctional = false, bool isInverseFunctional = false) {
			var schema = GetSchema();
			var objPropClass = schema.FindClassByID(OwlConfig.ObjectPropertyClassID);

			var rdfTypeClass = schema.FindClassByID(OwlConfig.RdfTypeClassID);
			var funcPropClass = schema.FindClassByID(OwlConfig.FunctionalPropertyClassID);
			var invFuncPropClass = schema.FindClassByID(OwlConfig.InverseFunctionalPropertyClassID);
			var objPropPropRdfTypeRel = objPropClass.FindRelationship(rdfTypeClass, schema.FindClassByID(OwlConfig.SuperClassID) );

			var newObj = new ObjectContainer(objPropClass);
			newObj[OwlConfig.SuperIdPropertyID] = id;
			newObj[OwlConfig.LabelClassID] = label;
			ObjectStorage.Insert(newObj);

			if (isFunctional)
				ObjectStorage.AddRelation( new ObjectRelation( newObj.ID.Value, objPropPropRdfTypeRel, funcPropClass.CompactID ) );
			if (isInverseFunctional)
				ObjectStorage.AddRelation( new ObjectRelation( newObj.ID.Value, objPropPropRdfTypeRel, invFuncPropClass.CompactID ) );

			Refresh();
		}

		public void SetObjectPropertyDomain(string id, params Class[] classes) {
			var schema = GetSchema();
			var objPropClass = schema.FindClassByID(OwlConfig.ObjectPropertyClassID);
			var domainClass = schema.FindClassByID(OwlConfig.DomainClassID);

			var objPropDomainRel = objPropClass.FindRelationship(domainClass, schema.FindClassByID(OwlConfig.ObjectClassID) );
			var objProp = schema.FindClassByID(id);
			if (objProp==null || !objProp.IsPredicate)
				throw new ArgumentException("Cannot locate object property with ID="+id);

			foreach (var c in classes)
				ObjectStorage.AddRelation( new ObjectRelation( objProp.CompactID, objPropDomainRel, c.CompactID ) );

			Refresh();
		}

		public void SetObjectPropertyRange(string id, params Class[] classes) {
			var schema = GetSchema();
			var objPropClass = schema.FindClassByID(OwlConfig.ObjectPropertyClassID);
			var rangeClass = schema.FindClassByID(OwlConfig.RangeClassID);

			var objPropRangeRel = objPropClass.FindRelationship(rangeClass, schema.FindClassByID(OwlConfig.ObjectClassID) );
			var objProp = schema.FindClassByID(id);
			if (objProp==null || !objProp.IsPredicate)
				throw new ArgumentException("Cannot locate object property with ID="+id);

			foreach (var c in classes)
				ObjectStorage.AddRelation( new ObjectRelation( objProp.CompactID, objPropRangeRel, c.CompactID ) );

			Refresh();
		}

		public void CreateDatatypeProperty(string id, string label, PropertyDataType dataType, bool isFunctional = true) {
			var schema = GetSchema();
			var datatypePropClass = schema.FindClassByID(OwlConfig.DatatypePropertyClassID);
			var rangeClass = schema.FindClassByID(OwlConfig.RangeClassID);
			var rdfTypeClass = schema.FindClassByID(OwlConfig.RdfTypeClassID);
			var funcPropClass = schema.FindClassByID(OwlConfig.FunctionalPropertyClassID);

			var datatypePropRangeRel = datatypePropClass.FindRelationship(rangeClass, schema.FindClassByID(OwlConfig.DatatypeClassID) );
			var datatypePropRdfTypeRel = datatypePropClass.FindRelationship(rdfTypeClass, schema.FindClassByID(OwlConfig.SuperClassID) );

			var dataTypeIds = ObjectStorage.GetObjectIds( new Query(OwlConfig.DatatypeClassID, (QField)OwlConfig.SuperIdPropertyID==new QConst(dataType.ID) ) );
			if (dataTypeIds.Length!=1)
				throw new ArgumentException("Cannot locate object for datatype ID="+dataType.ID);

			var dtPropObj = new ObjectContainer(datatypePropClass);
			dtPropObj[OwlConfig.SuperIdPropertyID] = id;
			dtPropObj[OwlConfig.LabelClassID] = label;
			ObjectStorage.Insert(dtPropObj);

			if (isFunctional)
				ObjectStorage.AddRelation( new ObjectRelation( dtPropObj.ID.Value, datatypePropRdfTypeRel, funcPropClass.CompactID ) );
			ObjectStorage.AddRelation( new ObjectRelation( dtPropObj.ID.Value, datatypePropRangeRel, dataTypeIds[0] ) );

			Refresh();
		}

		public void SetDatatypePropertyDomain(string id, params Class[] classes) {
			var schema = GetSchema();
			var datatypePropClass = schema.FindClassByID(OwlConfig.DatatypePropertyClassID);
			var domainClass = schema.FindClassByID(OwlConfig.DomainClassID);

			var datatypePropDomainRel = datatypePropClass.FindRelationship(domainClass, schema.FindClassByID(OwlConfig.ObjectClassID) );
			var dtProp = schema.FindPropertyByID(id);
			foreach (var c in classes)
				ObjectStorage.AddRelation( new ObjectRelation( dtProp.CompactID, datatypePropDomainRel, c.CompactID ) );

			Refresh();
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

			public string PkPropertyID { get; set; }

			public string ObjectClassID { get; set;}
			public string DatatypePropertyClassID { get; set; }
			public string ObjectPropertyClassID { get; set;}
			public string DatatypeClassID { get; set; }
			public string DomainClassID { get; set; }
			public string RangeClassID { get; set; }
			public string LabelClassID { get; set; }
			public string RdfTypeClassID { get; set; }
			public string FunctionalPropertyClassID { get; set; }
			public string InverseFunctionalPropertyClassID { get; set; }

			public OwlConfiguration() {
			}

			public static readonly OwlConfiguration Default = new OwlConfiguration() {
				SuperClassCompactID = 0,
				SuperIdPropertyCompactID = 0,
				SuperClassID = "rdfs_Class",
				SuperIdPropertyID = "rdf_ID",
				
				PkPropertyID = "id",

				ObjectClassID = "owl_Class",
				DatatypePropertyClassID = "owl_DatatypeProperty",
				ObjectPropertyClassID = "owl_ObjectProperty",
				DatatypeClassID = "rdfs_Datatype",
				DomainClassID = "rdfs_domain",
				RangeClassID = "rdfs_range",
				LabelClassID = "rdfs_label",
				RdfTypeClassID = "rdf_type",
				FunctionalPropertyClassID = "owl_FunctionalProperty",
				InverseFunctionalPropertyClassID = "owl_InverseFunctionalProperty"
			};
		}




	}

}
