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

namespace NI.Data.Web {

	public class DalcDataSourceChangeEventArgs : CancelEventArgs {
		string _TableName;
		IDictionary _OldValues;
		IDictionary _Values;
		IDictionary _Keys;
		int _AffectedCount;

		public string TableName {
			get { return _TableName; }
			set { _TableName = value; }
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

		public DalcDataSourceChangeEventArgs(string tableName, IDictionary keys, IDictionary oldValues, IDictionary newValues) {
			TableName = tableName;
			Keys = keys;
			OldValues = oldValues;
			Values = newValues;
		}
	}
}
