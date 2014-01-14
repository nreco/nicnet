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

namespace NI.Ioc
{
	/// <summary>
	/// Represents property initalization definition
	/// </summary>
	public interface IPropertyInitInfo
	{
		/// <summary>
		/// Property name
		/// </summary>
		string Name { get; }
		
		/// <summary>
		/// Property value
		/// </summary>
		IValueInitInfo Value { get; }
	}
}
