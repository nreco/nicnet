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
	public class StorageDalcTests {
		
		DataSchema testSchema;
		StubObjectContainerStorageContext objContext;
		IDalc storageDalc;

		[SetUp]
		public void createTestStorageDalc() {
			testSchema = StubObjectContainerStorageContext.CreateTestSchema();
			Func<DataSchema> getTestSchema = () => { return testSchema; };

			objContext = new StubObjectContainerStorageContext(getTestSchema);
			storageDalc = new StorageDalc(objContext.StorageDbMgr.Dalc, objContext.ObjectContainerStorage, getTestSchema);
		}

		[Test]
		public void Insert() {
			// direct insert
			storageDalc.Insert("companies", new {
				title = "Microsoft"
			});

			Assert.AreEqual(1, objContext.StorageDS.Tables["objects"].Rows.Count);
			Assert.AreEqual(1, objContext.StorageDS.Tables["object_string_values"].Rows.Count);
			Assert.AreEqual("Microsoft", objContext.StorageDS.Tables["object_string_values"].Rows[0]["value"]);

			// dataset insert
			var ds = new DataSet();
			var contactsTbl = testSchema.FindClassByID("contacts").CreateDataTable();
			ds.Tables.Add(contactsTbl);

			var newRow = contactsTbl.NewRow();
			newRow["name"] = "John Smith";
			newRow["birthday"] = new DateTime(1980,1,1);
			contactsTbl.Rows.Add(newRow);
			storageDalc.Update( contactsTbl );
		}

	}
}
