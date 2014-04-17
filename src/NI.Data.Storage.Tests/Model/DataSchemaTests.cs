using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NI.Data.Storage.Model;

namespace NI.Data.Storage.Tests.Model {

	[TestFixture]
	public class DataSchemaTests {

		[Test]
		public void InferRelationshipByID() {
			var schema = DataSetStorageContext.CreateTestSchema();

			var r1 = schema.InferRelationshipByID("contacts_contactCompany_companies.companies_companyCountry_countries", schema.FindClassByID("contacts") );
			Assert.NotNull(r1);
			Assert.True(r1.Inferred);
			Assert.AreEqual(2, r1.InferredByRelationships.Count() );
			Assert.AreEqual("countries", r1.Object.ID );

			var r2 = schema.InferRelationshipByID("companies_companyCountry_countries.contacts_contactCompany_companies", schema.FindClassByID("countries"));
			Assert.NotNull(r2);
			Assert.True(r2.Inferred);
			Assert.AreEqual("contacts", r2.Object.ID);
		}

	}
}
