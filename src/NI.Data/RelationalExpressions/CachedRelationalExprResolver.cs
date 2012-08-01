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

using NI.Common;
using NI.Common.Caching;

namespace NI.Data.RelationalExpressions
{
	/// <summary>
	/// Relational Expr Resolver that uses simple caching
	/// </summary>
	public class CachedRelationalExprResolver : RelationalExprResolver
	{
		ICache _Cache = new Cache();
		
		/// <summary>
		/// Get or set cache instance used for caching relex results
		/// </summary>
		public ICache Cache {
			get { return _Cache; }
			set { _Cache = value; }
		}
		
		Hashtable cache = new Hashtable();
	
		public CachedRelationalExprResolver()
		{
		}
		
		public override object Evaluate(IDictionary context, string expression) {
			if (ReturnList)
				return base.Evaluate(context, expression);
			object cachedValue = Cache.Get(expression);
			if (cachedValue==null) {
				cachedValue = base.Evaluate(context, expression);
				Cache.Put(expression, cachedValue);
			}
			return cachedValue;
		}
		
	}
}
