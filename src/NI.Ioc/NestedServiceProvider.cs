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

namespace NI.Ioc
{
	/// <summary>
	/// NestedServiceProvider implementation used for defining composite ServiceProviders.
	/// </summary>
	public class NestedComponentFactory : ComponentFactory
	{
		IServiceProvider _ParentServiceProvider;
		IComponentFactory _ParentNamedServiceProvider;
		
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
		public IComponentFactory ParentNamedServiceProvider {
			get { return _ParentNamedServiceProvider; }
			set { _ParentNamedServiceProvider = value; }
		}
		
	
		public NestedComponentFactory(IComponentsConfig config) : base(config)
		{
		}
		
		/// <summary>
		/// Nested service provider logic: if component not fount in this provider,
		/// try to find it in parent provider.
		/// </summary>
		public override object GetService(Type serviceType) {
			object service = base.GetService(serviceType);
			if (service==null)
				service = ParentServiceProvider.GetService(serviceType);
			return service; 
		}

		public override object GetComponent(string name, Type requiredType) {
			object service = base.GetComponent(name, requiredType);
			if (service==null)
				service = ParentNamedServiceProvider.GetComponent(name);
			return service;
		}
		
		
	}
}
