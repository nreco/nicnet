using System;
using NI.Ioc;
using System.ComponentModel;
using System.Collections;

using NUnit.Framework;

namespace NI.Tests.Winter
{
	/// <summary>
	/// </summary>
	[TestFixture]
	[NUnit.Framework.Category("NI.Ioc")]
	public class ServiceProviderTest
	{
	
		public ServiceProviderTest()
		{
		}
		
		
		/// <summary>
		/// Test:
		/// 1) ComponentsConfig parsing and validating
		/// 2) Components factory
		/// 3) Get service
		/// </summary>
		[Test]
		public void test_ServiceProvider() {
			
			ComponentsConfig config = createConfig();
			ServiceProvider serviceProvider = new ServiceProvider(config);
			
			ApplicationContainer app = new ApplicationContainer();
			app.Add( serviceProvider );
			
			// analyse
			Assert.AreEqual( app.Components.Count, 4, "Invalid component instances count");
			
			Component2 simple = app.Components["simple"] as Component2;
			
			Assert.AreEqual( simple.Hehe.Length, 2, "Invalid initialization for 'simple.Hehe'");
			Assert.AreEqual( simple.Hehe[0], 1, "Invalid initialization for 'simple.Hehe[0]'");
			Assert.AreEqual( simple.Hehe[1], 2, "Invalid initialization for 'simple.Hehe[1]'");
			
			Component1 child = app.Components["child"] as Component1;
			if (child.Dependency1==null || !(child.Dependency1 is Component2))
				throw new Exception("Invalid initialization for compontent 'child'");
			
			Component1 parent = app.Components["parent"] as Component1;
			
			Assert.AreEqual( parent.Dependency1, child, "Invalid initialization for 'parent.Dependency1'");
			Assert.AreEqual( parent.PropInt, 6, "Invalid initialization for 'parent.PropInt'");
			Assert.AreEqual( parent.initCalled, true, "parent init not called");
			
			// get service
			simple = (serviceProvider as IServiceProvider).GetService( typeof(Component2) ) as Component2;
			if (simple==null)
				throw new Exception("Get Service fails");
			
		}
		
		ComponentsConfig createConfig() {

			string xml_config = @"
				<components>
					<component name='simple_template_template'>
						<constructor-arg index='1'>
							<map>
								<entry key='IUserAccount'><value>accounts</value></entry>
								<entry key='ICmsPage'><value>pages</value></entry>
							</map>
						</constructor-arg>
					</component>
				
					<component name='simple_template' parent='simple_template_template'/>
				
					<component name='simple'
						parent='simple_template'
						type='NI.Tests.Winter.Component2,NI.Tests' singleton='true'>
						<constructor-arg index='0'>
							<list>
								<entry><value>1</value></entry>
								<entry><value>2</value></entry>
							</list>
						</constructor-arg>
					</component>
				
					<component name='child' type='NI.Tests.Winter.Component1,NI.Tests'  singleton='true'>
						<constructor-arg index='0'>
							<ref name='simple'/>
						</constructor-arg>
					</component>
					
					<component name='parent_template'
						type='NI.Tests.Winter.Component1,NI.Tests' 
						init-method='init' singleton='false'>
						<property name='PropInt'>
							<value>6</value>
						</property>
					</component>
					
					<component
						name='parent'
						parent='parent_template'
						singleton='true'>
					
						<property name='Dependency1'>
							  <ref name='child'/>
						</property>
						
						
					</component>
					
				</components>
			";
			
			XmlComponentsConfig config = new XmlComponentsConfig(xml_config);
			
			// check
			int i = 0;
			foreach (IComponentInitInfo cInfo in config) {
				switch (cInfo.Name) {
					case "simple":
						if (cInfo.ConstructorArgs.Length!=2 ||
							cInfo.ComponentType!=typeof(Component2) ||
							cInfo.Properties.Length!=0)
								throw new Exception("Invalid component info");
						break;
					case "child":
						if (cInfo.ConstructorArgs.Length!=1 ||
							cInfo.ComponentType!=typeof(Component1) ||
							cInfo.Properties.Length!=0)
							throw new Exception("Invalid component info");
						break;
					case "parent":
						if (cInfo.ConstructorArgs.Length!=0 ||
							cInfo.ComponentType!=typeof(Component1) ||
							cInfo.Properties.Length!=2 )
							throw new Exception("Invalid component info");
						break;						
				}
				
				i++;
			}
			//if (i!=3) throw new Exception("Invalid components number");

			return config;
		}
		
		
		
		
		
	}
	
	
		
	public class Component2 : Component {
		
		public int[] Hehe;
		public IDictionary Map;
		
		public Component2(int[] hehe, IDictionary map) {
			Hehe = hehe;
			Map = map;
		}
	}
			
	public class Component1 : Component {
		IComponent _Dependency1;
		int _PropInt;
		bool _initCalled = false;
			
		public IComponent Dependency1 {
			get { return _Dependency1; }
			set { _Dependency1 = value; }
		}
			
		public int PropInt {
			get { return _PropInt; }
			set { _PropInt = value; } 
		}
		
		public bool initCalled {
			get {
				return _initCalled;
			}
		}

		public Component1() { }
			
		public void init() {
			_initCalled = true;
		}
			
		public Component1( IComponent dependency1 ) {
			Dependency1 = dependency1;
		}
	}
		
		
	
	
	
}
