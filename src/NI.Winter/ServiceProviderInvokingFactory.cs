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

using NI.Common;

namespace NI.Winter
{
	/// <summary>
	/// ServiceProviderInvokingFactory used for defining instance as result of calling specified INamedServiceProvider implementation.
	/// </summary>
	public class ServiceProviderInvokingFactory : INamedServiceProviderAware, IFactoryComponent
	{
		INamedServiceProvider _NamedServiceProvider;
		string _ServiceName;
		
		/// <summary>
		/// Get or set context service provider
		/// </summary>
		public INamedServiceProvider NamedServiceProvider {
			get { return _NamedServiceProvider; }
			set { _NamedServiceProvider = value; }
		}
		
		/// <summary>
		/// Get or set service name to retrieve from service provider
		/// </summary>
		[Dependency]
		public string ServiceName {
			get { return _ServiceName; }
			set { _ServiceName = value; }
		}
		
		public ServiceProviderInvokingFactory()
		{
		}
		
		public object GetObject() {
			return NamedServiceProvider.GetService(ServiceName);
		}
		
		public Type GetObjectType() {
			return typeof(object);
		}
		
		
	}
}
