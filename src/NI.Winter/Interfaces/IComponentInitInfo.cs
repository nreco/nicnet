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
using System.Collections;

namespace NI.Winter
{
	/// <summary>
	/// Component init definition interface.
	/// </summary>
	public interface IComponentInitInfo
	{

		/// <summary>
		/// Singleton flag.
		/// </summary>
		bool Singleton { get; }

		/// <summary>
		/// Lazy init flag.
		/// </summary>
		bool LazyInit { get; }
	
		/// <summary>
		/// Component name (alias).
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Component System.Type
		/// </summary>
		Type ComponentType { get; }
		
		/// <summary>
		/// Component description. Null by default.
		/// </summary>
		string Description { get; }
	
		/// <summary>
		/// Constructor arguments.
		/// </summary>
		IValueInitInfo[] ConstructorArgs { get; }
	
		/// <summary>
		/// Properies to set
		/// </summary>
		IPropertyInitInfo[] Properties { get; }
		
		/// <summary>
		/// Initialization method name. Null by default.
		/// </summary>
		string InitMethod { get; }


	}
}
