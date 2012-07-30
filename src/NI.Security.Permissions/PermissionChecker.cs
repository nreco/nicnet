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

using NI.Common.Providers;
using NI.Common;

namespace NI.Security.Permissions
{
	/// <summary>
	/// Generic permission checker implementation
	/// </summary>
	public class PermissionChecker : IPermissionChecker, IBooleanProvider
	{
		bool _DefaultCheckResult = true;
	
		IPermissionAclEntry[] _AllowAclEntries = new IPermissionAclEntry[0];
		IPermissionAclEntry[] _DenyAclEntries = new IPermissionAclEntry[0];
		

		public IPermissionAclEntry[] AllowAclEntries {
			get { return _AllowAclEntries; }
			set { _AllowAclEntries = value; }
		}
		
		public IPermissionAclEntry[] DenyAclEntries {
			get { return _DenyAclEntries; }
			set { _DenyAclEntries = value; }
		}

		/// <summary>
		/// Get or set check result in case when neither deny nor allow entry is found.
		/// </summary>
		public bool DefaultCheckResult {
			get { return _DefaultCheckResult; }
			set { _DefaultCheckResult = value; }
		}
		
		
		public PermissionChecker()
		{

		}

		public virtual bool Check(Permission permission) {
			// check in deny ACL entries
			for (int i=0; i<DenyAclEntries.Length; i++)
				if (DenyAclEntries[i].IsMatch(permission))
					if (DenyAclEntries[i].Check(permission))
						return false;
			
			// check in allow ACL entries
			for (int i=0; i<AllowAclEntries.Length; i++)
				if (AllowAclEntries[i].IsMatch(permission))
					if (AllowAclEntries[i].Check(permission))
						return true;
			
			return DefaultCheckResult;
		}

		public virtual bool[] Check(Permission[] permissions) {
			bool[] results = new bool[permissions.Length];
			for (int i=0; i<permissions.Length; i++)
				results[i] = Check(permissions[i]);
			return results;
		}


		public bool GetBoolean(object contextObj) {
			if (contextObj is Permission) {
				return Check( (Permission)contextObj );
			}
			throw new ArgumentException("Expected context type is Permission", "contextObj");
		}

	}
}
