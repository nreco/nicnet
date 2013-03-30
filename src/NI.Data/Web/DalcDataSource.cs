#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
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
using System.Data;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NI.Data;

namespace NI.Data.Web {

	public class DalcDataSource : DataSourceControl {
		string _SourceName;
		string _SelectSourceName = null;
		IDalc _Dalc;
		QueryNode _Condition = null;
		bool _DataSetMode = false;
		bool _InsertMode = false;
		string[] _AutoIncrementNames = null;
		string[] _DataKeyNames = null;

		public event DalcDataSourceSelectEventHandler Selecting;
		public event DalcDataSourceSelectEventHandler Selected;
		public event DalcDataSourceSaveEventHandler Updating;
		public event DalcDataSourceSaveEventHandler Updated;
		public event DalcDataSourceSaveEventHandler Inserting;
		public event DalcDataSourceSaveEventHandler Inserted;
		public event DalcDataSourceSaveEventHandler Deleting;
		public event DalcDataSourceSaveEventHandler Deleted;

		/// <summary>
		/// Determines whether datasource should use DataSet for all operations (false by default).
		/// </summary>
		public bool DataSetMode {
			get { return _DataSetMode; }
			set { _DataSetMode = value; }
		}

		/// <summary>
		/// Determines whether datasource should return one new row if no records found (false by default).
		/// </summary>
		public bool InsertMode {
			get { return _InsertMode; }
			set { _InsertMode = value; }
		}


		/// <summary>
		/// Get or set list of autoincrement field names (optional).
		/// </summary>
		/// <remarks>
		/// Instead of setting this property DataSetProvider may be used for providing all
		/// required information about data schema.
		/// </remarks>
		[TypeConverterAttribute(typeof(StringArrayConverter))]
		public string[] AutoIncrementNames {
			get { return _AutoIncrementNames; }
			set { _AutoIncrementNames = value; }
		}

		/// <summary>
		/// Get or set list of primary key field names (optional).
		/// </summary>
		/// <remarks>
		/// Instead of setting this property DataSetProvider may be used for providing all
		/// required information about data schema.
		/// </remarks>
		[TypeConverterAttribute(typeof(StringArrayConverter))]
		public string[] DataKeyNames {
			get { return _DataKeyNames; }
			set { _DataKeyNames = value; }
		}

		/// <summary>
		/// Get or set sourcename for this datasource (required).
		/// </summary>
		public string SourceName {
			get { return _SourceName; }
			set { _SourceName = value; }
		}

		/// <summary>
		/// Get or set sourcename for select action (optional).
		/// </summary>
		public string SelectSourceName {
			get { return _SelectSourceName == null ? SourceName : _SelectSourceName; }
			set { _SelectSourceName = value; }
		}

		/// <summary>
		/// Get or set DALC instance for this datasource (required).
		/// </summary>
		public IDalc Dalc {
			get { return _Dalc; }
			set { _Dalc = value; }
		}

		/// <summary>
		/// Get or set DataSet instance provider for specificed sourcename (optional).
		/// </summary>
		public Func<string, DataSet> DataSetProvider { get; set; }

		/// <summary>
		/// Get or set data condition (optional).
		/// </summary>
		public QueryNode Condition {
			get { return _Condition; }
			set { _Condition = value; }
		}

		public DalcDataSource() { }
		
		public Query GetSelectQuery(string viewName) {
			Query q = new Query(viewName == SourceName ? SelectSourceName : viewName);
			q.Condition = Condition;
			DataSourceSelectArguments selectArgs = new DataSourceSelectArguments();
			DataSet ds = DataSetProvider(viewName);
			DalcDataSourceSelectEventArgs eArgs = new DalcDataSourceSelectEventArgs(q, selectArgs, ds);
			// raise event
			OnSelecting(this, eArgs);
			return q;
		}
		
		protected override DataSourceView GetView(string viewName) {
			return new DalcDataSourceView(this, String.IsNullOrEmpty(viewName) ? SourceName : viewName );
		}

		internal void OnSelecting(object sender, DalcDataSourceSelectEventArgs e) {
			if (Selecting != null)
				Selecting(sender, e);
		}
		internal void OnSelected(object sender, DalcDataSourceSelectEventArgs e) {
			if (Selected != null)
				Selected(sender, e);
		}

		internal void OnUpdating(object sender, DalcDataSourceSaveEventArgs e) {
			if (Updating != null)
				Updating(sender, e);
		}
		internal void OnUpdated(object sender, DalcDataSourceSaveEventArgs e) {
			if (Updated != null)
				Updated(sender, e);
		}

		internal void OnInserting(object sender, DalcDataSourceSaveEventArgs e) {
			if (Inserting != null)
				Inserting(sender, e);
		}
		internal void OnInserted(object sender, DalcDataSourceSaveEventArgs e) {
			if (Inserted != null)
				Inserted(sender, e);
		}

		internal void OnDeleting(object sender, DalcDataSourceSaveEventArgs e) {
			if (Deleting != null)
				Deleting(sender, e);
		}
		internal void OnDeleted(object sender, DalcDataSourceSaveEventArgs e) {
			if (Deleted != null)
				Deleted(sender, e);
		}


	}

	public delegate void DalcDataSourceSelectEventHandler(object sender, DalcDataSourceSelectEventArgs e);
	public delegate void DalcDataSourceSaveEventHandler(object sender, DalcDataSourceSaveEventArgs e);

	public class DalcDataSourceSaveEventArgs : CancelEventArgs {
		string _SourceName;
		IDictionary _OldValues;
		IDictionary _Values;
		IDictionary _Keys;
		int _AffectedCount;

		public string SourceName {
			get { return _SourceName; }
			set { _SourceName = value; }
		}

		public IDictionary OldValues {
			get { return _OldValues; }
			set { _OldValues = value; }
		}

		public IDictionary Values {
			get { return _Values; }
			set { _Values = value; } 
		}

		public IDictionary Keys {
			get { return _Keys; }
			set { _Keys = value; }
		}
		public int AffectedCount {
			get { return _AffectedCount; }
			internal set { _AffectedCount = value; }
		}

		public DalcDataSourceSaveEventArgs(string sourcename, IDictionary keys, IDictionary oldValues, IDictionary newValues) {
			SourceName = sourcename;
			Keys = keys;
			OldValues = oldValues;
			Values = newValues;
		}
	}

	public class DalcDataSourceSelectEventArgs : CancelEventArgs {
		Query _SelectQuery;
		DataSet _Data;
		DataSourceSelectArguments _SelectArgs;

		public Query SelectQuery {
			get { return _SelectQuery; }
			set { _SelectQuery = value; }
		}
		
		public DataSet Data {
			get { return _Data; }
			set { _Data = value; }
		}

		public DataSourceSelectArguments SelectArgs {
			get { return _SelectArgs; }
			set { _SelectArgs = value; } 
		}

        public int FetchedRowCount {
            get { return Data.Tables[SelectQuery.SourceName].Rows.Count; }
        }

		public DalcDataSourceSelectEventArgs(Query q, DataSourceSelectArguments args, DataSet ds) {
			SelectQuery = q;
			SelectArgs = args;
			Data = ds;
		}
	}




}
