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
using System.Text;
using System.Globalization;
using System.Threading;

using NI.Common.Caching;
using NI.Common.Providers;

namespace NI.Common.Globalization {
	
	public class CacheResourceProvider : IResourceProvider {
		
		IResourceProvider _UnderlyingResourceProvider;
		ICache _Cache = new Cache();
		IStringProvider _CacheKeyProvider = new UniqueCacheKeyProvider();
		
		public IResourceProvider UnderlyingResourceProvider {
			get { return _UnderlyingResourceProvider; }
			set { _UnderlyingResourceProvider = value; }
		}

		/// <summary>
		/// Get or set cache key provider
		/// </summary>
		public IStringProvider CacheKeyProvider {
			get { return _CacheKeyProvider; }
			set { _CacheKeyProvider = value; }
		}			

		/// <summary>
		/// Get or set cache instance
		/// </summary>
		public ICache Cache {
			get { return _Cache; }
			set { _Cache = value; }
		}		
		
		protected string ComposeKey(string id, string placeId, CultureInfo culture) {
			if (culture==null)
				culture = Thread.CurrentThread.CurrentUICulture;
			return CacheKeyProvider.GetString(new object[] { id, placeId, culture.TwoLetterISOLanguageName });
		}
		
		public object GetResource(string id) {
			string key = ComposeKey(id,null,null);
			object cachedValue = Cache.Get(key);
			if (cachedValue==null) {
				cachedValue = UnderlyingResourceProvider.GetResource(id);
				Cache.Put(key, cachedValue);
			}
			return cachedValue;
		}

		public object GetResource(string id, string placeId) {
			string key = ComposeKey(id, placeId, null);
			object cachedValue = Cache.Get(key);
			if (cachedValue == null) {
				cachedValue = UnderlyingResourceProvider.GetResource(id, placeId);
				Cache.Put(key, cachedValue);
			}
			return cachedValue;
		}

		public object GetResource(string id, string placeId, CultureInfo culture) {
			string key = ComposeKey(id, placeId, culture);
			object cachedValue = Cache.Get(key);
			if (cachedValue == null) {
				cachedValue = UnderlyingResourceProvider.GetResource(id, placeId, culture);
				Cache.Put(key, cachedValue);
			}
			return cachedValue;
		}

		public object GetResource(string id, CultureInfo culture) {
			string key = ComposeKey(id, null, culture);
			object cachedValue = Cache.Get(key);
			if (cachedValue == null) {
				cachedValue = UnderlyingResourceProvider.GetResource(id, culture);
				Cache.Put(key, cachedValue);
			}
			return cachedValue;
		}
		
		
	}
}
