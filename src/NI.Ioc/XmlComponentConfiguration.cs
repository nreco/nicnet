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
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.ComponentModel;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.IO;
using System.Reflection;

using NI.Ioc.Exceptions;

namespace NI.Ioc
{
	/// <summary>
	/// IComponentsConfig implementation based on XML.
	/// </summary>
	public class XmlComponentConfiguration : ComponentConfiguration
	{

		public XmlComponentConfiguration(string xml)
			: this( new XmlTextReader( new StringReader(xml))) {
		}

		public XmlComponentConfiguration(XmlReader configXmlReader)
			: this( LoadXPathDoc(configXmlReader) ) {
		}

		internal XmlComponentConfiguration(XPathDocument configXml)
			: base(new XmlConfigurationReader().ReadComponents(configXml)) {

			// extract description value
			var rootNav = configXml.CreateNavigator();
			var xmlNs = GetNsManager(rootNav);
			var descriptionNode = rootNav.SelectSingleNode("/ioc:components/ioc:description",xmlNs);
			if (descriptionNode != null)
				Description = descriptionNode.Value;
		}

		static XPathDocument LoadXPathDoc(XmlReader rdr) {
			var xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.ValidationType = ValidationType.Schema;
			xmlReaderSettings.Schemas.Add( XmlSchema.Read(GetXsdStream(), null) );
			xmlReaderSettings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

			// load XML into XmlDocument
			var xmlDoc = new XPathDocument( XmlReader.Create(rdr,xmlReaderSettings), XmlSpace.Preserve);
			return xmlDoc;
		}

		static Stream GetXsdStream() {
			string name = typeof(XmlComponentConfiguration).Namespace + ".ComponentsConfigSchema.xsd";
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
		}

		static void ValidationCallBack(object sender, ValidationEventArgs args) {
			throw new XmlConfigurationException(
					String.Format("{0} at line {1}", args.Message, args.Exception.LineNumber), 
					args.Exception);
		}

		static XmlNamespaceManager GetNsManager(XPathNavigator nav) {
			var xmlNsMgr = new XmlNamespaceManager(nav.NameTable);
			xmlNsMgr.AddNamespace("ioc", "urn:schemas-nicnet:ioc:v1");
			return xmlNsMgr;
		}

		internal class XmlConfigurationReader {

			public TypeResolver TypeResolver;
			bool StrictComponentNames = true;
			bool DefaultLazyInit = false;

			public XmlConfigurationReader() {
				TypeResolver = new TypeResolver();
			}


			public IComponentInitInfo[] ReadComponents(XPathDocument componentsXmlDoc) {
				var rootNav = componentsXmlDoc.CreateNavigator();
				var xmlNsMgr = GetNsManager(rootNav);

				var componentsNode = rootNav.SelectSingleNode("/ioc:components", xmlNsMgr);
				if (componentsNode == null)
					throw new XmlConfigurationException("Invalid configuration: root 'components' in namespace 'urn:schemas-nicnet:ioc:v1' node is missing");

				// extract default lazy init value
				var defaultLazyInitAttr = componentsNode.GetAttribute("default-lazy-init",String.Empty);
				if (!String.IsNullOrEmpty(defaultLazyInitAttr))
					DefaultLazyInit = Convert.ToBoolean(defaultLazyInitAttr);
				var strictNamesAttr = componentsNode.GetAttribute("strict-names", String.Empty);
				if (!String.IsNullOrEmpty(strictNamesAttr))
					StrictComponentNames = Convert.ToBoolean(strictNamesAttr);

				// build components info collection
				var componentsIterator = componentsNode.Select("ioc:component", xmlNsMgr);
				var Components = new List<ComponentInitInfo>(componentsIterator.Count);
				var ComponentsByName = new Dictionary<string, ComponentInitInfo>();

				foreach (XPathNavigator componentNode in componentsIterator) {
					var componentInfo = ReadComponentInitInfo(componentNode, xmlNsMgr);
					Components.Add( componentInfo );

					if (componentInfo.Name != null) {
						if (ComponentsByName.ContainsKey(componentInfo.Name)) {
							if (StrictComponentNames)
								throw new Exception(
									String.Format("Duplicate component name is found: {0} ({1})", componentInfo.Name,
										componentsIterator.Current.OuterXml ) );
						} else {
							ComponentsByName[componentInfo.Name] = componentInfo;
						}
					}

				}

				int componentIndex = 0;

				foreach (XPathNavigator componentNode in componentsIterator) {
					var componentInfo = Components[componentIndex++];
					try {
						InitValues(componentInfo, componentNode, xmlNsMgr, ComponentsByName);
					} catch (Exception ex) {
						throw new XmlConfigurationException(
							String.Format("Cannot resolve values for '{0}' component definition", componentInfo.Name), ex);
					}
				}

				return Components.ToArray();
			}
			
			protected ComponentInitInfo ReadComponentInitInfo(XPathNavigator componentNode, IXmlNamespaceResolver xmlNs)
			{
				var componentInit = new ComponentInitInfo();

				// extract component name (optional)
				var nameAttr = componentNode.GetAttribute("name", String.Empty);
				if (!String.IsNullOrEmpty(nameAttr))
					componentInit.Name = nameAttr;

				// extract component parent (optional)
				var parentAttr = componentNode.GetAttribute("parent", String.Empty);
				if (!String.IsNullOrEmpty(parentAttr))
					componentInit.Parent = parentAttr;
			
				// extract component type (optional)
				var typeAttr = componentNode.GetAttribute("type", String.Empty);
				if (!String.IsNullOrEmpty(typeAttr)) {
					try {
						componentInit.ComponentType = TypeResolver.ResolveType(typeAttr);
					} catch (Exception ex) {
						throw new XmlConfigurationException(ex.Message, ex);
					}
					if (componentInit.ComponentType == null)
						throw new XmlConfigurationException("Cannot resolve type " + typeAttr);
				}
			
				// extract component lazy init flag
				componentInit.LazyInit = DefaultLazyInit;
				var lazyInitAttr = componentNode.GetAttribute("lazy-init", String.Empty);
				if (!String.IsNullOrEmpty(lazyInitAttr))
					componentInit.LazyInit = Convert.ToBoolean(lazyInitAttr);
			
				// extract component singleton flag
				var singletonAttr = componentNode.GetAttribute("singleton", String.Empty);
				if (!String.IsNullOrEmpty(singletonAttr))
					componentInit.Singleton = Convert.ToBoolean(singletonAttr);

				// extract init-method value
				var initMethodAttr = componentNode.GetAttribute("init-method", String.Empty);
				if (!String.IsNullOrEmpty(initMethodAttr))
					componentInit.InitMethod = initMethodAttr;

				// extract description value
				var descriptionNode = componentNode.SelectSingleNode("ioc:description", xmlNs);
				if (descriptionNode!=null)
					componentInit.Description = descriptionNode.Value;

				return componentInit;
			}



			/// <summary>
			/// Initialize component values (constructor arguments / properties )
			/// </summary>
			protected void InitValues(ComponentInitInfo componentInit, XPathNavigator componentNode, IXmlNamespaceResolver xmlNs, IDictionary<string,ComponentInitInfo> componentInfoByName) {
				Dictionary<int, IValueInitInfo> constructorArgs = new Dictionary<int, IValueInitInfo>();
				List<IPropertyInitInfo> propsList = new List<IPropertyInitInfo>();
				Dictionary<string, int> propsIndex = new Dictionary<string, int>();

				// parent exists?
				if (componentInit.Parent != null) {
					IComponentInitInfo parentComponentInfo = componentInfoByName[componentInit.Parent];
					if (parentComponentInfo == null)
						throw new Exception("Cannot find parent component with name='" + componentInit.Parent + "'");

					// copy all property-definitions
					for (int i = 0; i < parentComponentInfo.Properties.Length; i++) {
						propsList.Add(parentComponentInfo.Properties[i]);
						propsIndex[parentComponentInfo.Properties[i].Name] = i;
					}
					// copy all constructor-arg-definitions
					for (int i = 0; i < parentComponentInfo.ConstructorArgs.Length; i++)
						constructorArgs[i] = parentComponentInfo.ConstructorArgs[i];
					// copy type attribute (if not set)
					if (componentInit.ComponentType == null)
						componentInit.ComponentType = parentComponentInfo.ComponentType;
					// copy init method attribute (if not set)
					if (componentInit.InitMethod == null)
						componentInit.InitMethod = parentComponentInfo.InitMethod;
				}

				// extract constructor arguments
				var constructorArgNodes = componentNode.Select("ioc:constructor-arg",xmlNs);
				var autoIdx = 0;
				while (constructorArgNodes.MoveNext()) {
					var constructorArgNode = constructorArgNodes.Current;
					var indexAttr = constructorArgNode.GetAttribute("index", String.Empty);
					int index = !String.IsNullOrEmpty(indexAttr) ? Convert.ToInt32(indexAttr) : autoIdx;

					var explicitArgName = constructorArgNode.GetAttribute("name", String.Empty);
					if (!String.IsNullOrEmpty(explicitArgName)) {
						// try to find argument by name
						var paramByNameFound = false;
						foreach (var constrInfo in componentInit.ComponentType.GetConstructors()) {
							var cParams = constrInfo.GetParameters();
							if (cParams.Length == constructorArgNodes.Count) {
								for (int i = 0; i < cParams.Length; i++)
									if (cParams[i].Name == explicitArgName) {
										index = cParams[i].Position;
										paramByNameFound = true;
										break;
									}
							}
							if (paramByNameFound)
								break;
						}
						if (!paramByNameFound)
							throw new Exception(String.Format("Cannot find constructor argument by name='{0}' for component (name='{1}', type='{2}')",
								explicitArgName, componentInit.Name, componentInit.ComponentType));
					}
					try {
						constructorArgs[index] = ResolveValueInfo(constructorArgNode, xmlNs, componentInfoByName);
					} catch (Exception ex) {
						throw new XmlConfigurationException(
							String.Format("Cannot resolve value for constructor arg #{0}", index), ex);
					}
					autoIdx++;
				}
				// compose final constructor args list
				// 1) find greatest index
				int maxConstructorArgIndex = -1;
				foreach (int idx in constructorArgs.Keys)
					if (idx > maxConstructorArgIndex)
						maxConstructorArgIndex = (int)idx;
				// 2) create constructor args array
				componentInit.ConstructorArgs = new IValueInitInfo[maxConstructorArgIndex + 1];
				// 3) initialize constructor args array
				foreach (int idx in constructorArgs.Keys)
					componentInit.ConstructorArgs[idx] = constructorArgs[idx];


				// extract properies
				var propertyNodes = componentNode.Select("ioc:property",xmlNs);
				while (propertyNodes.MoveNext()) {
					var propertyNode = propertyNodes.Current;
					var propNameAttr = propertyNode.GetAttribute("name", String.Empty);
					try {
						PropertyInfo pInfo = new PropertyInfo(
							propNameAttr,
							ResolveValueInfo(propertyNode, xmlNs, componentInfoByName));
						if (propsIndex.ContainsKey(pInfo.Name)) {
							propsList[propsIndex[pInfo.Name]] = pInfo;
						} else {
							int idx = propsList.Count;
							propsList.Add(pInfo);
							propsIndex[pInfo.Name] = idx;
						}
					} catch (Exception ex) {
						throw new XmlConfigurationException(
							String.Format("Cannot resolve value for property '{0}' of component '{1}'", propNameAttr, componentInit.Name), ex);
					}
				}
				// compose final properties list
				componentInit.Properties = propsList.ToArray();
			}


			/// <summary>
			/// Resolve object instance by its definition in config
			/// </summary>
			protected IValueInitInfo ResolveValueInfo(XPathNavigator objectDefinition, IXmlNamespaceResolver xmlNs, IDictionary<string, ComponentInitInfo> components) {

				// component definition ?
				var componentNode = objectDefinition.SelectSingleNode("ioc:component", xmlNs);
				if (componentNode != null) {
					// build nested component init info
					ComponentInitInfo nestedComponentInfo = ReadComponentInitInfo(componentNode, xmlNs);
					InitValues(nestedComponentInfo, componentNode, xmlNs, components);

					return new RefValueInfo(nestedComponentInfo);
				}

				// reference ?
				var refNode = objectDefinition.SelectSingleNode("ioc:ref", xmlNs);
				if (refNode != null) {
					string refName = refNode.GetAttribute("name",String.Empty);
					if (components[refName] == null)
						throw new NullReferenceException("Reference to non-existent component: " + refName);
					return new RefValueInfo(components[refName]);
				}

				// value ?
				var valueNode = objectDefinition.SelectSingleNode("ioc:value",xmlNs);
				if (valueNode != null)
					return new ValueInitInfo(valueNode.Value);

				// xml ?
				var xmlNode = objectDefinition.SelectSingleNode("ioc:xml",xmlNs);
				if (xmlNode != null)
					return new ValueInitInfo(xmlNode.InnerXml);


				// System.Type reference ?
				var typeNode = objectDefinition.SelectSingleNode("ioc:type", xmlNs);
				if (typeNode != null)
					return new TypeValueInitInfo(TypeResolver.ResolveType(typeNode.InnerXml));


				// list ?
				var listNode = objectDefinition.SelectSingleNode("ioc:list", xmlNs);
				if (listNode != null) {
					var entryNodes = listNode.Select("ioc:entry", xmlNs);
					IValueInitInfo[] entries = new IValueInitInfo[entryNodes.Count];
					int listEntryIdx = 0;
					while (entryNodes.MoveNext()) {
						entries[listEntryIdx++] = ResolveValueInfo(entryNodes.Current, xmlNs, components);
					}
					return new ListValueInitInfo(entries);
				}

				// map ?
				var mapNode = objectDefinition.SelectSingleNode("ioc:map", xmlNs);
				if (mapNode != null) {
					var entryNodes = mapNode.Select("ioc:entry", xmlNs);
					MapEntryInfo[] entries = new MapEntryInfo[entryNodes.Count];
					int mapEntryIndex = 0;
					while (entryNodes.MoveNext())
						entries[mapEntryIndex++] = new MapEntryInfo(
							entryNodes.Current.GetAttribute("key",String.Empty),
							ResolveValueInfo(entryNodes.Current, xmlNs, components));
					return new MapValueInitInfo(entries);
				}

				// unknown object definition (???)
				throw new XmlConfigurationException("Unknown object definition");
			}



		}

	
		
	}
}
