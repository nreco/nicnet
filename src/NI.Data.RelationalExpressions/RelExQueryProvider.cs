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

using NI.Data.Dalc;
using NI.Common;
using NI.Common.Expressions;
using NI.Common.Providers;

namespace NI.Data.RelationalExpressions
{
	/// <summary>
	/// Relex-based query provider.
	/// </summary>
	public class RelExQueryProvider : NI.Data.Dalc.IQueryProvider, IObjectProvider
	{
		IExpressionResolver _ExprResolver;
		string _RelEx;
		IRelExQueryParser _RelExQueryParser = new RelExQueryParser();
		string _ContextArgumentKey = "arg";
		IStringListProvider _SortProvider = null;
        IObjectProvider _ExtendedPropertiesProvider = null;

		
		/// <summary>
		/// Get or set key in context where provider argument should be stored
		/// </summary>
		[Dependency(Required=false)]
		public string ContextArgumentKey {
			get { return _ContextArgumentKey; }
			set { _ContextArgumentKey = value; }
		}		
		
		/// <summary>
		/// Get or set relational expression parser
		/// </summary>
		[Dependency(Required=false)]
		public IRelExQueryParser RelExQueryParser {
			get { return _RelExQueryParser; }
			set { _RelExQueryParser = value; }
		}
		
		/// <summary>
		/// Get or set relational expression
		/// </summary>
		[Dependency]
		public string RelEx {
			get { return _RelEx; }
			set { _RelEx = value; }
		}
		
		/// <summary>
		/// Get or set expression resolver used for preparsing relex
		/// </summary>
		[Dependency]
		public IExpressionResolver ExprResolver {
			get { return _ExprResolver; }
			set { _ExprResolver = value; }
		}
		
		/// <summary>
		/// Get or set query sort settings 
		/// </summary>
		[Dependency(Required=false)]
		public IStringListProvider SortProvider {
			get { return _SortProvider; }
			set { _SortProvider = value; }
		}

        /// <summary>
        /// Get or set query extended properties provider
        /// </summary>
        [Dependency(Required = false)]
        public IObjectProvider ExtendedPropertiesProvider
        {
            get { return _ExtendedPropertiesProvider; }
            set { _ExtendedPropertiesProvider = value; }
        }
		
	
		public RelExQueryProvider()
		{
		}
		
		public virtual IQuery GetQuery(object contextObj) {
			IDictionary context;
			if (contextObj is IDictionary) {
				context = (IDictionary)contextObj;
			} else {
				context = new ListDictionary();
				context[ContextArgumentKey] = contextObj;
			}
			
			string relEx = Convert.ToString( ExprResolver.Evaluate(context, RelEx) );
			IQuery q = RelExQueryParser.Parse(relEx);
			if (q is Query) {
				Query query = (Query)q;
				if (SortProvider!=null)
					query.Sort = SortProvider.GetStringList(context);
                if (ExtendedPropertiesProvider != null) {
                    object extPropsObj = ExtendedPropertiesProvider.GetObject(context);
                    if(extPropsObj is IDictionary)
                        query.ExtendedProperties = (IDictionary)extPropsObj;
                }
			}
        
			return q;
		}

		public object GetObject(object context) {
			return GetQuery(context);
		}

	}
}
