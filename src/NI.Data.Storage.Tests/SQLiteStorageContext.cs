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

		DbDalc StorageDalc;
		public IDbConnection Connection;
		string dbFileName;
		public DataRowDalcMapper StorageDbMgr;
		public IObjectContainerStorage ObjectContainerStorage;
		public DataSchemaDalcStorage DataSchemaStorage;

		public SQLiteStorageContext() {
			dbFileName = Path.GetTempFileName() + ".db";
			var connStr = String.Format("Data Source={0};FailIfMissing=false;Pooling=False;", dbFileName);
			StorageDalc = new DbDalc(new SQLiteDalcFactory(), connStr);

			InitDbSchema();

			StorageDbMgr = new DataRowDalcMapper(StorageDalc, new StorageDataSetPrv(CreateStorageSchemaDS()).GetDataSet);
			DataSchemaStorage = new DataSchemaDalcStorage(StorageDbMgr);
			ObjectContainerStorage = new ObjectContainerDalcStorage(StorageDbMgr, StorageDalc, DataSchemaStorage.GetSchema);
		}


		public void Destroy() {
			((SQLiteConnection)Connection).Dispose();
			SQLiteConnection.ClearAllPools();
			GC.Collect();
			if (dbFileName != null && File.Exists(dbFileName))
				File.Delete(dbFileName);

		}

		void InitDbSchema() {

			StorageDalc.ExecuteNonQuery(@"
				CREATE TABLE [metadata_classes]  (
					[id] TEXT PRIMARY KEY,
					[name] TEXT,
					[hidden] INTEGER,
					[indexable] INTEGER,
					[predefined] INTEGER,
					[predicate] INTEGER,
					[compact_id] INTEGER
				)");

			StorageDalc.ExecuteNonQuery(@"
				CREATE TABLE [metadata_properties]  (
					[id] TEXT PRIMARY KEY,
					[name] TEXT,
					[hidden] INTEGER,
					[indexable] INTEGER,
					[predefined] INTEGER,
					[compact_id] INTEGER
				)");

			StorageDalc.ExecuteNonQuery(@"
				CREATE TABLE [metadata_class_relationships]  (
					[subject_class_id] INTEGER PRIMARY KEY,
					[predicate_class_id] INTEGER PRIMARY KEY,
					[object_class_id] INTEGER PRIMARY KEY,
					[subject_multiplicity] INTEGER,
					[object_multiplicity] INTEGER
				)");

			StorageDalc.ExecuteNonQuery(@"
				CREATE TABLE [metadata_property_to_class]  (
					[property_id] TEXT PRIMARY KEY,
					[class_id] TEXT PRIMARY KEY
				)");

			
			StorageDalc.ExecuteNonQuery(@"
				CREATE TABLE [objects]  (
					[id] INTEGER PRIMARY KEY AUTOINCREMENT,
					[compact_class_id] INTEGER
				)");
			StorageDalc.ExecuteNonQuery(@"
				CREATE TABLE [objects_log]  (
					[id] INTEGER PRIMARY KEY AUTOINCREMENT,
					[compact_class_id] INTEGER,
					[object_id] INTEGER,
					[account_id] INTEGER,
					[timestamp] INTEGER,
					[action] TEXT
				)");

			StorageDalc.ExecuteNonQuery(@"
				CREATE TABLE [object_relations]  (
					[subject_id] INTEGER PRIMARY KEY,
					[predicate_class_compact_id] INTEGER PRIMARY KEY,
					[object_id] INTEGER PRIMARY KEY
				)");
			StorageDalc.ExecuteNonQuery(@"
				CREATE TABLE [object_relations_log]  (
					[id] INTEGER PRIMARY KEY AUTOINCREMENT,
					[subject_id] INTEGER,
					[predicate_class_compact_id] INTEGER,
					[object_id] INTEGER,
					[account_id] INTEGER,
					[timestamp] INTEGER,
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
					[timestamp] INTEGER,
					[deleted] INTEGER
				)
			";

			StorageDalc.ExecuteNonQuery(String.Format(valueTableCreateSqlTemplate, "object_datetime_values", "INTEGER"));
			StorageDalc.ExecuteNonQuery(String.Format(valueTableCreateSqlTemplate, "object_number_values", "REAL"));
			StorageDalc.ExecuteNonQuery(String.Format(valueTableCreateSqlTemplate, "object_string_values", "TEXT"));

			StorageDalc.ExecuteNonQuery(String.Format(valueLogTableCreateSqlTemplate, "object_datetime_values_log", "INTEGER"));
			StorageDalc.ExecuteNonQuery(String.Format(valueLogTableCreateSqlTemplate, "object_number_values_log", "REAL"));
			StorageDalc.ExecuteNonQuery(String.Format(valueLogTableCreateSqlTemplate, "object_string_values_log", "TEXT"));

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
			ds.Tables.Add(DataSetStorageContext.CreateValueTable("object_number_values", typeof(decimal)));
			ds.Tables.Add(DataSetStorageContext.CreateValueLogTable("object_number_values_log", typeof(decimal)));
			ds.Tables.Add(DataSetStorageContext.CreateValueTable("object_string_values", typeof(string)));
			ds.Tables.Add(DataSetStorageContext.CreateValueLogTable("object_string_values_log", typeof(string)));

			ds.Tables.Add(DataSetStorageContext.CreateRelationsTable());
			ds.Tables.Add(DataSetStorageContext.CreateRelationsLogTable());

			ds.Tables.Add(DataSetStorageContext.CreateObjectTable());
			ds.Tables.Add(DataSetStorageContext.CreateObjectLogTable());
			return ds;
		}


	}
}
