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

namespace NI.Data.Permissions
{
	/// <summary>
	/// Dalc permission context
	/// </summary>
	public class PermissionContext
	{
		public string SourceName { get; private set; }

		public DalcOperation Operation { get; private set; }

		public IPrincipal Principal { get; private set; }

		public PermissionContext(string sourceName, DalcOperation operation) {
			SourceName = sourceName;
			Principal = Thread.CurrentPrincipal;
			Operation = operation;
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
