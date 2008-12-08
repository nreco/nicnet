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

using NI.Common;

namespace NI.Winter
{
	/// <summary>
	/// Simple application container based on IServiceProvider implementation.
	/// </summary>
	/// <example><code>
	/// IComponentsConfig cfg;
	/// IComponent someDotNetComponent;
	/// 
	/// ApplicationContainer appContainer = new ApplicationContainer();
	/// /*inject service provider*/
	///	appContainer.ServiceProvider = new NI.Winter.ServiceProvider( cfg  );
	/// appContainer.Add(someDotNetComponent);
	/// /* now someDotNetComponent 'sited' and can use interface ISite to obtain services */</code>
	/// </example>
	public class ApplicationContainer : Container
	{
		IServiceProvider _ServiceProvider = null;
		INamedServiceProvider _NamedServiceProvider = null;
		
		/// <summary>
		/// Service provider
		/// If not set, Container will try to find it in components collection
		/// </summary>
		[Dependency]
		public IServiceProvider ServiceProvider {
			get { return _ServiceProvider; }
			set { 
				_ServiceProvider = value; 
				if (value is IComponent && ((IComponent)value).Site==null)
					Add( value as IComponent );
			}
		}
		
		/// <summary>
		/// Named service provider associated with this container.
		/// </summary>
		/// <remarks>
		/// This property is not used actually by container and can be used as 'entry point' to service provider from objects outside container.
		/// </remarks>
		[Dependency]
		public INamedServiceProvider NamedServiceProvider {
			get { return _NamedServiceProvider; }
			set {
				_NamedServiceProvider = value; 
				if (value is IComponent && ((IComponent)value).Site==null)
					Add( value as IComponent );
			}
		}
		

		/// <summary>
		/// Get service of specified type. Search sequence:
		/// 1) base.GetService (default Container behaviour)
		/// 2) ServiceProvider
		/// </summary>
		protected override object GetService(Type serviceType) {
			object res = null;
			res = base.GetService(serviceType);
			if (res==null && ServiceProvider!=null)
				res = ServiceProvider.GetService(serviceType);
			return res;
		}
		
		/// <summary>
		/// Do not use Container.CreateSite because of buggy MONO 1.1 implementation of this method.
		/// </summary>
		protected override ISite CreateSite(IComponent component, string name) {
			// explicit initialization
			if (ServiceProvider==null && (component is IServiceProvider))
				_ServiceProvider = (IServiceProvider)component;
			if (NamedServiceProvider==null && (component is INamedServiceProvider))
				_NamedServiceProvider = (INamedServiceProvider)component;

			return new ApplicationContainer.ApplicationContainerSite(component, this, name);
		}

		
		class ApplicationContainerSite : ISite {
			private IComponent component;
			private ApplicationContainer container;
			private string     name;
			
			public ApplicationContainerSite(IComponent component, ApplicationContainer container, string name) {
				this.component = component;
				this.container = container;
				this.name = name;
			}

			public IComponent Component {
				get { return component; }
			}

			public IContainer Container {
				get { return container; }
			}

			public bool DesignMode { get { return false; } }

			public string Name {
				get { return name; }
				set { name = value; }
			}

			public object GetService(Type service) {
				if (service != typeof(ISite)) {
					return this.container.GetService(service);
				}
				return this;
			}
 

		} 
		
		
		
	
	}
}
