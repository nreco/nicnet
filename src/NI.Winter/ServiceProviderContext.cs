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
using System.ComponentModel;

namespace NI.Winter
{
	/// <summary>
	/// ServiceProviderContext used for referencing to IoC container inside its configuration.
	/// </summary>
	public class ServiceProviderContext : IServiceProviderAware, IFactoryComponent
	{
		IServiceProvider _ServiceProvider;
		
		/// <summary>
		/// Get or set context service provider
		/// </summary>
		public IServiceProvider ServiceProvider {
			get { return _ServiceProvider; }
			set { _ServiceProvider = value; }
		}
	
		public object GetObject() {
			return ServiceProvider;
		}
		
		public Type GetObjectType() {
			return typeof(IServiceProvider);
		}
	}
}
