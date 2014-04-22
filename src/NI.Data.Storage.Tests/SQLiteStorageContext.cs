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
using System.IO;

using System.Data;
using NI.Data;
using NI.Data.Storage.Model;
using NI.Data.Storage;

using NI.Data.SQLite;
using System.Data.SQLite;

namespace NI.Data.Storage.Tests {

	public class SQLiteStorageContext {

		DbDalc InternalDalc;
		public IDbConnection Connection;
		string dbFileName;
		public DataRowDalcMapper StorageDbMgr;
		public IObjectContainerStorage ObjectContainerStorage;
		public DataSchemaDalcStorage DataSchemaStorage;
		public IDalc StorageDalc;

		public SQLiteStorageContext() {
			dbFileName = Path.GetTempFileName() + ".db";
			var connStr = String.Format("Data Source={0};FailIfMissing=false;Pooling=False;", dbFileName);
			var sqliteDalcFactory = new SQLiteDalcFactory();
			Connection = sqliteDalcFactory.CreateConnection();
			Connection.ConnectionString = connStr;
			InternalDalc = new DbDalc(sqliteDalcFactory, Connection, new [] {
				new DbDalcView("objects_view", @"
					SELECT @SqlFields
					FROM objects
					@Joins
					@SqlWhere[where {0}]
					@SqlOrderBy[order by {0}]
				") {
					FieldMapping = new Dictionary<string,string>() {
						{"id", "objects.id"},
						{"compact_class_id", "objects.compact_class_id"}
					}
				}
			});
			var dbEventsBroker = new DataEventBroker(InternalDalc);
			var sqlTraceLogger = new NI.Data.Triggers.SqlCommandTraceTrigger(dbEventsBroker);

			InitDbSchema();

			StorageDbMgr = new DataRowDalcMapper(InternalDalc, new StorageDataSetPrv(CreateStorageSchemaDS()).GetDataSet);
			DataSchemaStorage = new DataSchemaDalcStorage(StorageDbMgr);
			var objStorage = new ObjectContainerSqlDalcStorage(StorageDbMgr, InternalDalc, DataSchemaStorage.GetSchema);
			objStorage.ObjectViewName = "objects_view";
			ObjectContainerStorage = objStorage;

			StorageDalc = new StorageDalc(InternalDalc, ObjectContainerStorage, DataSchemaStorage.GetSchema );
		}


		public void Destroy() {
			((SQLiteConnection)Connection).Dispose();
			SQLiteConnection.ClearAllPools();
			GC.Collect();
			if (dbFileName != null && File.Exists(dbFileName))
				File.Delete(dbFileName);

		}

		public void CreateTestDataSchema() {
			StorageDbMgr.Insert( "metadata_classes", new Dictionary<string,object>() {
				{"id", "companies"},
				{"name", "Company"},
				{"hidden", false},
				{"indexable", false},
				{"predefined", false},
				{"predicate", false},
				{"compact_id", 1},
				{"object_location", "ObjectTable"}
			});
			StorageDbMgr.Insert("metadata_classes", new Dictionary<string, object>() {
				{"id", "contacts"},
				{"name", "Contact"},
				{"hidden", false},
				{"indexable", false},
				{"predefined", false},
				{"predicate", false},
				{"compact_id", 2},
				{"object_location", "ObjectTable"}
			});

			StorageDbMgr.Insert("metadata_classes", new Dictionary<string, object>() {
				{"id", "employee"},
				{"name", "Employee"},
				{"hidden", false},
				{"indexable", false},
				{"predefined", false},
				{"predicate", true},
				{"compact_id", 3},
				{"object_location", "ObjectTable"}
			});

			StorageDbMgr.Insert("metadata_classes", new Dictionary<string, object>() {
				{"id", "countries"},
				{"name", "Country Lookup"},
				{"hidden", false},
				{"indexable", false},
				{"predefined", false},
				{"predicate", false},
				{"compact_id", 4},
				{"object_location", "ObjectTable"}
			});

			StorageDbMgr.Insert("metadata_classes", new Dictionary<string, object>() {
				{"id", "country"},
				{"name", "Country"},
				{"hidden", false},
				{"indexable", false},
				{"predefined", false},
				{"predicate", true},
				{"compact_id", 5},
				{"object_location", "ObjectTable"}
			});


			StorageDbMgr.Insert("metadata_class_relationships", new Dictionary<string, object>() {
				{"subject_class_id", "contacts"},
				{"predicate_class_id", "employee"},
				{"object_class_id", "companies"},
				{"subject_multiplicity", true},
				{"object_multiplicity", false}
			});

			StorageDbMgr.Insert("metadata_class_relationships", new Dictionary<string, object>() {
				{"subject_class_id", "companies"},
				{"predicate_class_id", "country"},
				{"object_class_id", "countries"},
				{"subject_multiplicity", true},
				{"object_multiplicity", false}
			});


			StorageDbMgr.Insert("metadata_properties", new Dictionary<string, object>() {
				{"id", "name"},
				{"name", "Name"},
				{"hidden", false},
				{"indexable", false},
				{"predefined", false},
				{"datatype", "string"},
				{"compact_id", 1},
				{"value_location", "ValueTable"}
			});

			StorageDbMgr.Insert("metadata_properties", new Dictionary<string, object>() {
				{"id", "birthday"},
				{"name", "Birthday"},
				{"hidden", false},
				{"indexable", false},
				{"predefined", false},
				{"datatype", "date"},
				{"compact_id", 2},
				{"value_location", "ValueTable"}
			});
			StorageDbMgr.Insert("metadata_properties", new Dictionary<string, object>() {
				{"id", "is_primary"},
				{"name", "Is Primary?"},
				{"hidden", false},
				{"indexable", false},
				{"predefined", false},
				{"datatype", "boolean"},
				{"compact_id", 3},
				{"value_location", "ValueTable"}
			});
			StorageDbMgr.Insert("metadata_properties", new Dictionary<string, object>() {
				{"id", "id"},
				{"name", "ID"},
				{"hidden", false},
				{"indexable", false},
				{"predefined", true},
				{"datatype", "integer"},
				{"compact_id", 4},
				{"primary_key", true},
				{"value_location", "ObjectTableColumn"}
			});

			StorageDbMgr.Insert("metadata_property_to_class", new Dictionary<string, object>() {
				{"property_id", "name"},
				{"class_id", "companies"} } );
			StorageDbMgr.Insert("metadata_property_to_class", new Dictionary<string, object>() {
				{"property_id", "is_primary"},
				{"class_id", "contacts"} });
			StorageDbMgr.Insert("metadata_property_to_class", new Dictionary<string, object>() {
				{"property_id", "name"},
				{"class_id", "contacts"} });
			StorageDbMgr.Insert("metadata_property_to_class", new Dictionary<string, object>() {
				{"property_id", "birthday"},
				{"class_id", "contacts"} });
			StorageDbMgr.Insert("metadata_property_to_class", new Dictionary<string, object>() {
				{"property_id", "name"},
				{"class_id", "countries"} });

			StorageDbMgr.Insert("metadata_property_to_class", new Dictionary<string, object>() {
				{"property_id", "id"},
				{"class_id", "countries"} });
			StorageDbMgr.Insert("metadata_property_to_class", new Dictionary<string, object>() {
				{"property_id", "id"},
				{"class_id", "contacts"} });
			StorageDbMgr.Insert("metadata_property_to_class", new Dictionary<string, object>() {
				{"property_id", "id"},
				{"class_id", "companies"} });
		}

		void InitDbSchema() {

			InternalDalc.ExecuteNonQuery(@"
				CREATE TABLE [metadata_classes]  (
					[id] TEXT PRIMARY KEY,
					[name] TEXT,
					[hidden] INTEGER,
					[indexable] INTEGER,
					[predefined] INTEGER,
					[predicate] INTEGER,
					[compact_id] INTEGER,
					[object_location] TEXT
				)");

			InternalDalc.ExecuteNonQuery(@"
				CREATE TABLE [metadata_properties]  (
					[id] TEXT PRIMARY KEY,
					[name] TEXT,
					[datatype] TEXT,
					[hidden] INTEGER,
					[indexable] INTEGER,
					[predefined] INTEGER,
					[compact_id] INTEGER,
					[value_location] TEXT,
					[primary_key] INTEGER
				)");

			InternalDalc.ExecuteNonQuery(@"
				CREATE TABLE [metadata_class_relationships]  (
					[subject_class_id] TEXT,
					[predicate_class_id] TEXT,
					[object_class_id] TEXT,
					[subject_multiplicity] INTEGER,
					[object_multiplicity] INTEGER,
					PRIMARY KEY (subject_class_id,predicate_class_id,object_class_id)
				)");

			InternalDalc.ExecuteNonQuery(@"
				CREATE TABLE [metadata_property_to_class]  (
					[property_id] TEXT,
					[class_id] TEXT,
					PRIMARY KEY (property_id, class_id)
				)");

			
			InternalDalc.ExecuteNonQuery(@"
				CREATE TABLE [objects]  (
					[id] INTEGER PRIMARY KEY AUTOINCREMENT,
					[compact_class_id] INTEGER
				)");
			InternalDalc.ExecuteNonQuery(@"
				CREATE TABLE [objects_log]  (
					[id] INTEGER PRIMARY KEY AUTOINCREMENT,
					[compact_class_id] INTEGER,
					[object_id] INTEGER,
					[account_id] INTEGER,
					[timestamp] TEXT,
					[action] TEXT
				)");

			InternalDalc.ExecuteNonQuery(@"
				CREATE TABLE [object_relations]  (
					[subject_id] INTEGER,
					[predicate_class_compact_id] INTEGER,
					[object_id] INTEGER,
					PRIMARY KEY (subject_id,predicate_class_compact_id,object_id)
				)");
			InternalDalc.ExecuteNonQuery(@"
				CREATE TABLE [object_relations_log]  (
					[id] INTEGER PRIMARY KEY AUTOINCREMENT,
					[subject_id] INTEGER,
					[predicate_class_compact_id] INTEGER,
					[object_id] INTEGER,
					[account_id] INTEGER,
					[timestamp] TEXT,
					[deleted] INTEGER
				)");

			var valueTableCreateSqlTemplate = @"
				CREATE TABLE [{0}]  (
					[id] INTEGER PRIMARY KEY AUTOINCREMENT,
					[object_id] INTEGER,
					[property_compact_id] INTEGER,
					[value] {1}
				)
			";

			var valueLogTableCreateSqlTemplate = @"
				CREATE TABLE [{0}]  (
					[id] INTEGER PRIMARY KEY AUTOINCREMENT,
					[object_id] INTEGER,
					[property_compact_id] INTEGER,
					[value] {1},
					[account_id] INTEGER,
					[timestamp] TEXT,
					[deleted] INTEGER
				)
			";

			InternalDalc.ExecuteNonQuery(String.Format(valueTableCreateSqlTemplate, "object_datetime_values", "TEXT"));
			InternalDalc.ExecuteNonQuery(String.Format(valueTableCreateSqlTemplate, "object_decimal_values", "REAL"));
			InternalDalc.ExecuteNonQuery(String.Format(valueTableCreateSqlTemplate, "object_integer_values", "INTEGER"));
			InternalDalc.ExecuteNonQuery(String.Format(valueTableCreateSqlTemplate, "object_string_values", "TEXT"));

			InternalDalc.ExecuteNonQuery(String.Format(valueLogTableCreateSqlTemplate, "object_datetime_values_log", "TEXT"));
			InternalDalc.ExecuteNonQuery(String.Format(valueLogTableCreateSqlTemplate, "object_decimal_values_log", "REAL"));
			InternalDalc.ExecuteNonQuery(String.Format(valueLogTableCreateSqlTemplate, "object_integer_values_log", "INTEGER"));
			InternalDalc.ExecuteNonQuery(String.Format(valueLogTableCreateSqlTemplate, "object_string_values_log", "TEXT"));

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

		protected DataSet CreateStorageSchemaDS() {
			var ds = new DataSet();

			ds.Tables.Add(DataSetStorageContext.CreateValueTable("object_datetime_values", typeof(DateTime)));
			ds.Tables.Add(DataSetStorageContext.CreateValueLogTable("object_datetime_values_log", typeof(DateTime)));
			
			ds.Tables.Add(DataSetStorageContext.CreateValueTable("object_decimal_values", typeof(decimal)));
			ds.Tables.Add(DataSetStorageContext.CreateValueLogTable("object_decimal_values_log", typeof(decimal)));

			ds.Tables.Add(DataSetStorageContext.CreateValueTable("object_integer_values", typeof(long)));
			ds.Tables.Add(DataSetStorageContext.CreateValueLogTable("object_integer_values_log", typeof(long)));

			ds.Tables.Add(DataSetStorageContext.CreateValueTable("object_string_values", typeof(string)));
			ds.Tables.Add(DataSetStorageContext.CreateValueLogTable("object_string_values_log", typeof(string)));

			ds.Tables.Add(DataSetStorageContext.CreateRelationsTable());
			ds.Tables.Add(DataSetStorageContext.CreateRelationsLogTable());

			ds.Tables.Add(DataSetStorageContext.CreateObjectTable());
			ds.Tables.Add(DataSetStorageContext.CreateObjectLogTable());

			ds.Tables.Add(DataSetStorageContext.CreateMetadataClassTable());
			ds.Tables.Add(DataSetStorageContext.CreateMetadataPropertyTable());
			ds.Tables.Add(DataSetStorageContext.CreateMetadataPropertyToClassTable());

			ds.Tables.Add(DataSetStorageContext.CreateMetadataRelationshipTable());
			return ds;
		}


	}
}
