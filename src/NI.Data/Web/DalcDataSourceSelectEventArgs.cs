#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2014 NewtonIdeas
 * Copyright 2008-2014 Vitalii Fedorchenko (changes and v.2)
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
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Data;
using System.Web.UI;

namespace NI.Data.Web {

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
			get { return Data.Tables[SelectQuery.Table].Rows.Count; }
		}

		public DalcDataSourceSelectEventArgs(Query q, DataSourceSelectArguments args, DataSet ds) {
			SelectQuery = q;
			SelectArgs = args;
			Data = ds;
		}
	}


}
