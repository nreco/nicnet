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

namespace NI.Common.Caching
{
	/// <summary>
	/// ICache wrapper that adds special prefix to keys.
	/// </summary>
	public class PrefixCacheWrapper : ICache
	{
		ICache _UnderlyingCache;
		string _Prefix;
		
		public ICache UnderlyingCache {
			get { return _UnderlyingCache; }
			set { _UnderlyingCache = value; }
		}
		
		public string Prefix {
			get { return _Prefix; }
			set { _Prefix = value; }
		}
		
		public PrefixCacheWrapper() {
		}

		public void Clear() {
			// TBD: may be clear only values with prefix ?
			UnderlyingCache.Clear();
		}

		public void Put(string key, object value) {
			UnderlyingCache.Put( Prefix+key, value );
		}

		public void Put(string key, object value, ICacheEntryValidator validator) {
			UnderlyingCache.Put( Prefix+key, value, validator );
		}

		public object Get(string key) {
			return UnderlyingCache.Get(Prefix+key);
		}

		public void Remove(string key) {
			UnderlyingCache.Remove(Prefix+key);
		}
		
		public IDictionaryEnumerator GetEnumerator() {
			Hashtable hash = new Hashtable();
			// TBD: may be return only values with prefix?
			
			return UnderlyingCache.GetEnumerator();
		}
		
		
	}
}
