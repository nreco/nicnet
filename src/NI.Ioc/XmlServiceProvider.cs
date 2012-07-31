using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using System.Collections;


namespace NI.Factory.Xml
{
	/// <summary>
	/// Xml configurable components factory
	/// </summary>
	public class XmlServiceProvider : Component, IServiceProvider
	{
		ComponentsConfig _Config = null;
		
		ArrayList services;
		Hashtable servicesByName; 
		Hashtable servicesByInstance;

		/// <summary>
		/// Config for factory
		/// If not set, component will try to find it using GetService
		/// </summary>
		public ComponentsConfig Config {
			get { return _Config; }  
			set {
				_Config = value;
				
				Init();
			}
		}
		
		public XmlServiceProvider()
		{
			servicesByName = new Hashtable();
			servicesByInstance = new Hashtable();
			services = new ArrayList();
		}
		
		public XmlServiceProvider(ComponentsConfig config) : this() {
			Config = config;
		}
		
		public override ISite Site {
			get { return base.Site; }
			set {
				base.Site = value;
				if (Config==null)
					Config = (ComponentsConfig)GetService(typeof(ComponentsConfig));
				
				for (int i=0; i<services.Count; i++)
					if (services[i] is IComponent)
						Site.Container.Add( services[i] as IComponent, servicesByInstance[services[i]] as string );
			}
		}		
		
		/// <summary>
		/// Create services
		/// </summary>
		public void Init() {
			servicesByName.Clear();
			services.Clear();
			servicesByInstance.Clear();
	
			// initialize non-lazy components
			foreach (IComponentInfo cInfo in Config.Components) 
				if (!cInfo.LazyInit)
					CreateInstance(cInfo);		
		}
		
		/// <summary>
		/// Gets the requested service.
		/// </summary>
		public object GetService(Type serviceType) {
			// 0) request for service provider ?
			if (serviceType is IServiceProvider) return this;
			
			// 1) find IComponentInfo for this type
			IComponentInfo cInfo = Config.Components[serviceType];
			if (cInfo==null) return null;
			
			// 2) singleton and already instantiated ? 
			if (cInfo.Singleton) {
				object serviceInstance = FindServiceInstance(serviceType);
				if (serviceInstance!=null)
					return serviceInstance;
			}
				
			// 3) create instance
			object instance = CreateInstance(cInfo);
			
				
			return instance;
		}
		
		protected object FindServiceInstance(Type type) {
			for (int i=0; i<services.Count; i++)	
				if (type.IsInstanceOfType(services[i]) )
					return services[i];
			return null;
		}

		protected object CreateInstance(IComponentInfo componentInfo) {
			object instance = null;
			
			// find appropriate constructor and create instance
			ConstructorInfo[] constructors = componentInfo.ComponentType.GetConstructors();
			foreach (ConstructorInfo constructor in constructors) {
				ParameterInfo[] args = constructor.GetParameters();
				if (args.Length!=componentInfo.ConstructorArgs.Length) continue;
				
				// compose constructor arguments
				object[] constructorArgs = new object[componentInfo.ConstructorArgs.Length];
				try {
					for (int i=0; i<constructorArgs.Length; i++)
						constructorArgs[i] = CreateValue( componentInfo.ConstructorArgs[i], args[i].ParameterType );
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
			foreach (IPropertyInfo propertySchema in componentInfo.Properties) {
				// find property
				System.Reflection.PropertyInfo propInfo = componentInfo.ComponentType.GetProperty( propertySchema.Name );
				if (propInfo==null) 
					throw new MissingMethodException( componentInfo.ComponentType.ToString(), propertySchema.Name );
				object value = CreateValue( propertySchema.Value, propInfo.PropertyType );
				propInfo.SetValue( instance, value, null ); 
			}

			// if this is component, add it to container
			if (instance is IComponent && Site!=null)
				Site.Container.Add( instance as IComponent, componentInfo.Name );
			
			// if init method defined, call it
			if (componentInfo.InitMethod!=null) {
				MethodInfo initMethod = componentInfo.ComponentType.GetMethod( componentInfo.InitMethod );
				if (initMethod==null)
					throw new MissingMethodException( componentInfo.ComponentType.ToString(), componentInfo.InitMethod );
				initMethod.Invoke( instance, null );
			}
				
			
			// save in lookup hashtables
			services.Add( instance );
			if (componentInfo.Name!=null) {
				servicesByName[componentInfo.Name] = instance;
				servicesByInstance[instance] = componentInfo.Name;
			}
			
			return instance;
		}
		
		protected object CreateValue(IValueInfo valueInfo, Type conversionType) {
			object value = valueInfo.GetInstance(conversionType);
			if (value is IComponentInfo) {
				IComponentInfo refInfo = (IComponentInfo)value;
				// this component already instantiated ?
				object reference = servicesByName[refInfo.Name];
				if (reference!=null) return reference;
				// first instantiation
				return CreateInstance(refInfo);
			}
			return value;
		}
		
		
		
	}
}
