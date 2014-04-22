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

	public class DataSetStorageContext {

		public DataSet StorageDS;
		DataSetDalc StorageDalc;
		public DataRowDalcMapper StorageDbMgr;
		public IObjectContainerStorage ObjectContainerStorage;

		public DataSetStorageContext(Func<DataSchema> ontologyPrv) {
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

			StorageDS.Tables.Add(CreateValueTable("object_integer_values", typeof(decimal)));
			StorageDS.Tables.Add(CreateValueLogTable("object_integer_values_log", typeof(decimal)));
			
			StorageDS.Tables.Add(CreateValueTable("object_decimal_values", typeof(decimal)));
			StorageDS.Tables.Add(CreateValueLogTable("object_decimal_values_log", typeof(decimal)));

			StorageDS.Tables.Add(CreateValueTable("object_string_values", typeof(string)));
			StorageDS.Tables.Add(CreateValueLogTable("object_string_values_log", typeof(string)));

			StorageDS.Tables.Add(CreateRelationsTable());
			StorageDS.Tables.Add(CreateRelationsLogTable());

			StorageDS.Tables.Add(CreateObjectTable());
			StorageDS.Tables.Add(CreateObjectLogTable());

		}

		public static DataTable CreateObjectLogTable() {
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

		public static DataTable CreateObjectTable() {
			var t = new DataTable("objects");
			var idCol = t.Columns.Add("id", typeof(long));
			idCol.AutoIncrement = true;
			idCol.AutoIncrementSeed = 1;
			idCol.AutoIncrementStep = 1;
			t.Columns.Add("compact_class_id", typeof(int));

			t.PrimaryKey = new[] {idCol};
			return t;
		}

		public static DataTable CreateRelationsTable() {
			var t = new DataTable("object_relations");
			var sId = t.Columns.Add("subject_id", typeof(long) );
			var predClassCompId = t.Columns.Add("predicate_class_compact_id", typeof(int) );
			var oId =t.Columns.Add("object_id", typeof(long) );
			t.PrimaryKey = new[] { sId, predClassCompId, oId };
			return t;
		}
		public static DataTable CreateRelationsLogTable() {
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


		public static DataTable CreateValueTable(string tableName, Type valueType) {
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
		public static DataTable CreateValueLogTable(string tableName, Type valueType) {
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

		public static DataTable CreateMetadataClassTable() {
			var t = new DataTable("metadata_classes");
			var idCol = t.Columns.Add("id", typeof(string));
			t.Columns.Add("compact_id", typeof(int));
			t.Columns.Add("name", typeof(string));
			t.Columns.Add("hidden", typeof(bool));
			t.Columns.Add("indexable", typeof(bool));
			t.Columns.Add("predefined", typeof(bool));
			t.Columns.Add("predicate", typeof(bool));
			t.Columns.Add("object_location", typeof(string));

			t.PrimaryKey = new[] { idCol };
			return t;
		}


		public static DataTable CreateMetadataPropertyTable() {
			var t = new DataTable("metadata_properties");
			var idCol = t.Columns.Add("id", typeof(string));
			t.Columns.Add("compact_id", typeof(int));
			t.Columns.Add("name", typeof(string));
			t.Columns.Add("hidden", typeof(bool));
			t.Columns.Add("indexable", typeof(bool));
			t.Columns.Add("predefined", typeof(bool));
			t.Columns.Add("datatype", typeof(string));
			t.Columns.Add("value_location", typeof(string));
			t.Columns.Add("primary_key", typeof(bool)).DefaultValue = false;

			t.PrimaryKey = new[] { idCol };
			return t;
		}

		public static DataTable CreateMetadataPropertyToClassTable() {
			var t = new DataTable("metadata_property_to_class");
			var propIdCol = t.Columns.Add("property_id", typeof(string));
			var classIdCol = t.Columns.Add("class_id", typeof(string));

			t.PrimaryKey = new[] { propIdCol, classIdCol };
			return t;
		}

		public static DataTable CreateMetadataRelationshipTable() {
			var t = new DataTable("metadata_class_relationships");
			var idCol = t.Columns.Add("id", typeof(int));
			idCol.AutoIncrement=true;

			t.Columns.Add("subject_class_id", typeof(string));
			t.Columns.Add("predicate_class_id", typeof(string));
			t.Columns.Add("object_class_id", typeof(string));
			t.Columns.Add("subject_multiplicity", typeof(bool));
			t.Columns.Add("object_multiplicity", typeof(bool));

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
				},
				new Class() {
					ID = "countries",
					CompactID = 5,
					Name = "Country Lookup",
				},
				new Class() {
					ID = "companyCountry",
					CompactID = 6,
					Name = "Country",
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
				},
				new Property() {
					ID = "id",
					CompactID = 6,
					Name = "ID",
					Predefined = true,
					PrimaryKey = true,
					DataType = PropertyDataType.Integer,
					ValueLocation = PropertyValueLocationMode.ObjectTableColumn
				}
			};
			var o = new DataSchema(classes, props);
			o.AddClassProperty(o.FindClassByID("companies"), o.FindPropertyByID("title"));
			o.AddClassProperty(o.FindClassByID("companies"), o.FindPropertyByID("net_income"));
			o.AddClassProperty(o.FindClassByID("contacts"), o.FindPropertyByID("name"));
			o.AddClassProperty(o.FindClassByID("contacts"), o.FindPropertyByID("birthday"));
			o.AddClassProperty(o.FindClassByID("contacts"), o.FindPropertyByID("is_primary"));
			o.AddClassProperty(o.FindClassByID("countries"), o.FindPropertyByID("title"));

			o.AddClassProperty(o.FindClassByID("countries"), o.FindPropertyByID("id"));
			o.AddClassProperty(o.FindClassByID("contacts"), o.FindPropertyByID("id"));
			o.AddClassProperty(o.FindClassByID("companies"), o.FindPropertyByID("id"));

			var contactToCompanyRel = new Relationship(
					o.FindClassByID("contacts"),
					o.FindClassByID("contactCompany"),
					o.FindClassByID("companies"),
					false,
					false );
			o.AddRelationship(contactToCompanyRel);

			var companyToContactRel = new Relationship(
				o.FindClassByID("companies"),
				o.FindClassByID("contactCompany"),
				o.FindClassByID("contacts"),
				true,
				true);

			o.AddRelationship(companyToContactRel);

			var companyToParentRel = new Relationship(
				o.FindClassByID("companies"),
				o.FindClassByID("parentCompany"),
				o.FindClassByID("companies"),
				false,
				false
			);

			o.AddRelationship(companyToParentRel);

			var companyToChildRel = new Relationship(
				o.FindClassByID("companies"),
				o.FindClassByID("parentCompany"),
				o.FindClassByID("companies"),
				true,
				true
			);

			o.AddRelationship(companyToChildRel);

			var companyToCountryRel = new Relationship(
				o.FindClassByID("companies"),
				o.FindClassByID("companyCountry"),
				o.FindClassByID("countries"),
				false, false
			);

			o.AddRelationship(companyToCountryRel);

			var countryToCompanyRel = new Relationship(
				o.FindClassByID("countries"),
				o.FindClassByID("companyCountry"),
				o.FindClassByID("companies"),
				true, true
			);

			o.AddRelationship(countryToCompanyRel);


			return o;
		}


	}
}
