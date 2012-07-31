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

namespace NI.Common.Expressions
{
	/// <summary>
	/// </summary>
	public interface IExpressionDescriptor
	{
		/// <summary>
		/// Get expression marker
		/// </summary>
		string Marker { get; }
		
		/// <summary>
		/// Ge expression resolver that should be used to evaluate expressions with given marker
		/// </summary>
		IExpressionResolver ExprResolver { get; }
	}
}
