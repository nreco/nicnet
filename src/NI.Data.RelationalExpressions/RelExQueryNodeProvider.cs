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
using System.Collections.Specialized;

using NI.Data;
using NI.Common;
using NI.Common.Expressions;
using NI.Common.Providers;

namespace NI.Data.RelationalExpressions
{
	/// <summary>
	/// Relex-based query node provider.
	/// </summary>
	public class RelExQueryNodeProvider : NI.Data.IQueryNodeProvider, IObjectProvider
	{
		IExpressionResolver _ExprResolver;
		string _RelExCondition;
		IRelExQueryParser _RelExQueryParser = new RelExQueryParser();
		string _ContextArgumentKey = "arg";
		
		/// <summary>
		/// Get or set key in context where provider argument should be stored
		/// </summary>
		public string ContextArgumentKey {
			get { return _ContextArgumentKey; }
			set { _ContextArgumentKey = value; }
		}		
		
		/// <summary>
		/// Get or set relational expression parser
		/// </summary>
		public IRelExQueryParser RelExQueryParser {
			get { return _RelExQueryParser; }
			set { _RelExQueryParser = value; }
		}
		
		/// <summary>
		/// Get or set relational expression condition
		/// </summary>
		public string RelExCondition {
			get { return _RelExCondition; }
			set { _RelExCondition = value; }
		}
		
		/// <summary>
		/// Get or set expression resolver used for preparsing relex
		/// </summary>
		public IExpressionResolver ExprResolver {
			get { return _ExprResolver; }
			set { _ExprResolver = value; }
		}
		
		public RelExQueryNodeProvider()
		{
		}
		
		public virtual IQueryNode GetQueryNode(object contextObj) {
			IDictionary context;
			if (contextObj is IDictionary) {
				context = (IDictionary)contextObj;
			} else {
				context = new ListDictionary();
				context[ContextArgumentKey] = contextObj;
			}
			string relexCondition = ExprResolver!=null ? Convert.ToString( ExprResolver.Evaluate(context, RelExCondition) ) : RelExCondition;
			string relEx = String.Format("sourcename({0})[*]", relexCondition);
			IQuery q = RelExQueryParser.Parse(relEx);
			return q.Root;
		}

		public object GetObject(object context) {
			return GetQueryNode(context);
		}

	}
}
