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

namespace NI.Ioc
{
	/// <summary>
	/// IComponentsConfig implementation based on XML.
	/// </summary>
	public class ComponentsConfig : Component, IComponentsConfig
	{
		ComponentInitInfo[] Components;
		IDictionary<string,ComponentInitInfo> ComponentsByName;
		
		bool _DefaultLazyInit = false;
		bool _StrictComponentNames = false;
		string _Description = null;
		
		/// <summary>
		/// Enables strict mode for component names (duplicate names will cause an exception)
		/// </summary>
		public bool StrictComponentNames {
			get { return _StrictComponentNames; }
			set { _StrictComponentNames = value; }
		}
		
		/// <summary>
		/// Default value of lazy init flag for components in this collection
		/// False by default
		/// </summary>
		public bool DefaultLazyInit { get { return _DefaultLazyInit; } }
		
		/// <summary>
		/// Components collection description
		/// Null by default
		/// </summary>
		public string Description { get { return _Description; } }
		
		public ComponentsConfig()
		{
			
		}
		
		public void Load(XmlNode componentsNode) {
			// extract default lazy init value
			if (componentsNode.Attributes["default-lazy-init"]!=null)
				_DefaultLazyInit = Convert.ToBoolean( componentsNode.Attributes["default-lazy-init"].Value );
			if (componentsNode.Attributes["strict-names"] != null)
				StrictComponentNames = Convert.ToBoolean(componentsNode.Attributes["strict-names"].Value);
		
			// extract description value
			XmlNode descriptionNode = componentsNode.SelectSingleNode("description");
			if (descriptionNode!=null) _Description = descriptionNode.InnerText;
			
			// build components info collection
			XmlNodeList componentNodes = componentsNode.SelectNodes("component");
			Components = new ComponentInitInfo[componentNodes.Count];

			ComponentsByName = new Dictionary<string, ComponentInitInfo>();
			for (int i=0; i<componentNodes.Count; i++) {
				var componentInfo = new ComponentInitInfo( componentNodes[i], this );;
				Components[i] = componentInfo;

				if (componentInfo.Name!=null) {
					if (ComponentsByName.ContainsKey(componentInfo.Name)) {
						if (StrictComponentNames)
							throw new System.Configuration.ConfigurationException(
								String.Format("Duplicate component name is found: {0}", componentInfo.Name), componentNodes[i]);
					} else {
						ComponentsByName[ componentInfo.Name ] = componentInfo;
					}
				}
				
			}
			
			// initialize components info
			for (int i=0; i<componentNodes.Count; i++)
				try {
					Components[i].InitValues( componentNodes[i], this );
				} catch (Exception ex) {
					throw new Exception(
						String.Format("Cannot resolve values for '{0}' component definition", Components[i].Name), ex);
				}
		}
		
		
		public IComponentInitInfo this[string name] {
			get {
				if (name==null)
					return null; // legacy compatibility behavour
				return ComponentsByName.ContainsKey(name) ? ComponentsByName[name] : null;
			}
		}
		
		public IComponentInitInfo this[Type type] {
			get {
				for (int i=0; i<Components.Length; i++)
					if (Components[i]!=null && type.IsAssignableFrom(Components[i].ComponentType) )
						return Components[i];
				return null;
			}
		}
		
		public IEnumerator GetEnumerator() {
			return Components.GetEnumerator();
		}		
		
	}
}
