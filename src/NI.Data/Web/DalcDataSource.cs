#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
 * Copyright 2008-2013 Vitalii Fedorchenko (changes and v.2)
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

	/// <summary>
	/// Enables the use of IDalc component in an ASP.NET Web page to retrieve and modify data from a DALC
	/// </summary>
	public class DalcDataSource : DataSourceControl {
		string _TableName;
		string _SelectTableName = null;
		IDalc _Dalc;
		QueryNode _Condition = null;
		bool _DataSetMode = false;
		bool _InsertMode = false;
		string[] _AutoIncrementNames = null;
		string[] _DataKeyNames = null;

		/// <summary>
		/// Occurs before a data-retrieval operation.
		/// </summary>
		public event EventHandler<DalcDataSourceSelectEventArgs> Selecting;

		/// <summary>
		/// Occurs when a data retrieval operation has finished.
		/// </summary>
		public event EventHandler<DalcDataSourceSelectEventArgs> Selected;
		
		/// <summary>
		/// Occurs before update operation
		/// </summary>
		public event EventHandler<DalcDataSourceChangeEventArgs> Updating;

		/// <summary>
		/// Occurs when update operation is finished
		/// </summary>
		public event EventHandler<DalcDataSourceChangeEventArgs> Updated;

		/// <summary>
		/// Occurs before insert operation
		/// </summary>
		public event EventHandler<DalcDataSourceChangeEventArgs> Inserting;

		/// <summary>
		/// Occurs when insert operation is finished
		/// </summary>
		public event EventHandler<DalcDataSourceChangeEventArgs> Inserted;

		/// <summary>
		/// Occurs before delete operation
		/// </summary>
		public event EventHandler<DalcDataSourceChangeEventArgs> Deleting;

		/// <summary>
		/// Occurs when delete operation is finished
		/// </summary>
		public event EventHandler<DalcDataSourceChangeEventArgs> Deleted;

		/// <summary>
		/// Determines whether datasource should use DataSet for insert/update/delete operations (false by default).
		/// </summary>
		public bool DataSetMode {
			get { return _DataSetMode; }
			set { _DataSetMode = value; }
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
		/// Get or set table name for this datasource (required).
		/// </summary>
		public string TableName {
			get { return _TableName; }
			set { _TableName = value; }
		}

		/// <summary>
		/// Get or set table name for select action (optional).
		/// </summary>
		public string SelectTableName {
			get { return _SelectTableName == null ? TableName : _SelectTableName; }
			set { _SelectTableName = value; }
		}

		/// <summary>
		/// Get or set DALC instance for this datasource (required).
		/// </summary>
		public IDalc Dalc {
			get { return _Dalc; }
			set { _Dalc = value; }
		}

		/// <summary>
		/// Get or set DataSet instance provider for specificed table name (optional).
		/// </summary>
		public Func<string, DataSet> CreateDataSet { get; set; }

		/// <summary>
		/// Get or set data retrieval condition (optional).
		/// </summary>
		public QueryNode Condition {
			get { return _Condition; }
			set { _Condition = value; }
		}

		public DalcDataSource() { }
		
		public Query GetSelectQuery(string viewName) {
			Query q = new Query(viewName == TableName ? SelectTableName : viewName);
			q.Condition = Condition;
			DataSourceSelectArguments selectArgs = new DataSourceSelectArguments();
			DataSet ds = CreateDataSet(viewName);
			DalcDataSourceSelectEventArgs eArgs = new DalcDataSourceSelectEventArgs(q, selectArgs, ds);
			// raise event
			OnSelecting(this, eArgs);
			return q;
		}
		
		protected override DataSourceView GetView(string viewName) {
			return new DalcDataSourceView(this, String.IsNullOrEmpty(viewName) ? TableName : viewName );
		}

		internal void OnSelecting(object sender, DalcDataSourceSelectEventArgs e) {
			if (Selecting != null)
				Selecting(sender, e);
		}
		internal void OnSelected(object sender, DalcDataSourceSelectEventArgs e) {
			if (Selected != null)
				Selected(sender, e);
		}

		internal void OnUpdating(object sender, DalcDataSourceChangeEventArgs e) {
			if (Updating != null)
				Updating(sender, e);
		}
		internal void OnUpdated(object sender, DalcDataSourceChangeEventArgs e) {
			if (Updated != null)
				Updated(sender, e);
		}

		internal void OnInserting(object sender, DalcDataSourceChangeEventArgs e) {
			if (Inserting != null)
				Inserting(sender, e);
		}
		internal void OnInserted(object sender, DalcDataSourceChangeEventArgs e) {
			if (Inserted != null)
				Inserted(sender, e);
		}

		internal void OnDeleting(object sender, DalcDataSourceChangeEventArgs e) {
			if (Deleting != null)
				Deleting(sender, e);
		}
		internal void OnDeleted(object sender, DalcDataSourceChangeEventArgs e) {
			if (Deleted != null)
				Deleted(sender, e);
		}


	}

}
