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
	/// Dependency attribute. Used for marking properties that should
	/// be initialized before 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class DependencyAttribute : Attribute
	{
		bool _Required = true;
		
		/// <summary>
		/// Get or set flag that indicates whether target property is required dependency
		/// </summary>
		public bool Required {
			get { return _Required; }
			set { _Required = value; }
		}
		
		/// <summary>
		/// Initializes a new instance of the DependencyAttribute class.
		/// </summary>
		public DependencyAttribute()  {
		}
		

		
	}
}
