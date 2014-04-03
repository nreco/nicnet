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

			var rel = testSchema.FindClassByID("contacts").FindRelationship(
				testSchema.FindClassByID("employee"), testSchema.FindClassByID("companies"));
			StorageContext.ObjectContainerStorage.AddRelations( 
				new ObjectRelation( johnContact.ID.Value, rel, googCompany.ID.Value )
			);
		}

		[Test]
		public void LoadAndSubquery() {
			addTestData();
			var storageDalc = StorageContext.StorageDalc;

			var primaryContacts = storageDalc.LoadAllRecords( new Query("contacts", (QField)"is_primary"==new QConst(true) ) );
			Assert.AreEqual(2, primaryContacts.Length);
			Assert.True( primaryContacts.Where(r=>r["name"].ToString()=="John").Any() );
			Assert.True(primaryContacts.Where(r => r["name"].ToString() == "Bob").Any());

			// load only some fields
			var ds = new DataSet();
			var contactsTbl = storageDalc.Load( new Query("contacts") { Fields = new[] { (QField)"name" } }, ds );
			Assert.AreEqual(1, contactsTbl.Columns.Count );
			Assert.AreEqual("name", contactsTbl.Columns[0].ColumnName);

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
		}

	}
}
