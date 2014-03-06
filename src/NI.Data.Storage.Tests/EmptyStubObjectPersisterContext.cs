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
	public class EmptyStubObjectPersisterContext {

		public DataSet StorageDS;
		DataSetDalc StorageDalc;
		public DataRowDalcMapper StorageDbMgr;
		public ObjectPersister ObjectPersisterInstance;

		public EmptyStubObjectPersisterContext(Func<Ontology> ontologyPrv) {
			InitStorageDS();
			StorageDalc = new DataSetDalc(StorageDS);

			StorageDbMgr = new DataRowDalcMapper(StorageDalc, new StorageDataSetPrv(StorageDS).GetDataSet );
			ObjectPersisterInstance = new ObjectPersister(StorageDbMgr, StorageDalc, ontologyPrv);
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

	}
}
