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

namespace NI.Ioc
{
	/// <summary>
	/// The root interface for accessing a IoC components container.
	/// </summary>
	/// <remarks>
	/// This is the basic client view of a components container.
	/// </remarks>
	public interface IComponentFactory : IServiceProvider
	{
		/// <summary>
		/// Return an instance, which may be shared or independent, of specified component
		/// </summary>
		object GetComponent(string name);

		/// <summary>
		/// Return an instance, which may be shared or independent, of the specified component.
		/// </summary>
		/// <param name="name">component name</param>
		/// <param name="requiredType">type the component must match. Can be an interface or base of the actual class, or null for any match.</param>
		/// <returns>component instance</returns>
		object GetComponent(string name, Type requiredType);

	}
}
