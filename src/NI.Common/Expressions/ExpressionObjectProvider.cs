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
using System.Collections.Specialized;

using NI.Common.Providers;

namespace NI.Common.Expressions
{
	/// <summary>
	/// Object provider based on expressions.
	/// </summary>
	public class ExpressionObjectProvider : IObjectProvider,
		IStringProvider,
		IDateTimeProvider,
		IBooleanProvider
	{
		IExpressionResolver _ExprResolver;
		string _Expression = null;

		/// <summary>
		/// Get or set expression string (ignored in case when ExpressionProvider specified)
		/// </summary>
		public string Expression {
			get { return _Expression; }
			set { _Expression = value; }
		}
		
		/// <summary>
		/// Get or set expression resolver used to evaluate expression
		/// </summary>
		public IExpressionResolver ExprResolver {
			get { return _ExprResolver; }
			set { _ExprResolver = value; }
		}
		
		public ExpressionObjectProvider()
		{
		}
		
		/// <summary>
		/// Returns expression evaluation result in specified context
		/// </summary>
		/// <returns>evaluation result</returns>
		public object GetObject(object contextObj) {
			IDictionary context = contextObj as IDictionary;
			if (context == null) {
				context = new Hashtable();
				context["arg"] = contextObj; // this is default behaviour for nic.net, let it be
			}
			return ExprResolver.Evaluate(context, Expression);
		}

		/// <summary>
		/// Returns expression evaluation result as string value
		/// </summary>
		/// <returns>string result</returns>
		public string GetString(object contextObj) {
			return Convert.ToString( GetObject(contextObj) );
		}

		/// <summary>
		/// Returns expression evaluation result as DateTime object
		/// </summary>
		/// <returns>DateTime result</returns>
		/// <exception cref="InvalidCastException">when result cannot be converted to DateTime object</exception>
		public DateTime GetDateTime(object contextObj) {
			return Convert.ToDateTime( GetObject(contextObj) );
		}

		/// <summary>
		/// Returns expression evaluation result as bool object
		/// </summary>
		/// <returns>boolean result</returns>
		/// <exception cref="InvalidCastException">when result cannot be converted to bool object</exception>
		public bool GetBoolean(object contextObj) {
			return Convert.ToBoolean( GetObject(contextObj) );
		}

	}
}
