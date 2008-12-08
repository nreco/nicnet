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

using NI.Common;
using NI.Common.Providers;

namespace NI.Security.Permissions
{
	/// <summary>
	/// Generic permission ACL entry based on groups and explicit permissions list.
	/// </summary>
	public class PermissionAclEntry : IPermissionAclEntry {

		// by default initialized with providers that return one group - object itself
		IObjectListProvider _SubjectGroupListProvider = new GroupListProvider();
		IObjectListProvider _OperationGroupListProvider = new GroupListProvider();
		IObjectListProvider _ObjectGroupListProvider = new GroupListProvider();        
		Permission[] _Permissions;
        IPermissionListProvider _PermissionListProvider;
        private IBooleanProvider _IsMatchProvider = null;

		/// <summary>
		/// Get or set explicit permissions list for this ACL entry (obsolete, use provider instead)
		/// </summary>
		[Dependency(Required=false)]
		public Permission[] Permissions {
			get { return _Permissions; }
			set { _Permissions = value; }
		}

        /// <summary>
        /// Get or set permissions list provider
        /// </summary>
        [Dependency(Required=false)]
        public IPermissionListProvider PermissionListProvider {
            get { return _PermissionListProvider; }
            set { _PermissionListProvider = value;  }
        }
		
		/// <summary>
		/// Subject groups list provider by subject passed as context
		/// </summary>
		[Dependency]
		public IObjectListProvider SubjectGroupListProvider {
			get { return _SubjectGroupListProvider; }
			set { _SubjectGroupListProvider = value; }
		}
		
		/// <summary>
		/// Operation groups list provider by operation passed as context
		/// </summary>
		[Dependency]
		public IObjectListProvider OperationGroupListProvider {
			get { return _OperationGroupListProvider; }
			set { _OperationGroupListProvider = value; }
		}
		
		/// <summary>
		/// Object groups list provider by object passed as context
		/// </summary>		
		[Dependency]
		public IObjectListProvider ObjectGroupListProvider {
			get { return _ObjectGroupListProvider; }
			set { _ObjectGroupListProvider = value; }
		}

        /// <summary>
        /// Provider for deciding whether current permission matches this acl entry;
        /// null by default (i.e. permission always matches) 
        /// </summary>		
        public IBooleanProvider IsMatchProvider
        {
            get { return _IsMatchProvider; }
            set { _IsMatchProvider = value; }
        }
		
		public PermissionAclEntry() {
		}
		
		public bool IsMatch(Permission permission) 
        {
            return IsMatchProvider == null || IsMatchProvider.GetBoolean(permission);
		}
		
		public bool Check(Permission permission) {

			IList subjGroups = SubjectGroupListProvider.GetObjectList(permission);
			IList opGroups = OperationGroupListProvider.GetObjectList(permission);
			IList objGroups = ObjectGroupListProvider.GetObjectList(permission);
            Permission[] permissions = Permissions != null ? 
                    Permissions : PermissionListProvider.GetPermissionList(null);

			foreach (Permission permissionInstance in permissions) {
				if (subjGroups.Contains(permissionInstance.Subject) &&
					opGroups.Contains(permissionInstance.Operation) &&
					objGroups.Contains(permissionInstance.Object))
					return true;
			}
			
			return false;
		}

		
		
	}
}
