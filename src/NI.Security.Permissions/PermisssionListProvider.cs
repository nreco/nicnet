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
using NI.Common;
using NI.Common.Providers;
namespace NI.Security.Permissions {
    /// <summary>
    /// Permission list provider based on IDictionaryListProvider 
    /// </summary>
    public class PermissionListProvider: IPermissionListProvider {

        private IDictionaryListProvider _UnderlyingDictionaryListProvider;
        private string _SubjectKey;
        private string _OperationKey;
        private string _ObjectKey;

        public string SubjectKey {
            get { return _SubjectKey; }
            set { _SubjectKey = value; }
        }

        public string OperationKey {
            get { return _OperationKey; }
            set { _OperationKey = value; }
        }

        public string ObjectKey {
            get { return _ObjectKey; }
            set { _ObjectKey = value; }
        }

        public IDictionaryListProvider UnderlyingDictionaryListProvider {
            get { return _UnderlyingDictionaryListProvider; }
            set { _UnderlyingDictionaryListProvider = value;  }
        }

        public Permission[] GetPermissionList(object context) {
            IDictionary[] permissionDictionaries = UnderlyingDictionaryListProvider.GetDictionaryList(context);
            ArrayList permissions = new ArrayList();
            foreach (IDictionary permission in permissionDictionaries) {
                permissions.Add(new Permission(permission[SubjectKey], 
                                    permission[OperationKey], permission[ObjectKey]));
                
            } 
            return (Permission[])permissions.ToArray(typeof(Permission));
        }
    }
}
