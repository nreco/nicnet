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
	public class SQLiteStorageDalcTests {
		
		SQLiteStorageContext StorageContext;

		[SetUp]
		public void SetUp() {
			StorageContext = new SQLiteStorageContext();
			StorageContext.CreateTestDataSchema();

			Logger.SetInfo((t, msg) => {
				Console.WriteLine("[{0}] {1}", t, msg);
			});
		}

		[TearDown]
		public void CleanUp() {
			StorageContext.Destroy();
		}

		protected void addTestData() {
			var testSchema = StorageContext.DataSchemaStorage.GetSchema();

			var googCompany = new ObjectContainer(testSchema.FindClassByID("companies"));
			googCompany["name"] = "Google";
			
			var msCompany = new ObjectContainer(testSchema.FindClassByID("companies"));
			msCompany["name"] = "Microsoft";
			
			StorageContext.ObjectContainerStorage.Insert(googCompany);
			StorageContext.ObjectContainerStorage.Insert(msCompany);

			var johnContact = new ObjectContainer(testSchema.FindClassByID("contacts"));
			johnContact["name"] = "John";
			johnContact["is_primary"] = true;
			var maryContact = new ObjectContainer(testSchema.FindClassByID("contacts"));
			maryContact["name"] = "Mary";
			maryContact["is_primary"] = false;
			maryContact["birthday"] = new DateTime(1999, 5, 20);
			var bobContact = new ObjectContainer(testSchema.FindClassByID("contacts"));
			bobContact["name"] = "Bob";
			bobContact["is_primary"] = true;

			StorageContext.ObjectContainerStorage.Insert(johnContact);
			StorageContext.ObjectContainerStorage.Insert(maryContact);
			StorageContext.ObjectContainerStorage.Insert(bobContact);

			var usaCountry = new ObjectContainer(testSchema.FindClassByID("countries"));
			usaCountry["name"] = "USA";
			var canadaCountry = new ObjectContainer(testSchema.FindClassByID("countries"));
			canadaCountry["name"] = "Canada";
			StorageContext.ObjectContainerStorage.Insert(usaCountry);
			StorageContext.ObjectContainerStorage.Insert(canadaCountry);

			var rel = testSchema.FindClassByID("contacts").FindRelationship(
				testSchema.FindClassByID("employee"), testSchema.FindClassByID("companies"));
			var countryRel = testSchema.FindRelationshipByID("companies_country_countries");
			
			StorageContext.ObjectContainerStorage.AddRelations( 
				new ObjectRelation( johnContact.ID.Value, rel, googCompany.ID.Value )
			);
			StorageContext.ObjectContainerStorage.AddRelations(
				new ObjectRelation(bobContact.ID.Value, rel, msCompany.ID.Value)
			);

			StorageContext.ObjectContainerStorage.AddRelations(
				new ObjectRelation(msCompany.ID.Value, countryRel, usaCountry.ID.Value)
			);
			StorageContext.ObjectContainerStorage.AddRelations(
				new ObjectRelation(googCompany.ID.Value, countryRel, canadaCountry.ID.Value)
			);

		}

		[Test]
		public void Insert() {
			var testSchema = StorageContext.DataSchemaStorage.GetSchema();
			// add using DataRow
			
			var ds = new DataSet();
			var contactsTbl = testSchema.FindClassByID("contacts").CreateDataTable();
			ds.Tables.Add(contactsTbl);

			for (int i=0; i<5; i++) {
				var r = contactsTbl.NewRow();
				r["name"] = "Contact "+i.ToString();
				contactsTbl.Rows.Add(r);
			}

			StorageContext.StorageDalc.Update(contactsTbl);
			// should be 5 contacts
			Assert.AreEqual(5, StorageContext.ObjectContainerStorage.ObjectsCount( new Query("contacts") ) );

			// quick insert
			StorageContext.StorageDalc.Insert("companies", new {
				name = "TestCompany 1"
			});
			Assert.AreEqual(1, StorageContext.ObjectContainerStorage.ObjectsCount(new Query("companies")));
			StorageContext.StorageDalc.Insert("companies", new {
				name = "TestCompany 2"
			});
			Assert.AreEqual(2, StorageContext.ObjectContainerStorage.ObjectsCount(new Query("companies")));

			// test insert relation using datarow
			var rel = testSchema.FindRelationshipByID("contacts_employee_companies");
			var relTbl = rel.CreateDataTable();
			ds.Tables.Add(relTbl);
			var relRow = relTbl.NewRow();
			relRow["subject_id"] = contactsTbl.Rows[0]["id"];
			relRow["object_id"] = StorageContext.StorageDalc.LoadValue(
				new Query("companies", (QField)"name" == (QConst)"TestCompany 1") { Fields = new[]{(QField)"id"} } );
			relTbl.Rows.Add(relRow);

			StorageContext.StorageDalc.Update(relTbl);

			Assert.AreEqual(1, 
				StorageContext.ObjectContainerStorage.LoadRelations( 
					new ObjectContainer(testSchema.FindClassByID("contacts"), Convert.ToInt64(contactsTbl.Rows[0]["id"]) ), null ).Count() );

			// quick relation insert
			StorageContext.StorageDalc.Insert("contacts_employee_companies", new {
				subject_id = contactsTbl.Rows[1]["id"],
				object_id = StorageContext.StorageDalc.LoadValue(
				new Query("companies", (QField)"name" == (QConst)"TestCompany 2") { Fields = new[] { (QField)"id" } }) } );

			Assert.AreEqual(1,
				StorageContext.ObjectContainerStorage.LoadRelations(
					new ObjectContainer(testSchema.FindClassByID("contacts"), Convert.ToInt64(contactsTbl.Rows[1]["id"])), null).Count());
		}

		[Test]
		public void Update() {
			addTestData();
			var testSchema = StorageContext.DataSchemaStorage.GetSchema();

			// datarow update
			var ds = new DataSet();
			ds.Tables.Add(testSchema.FindClassByID("contacts").CreateDataTable());
			var contactsTbl = StorageContext.StorageDalc.Load( new Query("contacts", (QField)"name"==(QConst)"Bob" ), ds);
			contactsTbl.Rows[0]["name"] = "Bob1";
			contactsTbl.Rows[0]["birthday"] = new DateTime(1985, 2, 2);
			StorageContext.StorageDalc.Update(contactsTbl);

			var bob1Contact = StorageContext.StorageDalc.LoadRecord(new Query("contacts", (QField)"id" == new QConst(contactsTbl.Rows[0]["id"])));
			Assert.NotNull(bob1Contact);
			Assert.AreEqual("Bob1", bob1Contact["name"]);
			Assert.AreEqual(new DateTime(1985, 2, 2), bob1Contact["birthday"]);

			// quick update
			Assert.AreEqual(1, StorageContext.StorageDalc.Update( new Query("contacts",  (QField)"name" == new QConst("Bob1") ), new {
				name = "Bob2", birthday = (string)null
			}) );

			var bobObj = StorageContext.ObjectContainerStorage.Load( new [] {  Convert.ToInt64(contactsTbl.Rows[0]["id"]) } ).Values.First();
			Assert.AreEqual("Bob2", bobObj["name"] );
			Assert.AreEqual(null, bobObj["birthday"]);


		}

		[Test]
		public void Delete() {
			addTestData();
			var testSchema = StorageContext.DataSchemaStorage.GetSchema();

			// datarow delete
			var ds = new DataSet();
			ds.Tables.Add(testSchema.FindClassByID("contacts").CreateDataTable());
			var contactsTbl = StorageContext.StorageDalc.Load(new Query("contacts"), ds);
			Assert.AreEqual(3, contactsTbl.Rows.Count);

			contactsTbl.Rows[0].Delete();
			StorageContext.StorageDalc.Update(contactsTbl);
			Assert.AreEqual(2, StorageContext.ObjectContainerStorage.ObjectsCount( new Query("contacts") ) );

			// delete by query
			StorageContext.StorageDalc.Delete(new Query("contacts", (QField)"name" == new QConst(contactsTbl.Rows[0]["name"])));
			Assert.AreEqual(1, StorageContext.ObjectContainerStorage.ObjectsCount(new Query("contacts")));

			// delete relation row
			var rel = testSchema.FindRelationshipByID("contacts_employee_companies");
			var relTbl = rel.CreateDataTable();
			ds.Tables.Add(relTbl);
			StorageContext.StorageDalc.Load( new Query("contacts_employee_companies"), ds);
			Assert.AreEqual(1, relTbl.Rows.Count );

			relTbl.Rows[0].Delete();
			StorageContext.StorageDalc.Update(relTbl);

			Assert.AreEqual(0, StorageContext.ObjectContainerStorage.LoadRelations( new Query("contacts_employee_companies") ).Count() );
		}


		[Test]
		public void LoadAndSubquery() {
			addTestData();
			var storageDalc = StorageContext.StorageDalc;

			var primaryContacts = storageDalc.LoadAllRecords( new Query("contacts", (QField)"is_primary"==new QConst(true) ) );
			Assert.AreEqual(2, primaryContacts.Length);
			Assert.True( primaryContacts.Where(r=>r["name"].ToString()=="John").Any() );
			Assert.True(primaryContacts.Where(r => r["name"].ToString() == "Bob").Any());

			// load only some fields (including related field
			var ds = new DataSet();
			var contactsTbl = storageDalc.Load( new Query("contacts") { 
					Fields = new[] { (QField)"name", (QField)"contacts_employee_companies.name" } 
				}, ds );
			Assert.AreEqual(2, contactsTbl.Columns.Count );
			Assert.AreEqual(3, contactsTbl.Rows.Count );
			Assert.AreEqual("name", contactsTbl.Columns[0].ColumnName);
			Assert.AreEqual("contacts_employee_companies_name", contactsTbl.Columns[1].ColumnName);
			var expectedCompanyNames = new Dictionary<string,object>(){
				{"John","Google"},
				{"Mary", DBNull.Value},
				{"Bob", "Microsoft"}
			};
			foreach (DataRow r in contactsTbl.Rows) {
				Assert.AreEqual( expectedCompanyNames[r["name"].ToString()], r["contacts_employee_companies_name"] );
			}


			Assert.AreEqual( new DateTime(1999, 5, 20), 
				storageDalc.LoadValue( new Query("contacts", (QField)"name"==(QConst)"Mary" ) {
					Fields = new[] { (QField)"birthday" }
				} ) );

			// sort 
			var companies = storageDalc.LoadAllRecords( new Query("companies") { 
				Sort = new[] { new QSort("name", System.ComponentModel.ListSortDirection.Descending) } } );
			Assert.AreEqual("Microsoft", companies[0]["name"] );
			Assert.AreEqual("Google", companies[1]["name"]);

			var sortedContactsQuery = new Query("contacts") {
				Fields = new [] { (QField)"id" },
				Sort = new[] { 
					new QSort("birthday", System.ComponentModel.ListSortDirection.Ascending),
					new QSort("is_primary", System.ComponentModel.ListSortDirection.Descending),
					new QSort("name", System.ComponentModel.ListSortDirection.Descending) }
				};
			var sortedContactIds = storageDalc.LoadAllValues(sortedContactsQuery);

			Assert.AreEqual(3, sortedContactIds[0]);
			Assert.AreEqual(5, sortedContactIds[1]);
			Assert.AreEqual(4, sortedContactIds[2]);

			sortedContactsQuery.StartRecord = 1;
			sortedContactsQuery.RecordCount = 1;
			var pagedContactIds = storageDalc.LoadAllValues( sortedContactsQuery );
			Assert.AreEqual(1, pagedContactIds.Length);
			Assert.AreEqual(5, pagedContactIds[0]);

			// load relation
			var googContactIds = storageDalc.LoadAllValues(new Query("contacts_employee_companies",
				(QField)"object_id" == new QConst(1) ) { Fields = new[] {(QField)"subject_id"} } );
			Assert.AreEqual(1, googContactIds.Length );
			Assert.AreEqual(3, googContactIds[0]);

			// load with subquery
			Assert.AreEqual("Google", StorageContext.StorageDalc.LoadValue(
						new Query("companies",
							new QueryConditionNode((QField)"id", Conditions.In,
								new Query("contacts_employee_companies",
									new QueryConditionNode(
										(QField)"subject_id", Conditions.In,
										new Query("contacts", new QueryConditionNode((QField)"name", Conditions.Like, (QConst)"Jo%")) {
											Fields = new[] { (QField)"id" }
										}
									)
								) {
									Fields = new[] { (QField)"object_id" }
								}
							)
						) { Fields = new[] { (QField)"name" } }
					));

			// load by related field (identical to query above)
			Assert.AreEqual("Google", StorageContext.StorageDalc.LoadValue(
				new Query("companies",
					new QueryConditionNode((QField)"contacts_employee_companies.name",Conditions.Like, (QConst)"Jo%")
				) { Fields = new[] { (QField)"name" } }
			));

			// sort by related field
			var contactsByCompanyName = StorageContext.StorageDalc.LoadAllRecords( new Query("contacts") {
				Sort = new[] { (QSort)"contacts_employee_companies.name asc" },
				Fields = new [] { (QField)"name", (QField)"contacts_employee_companies.companies_country_countries.name" }
			});
			Assert.AreEqual(3, contactsByCompanyName.Length);
			Assert.AreEqual("Mary", contactsByCompanyName[0]["name"]);
			Assert.AreEqual(DBNull.Value, contactsByCompanyName[0]["contacts_employee_companies_companies_country_countries_name"]);
			Assert.AreEqual("John", contactsByCompanyName[1]["name"]);
			Assert.AreEqual("Canada", contactsByCompanyName[1]["contacts_employee_companies_companies_country_countries_name"]);
			Assert.AreEqual("Bob", contactsByCompanyName[2]["name"]);
			Assert.AreEqual("USA", contactsByCompanyName[2]["contacts_employee_companies_companies_country_countries_name"]);

			// order by rel
			var contactsByCompanyNameDesc = StorageContext.StorageDalc.LoadAllRecords(new Query("contacts") {
				Sort = new[] { (QSort)"contacts_employee_companies.name desc" }
			});
			Assert.AreEqual(3, contactsByCompanyNameDesc.Length);
			Assert.AreEqual("Bob", contactsByCompanyNameDesc[0]["name"]);
			Assert.AreEqual("John", contactsByCompanyNameDesc[1]["name"]);
			Assert.AreEqual("Mary", contactsByCompanyNameDesc[2]["name"]);

			// order by inferred rel
			var contactsByCompanyCountryAsc = StorageContext.StorageDalc.LoadAllRecords(new Query("contacts") {
				Fields = new[] { (QField)"name", (QField)"contacts_employee_companies.companies_country_countries.name" },
				Sort = new[] { (QSort)"contacts_employee_companies.companies_country_countries.name asc" }
			});
			Assert.AreEqual(3, contactsByCompanyCountryAsc.Length);
			Assert.AreEqual(DBNull.Value, contactsByCompanyCountryAsc[0]["contacts_employee_companies_companies_country_countries_name"]);
			Assert.AreEqual("Canada", contactsByCompanyCountryAsc[1]["contacts_employee_companies_companies_country_countries_name"]);
			Assert.AreEqual("USA", contactsByCompanyCountryAsc[2]["contacts_employee_companies_companies_country_countries_name"]);

			// filter by inferred rel
			Assert.AreEqual("Bob", StorageContext.StorageDalc.LoadValue(
				new Query("contacts", (QField)"contacts_employee_companies.companies_country_countries.name"==(QConst)"USA" ) {
					Fields = new[] {(QField)"name"}
				}
			));
		}

	}
}
