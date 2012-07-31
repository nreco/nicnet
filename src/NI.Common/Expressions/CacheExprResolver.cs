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
using NI.Common.Caching;
using NI.Common.Providers;

namespace NI.Common.Expressions
{
	/// <summary>
	/// IExpressionResolver proxy with caching.
	/// </summary>
	public class CacheExprResolver : IExpressionResolver
	{
		IExpressionResolver _UnderlyingExprResolver;
		ICache _Cache;
		IStringProvider _CacheKeyProvider = new UniqueCacheKeyProvider();

		/// <summary>
		/// Get or set ICache instance
		/// </summary>
		public ICache Cache {
			get { return _Cache; }
			set { _Cache = value; }
		}
		
		/// <summary>
		/// Get or set underlying expression provider
		/// </summary>
		public IExpressionResolver UnderlyingExprResolver {
			get { return _UnderlyingExprResolver; }
			set { _UnderlyingExprResolver = value; }
		}
		
		/// <summary>
		/// Get or set cache-key provider
		/// </summary>
		public IStringProvider CacheKeyProvider {
			get { return _CacheKeyProvider; }
			set { _CacheKeyProvider = value; }
		}		
	
		public CacheExprResolver() {
		}

		public object Evaluate(IDictionary context, string expression) {
			string cacheKey = CacheKeyProvider.GetString(context)+expression;
			object cachedValue = Cache.Get( cacheKey );
			if (cachedValue==null) {
				cachedValue = UnderlyingExprResolver.Evaluate(context, expression);
				if (cachedValue==null)
					cachedValue = new NullPlaceholder();
				Cache.Put(cacheKey, cachedValue);
			}
			return cachedValue is NullPlaceholder ? null : cachedValue;
		}

		[Serializable]
		public class NullPlaceholder { }
		

	}
}
