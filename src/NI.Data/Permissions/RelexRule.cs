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
using System.Collections;
using System.Text;

namespace NI.Data.Permissions {
	
	/// <summary>
	/// Generic query rule
	/// </summary>
	public class RelexRule : IDalcQueryRule {

		public string SourceName { get; set; }

		public DalcOperation Operation { get; set; }

		public bool IsMatch(PermissionContext context) {
			if ( (Operation & context.Operation)!=context.Operation )
				return false;

			return true;
		}

		public QueryNode ComposeCondition(PermissionContext context) {
			return null;
		}

	}
		
}
