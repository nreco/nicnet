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
						obj["birthday"] = dt.AddHours( i%24 );
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
			Assert.AreEqual( 100, contactIds.Length );
			Assert.AreEqual( dt.AddHours(23), contactIds[0] );
			Assert.AreEqual(dt.AddHours(23), contactIds[1]);
			Assert.AreEqual(dt, contactIds[99]);
		}

		[TestFixtureTearDown]
		public void CleanUp() {
			StorageContext.Destroy();
		}

	}
}
