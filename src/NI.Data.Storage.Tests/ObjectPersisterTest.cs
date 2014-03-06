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
    public class ObjectPersisterTest
    {
		
		public Ontology composeTestOntology() {
			var classes = new[] {
				new Class() {
					ID = "contacts",
					CompactID = 1,
					Name = "Contacts",
					ObjectLocation = ClassObjectLocationMode.ObjectTable
				},
				new Class() {
					ID = "companies",
					CompactID = 2,
					Name = "Companies",
					ObjectLocation = ClassObjectLocationMode.ObjectTable
				},
				new Class() {
					ID = "contactCompany",
					CompactID = 3,
					Name = "Company",
					IsPredicate = true
				},
				new Class() {
					ID = "parentCompany",
					CompactID = 4,
					Name = "Parent Company",
					IsPredicate = true
				}
			};
			var props = new[] {
				new Property() {
					ID = "name",
					CompactID = 1,
					Name = "Name",
					DataType = PropertyDataType.String,
					ValueLocation = PropertyValueLocationMode.ValueTable
				},
				new Property() {
					ID = "title",
					CompactID = 2,
					Name = "Title",
					DataType = PropertyDataType.String, 
					ValueLocation = PropertyValueLocationMode.ValueTable
				},
				new Property() {
					ID = "birthday",
					CompactID = 3,
					Name = "Birthday",
					DataType = PropertyDataType.DateTime,
					ValueLocation = PropertyValueLocationMode.ValueTable
				},
				new Property() {
					ID = "is_primary",
					CompactID = 4,
					Name = "Primary",
					DataType = PropertyDataType.Boolean
				},
				new Property() {
					ID = "net_income",
					CompactID = 5,
					Name = "Net Income",
					DataType = PropertyDataType.Decimal
				}
			};
			var o = new Ontology(classes, props);
			o.AddClassProperty( o.FindClassByID("companies"), o.FindPropertyByID("title") );
			o.AddClassProperty(o.FindClassByID("companies"), o.FindPropertyByID("net_income"));
			o.AddClassProperty(o.FindClassByID("contacts"), o.FindPropertyByID("name"));
			o.AddClassProperty(o.FindClassByID("contacts"), o.FindPropertyByID("birthday"));
			o.AddClassProperty(o.FindClassByID("contacts"), o.FindPropertyByID("is_primary"));
			
			var contactToCompanyRel = new Relationship() {
				Subject = o.FindClassByID("contacts"),
				Predicate = o.FindClassByID("contactCompany"),
				Object = o.FindClassByID("companies"),
				Reversed = false,
				Multiplicity = false
			};
			o.AddRelationship(contactToCompanyRel);

			var companyToContactRel = new Relationship() {
				Object = o.FindClassByID("contacts"),
				Predicate = o.FindClassByID("contactCompany"),
				Subject = o.FindClassByID("companies"),
				Reversed = true,
				Multiplicity = true
			};

			o.AddRelationship(companyToContactRel);

			var companyToParentRel = new Relationship() {
				Subject = o.FindClassByID("companies"),
				Predicate = o.FindClassByID("parentCompany"),
				Object = o.FindClassByID("companies"),
				Reversed = false,
				Multiplicity = false
			};

			o.AddRelationship(companyToParentRel);
			
			var companyToChildRel = new Relationship() {
				Object = o.FindClassByID("companies"),
				Predicate = o.FindClassByID("parentCompany"),
				Subject = o.FindClassByID("companies"),
				Reversed = true,
				Multiplicity = true
			};

			o.AddRelationship(companyToChildRel);

			return o;
		}

		protected void AssertObjectLog(DataSet ds, long objId, string action) {
			Assert.True(
				ds.Tables["objects_log"].Rows.Cast<DataRow>().Where( r =>
					Convert.ToInt64(r["object_id"])==objId && r["action"].ToString()==action
					&& r.Field<DateTime>("timestamp")<=DateTime.Now
				).Any(), String.Format("Log entry for object ID={0} action={1} not found", objId, action ) );
		}

		[Test]
		public void Test_ObjectPersister_BasicFunctions() {

			var objPersisterContext = new EmptyStubObjectPersisterContext( composeTestOntology );
			var o = composeTestOntology();

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
			maryContact["name"] = "Bob";
			maryContact["is_primary"] = true;


			objPersisterContext.ObjectPersisterInstance.Insert(googCompany);
			Assert.True(googCompany.ID.HasValue);
			AssertObjectLog(objPersisterContext.StorageDS, googCompany.ID.Value, "insert");
			objPersisterContext.ObjectPersisterInstance.Insert(yahooCompany);
			objPersisterContext.ObjectPersisterInstance.Insert(googleChildCompany);

			objPersisterContext.ObjectPersisterInstance.Insert(johnContact);
			objPersisterContext.ObjectPersisterInstance.Insert(maryContact);
			objPersisterContext.ObjectPersisterInstance.Insert(bobContact);

			// load test
			var maryCopy = objPersisterContext.ObjectPersisterInstance.Load(maryContact.ID.Value).FirstOrDefault();
			Assert.NotNull(maryCopy, "Object Load failed");
			Assert.AreEqual((string)maryContact["name"], (string)maryCopy["name"]);
			Assert.AreEqual((bool)maryContact["is_primary"], (bool)maryCopy["is_primary"]);
			Assert.AreEqual((DateTime)maryContact["birthday"], (DateTime)maryCopy["birthday"]);

			var googCopy = objPersisterContext.ObjectPersisterInstance.Load(googCompany.ID.Value).FirstOrDefault();
			Assert.NotNull(googCopy, "Object Load failed");
			Assert.AreEqual((string)googCompany["title"], (string)googCopy["title"]);
			Assert.AreEqual((decimal)googCompany["net_income"], (decimal)googCopy["net_income"]);

			// update test
			maryCopy["name"] = "Mary Second";
			maryCopy["birthday"] = new DateTime(1988, 2, 10);
			maryCopy["is_primary"] = true;
			objPersisterContext.ObjectPersisterInstance.Update(maryCopy);
			AssertObjectLog(objPersisterContext.StorageDS, maryCopy.ID.Value, "update");

			// reload mary contact
			maryContact = objPersisterContext.ObjectPersisterInstance.Load(maryContact.ID.Value).FirstOrDefault();
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
			objPersisterContext.ObjectPersisterInstance.AddRelations(
				new ObjectRelation(maryContact.ID.Value, contactToCompanyRel, googCompany.ID.Value ),
				new ObjectRelation(johnContact.ID.Value, contactToCompanyRel, googCompany.ID.Value ),
				new ObjectRelation(bobContact.ID.Value, contactToCompanyRel, yahooCompany.ID.Value),
				new ObjectRelation(googleChildCompany.ID.Value, companyToParentCompanyRel, googCompany.ID.Value)
			);

			var googCompanyRels = objPersisterContext.ObjectPersisterInstance.LoadRelations(googCompany);
			Assert.AreEqual(3, googCompanyRels.Count(), "Expected 3 relations for Google company");

			var yahooCompanyRels = objPersisterContext.ObjectPersisterInstance.LoadRelations(yahooCompany);
			Assert.AreEqual(1, yahooCompanyRels.Count(), "Expected 1 relation for Yahoo company");
			Assert.AreEqual(bobContact.ID.Value, yahooCompanyRels.First().ObjectID, "Bob should be a only contact of Yahoo");

			// remove rel
			var maryRel = googCompanyRels.Where( r=>r.ObjectID == maryContact.ID.Value ).First();
			objPersisterContext.ObjectPersisterInstance.RemoveRelations( 
				new ObjectRelation(
					googCompany.ID.Value, 
					googCompany.GetClass().FindRelationship(o.FindClassByID("contactCompany"), maryContact.GetClass()), 
					maryContact.ID.Value )
			);

			Assert.AreEqual(1, objPersisterContext.ObjectPersisterInstance.LoadRelations(googCompany, new []{ o.FindClassByID("contactCompany") }).Count(),
				 "Expected 1 relation for Google company after Mary removal");


			Console.WriteLine("DataSet after test:\n" + objPersisterContext.StorageDS.GetXml());
		}

    }
}
