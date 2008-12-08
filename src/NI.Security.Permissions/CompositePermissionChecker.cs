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
using System.Text;

namespace NI.Security.Permissions {

	/// <summary>
	/// Logically combines two or more permissions checker (AND)
	/// </summary>
	public class CompositePermissionChecker : IPermissionChecker {
		IPermissionChecker[] _PermissionCheckers;

		public IPermissionChecker[] PermissionCheckers {
			get { return _PermissionCheckers; }
			set { _PermissionCheckers = value; }
		}

		public CompositePermissionChecker() {
		}

		public bool Check(Permission permission) {
			bool result = true;
			for (int i=0; i<PermissionCheckers.Length; i++)
				result &= PermissionCheckers[i].Check(permission);
			return result;
		}

		public bool[] Check(Permission[] permissions) {
			bool[] results = new bool[permissions.Length];
			for (int i=0; i<results.Length; i++) results[i] = true;
			
			for (int i = 0; i < PermissionCheckers.Length; i++) {
				bool[] checkerResults = PermissionCheckers[i].Check(permissions);
				for (int j = 0; j < results.Length; j++) results[j] &= checkerResults[j];
			}
			
			return results;
		}
	}	
	
}
