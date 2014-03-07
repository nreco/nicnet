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
using NI.Data;
using NI.Data.Storage.Model;
using NI.Data.Storage;

namespace NI.Data.Storage.Tests {

	public class StubObjectContainerStorageContext {

		public DataSet StorageDS;
		DataSetDalc StorageDalc;
		public DataRowDalcMapper StorageDbMgr;
		public IObjectContainerStorage ObjectContainerStorage;

		public StubObjectContainerStorageContext(Func<DataSchema> ontologyPrv) {
			InitStorageDS();
			StorageDalc = new DataSetDalc(StorageDS);

			StorageDbMgr = new DataRowDalcMapper(StorageDalc, new StorageDataSetPrv(StorageDS).GetDataSet );
			ObjectContainerStorage = new ObjectContainerDalcStorage(StorageDbMgr, StorageDalc, ontologyPrv);
		}

		public class StorageDataSetPrv : IDataSetFactory {
			DataSet ds;
			public StorageDataSetPrv(DataSet sampleDs) {
				ds = sampleDs;
			}
			public DataSet GetDataSet(string context) {
				var tblName = Convert.ToString(context);
				var newDs = new DataSet();
				if (ds.Tables.Contains(tblName)) {
					newDs.Tables.Add( ds.Tables[tblName].Clone() );
				}
				return newDs;
			}
		}

		protected void InitStorageDS() {
			StorageDS = new DataSet();

			StorageDS.Tables.Add(CreateValueTable("object_datetime_values", typeof(DateTime)));
			StorageDS.Tables.Add(CreateValueLogTable("object_datetime_values_log", typeof(DateTime)));
			StorageDS.Tables.Add(CreateValueTable("object_number_values", typeof(decimal)));
			StorageDS.Tables.Add(CreateValueLogTable("object_number_values_log", typeof(decimal)));
			StorageDS.Tables.Add(CreateValueTable("object_string_values", typeof(string)));
			StorageDS.Tables.Add(CreateValueLogTable("object_string_values_log", typeof(string)));

			StorageDS.Tables.Add(CreateRelationsTable());
			StorageDS.Tables.Add(CreateRelationsLogTable());

			StorageDS.Tables.Add(CreateObjectTable());
			StorageDS.Tables.Add(CreateObjectLogTable());

		}

		private DataTable CreateObjectLogTable() {
			var t = new DataTable("objects_log");
			var idCol = t.Columns.Add("id", typeof(long));
			idCol.AutoIncrement = true;
			idCol.AutoIncrementSeed = 1;
			idCol.AutoIncrementStep = 1;
			t.Columns.Add("compact_class_id", typeof(int));

			var oId = t.Columns.Add("object_id", typeof(long));
			t.Columns.Add("account_id", typeof(int));
			t.Columns.Add("timestamp", typeof(DateTime));
			t.Columns.Add("action", typeof(string));

			t.PrimaryKey = new[] {idCol};
			return t;
		}

		private DataTable CreateObjectTable() {
			var t = new DataTable("objects");
			var idCol = t.Columns.Add("id", typeof(long));
			idCol.AutoIncrement = true;
			idCol.AutoIncrementSeed = 1;
			idCol.AutoIncrementStep = 1;
			t.Columns.Add("compact_class_id", typeof(int));

			t.PrimaryKey = new[] {idCol};
			return t;
		}

		private DataTable CreateRelationsTable() {
			var t = new DataTable("object_relations");
			var sId = t.Columns.Add("subject_id", typeof(long) );
			var predClassCompId = t.Columns.Add("predicate_class_compact_id", typeof(int) );
			var oId =t.Columns.Add("object_id", typeof(long) );
			t.PrimaryKey = new[] { sId, predClassCompId, oId };
			return t;
		}
		private DataTable CreateRelationsLogTable() {
			var t = new DataTable("object_relations_log");
			var sId = t.Columns.Add("subject_id", typeof(long));
			var predClassCompId = t.Columns.Add("predicate_class_compact_id", typeof(int));
			var oId = t.Columns.Add("object_id", typeof(long));

			var idCol = t.Columns.Add("id", typeof(long));
			idCol.AutoIncrement = true;
			idCol.AutoIncrementSeed = 1;
			idCol.AutoIncrementStep = 1;

			t.Columns.Add("account_id", typeof(int));
			t.Columns.Add("timestamp", typeof(DateTime));
			var deletedCol = t.Columns.Add("deleted", typeof(bool));
			deletedCol.DefaultValue = false;

			t.PrimaryKey = new[] { idCol };
			return t;
		}


		DataTable CreateValueTable(string tableName, Type valueType) {
			var t = new DataTable(tableName);
			var idCol = t.Columns.Add("id", typeof(long));
			idCol.AutoIncrement = true;
			idCol.AutoIncrementSeed = 1;
			idCol.AutoIncrementStep = 1;

			var objIdCol = t.Columns.Add("object_id", typeof(long));
			t.Columns.Add("property_compact_id", typeof(int));
			t.Columns.Add("value", valueType);
			t.PrimaryKey = new[] { idCol };
			return t;
		}
		DataTable CreateValueLogTable(string tableName, Type valueType) {
			var t = new DataTable(tableName);
			var idCol = t.Columns.Add("id", typeof(long));
			idCol.AutoIncrement = true;
			idCol.AutoIncrementSeed = 1;
			idCol.AutoIncrementStep = 1;

			var objIdCol = t.Columns.Add("object_id", typeof(long));
			t.Columns.Add("account_id", typeof(int));
			t.Columns.Add("timestamp", typeof(DateTime));
			t.Columns.Add("property_compact_id", typeof(int));

			t.Columns.Add("value", valueType);
			var deletedCol = t.Columns.Add("deleted", typeof(bool));
			deletedCol.DefaultValue = false;

			t.PrimaryKey = new[] { idCol };
			return t;
		}


		public static DataSchema CreateTestSchema() {
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
			var o = new DataSchema(classes, props);
			o.AddClassProperty(o.FindClassByID("companies"), o.FindPropertyByID("title"));
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


	}
}
