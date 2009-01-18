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
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using NI.Data.Dalc;

namespace NI.Data.Dalc.Web {

	public class DalcDataSource : DataSourceControl {
		string _SourceName;
		IDalc _Dalc;

		public string SourceName {
			get { return _SourceName; }
			set { _SourceName = value; }
		}

		public IDalc Dalc {
			get { return _Dalc; }
			set { _Dalc = value; }
		}

		public DalcDataSource() { }

		protected override DataSourceView GetView(string viewName) {
			return new DalcDataSourceView(this, String.IsNullOrEmpty(viewName) ? SourceName : viewName );
		}
	}
}
