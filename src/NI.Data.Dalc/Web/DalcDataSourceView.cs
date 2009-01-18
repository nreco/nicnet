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
			if (arguments.RetrieveTotalRowCount) {
				arguments.TotalRowCount = DataSource.Dalc.RecordsCount(q.SourceName, q.Root);
			}

			if (!String.IsNullOrEmpty(arguments.SortExpression))
				q.Sort = arguments.SortExpression.Split(',');
			q.StartRecord = arguments.StartRowIndex;
			if (arguments.MaximumRows>0)
				q.RecordCount = arguments.MaximumRows;
			DataSet ds = new DataSet();
			DataSource.Dalc.Load(ds, q);
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
			DataSource.Dalc.Insert(values, Name);
			return 1;
		}

		protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues) {
			var uidCondition = ComposeUidCondition(keys);
			return DataSource.Dalc.Update(values, new Query(Name, uidCondition));
		}

		protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues) {
			var uidCondition = ComposeUidCondition(keys);
			return DataSource.Dalc.Delete(new Query(Name, uidCondition));
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
