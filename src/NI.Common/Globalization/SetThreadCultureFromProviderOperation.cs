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
using System.Diagnostics;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Web;

using NI.Common.Providers;
using NI.Common.Operations;

namespace NI.Common.Globalization {

	public class SetThreadCultureFromProviderOperation : SetThreadCultureOperation {
		IStringProvider _CultureNameProvider;
		
		public IStringProvider CultureNameProvider {
			get { return _CultureNameProvider; }
			set { _CultureNameProvider = value; }
		}
		
		public override void Execute(IDictionary context) {
			string langName = CultureNameProvider.GetString(context);
			if (langName != null) {
				SetCulture(langName);
			}
		}
		
	}
	
}
