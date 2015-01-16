using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Data;

using NUnit.Framework;
using NI.Data.Storage.Model;

namespace NI.Data.Storage.Tests {
	
	public class ObjectContainerSqlDalcStorageTests {
		
		SQLiteStorageContext StorageContext;

		[SetUp]
		public void SetUp() {
			StorageContext = new SQLiteStorageContext((StorageDbMgr, ObjectContainerStorage) => {
				return new MetadataTableSchemaStorage(StorageDbMgr);	
			});
		}

		[TearDown]
		public void CleanUp() {
			StorageContext.Destroy();
		}

		[Test]
		public void InsertAndLoadWithSort() {
			Logger.SetInfo( (t,msg)=> {
				Console.WriteLine("[{0}] {1}", t,msg);
			});

			StorageContext.CreateTestDataSchema();

			var schema = StorageContext.DataSchemaStorage.GetSchema();
			Assert.AreEqual(3, schema.Classes.Where(c=>!c.IsPredicate).Count() );
			
			var contactsClass = schema.FindClassByID("contacts");
			Assert.IsNotNull(contactsClass);

			var dt = DateTime.Now.Date;
			using (var t = new TransactionScope()) {
				DataHelper.EnsureConnectionOpen(StorageContext.Connection, () => {
					// insert 1000 records
					for (int i=0; i<100; i++) {
						var obj = new ObjectContainer( contactsClass );
						obj["name"] = String.Format("Contact_{0}", i);
						obj["is_primary"] = i%10==0;
						var birthday = dt.AddDays( i );
						obj["birthday"] = birthday;
						StorageContext.ObjectContainerStorage.Insert( obj );
					}
				});
				t.Complete();
			}

			Assert.AreEqual(100, StorageContext.ObjectContainerStorage.GetObjectsCount( new Query("contacts") ) );

			var selectWithSortQuery = new Query("contacts", (QField)"is_primary" == new QConst(true) ) {
				Sort = new[] { new QSort("birthday", System.ComponentModel.ListSortDirection.Descending) }
			}; 

			var contactIds = StorageContext.ObjectContainerStorage.GetObjectIds( selectWithSortQuery );
			Assert.AreEqual( 10, contactIds.Length );
			Assert.AreEqual( 91, contactIds[0] );
			Assert.AreEqual( 1, contactIds[9] );
			
			var contacts = StorageContext.ObjectContainerStorage.Load( contactIds );
			Assert.AreEqual(10, contacts.Count);
			Assert.AreEqual( dt, contacts[contactIds[9]]["birthday"] ); //dt - smallest value that can be
			Assert.True( (DateTime)contacts[contactIds[0]]["birthday"] > (DateTime)contacts[contactIds[9]]["birthday"]);
		}

		[Test]
		public void LoadBySubqueryAndRelation() {
			StorageContext.CreateTestDataSchema();

			var schema = StorageContext.DataSchemaStorage.GetSchema();
			var contactsClass = schema.FindClassByID("contacts");
			var companiesClass = schema.FindClassByID("companies");
			var emplClass = schema.FindClassByID("employee");
			Assert.NotNull(emplClass);
			
			var contactCompanyEmployeeRel = contactsClass.FindRelationship( emplClass, companiesClass );
			Assert.NotNull(contactCompanyEmployeeRel);

			using (var t = new TransactionScope()) {
				DataHelper.EnsureConnectionOpen(StorageContext.Connection, () => {
					for (int i=0; i<10; i++) {
						var objCompany = new ObjectContainer(companiesClass);
						objCompany["name"] = String.Format("Company_{0}", i);						
						StorageContext.ObjectContainerStorage.Insert(objCompany);

						for (int j=0; j<10; j++) {
							var objContact = new ObjectContainer(contactsClass);
							objContact["name"] = String.Format("Company_{0} Contact_{1}", i, j);
							StorageContext.ObjectContainerStorage.Insert(objContact);

							StorageContext.ObjectContainerStorage.AddRelation( 
								new ObjectRelation(objContact.ID.Value, contactCompanyEmployeeRel, objCompany.ID.Value ) );
						}
					}
				});
				t.Complete();
			}

			// subquery test
			Assert.AreEqual("Company_1", StorageContext.StorageDalc.LoadValue(
					new Query("companies", 
						new QueryConditionNode( (QField)"id", Conditions.In,
							new Query("contacts_employee_companies", 
								new QueryConditionNode(
									(QField)"subject_id", Conditions.In,
									new Query("contacts", new QueryConditionNode((QField)"name", Conditions.Like, (QConst)"Company_1%") ) {
										Fields = new [] { (QField)"id" }
									}
								)
							) {
								Fields = new[] {(QField)"object_id"}
							}
						)
					) { Fields = new [] { (QField)"name" } }
				) );

			// relation load test
			var company0rels = StorageContext.ObjectContainerStorage.LoadRelations( 
					"contacts_employee_companies",
					new QueryConditionNode( 
						(QField)"object_id",
						Conditions.In,
						new Query("companies.c", (QField)"name"==(QConst)"Company_0" ) {
							Fields = new[] { (QField)"c.id" }
						}
					)
			);
			Assert.AreEqual(10, company0rels.Count() );
		}


	}
}
