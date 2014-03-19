#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013 Vitalii Fedorchenko
 * Copyright 2014 NewtonIdeas
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
using System.Threading.Tasks;
using System.Data;

using NUnit.Framework;
using NI.Data.Storage.Model;

namespace NI.Data.Storage.Tests
{
	
	[TestFixture]
    public class ObjectContainerDalcStorageTest
    {
		

		protected void AssertObjectLog(DataSet ds, long objId, string action) {
			Assert.True(
				ds.Tables["objects_log"].Rows.Cast<DataRow>().Where( r =>
					Convert.ToInt64(r["object_id"])==objId && r["action"].ToString()==action
					&& r.Field<DateTime>("timestamp")<=DateTime.Now
				).Any(), String.Format("Log entry for object ID={0} action={1} not found", objId, action ) );
		}

		[Test]
		public void InsertLoadUpdateDelete() {
			var o = StubDataSetDalcStorageContext.CreateTestSchema();

			var objPersisterContext = new StubDataSetDalcStorageContext( () => { return o; } );

			var googCompany = new ObjectContainer(o.FindClassByID("companies"));
			googCompany["title"] = "Google";
			Assert.Catch<ArgumentException>( new TestDelegate( () => {
				googCompany["name"] = "ZZZ";
			}));
			googCompany["net_income"] = 999;

			var yahooCompany = new ObjectContainer(o.FindClassByID("companies"));
			yahooCompany["title"] = "Yahoo Inc";
			var googleChildCompany = new ObjectContainer(o.FindClassByID("companies"));
			googleChildCompany["title"] = "YouTube";


			var johnContact = new ObjectContainer(o.FindClassByID("contacts"));
			johnContact["name"] = "John";
			johnContact["is_primary"] = true;
			var maryContact = new ObjectContainer(o.FindClassByID("contacts"));
			maryContact["name"] = "Mary";
			maryContact["is_primary"] = false;
			maryContact["birthday"] = new DateTime(1999, 5, 20);
			var bobContact = new ObjectContainer(o.FindClassByID("contacts"));
			bobContact["name"] = "Bob";
			bobContact["is_primary"] = true;


			objPersisterContext.ObjectContainerStorage.Insert(googCompany);
			Assert.True(googCompany.ID.HasValue);
			AssertObjectLog(objPersisterContext.StorageDS, googCompany.ID.Value, "insert");
			objPersisterContext.ObjectContainerStorage.Insert(yahooCompany);
			objPersisterContext.ObjectContainerStorage.Insert(googleChildCompany);

			objPersisterContext.ObjectContainerStorage.Insert(johnContact);
			objPersisterContext.ObjectContainerStorage.Insert(maryContact);
			objPersisterContext.ObjectContainerStorage.Insert(bobContact);

			// load test
			var maryCopy = objPersisterContext.ObjectContainerStorage.Load(new[]{ maryContact.ID.Value }).Values.FirstOrDefault();
			Assert.NotNull(maryCopy, "Object Load failed");
			Assert.AreEqual((string)maryContact["name"], (string)maryCopy["name"]);
			Assert.AreEqual((bool)maryContact["is_primary"], (bool)maryCopy["is_primary"]);
			Assert.AreEqual((DateTime)maryContact["birthday"], (DateTime)maryCopy["birthday"]);

			var googCopy = objPersisterContext.ObjectContainerStorage.Load(new[]{ googCompany.ID.Value }).Values.FirstOrDefault();
			Assert.NotNull(googCopy, "Object Load failed");
			Assert.AreEqual((string)googCompany["title"], (string)googCopy["title"]);
			Assert.AreEqual((decimal)googCompany["net_income"], (decimal)googCopy["net_income"]);

			// update test
			maryCopy["name"] = "Mary Second";
			maryCopy["birthday"] = new DateTime(1988, 2, 10);
			maryCopy["is_primary"] = true;
			objPersisterContext.ObjectContainerStorage.Update(maryCopy);
			AssertObjectLog(objPersisterContext.StorageDS, maryCopy.ID.Value, "update");

			// reload mary contact
			maryContact = objPersisterContext.ObjectContainerStorage.Load(new[]{ maryContact.ID.Value}).Values.FirstOrDefault();
			Assert.AreEqual((string)maryContact["name"], "Mary Second");
			Assert.AreEqual((bool)maryContact["is_primary"], true);
			Assert.AreEqual((DateTime)maryContact["birthday"], (DateTime)maryCopy["birthday"]);

			// test relations
			var contactToCompanyRel = maryContact.GetClass().FindRelationship(
				o.FindClassByID("contactCompany"), googCompany.GetClass()
			);
			var companyToParentCompanyRel = googleChildCompany.GetClass().FindRelationship(
				o.FindClassByID("parentCompany"), googCompany.GetClass(), false
			);
			objPersisterContext.ObjectContainerStorage.AddRelations(
				new ObjectRelation(maryContact.ID.Value, contactToCompanyRel, googCompany.ID.Value ),
				new ObjectRelation(johnContact.ID.Value, contactToCompanyRel, googCompany.ID.Value ),
				new ObjectRelation(bobContact.ID.Value, contactToCompanyRel, yahooCompany.ID.Value),
				new ObjectRelation(googleChildCompany.ID.Value, companyToParentCompanyRel, googCompany.ID.Value)
			);

			var googCompanyRels = objPersisterContext.ObjectContainerStorage.LoadRelations(googCompany);
			Assert.AreEqual(3, googCompanyRels.Count(), "Expected 3 relations for Google company");

			var yahooCompanyRels = objPersisterContext.ObjectContainerStorage.LoadRelations(yahooCompany);
			Assert.AreEqual(1, yahooCompanyRels.Count(), "Expected 1 relation for Yahoo company");
			Assert.AreEqual(bobContact.ID.Value, yahooCompanyRels.First().ObjectID, "Bob should be a only contact of Yahoo");

			// remove rel
			var maryRel = googCompanyRels.Where( r=>r.ObjectID == maryContact.ID.Value ).First();
			objPersisterContext.ObjectContainerStorage.RemoveRelations( 
				new ObjectRelation(
					googCompany.ID.Value, 
					googCompany.GetClass().FindRelationship(o.FindClassByID("contactCompany"), maryContact.GetClass()), 
					maryContact.ID.Value )
			);

			Assert.AreEqual(1, objPersisterContext.ObjectContainerStorage.LoadRelations(googCompany, new []{ o.FindClassByID("contactCompany") }).Count(),
				 "Expected 1 relation for Google company after Mary removal");


			Console.WriteLine("DataSet after test:\n" + objPersisterContext.StorageDS.GetXml());
		}

    }
}
