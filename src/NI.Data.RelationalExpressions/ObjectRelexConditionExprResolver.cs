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
using System.Text;

using NI.Data.Dalc;
using NI.Common.Expressions;
using NI.Common.Providers;


namespace NI.Data.RelationalExpressions {
	
	/// <summary>
	/// Object RelEx condition expression resolver. Evaluates condition in RelEx syntax in some 'object' context.
	/// </summary>
	/// <remarks>This resolver does not support all features of RelEx syntax (nested queries for instance).</remarks>
	public class ObjectRelexConditionExprResolver : ObjectQueryConditionEvaluator, IExpressionResolver {

		IRelExQueryNodeParser _ConditionParser = new RelExQueryNodeParser();
		
		public IRelExQueryNodeParser ConditionParser {
			get { return _ConditionParser; }
			set { _ConditionParser = value; }
		}
		
		public object Evaluate(IDictionary context, string expression) {
			IQueryNode condition = ConditionParser.Parse(expression);
			return EvaluateInternal(context, condition);
		}
		
	}
}
