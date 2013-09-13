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
using System.Security.Principal;
using System.Threading;

using NI.Data;

namespace NI.Data.Permissions
{
	/// <summary>
	/// DALC permission.
	/// </summary>
	public class DalcPermission 
	{
		public IPrincipal Subject { get; private set; }

		public DalcRecordInfo Object { get; private set; }
		
		public DalcOperation Operation { get; private set; }

		public DalcPermission(DalcOperation op, DalcRecordInfo obj) : this(Thread.CurrentPrincipal, op, obj) {

		}

		public DalcPermission(IPrincipal subject, DalcOperation op, DalcRecordInfo obj) {
			this.Operation = op;
			this.Object = obj;
		}
		
	}
}
