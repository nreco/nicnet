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

namespace NI.Common.Expressions
{
	/// <summary>
	/// A Resolver allows custom resolution of the expression,
	/// and can be added in front of the exl engine, or after in the evaluation.
	/// </summary>
	public interface IExpressionResolver
	{
	
		/// <summary>
		/// Evaluates an expression against the context 
		/// </summary>
		/// <param name="context">current data context</param>
		/// <param name="expression">expression to evauluate</param>
		/// <returns>value (may be null)</returns>
		object Evaluate(IDictionary context, string expression);
	}
}
