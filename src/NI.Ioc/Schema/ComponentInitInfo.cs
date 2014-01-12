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
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace NI.Ioc
{
	/// <summary>
	/// Component Info
	/// </summary>
	public class ComponentInitInfo : IComponentInitInfo
	{
		
		bool ValuesInitialized = false;
		
		/// <summary>
		/// Singleton flag. True by default.
		/// </summary>
		public bool Singleton { get; set; }

		/// <summary>
		/// Lazy init flag. False by default.
		/// </summary>
		public bool LazyInit { get; set; }
	
		/// <summary>
		/// Component name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Component parent name
		/// </summary>
		public string Parent { get; set; }

		/// <summary>
		/// Initialization method name. Null by default.
		/// </summary>
		public string InitMethod { get; set; }
		
		/// <summary>
		/// Component System.Type
		/// </summary>
		public Type ComponentType { get; set; }
		
		/// <summary>
		/// Component description. Null by default.
		/// </summary>
		public string Description { get; set; }
	
		/// <summary>
		/// Constructor arguments.
		/// </summary>
		public IValueInitInfo[] ConstructorArgs { get; set; }
		
		/// <summary>
		/// Properies to set
		/// </summary>
		public IPropertyInitInfo[] Properties { get; set; }


		public ComponentInitInfo() { }

		public ComponentInitInfo(string name, Type t, bool singleton, bool lazyInit) {
			Name = name;
			ComponentType = t;
			Singleton = singleton;
			LazyInit = lazyInit;
		}		
		
	}
}
