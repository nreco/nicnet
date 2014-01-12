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

namespace NI.Ioc
{
	/// <summary>
	/// Represents components graph configuration
	/// </summary>
	public interface IComponentsConfig : IEnumerable
	{
		/// <summary>
		/// Default value of lazy init flag for components in this collection
		/// </summary>
		/// <remarks>False by default</remarks>
		bool DefaultLazyInit { get; }
		
		/// <summary>
		/// Components collection description text
		/// </summary>
		/// <remarks>Null by default</remarks>
		string Description { get; }

		/// <summary>
		/// Get number of components definitions
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Get component definition by name (alias)
		/// </summary>
		/// <value>ICompanyInitInfo object or null</value>
		IComponentInitInfo this[string name] { get; }
		
		/// <summary>
		/// Get component definition by System.Type
		/// </summary>
		/// <value>ICompanyInitInfo object or null</value>
		IComponentInitInfo this[Type type] { get; }

	}
}
