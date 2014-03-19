using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NI.Data.Storage.Tests {
	
	public class ObjectContainerSqlDalcStorageTests {
		
		SQLiteStorageContext StorageContext;

		[TestFixtureSetUp]
		public void SetUp() {
			StorageContext = new SQLiteStorageContext();
		}

		[Test]
		public void LoadWithSort() {
			
		}

		[TestFixtureTearDown]
		public void CleanUp() {
			StorageContext.Destroy();
		}

	}
}
