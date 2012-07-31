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
using System.Data;
using System.Collections;
using System.Security;
using System.Threading;

using NI.Common;
using NI.Common.Collections;

namespace NI.Data.Permissions
{
	/// <summary>
	/// Permission-checking DALC proxy.
	/// </summary>
	public class DalcProxy : BaseDalcProxy
	{
		IDalc _UnderlyingDalc;
		
		protected override IDalc Dalc {
			get { return _UnderlyingDalc; }
		}
		
		/// <summary>
		/// Get or set underlying DALC component
		/// </summary>
		public IDalc UnderlyingDalc {
			get { return _UnderlyingDalc; }
			set { _UnderlyingDalc = value; }
		}
		
		public DalcProxy()
		{
		}
		
	}
}
