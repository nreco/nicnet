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
	
	/// <summary>
	/// DALC DataSource view.
	/// </summary>
	public class DalcDataSourceView : DataSourceView {
		
		DalcDataSource _DataSource;
		protected DalcDataSource DataSource {
			get { return _DataSource; }
			set { _DataSource = value; }
		}

		public DalcDataSourceView(DalcDataSource owner, string sourceName) : base(owner, sourceName) {
			DataSource = owner;
		}

		protected virtual DataSet GetDataSet() {
			if (DataSource.DataSetProvider != null) {
				DataSet ds = DataSource.DataSetProvider.GetDataSet(Name);
				if (ds != null)
					return ds;
			}
			return new DataSet();
		}

		protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments) {
			Query q = new Query( Name==DataSource.SourceName ? DataSource.SelectSourceName : Name );
			q.Root = DataSource.Condition;
			if (!String.IsNullOrEmpty(arguments.SortExpression))
				q.Sort = arguments.SortExpression.Split(',');
			DataSet ds = GetDataSet();

			DalcDataSourceSelectEventArgs eArgs = new DalcDataSourceSelectEventArgs(q, arguments, ds);
			// raise event
			DataSource.OnSelecting(DataSource, eArgs);

			if (arguments.RetrieveTotalRowCount) {
				arguments.TotalRowCount = DataSource.Dalc.RecordsCount(q.SourceName, q.Root);
			}

			q.StartRecord = arguments.StartRowIndex;
			if (arguments.MaximumRows>0)
				q.RecordCount = arguments.MaximumRows;
			DataSource.Dalc.Load(ds, q);
			// raise event
			DataSource.OnSelected(DataSource, eArgs);

			if (ds.Tables[q.SourceName].Rows.Count == 0 && DataSource.InsertMode)
				ds.Tables[q.SourceName].Rows.Add(ds.Tables[q.SourceName].NewRow());
			
			return ds.Tables[q.SourceName].DefaultView;
		}

		protected IQueryNode ComposeUidCondition(IDictionary keys) {
			if (keys.Count == 0)
				throw new MissingPrimaryKeyException();
			// compose UID condition
			QueryGroupNode uidGroup = new QueryGroupNode(GroupType.And);
			foreach (DictionaryEntry key in keys)
				uidGroup.Nodes.Add(new QField(key.Key.ToString()) == new QConst(key.Value));
			return uidGroup;
		}

		protected override int ExecuteInsert(IDictionary values) {
			DalcDataSourceSaveEventArgs eArgs = new DalcDataSourceSaveEventArgs(Name, null, null, values);
			DataSource.OnInserting(DataSource, eArgs);
			if (DataSource.DataSetMode) {
				DataSet ds = GetDataSet();
				// if schema is unknown, lets try to load from datasource
				if (!ds.Tables.Contains(Name))
					DataSource.Dalc.Load(ds, new Query(Name, new QueryConditionNode((QConst)1, Conditions.Equal, (QConst)2)));
				DataTable tbl = ds.Tables[Name];
				EnsureDataSchema(tbl);
				
				DataRow r = tbl.NewRow();
				foreach (DataColumn col in tbl.Columns)
					if (values.Contains(col.ColumnName))
						r[col] = PrepareDataRowValue(values[col.ColumnName]);
				tbl.Rows.Add(r);
				DataSource.Dalc.Update(ds, Name);
				// push back into context all fields
				foreach (DataColumn c in tbl.Columns)
					values[c.ColumnName] = r[c];
				
			} else {
				DataSource.Dalc.Insert(values, Name);
			}
			DataSource.OnInserted(DataSource, eArgs);
			return 1;
		}

		protected void EnsureDataSchema(DataTable tbl) {
			if (DataSource.DataKeyNames != null) {
				List<DataColumn> pkCols = new List<DataColumn>();
				foreach (string keyName in DataSource.DataKeyNames)
					pkCols.Add(tbl.Columns[keyName]);
				tbl.PrimaryKey = pkCols.ToArray();
			}
			if (DataSource.AutoIncrementNames != null)
				foreach (string autoIncName in DataSource.AutoIncrementNames)
					tbl.Columns[autoIncName].AutoIncrement = true;
		}

		protected object PrepareDataRowValue(object o) {
			return o ?? DBNull.Value;
		}

		protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues) {
			DalcDataSourceSaveEventArgs eArgs = new DalcDataSourceSaveEventArgs(Name, keys, oldValues, values);
			DataSource.OnUpdating(DataSource, eArgs);
			IQueryNode uidCondition = ComposeUidCondition(keys);
			if (DataSource.DataSetMode) {
				DataSet ds = GetDataSet();
				DataSource.Dalc.Load(ds, new Query(Name, uidCondition));
				var tbl = ds.Tables[Name];
				EnsureDataSchema(tbl);

				eArgs.AffectedCount = tbl.Rows.Count;
				foreach (DataRow r in tbl.Rows)
					foreach (DataColumn col in tbl.Columns)
						if (values.Contains(col.ColumnName))
							r[col] = PrepareDataRowValue( values[col.ColumnName] );
				DataSource.Dalc.Update(ds, Name);
				// push back into context all fields (if only 1 record is updated)
				if (tbl.Rows.Count == 1)
					foreach (DataColumn c in tbl.Columns)
						values[c.ColumnName] = tbl.Rows[0][c];
			} else {
				eArgs.AffectedCount = DataSource.Dalc.Update(values, new Query(Name, uidCondition));
			}
			// raise event
			DataSource.OnUpdated(DataSource, eArgs);
			return eArgs.AffectedCount;
		}

		protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues) {
			DalcDataSourceSaveEventArgs eArgs = new DalcDataSourceSaveEventArgs(Name, keys, oldValues, oldValues);
			DataSource.OnDeleting(DataSource, eArgs);
			IQueryNode uidCondition = ComposeUidCondition(keys);

			if (DataSource.DataSetMode) {
				DataSet ds = GetDataSet();
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
			DataSource.OnDeleted(DataSource, eArgs);
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
