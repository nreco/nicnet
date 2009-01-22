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
using System.Reflection;
using System.Xml;
using System.Collections;

using NI.Common;
using NI.Common.Providers;

namespace NI.Winter
{
	/// <summary>
	/// Service provider based on Spring-like configuration.
	/// </summary>
	/// <remarks>
	/// This component takes care about objects initialization, creation, referencing etc. Fully integrated with System.ComponentModel: 
	/// <list type="bullet">
	/// <description>if Config property is not initialized and ServiceProvider added to Container, it will try to found it using ISite interface</description>
	/// <description>if Container is available, all 'singleton' IComponent objects created with ServiceProvider are added to the Container</description>
	/// </list>
	/// </remarks>
	/// <example><code>
	/// IComponentsConfig cfg;
	/// ServiceProvider srvProv = new ServiceProvider(cfg);
	/// object someService = srvProv.GetObject("someServiceName");
	/// </code></example>
	public class ServiceProvider : System.ComponentModel.Component, IServiceProvider, INamedServiceProvider, IObjectProvider
	{
		IComponentsConfig _Config = null;
		
		ArrayList services;
		Hashtable serviceInstanceByName; 
		Hashtable serviceNameByInstance;
		Hashtable serviceInstanceByType;
		bool _CountersEnabled = false;
		CountersData counters = new CountersData();
		IValueFactory _ValueFactory;
		static IComparer constructorInfoComparer = new ConstructorInfoComparer();

		public bool CountersEnabled {
			get { return _CountersEnabled; }
			set { _CountersEnabled = value; }
		}
		
		public IValueFactory ValueFactory {
			get { return _ValueFactory; }
			set { _ValueFactory = value; }
		}

		/// <summary>
		/// Config for factory
		/// If not set, component will try to find it using GetService
		/// </summary>
		[Dependency]
		public IComponentsConfig Config {
			get { return _Config; }  
			set {
				_Config = value;
				Init();
			}
		}
		
		public ServiceProvider()
		{
			serviceInstanceByName = new Hashtable();
			serviceNameByInstance = new Hashtable();
			serviceInstanceByType = new Hashtable();
			services = new ArrayList();
			ValueFactory = new LocalValueFactory(this); // default value factory
		}
		
		public ServiceProvider(IComponentsConfig config) : this() {
			Config = config;
		}
		public ServiceProvider(IComponentsConfig config, bool countersEnabled) : this() {
			CountersEnabled = countersEnabled;
			Config = config;
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			if (disposing) {
				// lets remove references
				if (serviceInstanceByName!=null)
					serviceInstanceByName.Clear();
				if (serviceNameByInstance!=null)
					serviceNameByInstance.Clear();
				if (serviceInstanceByType != null)
					serviceInstanceByType.Clear();
				if (services != null)
					services.Clear();								
				serviceInstanceByName = null;
				serviceNameByInstance = null;
				services = null;
				_Config = null;
			}
		}
		
		public CountersData Counters {
			get {
				if (!CountersEnabled)
					throw new InvalidOperationException("Counters are not enabled!");
				return counters;
			}
		}
		
		public override ISite Site {
			get { return base.Site; }
			set {
				base.Site = value;
				if (value==null) return;
				
				if (Config==null)
					Config = (ComponentsConfig)GetService(typeof(ComponentsConfig));
				
				for (int i=0; i<services.Count; i++)
					if (services[i] is IComponent) {
						string serviceName = serviceNameByInstance[services[i]] as string;
						if (serviceName!=null)
							if (!Config[serviceName].Singleton)
								serviceName = null;
						Site.Container.Add( services[i] as IComponent, serviceName );
					}
			}
		}		
		
		/// <summary>
		/// Create services
		/// </summary>
		public virtual void Init() {
			serviceInstanceByName.Clear();
			services.Clear();
			serviceNameByInstance.Clear();
	
			// initialize non-lazy components
			foreach (IComponentInitInfo cInfo in Config) 
				if (!cInfo.LazyInit)
					GetInstance(cInfo);
		}

		public object GetObject(object context) {
			if (context is string)
				return GetServiceInternal((string)context);
			if (context is Type)
				return GetServiceInternal((Type)context);
			throw new ArgumentException("Unsupported context type.");
		}
		
		
		/// <summary>
		/// Service provider: get requested service by type.
		/// </summary>
		object IServiceProvider.GetService(Type serviceType) {
			return GetServiceInternal(serviceType);
		}
		
		/// <summary>
		/// Service provider: get requested service by name.
		/// </summary>
		object INamedServiceProvider.GetService(string name) {
			return GetServiceInternal(name);
		}
		
		/// <summary>
		/// Service provider: get service by type internal implementation
		/// </summary>
		protected virtual object GetServiceInternal(Type serviceType) {
			// a request for service provider ?
			if (serviceType==typeof(IServiceProvider) ||
				serviceType==typeof(INamedServiceProvider) ) return this;
			
			// find IComponentInitInfo for requested type
			IComponentInitInfo cInfo = Config[serviceType];
			if (cInfo==null) return null;
			
			return GetInstance(cInfo);
		}
		
		/// <summary>
		/// Service provider: get service by name internal implementation
		/// </summary>
		protected virtual object GetServiceInternal(string name) {
			IComponentInitInfo cInfo = Config[name];
			if (cInfo==null) return null;
			return GetInstance(cInfo);			
		}
		
		protected virtual object FindServiceInstance(Type type) {
			if (serviceInstanceByType.Contains(type))
				return serviceInstanceByType[type];
			
			for (int i=0; i<services.Count; i++)	
				if (type.IsInstanceOfType(services[i]) ) {
					serviceInstanceByType[type] = services[i];
					return services[i];
				}
			serviceInstanceByType[type] = null;
			return null;
		}
		
		protected virtual object GetInstance(IComponentInitInfo componentInfo) {
			if (_CountersEnabled)
				counters.getInstance++;
			
			// is abstract?
			if (componentInfo.ComponentType==null)
				return null;
			
			// is unnamed singleton ?
			if (componentInfo.Name==null && componentInfo.Singleton) {
				object serviceInstance = FindServiceInstance(componentInfo.ComponentType);
				if (serviceInstance!=null)
					return ResolveInstance( serviceInstance );
			}
			
			// is named singleton ?
			if (componentInfo.Name!=null && componentInfo.Singleton) {
				object serviceInstance = serviceInstanceByName[componentInfo.Name];
				if (serviceInstance!=null && componentInfo.ComponentType.IsInstanceOfType(serviceInstance))
					return ResolveInstance( serviceInstance );
			}

			// create instance
			object instance = CreateInstance(componentInfo);
			
			
			return ResolveInstance( instance );
		}
		
		protected virtual object ResolveInstance(object instance) {
			if (instance is IFactoryComponent) {
				IFactoryComponent componentFactory = (IFactoryComponent)instance;
				return componentFactory.GetObject();
			}
			return instance;
		}
		
		
		/// <summary>
		/// Create instance by component initialization info
		/// </summary>
		/// <param name="componentInfo">component initialization info</param>
		/// <returns>initialized component instance</returns>
		protected virtual object CreateInstance(IComponentInitInfo componentInfo) {
			try {
				// counters
				if (_CountersEnabled)
					counters.createInstance++;
				
				object instance = null;
				
				IValueFactory factory = ValueFactory;
				
				// find appropriate constructor and create instance
				ConstructorInfo[] constructors = componentInfo.ComponentType.GetConstructors();
				// order is important only if at least one argument is present
				if (constructors.Length>0 && componentInfo.ConstructorArgs!=null && componentInfo.ConstructorArgs.Length>0)
					Array.Sort(constructors, constructorInfoComparer);

				foreach (ConstructorInfo constructor in constructors) {
					ParameterInfo[] args = constructor.GetParameters();
					// it should be always 'not null'. But lets ensure.
					if (args == null)
						throw new NullReferenceException("ConstructorInfo.GetParameters returns null for type = " + componentInfo.ComponentType.ToString() );
					if (componentInfo.ConstructorArgs == null)
						throw new NullReferenceException("IComponentInitInfo.ConstructorArgs is null for type = " + componentInfo.ComponentType.ToString());
					
					if (args.Length!=componentInfo.ConstructorArgs.Length) continue;
					
					// compose constructor arguments
					object[] constructorArgs = new object[componentInfo.ConstructorArgs.Length];
					try {
						for (int i=0; i<constructorArgs.Length; i++)
							constructorArgs[i] = componentInfo.ConstructorArgs[i].GetInstance(factory, args[i].ParameterType );
					} catch {
						// try next constructor ...
						continue;
					}
					
					instance = Activator.CreateInstance( componentInfo.ComponentType, constructorArgs );
					break;
				}
				
				// instance created ?
				if (instance==null)
					throw new MissingMethodException( componentInfo.ComponentType.ToString(), "constructor" );

				// fill properties
				foreach (IPropertyInitInfo propertyInitInfo in componentInfo.Properties) {
					// find property
					try {
						System.Reflection.PropertyInfo propInfo = componentInfo.ComponentType.GetProperty( propertyInitInfo.Name );
						if (propInfo==null) 
							throw new MissingMethodException( componentInfo.ComponentType.ToString(), propertyInitInfo.Name );
						object value = propertyInitInfo.Value.GetInstance(factory, propInfo.PropertyType );
						propInfo.SetValue( instance, value, null ); 
					} catch(Exception e) {
						throw new Exception(string.Format("Cannot initialize component property: {1}.{0}", propertyInitInfo.Name,componentInfo.Name),e);
					}
				}
				
				// if component is service provider aware, set service provider
				if (instance is IServiceProviderAware)
					((IServiceProviderAware)instance).ServiceProvider = this;
				if (instance is INamedServiceProviderAware)
					((INamedServiceProviderAware)instance).NamedServiceProvider = this;

				// if this is component, add it to container
				if (instance is IComponent && Site!=null) {
					string componentName = componentInfo.Name!=null && componentInfo.Singleton ? componentInfo.Name : null;			
					Site.Container.Add( instance as IComponent, componentName );
				}
				
				// if init method defined, call it
				if (componentInfo.InitMethod!=null) {
					MethodInfo initMethod = componentInfo.ComponentType.GetMethod( componentInfo.InitMethod, new Type[0] );
					if (initMethod==null)
						throw new MissingMethodException( componentInfo.ComponentType.ToString(), componentInfo.InitMethod );
					initMethod.Invoke( instance, null );
				}
				
				SaveServiceInLookups( instance, componentInfo );
				
				return instance;
			} catch (Exception ex) {
				throw new Exception( String.Format("Cannot create object with type={0} name={1}",
					componentInfo.ComponentType.ToString(), componentInfo.Name ), ex);
			}
		}
		
		protected virtual void SaveServiceInLookups(object instance, IComponentInitInfo componentInfo) {
			// remember service reference only if it named or singleton
			// should we remember only singletons ?
			if ( /*componentInfo.Name!=null ||*/ componentInfo.Singleton)
				services.Add( instance );
			
			// if named service, also remember reference by name
			if (componentInfo.Name!=null && componentInfo.Singleton) {
				serviceInstanceByName[componentInfo.Name] = instance;
				serviceNameByInstance[instance] = componentInfo.Name;
			}
		}

		internal class ConstructorInfoComparer : IComparer {
			public int Compare(object x, object y) {
				ConstructorInfo c1 = (ConstructorInfo)x;
				ConstructorInfo c2 = (ConstructorInfo)y;
				ParameterInfo[] c1Params = c1.GetParameters();
				ParameterInfo[] c2Params = c2.GetParameters();
				if (c1Params.Length != c2Params.Length)
					return c1Params.Length.CompareTo(c2Params.Length);
				// lets analyse types
				for (int i = 0; i < c1Params.Length; i++) {
					bool isXObj = c1Params[i].ParameterType==typeof(object);
					bool isYObj = c2Params[i].ParameterType==typeof(object);
					if (isXObj && isYObj) return 0;
					if (isXObj) return 1;
					if (isYObj) return -1;
				}
				return 0;
			}
		}

		public class LocalValueFactory : IValueFactory {
			ServiceProvider SP;

			public LocalValueFactory(ServiceProvider serviceProvider) {
				SP = serviceProvider;
			}

			public object CreateInstance(IValueInitInfo valueInfo, Type conversionType) {
				object v;
				if (valueInfo is ValueInitInfo) {
					v = ((ValueInitInfo)valueInfo).Value;
				} else if (valueInfo is RefValueInfo) {
					IComponentInitInfo cInfo = ((RefValueInfo)valueInfo).ComponentRef;
					v = SP.GetInstance(cInfo);
				} else
					throw new Exception("Cannot create instance by value info "+valueInfo.GetType().ToString() );

				if (conversionType==typeof(object)) return v; // avoid TypeConvertor 'NotSupportedException'
				// optimization
				if (v!=null && (conversionType==v.GetType() || conversionType.IsInstanceOfType(v) ) )
					return v;
				return ConvertTo(v, conversionType);
			}
	
			protected virtual object ConvertTo(object o, Type toType) {
				if (o==null) 
					return null; // nothing to convert
				return TypeDescriptor.GetConverter(toType).ConvertFrom(o);
			}
		}
		
		public class CountersData {
			internal int getInstance = 0;
			internal int createInstance = 0;
			
			public int GetInstance { get { return getInstance; } }
			public int CreateInstance { get { return createInstance; } }
			
			public void Reset() {
				getInstance = createInstance = 0;
			}
			public override string ToString() {
				return String.Format("GetInstance={0} CreateInstance={1}",GetInstance,CreateInstance);
			}
		}


	}
}
