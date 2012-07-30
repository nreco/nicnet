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

namespace NI.Common.Expressions
{
	/// <summary>
	/// Component reference expression resolver.
	/// </summary>
	public class ComponentExprResolver : IExpressionResolver
	{
		INamedServiceProvider _NamedServiceProvider;
		
		/// <summary>
		/// Get or set named-service provider component
		/// </summary>
		public INamedServiceProvider NamedServiceProvider {
			get { return _NamedServiceProvider; }
			set { _NamedServiceProvider = value; }
		}
			
		public ComponentExprResolver() {
		}

		public object Evaluate(IDictionary context, string expression) {
			return NamedServiceProvider.GetService(expression);
		}

	}
}
