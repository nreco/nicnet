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

using NI.Common;

namespace NI.Ioc
{
	/// <summary>
	/// NestedServiceProvider implementation used for defining composite ServiceProviders.
	/// </summary>
	public class NestedServiceProvider : ServiceProvider
	{
		IServiceProvider _ParentServiceProvider;
		INamedServiceProvider _ParentNamedServiceProvider;
		
		/// <summary>
		/// Get or set parent service provider
		/// </summary>
		public IServiceProvider ParentServiceProvider {
			get { return _ParentServiceProvider; }
			set { _ParentServiceProvider = value; }
		}
		
		/// <summary>
		/// Get or set 
		/// </summary>
		public INamedServiceProvider ParentNamedServiceProvider {
			get { return _ParentNamedServiceProvider; }
			set { _ParentNamedServiceProvider = value; }
		}
		
	
		public NestedServiceProvider()
		{
		}
		
		/// <summary>
		/// Nested service provider logic: if component not fount in this provider,
		/// try to find it in parent provider.
		/// </summary>
		protected override object GetServiceInternal(Type serviceType) {
			object service = base.GetServiceInternal(serviceType);
			if (service==null)
				service = ParentServiceProvider.GetService(serviceType);
			return service; 
		}
		
		protected override object GetServiceInternal(string name) {
			object service = base.GetServiceInternal(name);
			if (service==null)
				service = ParentNamedServiceProvider.GetService(name);
			return service;
		}
		
		
	}
}
