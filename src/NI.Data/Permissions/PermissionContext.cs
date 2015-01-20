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
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Collections.Generic;
using System.Globalization;

namespace NI.Data.Permissions
{
	/// <summary>
	/// Represents query permission context
	/// </summary>
	public class PermissionContext
	{
		public string TableName { get; private set; }

		public DalcOperation Operation { get; private set; }

		public IPrincipal Principal { get; private set; }

		public PermissionContext(string tableName, DalcOperation operation) {
			TableName = tableName;
			Principal = Thread.CurrentPrincipal;
			Operation = operation;
		}

		public string IdentityName {
			get {
				return Principal != null && Principal.Identity != null ? Principal.Identity.Name : null;
			}
		}

		public bool IsInRole(string role) {
			return Principal!=null ? Principal.IsInRole(role) : false;
		}

		public virtual object GetValue(string varName) {
			var memberName = varName.Trim();
			var t = this.GetType();
			var p = t.GetProperty(memberName);
			if (p!=null) {
				return p.GetValue(this, null);
			}
			if (memberName.IndexOf('(')>0 && memberName[memberName.Length-1] == ')') {
				var methodParts = varName.Split(new[]{'(',')'}, StringSplitOptions.RemoveEmptyEntries );
				var m = t.GetMethod(methodParts[0]);
				if (m!=null && m.GetParameters().Length==1) {
					var param = m.GetParameters()[0];
					return m.Invoke(this, new[] { Convert.ChangeType(methodParts[1], param.ParameterType, CultureInfo.InvariantCulture) });
				}
			}
			return null;
		}
	}

	[Flags]
	public enum DalcOperation {
		Select = 1,
		Update = 2,
		Delete = 4,
		Change = 2+4,
		Any = 1+2+4
	}
}
