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

namespace NI.Common
{
	/// <summary>
	/// Dependency resolver component
	/// </summary>
	public class DependencyResolver : IDependencyResolver
	{
		object[] _PatternObjects = new object[0];
		PatternPropertyDescriptor[] _PatternProperties = null;
		string[] _ExcludeTypeNamePrefixes = null;
		
		public string[] ExcludeTypeNamePrefixes {
			get { return _ExcludeTypeNamePrefixes; }
			set { _ExcludeTypeNamePrefixes = value; }
		}
		
		/// <summary>
		/// Get or set 'pattern objects' for dependecy resolver
		/// </summary>
		public object[] PatternObjects {
			get { return _PatternObjects; }
			set { _PatternObjects = value; }
		}
		
		/// <summary>
		/// Get or set 'pattern property descriptors' for dependency resolver
		/// </summary>
		public PatternPropertyDescriptor[] PatternProperties {
			get { return _PatternProperties; }
			set { _PatternProperties = value; }
		}
		
		/// <summary>
		/// Resolve component dependencies using current Site
		/// </summary>
		/// <param name="component"></param>
		public void Resolve(IComponent component) {
			Resolve(component, component.Site);
		}
		
		/// <summary>
		/// <see cref="IDependencyResolver.Resolve"/>
		/// </summary>
		public void Resolve(object component, IServiceProvider serviceProvider) {
			if (ExcludeTypeNamePrefixes!=null && component!=null) {
				string typeFullName = component.GetType().FullName;
				for (int i=0; i<ExcludeTypeNamePrefixes.Length; i++)
					if (typeFullName.StartsWith(ExcludeTypeNamePrefixes[i]))
						return;
			}
			
			if (serviceProvider==null) return;
			PropertyInfo[] props = component.GetType().GetProperties();
			foreach (PropertyInfo p in props) 
				if (p.IsDefined( typeof(DependencyAttribute), true) ) {
					object service = GetService(component, p, serviceProvider );
					if (service!=null)
						p.SetValue( component, service, null);
				}
		}
		
		protected virtual object GetService(object component, PropertyInfo p, IServiceProvider serviceProvider) {
			for (int i=0; i<PatternObjects.Length; i++)
				if (PatternObjects[i].GetType().IsInstanceOfType( component ) )
					if (PatternObjects[i].GetType().GetProperty(p.Name)!=null)
						return p.GetValue(PatternObjects[i], null);
			
			if (PatternProperties!=null)
				for (int i=0; i<PatternProperties.Length; i++) {
					PatternPropertyDescriptor pattern = PatternProperties[i];
					bool matchType = pattern.MatchType==null || pattern.MatchType.IsInstanceOfType(component);
					bool matchPropName = pattern.MatchPropertyName==null || pattern.MatchPropertyName==p.Name;
					bool matchPropType = pattern.MatchPropertyType==null || p.PropertyType.IsAssignableFrom(pattern.MatchPropertyType);
					bool matchService = pattern.Service==null || p.PropertyType.IsAssignableFrom(pattern.Service.GetType());
					if (matchType && matchPropName && matchPropType && matchService)
						return pattern.Service;
				}
			
			// do not autoresolve concrete classes
			if (!p.PropertyType.IsInterface || p.GetValue(component,null)!=null)
				return null;
			
			return serviceProvider.GetService( p.PropertyType );
		}
		
		public class PatternPropertyDescriptor {
			Type _MatchType = null;
			string _MatchPropertyName = null;
			Type _MatchPropertyType = null;
			object _Service;
			
			public Type MatchType {
				get { return _MatchType; }
				set { _MatchType = value; }
			}
			public string MatchPropertyName {
				get { return _MatchPropertyName; }
				set { _MatchPropertyName = value; }
			}
			public Type MatchPropertyType {
				get { return _MatchPropertyType; }
				set { _MatchPropertyType = value; }
			}
			public object Service {
				get { return _Service; }
				set { _Service = value; }
			}			
		}

	}
}
