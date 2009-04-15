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

namespace NI.Common.Caching
{
	/// <summary>
	/// Generic ICache implementation based on IDictionary.
	/// </summary>
	public class Cache : ICache
	{
		IDictionary CacheDictionary;

        private bool _Enabled = true;

        [Dependency]
        public bool Enabled {
            get { return _Enabled; }
            set { _Enabled = value; }
        }

        string validatorKeySuffix = "$validator";

		public Cache() {
			CacheDictionary = Hashtable.Synchronized( new Hashtable() );
		}

		public Cache(IDictionary dictionary) {
			CacheDictionary = dictionary;
		}

        /// <summary>
        /// Custom suffix added to entry key to store entry validator.
        /// </summary>
        public string ValidatorKeySuffix {
            get { return validatorKeySuffix; }
            set { validatorKeySuffix = value; }
        }

		public void Clear() {
			CacheDictionary.Clear();
		}

		public void Put(string key, object value) {
            Put(key, value, null);
		}

        protected string GetValidatorKey(string key){
            return key + validatorKeySuffix;
        }

		public void Put(string key, object value, ICacheEntryValidator validator) {
            if (!Enabled) return;
            Remove( key );
			CacheDictionary[key] = value;
            if (validator != null){
                CacheDictionary[GetValidatorKey(key)] = validator;
            }
		}

		public object Get(string key) {
            if (key != null){
                string validatorKey = GetValidatorKey(key);
                if ( CacheDictionary.Contains(validatorKey) ){
                    ICacheEntryValidator validator = 
                        CacheDictionary[validatorKey] as ICacheEntryValidator;
                    if ( validator != null && !validator.IsValid ){
                        Remove( key );
                        return null;
                    }
                }
			    return CacheDictionary[key];
            }
            return null;
		}

		public void Remove(string key) {
            if ( CacheDictionary.Contains(key) ){
                CacheDictionary.Remove( key );
            }
            string validatorKey = GetValidatorKey(key);
            if ( CacheDictionary.Contains(validatorKey) ){
                CacheDictionary.Remove( validatorKey );
            }
		}
		
		public IDictionaryEnumerator GetEnumerator() {
			return CacheDictionary.GetEnumerator();
		}

	}
}
