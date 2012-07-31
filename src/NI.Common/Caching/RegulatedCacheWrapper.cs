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

using NI.Common.Providers;

namespace NI.Common.Caching
{
	public class RegulatedCacheWrapper : ICache
    {
        private object _Enabled = null;
        private ICache _UnderlyingCache;
        private IBooleanProvider _IsEnabledProvider;

        public ICache UnderlyingCache
        {
            get { return _UnderlyingCache; }
            set { _UnderlyingCache = value; }
        }

        public IBooleanProvider IsEnabledProvider
        {
            get { return _IsEnabledProvider; }
            set { _IsEnabledProvider = value; }
        }

        public bool Enabled
        {
            get 
            {
                if (_Enabled == null && IsEnabledProvider != null)
                {
                    _Enabled = IsEnabledProvider.GetBoolean(new Hashtable());
                }
                return _Enabled == null || Convert.ToBoolean(_Enabled); 
            }
            set { _Enabled = value; }
        }

        public void Clear()
        {
            UnderlyingCache.Clear();
        }

        public void Put(string key, object value)
        {
            if (Enabled)
            {
                UnderlyingCache.Put(key, value);
            }
        }

        public void Put(string key, object value, ICacheEntryValidator validator)
        {
            if (Enabled)
            {
                UnderlyingCache.Put(key, value, validator);
            }
        }

        public object Get(string key)
        {
            return Enabled ? UnderlyingCache.Get(key) : null;
        }

        public void Remove(string key)
        {
            if (Enabled)
            {
                UnderlyingCache.Remove(key);
            }
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return UnderlyingCache.GetEnumerator();
        }
    }
}
