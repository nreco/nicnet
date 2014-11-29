using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

using NUnit.Framework;
using NI.Data;

using NI.Data.Storage.Model;

namespace NI.Data.Storage.Tests {
	
	[TestFixture]
	public class OwlEmbeddedSchemaStorageTests {
		
		SQLiteStorageContext StorageContext;
		OwlEmbeddedSchemaStorage OwlSchemaStorage;

		[SetUp]
		public void SetUp() {
			StorageContext = new SQLiteStorageContext( (StorageDbMgr, ObjectContainerStorage) => {
				OwlSchemaStorage = new OwlEmbeddedSchemaStorage(ObjectContainerStorage);
				return OwlSchemaStorage;
			});

			Logger.SetInfo((t, msg) => {
				Console.WriteLine("[{0}] {1}", t, msg);
			});
		}

		[TearDown]
		public void CleanUp() {
			StorageContext.Destroy();
		}

		void addOwlSchema(IDictionary<string,long> dataTypeMap) {
			var owlClassInstances = new string[] {
				OwlSchemaStorage.OwlConfig.ObjectClassID,
				OwlSchemaStorage.OwlConfig.ObjectPropertyClassID,
				OwlSchemaStorage.OwlConfig.DatatypePropertyClassID,
				OwlSchemaStorage.OwlConfig.DatatypeClassID,
				OwlSchemaStorage.OwlConfig.DomainClassID,
				OwlSchemaStorage.OwlConfig.RangeClassID,
				OwlSchemaStorage.OwlConfig.LabelClassID,
				OwlSchemaStorage.OwlConfig.RdfTypeClassID,
				OwlSchemaStorage.OwlConfig.FunctionalPropertyClassID,
				OwlSchemaStorage.OwlConfig.InverseFunctionalPropertyClassID
			};
			var owlClassIdToCompactId = new Dictionary<string,long>();
			foreach (var owlClassInstanceId in owlClassInstances) {
				var objClassRow = StorageContext.StorageDbMgr.Insert("objects", new Dictionary<string,object>() {
					{"compact_class_id", OwlSchemaStorage.OwlConfig.SuperClassCompactID}
				});
				StorageContext.StorageDbMgr.Insert("object_string_values", new Dictionary<string,object>() {
					{"object_id", objClassRow["id"]},
					{"property_compact_id", OwlSchemaStorage.OwlConfig.SuperIdPropertyCompactID },
					{"value", owlClassInstanceId}
				});
				owlClassIdToCompactId[owlClassInstanceId] = Convert.ToInt64( objClassRow["id"] );
			}

			foreach (var dataType in PropertyDataType.KnownDataTypes) {
				var objClassRow = StorageContext.StorageDbMgr.Insert("objects", new Dictionary<string,object>() {
					{"compact_class_id", owlClassIdToCompactId[OwlSchemaStorage.OwlConfig.DatatypeClassID]}
				});
				StorageContext.StorageDbMgr.Insert("object_string_values", new Dictionary<string,object>() {
					{"object_id", objClassRow["id"]},
					{"property_compact_id", OwlSchemaStorage.OwlConfig.SuperIdPropertyCompactID },
					{"value", dataType.ID}
				});
				dataTypeMap[dataType.ID] = Convert.ToInt64(objClassRow["id"]);
			}

		}

		void addTestDataSchema(DataSchema owlSchema, IDictionary<string,long> dataTypeMap) {
			var objStorage = StorageContext.ObjectContainerStorage;

			var owlClass = owlSchema.FindClassByID(OwlSchemaStorage.OwlConfig.ObjectClassID);
			var datatypePropClass = owlSchema.FindClassByID(OwlSchemaStorage.OwlConfig.DatatypePropertyClassID);
			var objPropClass = owlSchema.FindClassByID(OwlSchemaStorage.OwlConfig.ObjectPropertyClassID);
			var rangeClass = owlSchema.FindClassByID(OwlSchemaStorage.OwlConfig.RangeClassID);
			var domainClass = owlSchema.FindClassByID(OwlSchemaStorage.OwlConfig.DomainClassID);
			var rdfTypeClass = owlSchema.FindClassByID(OwlSchemaStorage.OwlConfig.RdfTypeClassID);
			var funcPropClass = owlSchema.FindClassByID(OwlSchemaStorage.OwlConfig.FunctionalPropertyClassID);
			var invFuncPropClass = owlSchema.FindClassByID(OwlSchemaStorage.OwlConfig.InverseFunctionalPropertyClassID);
			
			var datatypePropRangeRel = datatypePropClass.FindRelationship(rangeClass, owlSchema.FindClassByID(OwlSchemaStorage.OwlConfig.DatatypeClassID) );
			var datatypePropDomainRel = datatypePropClass.FindRelationship(domainClass, owlSchema.FindClassByID(OwlSchemaStorage.OwlConfig.ObjectClassID) );
			var datatypePropRdfTypeRel = datatypePropClass.FindRelationship(rdfTypeClass, owlSchema.FindClassByID(OwlSchemaStorage.OwlConfig.SuperClassID) );

			var objPropRangeRel = objPropClass.FindRelationship(rangeClass, owlSchema.FindClassByID(OwlSchemaStorage.OwlConfig.ObjectClassID) );
			var objPropDomainRel = objPropClass.FindRelationship(domainClass, owlSchema.FindClassByID(OwlSchemaStorage.OwlConfig.ObjectClassID) );
			var objPropPropRdfTypeRel = objPropClass.FindRelationship(rdfTypeClass, owlSchema.FindClassByID(OwlSchemaStorage.OwlConfig.SuperClassID) );

			var cityObj = new ObjectContainer(owlClass);
			cityObj[OwlSchemaStorage.OwlConfig.SuperIdPropertyID] = "cities";
			cityObj[OwlSchemaStorage.OwlConfig.LabelClassID] = "City";
			objStorage.Insert(cityObj);

			var cityTitleObj = new ObjectContainer(datatypePropClass);
			cityTitleObj[OwlSchemaStorage.OwlConfig.SuperIdPropertyID] = "title";
			cityTitleObj[OwlSchemaStorage.OwlConfig.LabelClassID] = "Title";
			objStorage.Insert(cityTitleObj);

			objStorage.AddRelations( new ObjectRelation( cityTitleObj.ID.Value, datatypePropRangeRel, dataTypeMap[PropertyDataType.String.ID] ) );
			objStorage.AddRelations( new ObjectRelation( cityTitleObj.ID.Value, datatypePropRdfTypeRel, funcPropClass.CompactID ) );
			objStorage.AddRelations( new ObjectRelation( cityTitleObj.ID.Value, datatypePropDomainRel, cityObj.ID.Value ) );

			var personObj = new ObjectContainer(owlClass);
			personObj[OwlSchemaStorage.OwlConfig.SuperIdPropertyID] = "persons";
			personObj[OwlSchemaStorage.OwlConfig.LabelClassID] = "Person";
			objStorage.Insert(personObj);

			var nameObj = new ObjectContainer(datatypePropClass);
			nameObj[OwlSchemaStorage.OwlConfig.SuperIdPropertyID] = "name";
			nameObj[OwlSchemaStorage.OwlConfig.LabelClassID] = "Name";
			objStorage.Insert(nameObj);

			objStorage.AddRelations( new ObjectRelation( nameObj.ID.Value, datatypePropRdfTypeRel, funcPropClass.CompactID ) );

			var birthdayObj = new ObjectContainer(datatypePropClass);
			birthdayObj[OwlSchemaStorage.OwlConfig.SuperIdPropertyID] = "birthday";
			birthdayObj[OwlSchemaStorage.OwlConfig.LabelClassID] = "Birthday";
			objStorage.Insert(birthdayObj);

			objStorage.AddRelations( new ObjectRelation( nameObj.ID.Value, datatypePropRangeRel, dataTypeMap[PropertyDataType.String.ID] ) );
			objStorage.AddRelations( new ObjectRelation( birthdayObj.ID.Value, datatypePropRangeRel, dataTypeMap[PropertyDataType.Date.ID] ) );

			objStorage.AddRelations( new ObjectRelation( nameObj.ID.Value, datatypePropDomainRel, personObj.ID.Value ) );
			objStorage.AddRelations( new ObjectRelation( birthdayObj.ID.Value, datatypePropDomainRel, personObj.ID.Value ) );

			//object property
			var cityOfObj = new ObjectContainer(objPropClass);
			cityOfObj[OwlSchemaStorage.OwlConfig.SuperIdPropertyID] = "cityOf";
			cityOfObj[OwlSchemaStorage.OwlConfig.LabelClassID] = "City";
			objStorage.Insert(cityOfObj);
			objStorage.AddRelations( new ObjectRelation( cityOfObj.ID.Value, objPropDomainRel, cityObj.ID.Value ) );
			objStorage.AddRelations( new ObjectRelation( cityOfObj.ID.Value, objPropRangeRel, personObj.ID.Value ) );
			objStorage.AddRelations( new ObjectRelation( cityOfObj.ID.Value, objPropPropRdfTypeRel, invFuncPropClass.CompactID ) );

		}

		[Test]
		public void OwlClassesCheck() {
			var datatypeMap = new Dictionary<string,long>();
			addOwlSchema(datatypeMap);

			var schema = OwlSchemaStorage.GetSchema();
			Assert.AreEqual(11, schema.Classes.Count() );
		}

		[Test]
		public void DataOntologyCheck() {
			var datatypeMap = new Dictionary<string,long>();
			addOwlSchema(datatypeMap);
			var schema = OwlSchemaStorage.GetSchema();

			addTestDataSchema(schema, datatypeMap);
			OwlSchemaStorage.Refresh();

			schema = OwlSchemaStorage.GetSchema();
			Assert.NotNull( schema.FindClassByID("persons") );
			Assert.IsTrue( schema.FindClassByID("cityOf").IsPredicate );

			Assert.AreEqual("Person", schema.FindClassByID("persons").Name );
			Assert.AreEqual(2, schema.FindClassByID("persons").Properties.Count() );
			Assert.AreEqual(1, schema.FindClassByID("persons").Relationships.Count() );

			Assert.IsFalse( schema.FindPropertyByID("name").Multivalue );

			Assert.AreEqual(1, schema.FindClassByID("persons").Relationships.Count() );
			var personToCityRel = schema.FindClassByID("persons").FindRelationship(
					schema.FindClassByID("cityOf"), schema.FindClassByID("cities") );
			
			Assert.IsTrue( personToCityRel.Reversed);
			Assert.IsFalse( personToCityRel.Multiplicity);
			Assert.IsTrue( personToCityRel.ReversedRelationship.Multiplicity);
		}


	}
}
