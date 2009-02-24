#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2008 NewtonIdeas
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Web;
using System.Web.UI;
using NI.Data.Dalc;

namespace NI.Data.Dalc.Web {
	
	public class DalcDataSourceView : DataSourceView {
		
		DalcDataSource _DataSource;
		protected DalcDataSource DataSource {
			get { return _DataSource; }
			set { _DataSource = value; }
		}

		public DalcDataSourceView(DalcDataSource owner, string sourceName) : base(owner, sourceName) {
			DataSource = owner;
		}

		protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments) {
			Query q = new Query(Name);
			q.Root = DataSource.Condition;
			if (!String.IsNullOrEmpty(arguments.SortExpression))
				q.Sort = arguments.SortExpression.Split(',');
			DataSet ds = new DataSet();

			var eArgs = new DalcDataSourceSelectEventArgs { SelectQuery = q, SelectArgs = arguments, Data = ds };
			// raise event
			DataSource.OnSelecting(this, eArgs);

			if (arguments.RetrieveTotalRowCount) {
				arguments.TotalRowCount = DataSource.Dalc.RecordsCount(q.SourceName, q.Root);
			}

			q.StartRecord = arguments.StartRowIndex;
			if (arguments.MaximumRows>0)
				q.RecordCount = arguments.MaximumRows;
			DataSource.Dalc.Load(ds, q);
			// raise event
			DataSource.OnSelected(this, eArgs);
			return ds.Tables[q.SourceName].DefaultView;
		}

		protected IQueryNode ComposeUidCondition(IDictionary keys) {
			// compose UID condition
			var uidGroup = new QueryGroupNode(GroupType.And);
			foreach (DictionaryEntry key in keys)
				uidGroup.Nodes.Add(new QField(key.Key.ToString()) == new QConst(key.Value));
			return uidGroup;
		}

		protected override int ExecuteInsert(IDictionary values) {
			var eArgs = new DalcDataSourceSaveEventArgs { Values = values, SourceName = Name };
			DataSource.OnInserting(this, eArgs);
			if (DataSource.DataSetMode) {
				DataSet ds = new DataSet();
				DataSource.Dalc.Load(ds, new Query(Name, new QueryConditionNode((QConst)1, Conditions.Equal, (QConst)2)));
				DataTable tbl = ds.Tables[Name];
				EnsureDataSchema(tbl);
				
				DataRow r = tbl.NewRow();
				foreach (DataColumn col in tbl.Columns)
					if (values.Contains(col.ColumnName))
						r[col] = values[col.ColumnName];
				tbl.Rows.Add(r);
				DataSource.Dalc.Update(ds, Name);
				// push back autoincrement field
				if (DataSource.AutoIncrementNames != null)
					foreach (string autoIncName in DataSource.AutoIncrementNames)
						values[autoIncName] = r[autoIncName];
				
			} else {
				DataSource.Dalc.Insert(values, Name);
			}
			DataSource.OnInserted(this, eArgs);
			return 1;
		}

		protected void EnsureDataSchema(DataTable tbl) {
			List<DataColumn> pkCols = new List<DataColumn>();
			if (DataSource.DataKeyNames!=null)
				foreach (string keyName in DataSource.DataKeyNames)
					pkCols.Add(tbl.Columns[keyName]);
			tbl.PrimaryKey = pkCols.ToArray();
			if (DataSource.AutoIncrementNames != null)
				foreach (string autoIncName in DataSource.AutoIncrementNames)
					tbl.Columns[autoIncName].AutoIncrement = true;
		}

		protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues) {
			var eArgs = new DalcDataSourceSaveEventArgs { Values = values, SourceName = Name, OldValues = oldValues, Keys = keys };
			DataSource.OnUpdating(this, eArgs);
			var uidCondition = ComposeUidCondition(keys);
			if (DataSource.DataSetMode) {
				DataSet ds = new DataSet();
				DataSource.Dalc.Load(ds, new Query(Name, uidCondition));
				EnsureDataSchema(ds.Tables[Name]);

				eArgs.AffectedCount = ds.Tables[Name].Rows.Count;
				foreach (DataRow r in ds.Tables[Name].Rows)
					foreach (DataColumn col in ds.Tables[Name].Columns)
						if (values.Contains(col.ColumnName))
							r[col] = values[col.ColumnName];
				DataSource.Dalc.Update(ds, Name);
			} else {
				eArgs.AffectedCount = DataSource.Dalc.Update(values, new Query(Name, uidCondition));
			}
			// raise event
			DataSource.OnUpdated(this, eArgs);
			return eArgs.AffectedCount;
		}

		protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues) {
			var eArgs = new DalcDataSourceSaveEventArgs { SourceName = Name, OldValues = oldValues, Keys = keys };
			DataSource.OnDeleting(this, eArgs);
			var uidCondition = ComposeUidCondition(keys);

			if (DataSource.DataSetMode) {
				DataSet ds = new DataSet();
				DataSource.Dalc.Load(ds, new Query(Name, uidCondition));
				EnsureDataSchema(ds.Tables[Name]);
				eArgs.AffectedCount = ds.Tables[Name].Rows.Count;
				foreach (DataRow r in ds.Tables[Name].Rows)
					r.Delete();
				DataSource.Dalc.Update(ds, Name);

			} else {
				eArgs.AffectedCount = DataSource.Dalc.Delete(new Query(Name, uidCondition));
			}

			// raise event
			DataSource.OnDeleted(this, eArgs);
			return eArgs.AffectedCount;
		}

		public override bool CanDelete {
			get {
				return true;
			}
		}

		public override bool CanInsert {
			get {
				return true;
			}
		}

		public override bool CanUpdate {
			get {
				return true;
			}
		}

		public override bool CanPage {
			get {
				return true;
			}
		}

		public override bool CanRetrieveTotalRowCount {
			get {
				return true;
			}
		}

		public override bool CanSort {
			get {
				return true;
			}
		}

	}
}
