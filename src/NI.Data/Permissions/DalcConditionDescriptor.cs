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
using System.Collections;
using System.Text;

namespace NI.Data.Permissions {
	
	/// <summary>
	/// Generic implementation of IDalcConditionDescriptor interface
	/// </summary>
	public class DalcConditionDescriptor : IDalcConditionDescriptor {
		DalcOperation _Operation;
		string _SourceName;
		IQueryNodeProvider _ConditionProvider;

		public DalcOperation Operation {
			get { return _Operation; }
			set { _Operation = value; }
		}

		public string SourceName {
			get { return _SourceName; }
			set { _SourceName = value; }
		}

		public IQueryNodeProvider ConditionProvider {
			get { return _ConditionProvider; }
			set { _ConditionProvider = value; }
		}

	}
		
}
