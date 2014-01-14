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
using System.Collections;

namespace NI.Ioc
{
	/// <summary>
	/// Represents components graph configuration
	/// </summary>
	public interface IComponentFactoryConfiguration : IEnumerable
	{

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
