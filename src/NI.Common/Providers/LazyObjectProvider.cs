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

namespace NI.Common.Providers {
	
	/// <summary>
	/// Lazy object provider wrapper.
	/// </summary>
	public class LazyObjectProvider : IObjectProvider {
		INamedServiceProvider _NamedServiceProvider;
		string _ProviderName;

		public string ProviderName {
			get { return _ProviderName; }
			set { _ProviderName = value; }
		}

		public INamedServiceProvider NamedServiceProvider {
			get { return _NamedServiceProvider; }
			set { _NamedServiceProvider = value; }
		}

		public object GetObject(object context) {
			object operation = NamedServiceProvider.GetService(ProviderName);
			if (operation==null)
				throw new ArgumentException("invalid operation name");
			if (!(operation is IObjectProvider))
				throw new ArgumentException("operation does not implement IObjectProvider");
			return ((IObjectProvider)operation).GetObject(context);
		}

	}
}
