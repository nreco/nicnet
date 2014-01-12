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
using System.IO;
using System.Reflection;

namespace NI.Ioc
{
	/// <summary>
	/// IComponentsConfig implementation based on XML.
	/// </summary>
	public class XmlComponentConfiguration : ComponentConfiguration
	{

		public XmlComponentConfiguration(string xml)
			: this(LoadXmlDocument(xml)) {

		}

		static XmlDocument LoadXmlDocument(string xml) {
			// load XML into XmlDocument
			var xmlDoc = new XmlDocument();
			xmlDoc.PreserveWhitespace = true; // whitespaces are significant
			xmlDoc.LoadXml(xml);
			return xmlDoc;
		}

		public XmlComponentConfiguration(XmlDocument componentsXmlDoc)
			: base(new XmlConfigurationReader().ReadComponents(componentsXmlDoc)) {

			// extract description value
			XmlNode descriptionNode = componentsXmlDoc.DocumentElement.SelectSingleNode("description");
			if (descriptionNode != null)
				Description = descriptionNode.InnerText;
		}


		public class XmlConfigurationReader {

			public TypeResolver TypeResolver;
			bool StrictComponentNames = true;
			bool DefaultLazyInit = false;

			public XmlConfigurationReader() {
				TypeResolver = new TypeResolver();
			}

			public IComponentInitInfo[] ReadComponents(XmlDocument componentsXmlDoc) {
				// ensure that schema is correct
				ValidateXmlSchema(componentsXmlDoc);

				var componentsNode = componentsXmlDoc.DocumentElement;

				// extract default lazy init value
				if (componentsNode.Attributes["default-lazy-init"] != null)
					DefaultLazyInit = Convert.ToBoolean(componentsNode.Attributes["default-lazy-init"].Value);
				if (componentsNode.Attributes["strict-names"] != null)
					StrictComponentNames = Convert.ToBoolean(componentsNode.Attributes["strict-names"].Value);

				// build components info collection
				XmlNodeList componentNodes = componentsNode.SelectNodes("component");
				var Components = new ComponentInitInfo[componentNodes.Count];
				var ComponentsByName = new Dictionary<string, ComponentInitInfo>();

				for (int i = 0; i < componentNodes.Count; i++) {
					var componentInfo = ReadComponentInitInfo(componentNodes[i]);
					Components[i] = componentInfo;

					if (componentInfo.Name != null) {
						if (ComponentsByName.ContainsKey(componentInfo.Name)) {
							if (StrictComponentNames)
								throw new System.Configuration.ConfigurationException(
									String.Format("Duplicate component name is found: {0}", componentInfo.Name), componentNodes[i]);
						} else {
							ComponentsByName[componentInfo.Name] = componentInfo;
						}
					}

				}

				// initialize components info
				for (int i = 0; i < componentNodes.Count; i++)
					try {
						InitValues(Components[i], componentNodes[i], ComponentsByName);
					} catch (Exception ex) {
						throw new Exception(
							String.Format("Cannot resolve values for '{0}' component definition", Components[i].Name), ex);
					}

				return Components;
			}

			protected void ValidateXmlSchema(XmlDocument xmlDoc) {
				var schema = XmlSchema.Read(GetXsdStream(), null);
				xmlDoc.Schemas.Add(schema);
				xmlDoc.Validate(new ValidationEventHandler(ValidationCallBack));
			}

			Stream GetXsdStream() {
				string name = this.GetType().Namespace + ".ComponentsConfigSchema.xsd";
				return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
			}

			protected void ValidationCallBack(object sender, ValidationEventArgs args) {
				if (args.Exception is XmlSchemaValidationException) {
					XmlSchemaValidationException validationEx = (XmlSchemaValidationException)args.Exception;
					if (validationEx.SourceObject is XmlNode) {
						XmlNode xmlNode = (XmlNode)validationEx.SourceObject;
						throw new XmlSchemaValidationException(
							String.Format("{0}\nancestor axis:\n{1}",
								args.Message, FormatXmlValidationTrace(xmlNode)),
							 args.Exception, args.Exception.LineNumber, args.Exception.LinePosition);
					}
				}
				throw new XmlSchemaValidationException(String.Format("{0}", args.Message, args.Exception.LineNumber), args.Exception, args.Exception.LineNumber, args.Exception.LinePosition);
			}

			protected string FormatXmlValidationTrace(XmlNode node) {
				if (node == null)
					return String.Empty;
				var sb = new System.Text.StringBuilder();
				if (node is XmlElement) {
					sb.Append("<");
					sb.Append(node.Name);
					foreach (XmlAttribute xmlAttr in node.Attributes)
						sb.AppendFormat(" {0}=\"{1}\"", xmlAttr.Name, xmlAttr.Value);
					sb.Append(">\n");
				}
				if (node is XmlText) {
					sb.Append("[");
					sb.Append(((XmlText)node).Data);
					sb.Append("]\n");
				}
				return FormatXmlValidationTrace(node.ParentNode) + sb.ToString();
			}

			
			protected ComponentInitInfo ReadComponentInitInfo(XmlNode componentNode)
			{
				var componentInit = new ComponentInitInfo();

				// extract component name (optional)
				if (componentNode.Attributes["name"]!=null)
					componentInit.Name = componentNode.Attributes["name"].Value;

				// extract component parent (optional)
				if (componentNode.Attributes["parent"]!=null)
					componentInit.Parent = componentNode.Attributes["parent"].Value;
			
				// extract component type (optional)
				if (componentNode.Attributes["type"]!=null) {
					componentInit.ComponentType = TypeResolver.ResolveType(componentNode.Attributes["type"].Value);
					if (componentInit.ComponentType == null)
						throw new System.Configuration.ConfigurationException("Cannot resolve type "+componentNode.Attributes["type"].Value );
				}
			
				// extract component lazy init flag
				componentInit.LazyInit = DefaultLazyInit;
				if (componentNode.Attributes["lazy-init"]!=null)
					componentInit.LazyInit = Convert.ToBoolean(componentNode.Attributes["lazy-init"].Value);
			
				// extract component singleton flag
				if (componentNode.Attributes["singleton"]!=null)
					componentInit.Singleton = Convert.ToBoolean(componentNode.Attributes["singleton"].Value);
			
				// extract description value
				XmlNode descriptionNode = componentNode.SelectSingleNode("description");
				if (descriptionNode!=null)
					componentInit.Description = descriptionNode.InnerText;
			
				// extract init-method value
				if (componentNode.Attributes["init-method"]!=null)
					componentInit.InitMethod = componentNode.Attributes["init-method"].Value;

				return componentInit;
			}



			/// <summary>
			/// Initialize component values (constructor arguments / properties )
			/// </summary>
			protected void InitValues(ComponentInitInfo componentInit, XmlNode componentNode, IDictionary<string,ComponentInitInfo> componentInfoByName) {
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
				XmlNodeList constructorArgNodes = componentNode.SelectNodes("constructor-arg");
				var autoIdx = 0;
				foreach (XmlNode constructorArgNode in constructorArgNodes) {
					int index = constructorArgNode.Attributes["index"] != null ? Convert.ToInt32(constructorArgNode.Attributes["index"].Value) : autoIdx;
					if (constructorArgNode.Attributes["name"] != null) {
						var explicitArgName = constructorArgNode.Attributes["name"].Value;
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
						constructorArgs[index] = ResolveValueInfo(constructorArgNode, componentInfoByName);
					} catch (Exception ex) {
						throw new Exception(
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
				XmlNodeList propertyNodes = componentNode.SelectNodes("property");
				foreach (XmlNode propertyNode in propertyNodes) {
					try {
						PropertyInfo pInfo = new PropertyInfo(
							propertyNode.Attributes["name"].Value,
							ResolveValueInfo(propertyNode, componentInfoByName));
						if (propsIndex.ContainsKey(pInfo.Name)) {
							propsList[propsIndex[pInfo.Name]] = pInfo;
						} else {
							int idx = propsList.Count;
							propsList.Add(pInfo);
							propsIndex[pInfo.Name] = idx;
						}
					} catch (Exception ex) {
						throw new Exception(
							String.Format("Cannot resolve value for property '{0}'", propertyNode.Attributes["name"].Value), ex);
					}
				}
				// compose final properties list
				componentInit.Properties = propsList.ToArray();
			}


			/// <summary>
			/// Resolve object instance by its definition in config
			/// </summary>
			protected IValueInitInfo ResolveValueInfo(XmlNode objectDefinition, IDictionary<string, ComponentInitInfo> components) {

				// component definition ?
				XmlNode componentNode = objectDefinition.SelectSingleNode("component");
				if (componentNode != null) {
					// build nested component init info
					ComponentInitInfo nestedComponentInfo = ReadComponentInitInfo(componentNode);
					InitValues(nestedComponentInfo, componentNode, components);

					return new RefValueInfo(nestedComponentInfo);
				}

				// reference ?
				XmlNode refNode = objectDefinition.SelectSingleNode("ref");
				if (refNode != null) {
					string refName = refNode.Attributes["name"].Value;
					if (components[refName] == null)
						throw new NullReferenceException("Reference to non-existent component: " + refName);
					return new RefValueInfo(components[refName]);
				}

				// value ?
				XmlNode valueNode = objectDefinition.SelectSingleNode("value");
				if (valueNode != null)
					return new ValueInitInfo(valueNode.InnerText);

				// xml ?
				XmlNode xmlNode = objectDefinition.SelectSingleNode("xml");
				if (xmlNode != null)
					return new ValueInitInfo(xmlNode.InnerXml);


				// System.Type reference ?
				XmlNode typeNode = objectDefinition.SelectSingleNode("type");
				if (typeNode != null)
					return new TypeValueInitInfo(TypeResolver.ResolveType(typeNode.InnerXml));


				// list ?
				XmlNode listNode = objectDefinition.SelectSingleNode("list");
				if (listNode != null) {
					XmlNodeList entryNodes = listNode.SelectNodes("entry");
					IValueInitInfo[] entries = new IValueInitInfo[entryNodes.Count];
					for (int i = 0; i < entryNodes.Count; i++)
						entries[i] = ResolveValueInfo(entryNodes[i], components);
					return new ListValueInitInfo(entries);
				}

				// map ?
				XmlNode mapNode = objectDefinition.SelectSingleNode("map");
				if (mapNode != null) {
					XmlNodeList entryNodes = mapNode.SelectNodes("entry");
					MapEntryInfo[] entries = new MapEntryInfo[entryNodes.Count];
					for (int i = 0; i < entryNodes.Count; i++)
						entries[i] = new MapEntryInfo(
							entryNodes[i].Attributes["key"].Value,
							ResolveValueInfo(entryNodes[i], components));
					return new MapValueInitInfo(entries);
				}

				// unknown object definition (???)
				return null;
			}



		}

	
		
	}
}
