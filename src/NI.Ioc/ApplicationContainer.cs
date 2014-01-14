#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas,  Vitalii Fedorchenko (v.2 changes)
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



namespace NI.Ioc
{
	/// <summary>
	/// Application components container that can use IComponentFactory as service provider
	/// </summary>
	public class ApplicationContainer : Container
	{
		IComponentFactory _ComponentFactory = null;
		
		/// <summary>
		/// Component factory associated with this container.
		/// </summary>
		public IComponentFactory ComponentFactory {
			get { return _ComponentFactory; }
			set {
				_ComponentFactory = value; 
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
			if (res == null && ComponentFactory is IServiceProvider)
				res = ((IServiceProvider)ComponentFactory).GetService(serviceType);
			return res;
		}
		
		/// <summary>
		/// Do not use Container.CreateSite because of buggy MONO 1.1 implementation of this method.
		/// </summary>
		protected override ISite CreateSite(IComponent component, string name) {
			// explicit initialization
			if (ComponentFactory==null && (component is IComponentFactory))
				_ComponentFactory = (IComponentFactory)component;

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
