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
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using NI.Data.Dalc;

namespace NI.Data.Dalc.Web {

	public class DalcDataSource : DataSourceControl {
		string _SourceName;
		IDalc _Dalc;
		IQueryNode _Condition = null;

		public event DalcDataSourceSelectEventHandler Selecting;
		public event DalcDataSourceSelectEventHandler Selected;
		public event DalcDataSourceSaveEventHandler Updating;
		public event DalcDataSourceSaveEventHandler Updated;
		public event DalcDataSourceSaveEventHandler Inserting;
		public event DalcDataSourceSaveEventHandler Inserted;
		public event DalcDataSourceSaveEventHandler Deleting;
		public event DalcDataSourceSaveEventHandler Deleted;

		public string SourceName {
			get { return _SourceName; }
			set { _SourceName = value; }
		}

		public IDalc Dalc {
			get { return _Dalc; }
			set { _Dalc = value; }
		}

		public IQueryNode Condition {
			get { return _Condition; }
			set { _Condition = value; }
		}

		public DalcDataSource() { }

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

	public class DalcDataSourceSaveEventArgs {
		public string SourceName { get; set; }
		public IDictionary OldValues { get; set; }
		public IDictionary Values { get; set; }
		public IDictionary Keys { get; set; }
		public int AffectedCount { get; internal set; }
	}

	public class DalcDataSourceSelectEventArgs {
		public Query SelectQuery { get; set; }
		public DataSet Data { get; set; }
		public DataSourceSelectArguments SelectArgs { get; set; }
	}




}
