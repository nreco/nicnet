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
using System.Reflection;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Reflection.Emit;

namespace NI.Ioc
{
	/// <summary>
	/// Component factory implementation based on abstract IComponentConfiguration structure.
	/// </summary>
	/// <remarks>This class provides basic functionality of IoC-container: singleton cache, IFactoryComponent handling, components disposing.</remarks>
	/// <assemblyLink>https://code.google.com/p/nicnet/source/browse/src/NI.Ioc?name=nicnet2</assemblyLink>
	public class ComponentFactory : IComponent, IServiceProvider, IComponentFactory, IValueFactory
	{
		IComponentFactoryConfiguration _Config = null;

		ISite site;
		List<object> components;
		Dictionary<string,object> componentInstanceByName; 
		Dictionary<Type,object> componentInstanceByType;
		bool _CountersEnabled = false;
		bool _ReflectionCacheEnabled = false;
		CountersData counters = new CountersData();
		static IComparer constructorInfoComparer = new ConstructorInfoComparer();
		static IDictionary<ReflectionPropertyCacheKey,ReflectionPropertyCacheValue> propertyInfoCache = new Dictionary<ReflectionPropertyCacheKey,ReflectionPropertyCacheValue>();
		static IDictionary<Type,CreateObjectHandler> constructorInfoCache = new Dictionary<Type,CreateObjectHandler>();
		
		public bool CountersEnabled {
			get { return _CountersEnabled; }
			set { _CountersEnabled = value; }
		}

		/// <summary>
		/// Enables reflection cache using Reflection.Emit.DynamicMethod (increases components creation performance)
		/// </summary>
		public bool ReflectionCacheEnabled {
			get { return _ReflectionCacheEnabled; }
			set { _ReflectionCacheEnabled = value; }
		}

		/// <summary>
		/// Get or set components configuration for this factory
		/// </summary>
		public IComponentFactoryConfiguration Configuration {
			get { return _Config; }  
			private set { _Config = value; }
		}

		/// <summary>
		/// Represents the method that handles the Disposed event of this ComponentFactory.
		/// </summary>
		public event EventHandler Disposed;
		
		/// <summary>
		/// Initializes a new instance of the ComponentFactory class with specified componet configuration.
		/// </summary>
		/// <param name="config">IComponentFactoryConfiguration component</param>
		public ComponentFactory(IComponentFactoryConfiguration config) : this(config,false) {
		}

		/// <summary>
		/// Initializes a new instance of the ComponentFactory class with specified componet configuration and counters option.
		/// </summary>
		/// <param name="config">IComponentFactoryConfiguration component</param>
		/// <param name="countersEnabled">enables stats counters (component creation / retrieving)</param>
		public ComponentFactory(IComponentFactoryConfiguration config, bool countersEnabled) {
			CountersEnabled = countersEnabled;
			Configuration = config;
			Init();
		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				var flag = false;
				try {
					Monitor.Enter(this, ref flag);

					if (this.site != null && this.site.Container != null) {
						this.site.Container.Remove(this);
					}

					// lets remove references
					if (componentInstanceByName != null)
						componentInstanceByName.Clear();
					if (componentInstanceByType != null)
						componentInstanceByType.Clear();
					if (components != null) {
						// dispose components
						foreach (var c in components)
							if (c is IDisposable) {
								((IDisposable)c).Dispose();
							}
						components.Clear();
					}
					componentInstanceByName = null;
					components = null;
					
					_Config = null;

					if (Disposed != null)
						Disposed(this, EventArgs.Empty);

				} finally {
					if (flag) {
						Monitor.Exit(this);
					}
				}
			}
		}
		
		/// <summary>
		/// Get stats counters (available only if CountersEnabled=true)
		/// </summary>
		public CountersData Counters {
			get {
				if (!CountersEnabled)
					throw new InvalidOperationException("Counters are not enabled");
				return counters;
			}
		}
		
		/// <summary>
		/// Gets or sets the ISite associated with this ComponentFactory.
		/// </summary>
		/// <remarks>If Config is not initialized ComponentFactory tries to locate IComponentConfig by Site.GetService</remarks>
		public virtual ISite Site {
			get { return site; }
			set {
				site = value;
				if (value==null) return;

				if (Configuration == null) {
					Configuration = (XmlComponentConfiguration)site.GetService(typeof(IComponentFactoryConfiguration));
					Init();
				}
			}
		}		
		
		protected virtual void Init() {
			var estimatedComponentsCount = Configuration!=null ? Configuration.Count : 5;

			componentInstanceByName = new Dictionary<string,object>(estimatedComponentsCount);
			componentInstanceByType = new Dictionary<Type,object>(estimatedComponentsCount);
			components = new List<object>(estimatedComponentsCount);
			counters.Reset();
	
			// initialize non-lazy components
			if (Configuration != null) {
				foreach (IComponentInitInfo cInfo in Configuration)
					if (!cInfo.LazyInit)
						GetInstance(cInfo);
			}
		}

		/// <summary>
		/// Get requested service by type.
		/// </summary>
		public virtual object GetService(Type serviceType) {
			// a request for service provider ?
			if (serviceType == typeof(IServiceProvider) ||
				serviceType == typeof(IComponentFactory))
				return this;

			// find IComponentInitInfo for requested type
			IComponentInitInfo cInfo = Configuration[serviceType];
			if (cInfo == null)
				return null;

			return GetInstance(cInfo);			
		}
		
		/// <summary>
		/// <see cref="NI.Ioc.IComponentFactory(string)"/>
		/// </summary>
		public virtual object GetComponent(string name) {
			return GetComponent(name,null);
		}

		/// <summary>
		/// <see cref="NI.Ioc.IComponentFactory(string,System.Type)"/>
		/// </summary>
		public virtual object GetComponent(string name, Type requiredType) {
			IComponentInitInfo cInfo = Configuration[name];
			if (cInfo == null)
				return null;
			return ConvertTo( GetInstance(cInfo), requiredType);
		}

		object IValueFactory.GetInstance(object value, Type requiredType) {
			return ConvertTo(value, requiredType);
		}

		object IValueFactory.GetInstance(IComponentInitInfo componentInfo, Type requiredType) {
			var componentInstance = GetInstance(componentInfo);
			return ConvertTo(componentInstance, requiredType);
		}

		protected virtual object ConvertTo(object o, Type toType) {
			if (o == null)
				return null; // nothing to convert

			if (toType==null || toType==typeof(object) )
				return o; // avoid TypeConvertor 'NotSupportedException'

			// optimization
			if (o != null && (toType == o.GetType() || toType.IsInstanceOfType(o)))
				return o;

			// try component model converters
			TypeConverter converter = TypeDescriptor.GetConverter(toType);
			if (converter != null && converter.CanConvertFrom(o.GetType()))
				return converter.ConvertFrom(o);
			throw new InvalidCastException(String.Format("Cannot convert from {0} to {1}", o.GetType(), toType));
		}

		
		protected virtual object FindServiceInstance(Type type) {
			if (componentInstanceByType.ContainsKey(type))
				return componentInstanceByType[type];
			
			for (int i=0; i<components.Count; i++)	
				if (type.IsInstanceOfType(components[i]) ) {
					componentInstanceByType[type] = components[i];
					return components[i];
				}
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
				if (componentInstanceByName.ContainsKey(componentInfo.Name)) {
					object serviceInstance = componentInstanceByName[componentInfo.Name];
					if (serviceInstance!=null && componentInfo.ComponentType.IsInstanceOfType(serviceInstance))
						return ResolveInstance( serviceInstance );
				}
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
		
		// Create instance by component initialization info
		protected virtual object CreateInstance(IComponentInitInfo componentInfo) {
			try {
				// counters
				if (_CountersEnabled)
					counters.createInstance++;
				
				object instance = null;
				
				IValueFactory factory = this;
				
				if (componentInfo.ConstructorArgs==null || componentInfo.ConstructorArgs.Length==0) {
					instance = CreateObjectInstance(componentInfo.ComponentType);
				} else {
					// find appropriate constructor and create instance
					ConstructorInfo[] constructors = componentInfo.ComponentType.GetConstructors();
					// order is important only if at least one argument is present
					if (constructors.Length>0 && componentInfo.ConstructorArgs!=null && componentInfo.ConstructorArgs.Length>0)
						Array.Sort(constructors, constructorInfoComparer);

					Exception lastTryException = null;
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
								constructorArgs[i] = componentInfo.ConstructorArgs[i].GetValue(factory, args[i].ParameterType );
						} catch (Exception ex) {
							Console.WriteLine(ex.ToString());
							lastTryException = ex;
							// try next constructor ...
							continue;
						}
						
						instance = Activator.CreateInstance( componentInfo.ComponentType, constructorArgs );
						break;
					}
					if (instance == null && lastTryException!=null)
						throw new Exception(String.Format("Cannot find contructor for {0} (args count={1}) ", componentInfo.ComponentType,
							componentInfo.ConstructorArgs==null ? 0 : componentInfo.ConstructorArgs.Length), lastTryException);
				}
				
				// instance created ?
				if (instance==null)
					throw new MissingMethodException( componentInfo.ComponentType.ToString(), "constructor" );

				// fill properties
				for (int i=0; i<componentInfo.Properties.Length; i++) {
					// find property
					IPropertyInitInfo propertyInitInfo = componentInfo.Properties[i];
					try {
						SetObjectProperty(componentInfo.ComponentType, instance, propertyInitInfo.Name, factory, propertyInitInfo.Value);
					} catch(Exception e) {
						throw new Exception(string.Format("Cannot initialize component property: {1}.{0}", propertyInitInfo.Name,componentInfo.Name),e);
					}
				}
				
				// if component is service provider aware, set service provider
				if (instance is IServiceProviderAware)
					((IServiceProviderAware)instance).ServiceProvider = this;
				if (instance is IComponentFactoryAware)
					((IComponentFactoryAware)instance).ComponentFactory = this;

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
			// save component reference only if singleton
			if (componentInfo.Singleton)
				components.Add( instance );
			
			// if named service, also remember reference by name
			if (componentInfo.Name!=null && componentInfo.Singleton) {
				componentInstanceByName[componentInfo.Name] = instance;
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
		
		internal void SetObjectProperty(Type t, object o, string propName, IValueFactory factory, IValueInitInfo valueInfo) {
			if (ReflectionCacheEnabled) {
				ReflectionPropertyCacheKey cacheKey = new ReflectionPropertyCacheKey(t, propName);
				ReflectionPropertyCacheValue cacheValue;
				if (!propertyInfoCache.TryGetValue(cacheKey, out cacheValue)) {
					System.Reflection.PropertyInfo propInfo = t.GetProperty(propName);
					if (propInfo == null)
						throw new MissingMethodException(t.ToString(), propName);
					MethodInfo setMethodInfo = propInfo.GetSetMethod(false);

					DynamicMethod setDynMethod = new DynamicMethod(String.Empty, typeof(void),	new Type[] { typeof(object), typeof(object) }, t, true);
					ILGenerator setGenerator = setDynMethod.GetILGenerator();
					setGenerator.Emit(OpCodes.Ldarg_0);
					setGenerator.Emit(OpCodes.Ldarg_1);
					if (setMethodInfo.GetParameters()[0].ParameterType.IsValueType)
						setGenerator.Emit(OpCodes.Unbox_Any, setMethodInfo.GetParameters()[0].ParameterType);
					setGenerator.Emit(OpCodes.Call, setMethodInfo );
					setGenerator.Emit(OpCodes.Ret);

					cacheValue = new ReflectionPropertyCacheValue(
										(PropertySetHandler)setDynMethod.CreateDelegate(typeof(PropertySetHandler)),
										propInfo.PropertyType);
					// despite the fact Dictionary is thread safe, for some reason sometimes exceptions are thrown without extra lock
					lock (propertyInfoCache) {
						propertyInfoCache[cacheKey] = cacheValue;
					}
				}
				object value = valueInfo.GetValue( factory, cacheValue.PropertyType);
				cacheValue.SetHandler(o, value);
			} else {
				System.Reflection.PropertyInfo propInfo = t.GetProperty(propName);
				if (propInfo == null)
					throw new MissingMethodException(t.ToString(), propName);
				propInfo.SetValue(o, valueInfo.GetValue( factory, propInfo.PropertyType), null);
			}
			
		}
		
		internal object CreateObjectInstance(Type t) {
			if (ReflectionCacheEnabled) {
				CreateObjectHandler createHandler;
				if (!constructorInfoCache.TryGetValue(t, out createHandler)) {
					ConstructorInfo constructorInfo = t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
					if (constructorInfo==null)
						throw new MissingMemberException(t.FullName, "constructor");
					DynamicMethod dynamicMethod = new DynamicMethod(String.Empty,
								MethodAttributes.Static | MethodAttributes.Public, 
								CallingConventions.Standard, typeof(object), null, t, true);
					ILGenerator generator = dynamicMethod.GetILGenerator();
					generator.Emit(OpCodes.Newobj, constructorInfo);
					generator.Emit(OpCodes.Ret);
					createHandler = (CreateObjectHandler)dynamicMethod.CreateDelegate(typeof(CreateObjectHandler));					
					// despite the fact Dictionary is thread safe, for some reason sometimes exceptions are thrown without extra lock
					lock (constructorInfoCache) {
						constructorInfoCache[t] = createHandler;
					}
				}
				return createHandler();
			} else {
				return Activator.CreateInstance(t, false);
			}
		}
		
		internal delegate void PropertySetHandler(object source, object value);
		internal delegate object CreateObjectHandler();
		
		internal class ReflectionPropertyCacheKey {
			Type t;
			string propName;
			int hashCode;

			public ReflectionPropertyCacheKey(Type t, string propName) {
				this.t = t;
				this.propName = propName;
				hashCode = (t.FullName+propName).GetHashCode();
			}

			public override int GetHashCode() {
				return hashCode;
			}
			
			public override bool Equals(object obj) {
 				if (obj is ReflectionPropertyCacheKey) {
 					ReflectionPropertyCacheKey k = (ReflectionPropertyCacheKey)obj;
 					return k.t==t && k.propName==propName;
 				}
 				return base.Equals(obj);
			}
		}
		internal class ReflectionPropertyCacheValue {
			internal PropertySetHandler SetHandler;
			internal Type PropertyType;
			
			public ReflectionPropertyCacheValue(PropertySetHandler hdlr, Type propType) {
				SetHandler = hdlr;
				PropertyType = propType;
			}
		}
		

	}
}
