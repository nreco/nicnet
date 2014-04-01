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

		[TestFixtureSetUp]
		public void SetUp() {
			StorageContext = new SQLiteStorageContext();
		}

		[Test]
		public void InsertAndLoadWithSort() {
			Logger.SetInfo( (t,msg)=> {
				Console.WriteLine("[{0}] {1}", t,msg);
			});

			StorageContext.CreateTestDataSchema();

			var schema = StorageContext.DataSchemaStorage.GetSchema();
			Assert.AreEqual(2, schema.Classes.Count() );
			
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

			Assert.AreEqual(100, StorageContext.ObjectContainerStorage.ObjectsCount( new Query("contacts") ) );

			var selectWithSortQuery = new Query("contacts", (QField)"is_primary" == new QConst(true) ) {
				Sort = new[] { new QSort("birthday", System.ComponentModel.ListSortDirection.Descending) }
			}; 

			var contactIds = StorageContext.ObjectContainerStorage.ObjectIds( selectWithSortQuery );
			Assert.AreEqual( 10, contactIds.Length );
			Assert.AreEqual( 91, contactIds[0] );
			Assert.AreEqual( 1, contactIds[9] );
			
			var contacts = StorageContext.ObjectContainerStorage.Load( contactIds );
			Assert.AreEqual(10, contacts.Count);
			Assert.AreEqual( dt, contacts[contactIds[9]]["birthday"] ); //dt - smallest value that can be
			Assert.True( (DateTime)contacts[contactIds[0]]["birthday"] > (DateTime)contacts[contactIds[9]]["birthday"]);
		}

		[TestFixtureTearDown]
		public void CleanUp() {
			StorageContext.Destroy();
		}

	}
}
