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

namespace NI.Common.Caching {

	/// <summary>
	/// Abstract generic cache interface
	/// </summary>
	public interface ICache {
		
		/// <summary>
		/// Add or update cache entry.
        /// No entry validation or external validation is used in this case.
		/// </summary>
		/// <param name="key">cache entry key</param>
		/// <param name="value">cache entry value</param>
		void Put(string key, object value);
		
		/// <summary>
		/// Add or update cache entry with cache validator.
        /// In this case internal entry validation is used.
		/// </summary>
		/// <param name="key">cache entry key</param>
		/// <param name="value">cache entry value</param>
		void Put(string key, object value, ICacheEntryValidator validator);

        /// <summary>
		/// Get cache entry value by key
		/// </summary>
		/// <param name="key">cache entry key</param>
		/// <returns>cache entry value or null if entry does not exist</returns>
		object Get(string key);
		
		/// <summary>
		/// Remove cache entry with specified key if exists
		/// </summary>
		/// <param name="key">cache entry key</param>
		void Remove(string key);
		
		/// <summary>
		/// Retrieves a dictionary enumerator used to iterate through the key settings and
		/// their values contained in the cache.
		/// </summary>
		IDictionaryEnumerator GetEnumerator();

		/// <summary>
		/// Remove all values from cache
		/// </summary>
		void Clear();
	}
}
