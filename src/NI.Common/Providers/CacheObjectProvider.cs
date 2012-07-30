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
using NI.Common;
using NI.Common.Caching;

namespace NI.Common.Providers
{
	/// <summary>
	/// IObjectProvider proxy with caching.
	/// </summary>
	public class CacheObjectProvider : IObjectProvider
	{
		IObjectProvider _UnderlyingObjectProvider;
		ICache _Cache = new Cache();
		IStringProvider _CacheKeyProvider = new UniqueCacheKeyProvider();
		IObjectProvider _CacheFilter = null;

		/// <summary>
		/// Get or set optional cache filter
		/// </summary>
		/// <remarks>
		/// this filter allows perform some action with objects retrieved from cache \
		/// (if filter returns different object from context it will be updated in the cache)
		/// </remarks>
		public IObjectProvider CacheFilter {
			get { return _CacheFilter; }
			set { _CacheFilter = value; }
		}

		/// <summary>
		/// Get or set cache instance
		/// </summary>
		public ICache Cache {
			get { return _Cache; }
			set { _Cache = value; }
		}
		
		/// <summary>
		/// Get or set underlying object provider
		/// </summary>
		public IObjectProvider UnderlyingObjectProvider {
			get { return _UnderlyingObjectProvider; }
			set { _UnderlyingObjectProvider = value; }
		}
		
		/// <summary>
		/// Get or set cache key provider
		/// </summary>
		public IStringProvider CacheKeyProvider {
			get { return _CacheKeyProvider; }
			set { _CacheKeyProvider = value; }
		}		
	
		public CacheObjectProvider() {
		}

		public object GetObject(object context) {
			string cacheKey = CacheKeyProvider.GetString(context);
			object cachedValue = Cache.Get( cacheKey );
			if (cachedValue==null) {
				cachedValue = UnderlyingObjectProvider.GetObject(context);
				if (cachedValue==null)
					cachedValue = new NullPlaceholder();
				Cache.Put(cacheKey, cachedValue);
			} else if (CacheFilter!=null) {
				object filteredCachedValue = CacheFilter.GetObject(cachedValue);
				if (filteredCachedValue!=cachedValue) {
					Cache.Put(cacheKey, filteredCachedValue);
				}
				cachedValue = filteredCachedValue;
			}
			return cachedValue is NullPlaceholder ? null : cachedValue;
		}

		[Serializable]
		public class NullPlaceholder { }
		
	}
}
