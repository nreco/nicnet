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

namespace NI.Ioc {

	/// <summary>
	/// This attribute is used to mark properties as targets for injection. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class DependencyAttribute : Attribute
	{
		
		/// <summary>
		/// Get or set flag that indicates whether target property is required dependency
		/// </summary>
		public string Name { get; private set; }
		
		/// <summary>
		/// Create an instance of DependencyAttribute with no name (injection by type). 
		/// </summary>
		public DependencyAttribute()  {
		}

		/// <summary>
		/// Create an instance of DependencyAttribute with the given name.
		/// </summary>
		public DependencyAttribute(string name) {
			Name = name;
		}

		
	}
}
